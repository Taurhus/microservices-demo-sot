using ItemService.DTOs;
using ItemService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace ItemService.Controllers;

[ApiController]
[Route("api/items")]
[Produces("application/json")]
public class ItemController : ControllerBase
{
    private readonly ItemDb _db;
    private readonly ILogger<ItemController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public ItemController(
        ItemDb db, 
        ILogger<ItemController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all items with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ItemDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var items = await _db.Items
            .AsNoTracking()
            .OrderBy(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ItemDto(
                i.Id, 
                i.Name, 
                i.Description, 
                i.Category, 
                i.Rarity, 
                i.BaseValue, 
                i.IsStackable, 
                i.MaxStackSize, 
                i.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    /// <summary>
    /// Get an item by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new ItemDto(
                i.Id, 
                i.Name, 
                i.Description, 
                i.Category, 
                i.Rarity, 
                i.BaseValue, 
                i.IsStackable, 
                i.MaxStackSize, 
                i.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found", id);
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemDto>> Post(
        [FromBody] CreateItemDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Item item;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            item = new Item 
            { 
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Rarity = dto.Rarity,
                BaseValue = dto.BaseValue,
                IsStackable = dto.IsStackable,
                MaxStackSize = dto.MaxStackSize,
                IsActive = dto.IsActive
            };
            _db.Items.Add(item);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "item.created",
                new { ItemId = item.Id, Name = item.Name, Category = item.Category, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created item with ID {ItemId}", item.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create item - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = item.Id }, new ItemDto(
            item.Id, 
            item.Name, 
            item.Description, 
            item.Category, 
            item.Rarity, 
            item.BaseValue, 
            item.IsStackable, 
            item.MaxStackSize, 
            item.IsActive));
    }

    /// <summary>
    /// Update an existing item
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateItemDto dto,
        CancellationToken cancellationToken = default)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = await _db.Items.FindAsync(new object[] { id }, cancellationToken);
        if (item == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found for update", id);
            return NotFound();
        }

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Category = dto.Category;
        item.Rarity = dto.Rarity;
        item.BaseValue = dto.BaseValue;
        item.IsStackable = dto.IsStackable;
        item.MaxStackSize = dto.MaxStackSize;
        item.IsActive = dto.IsActive;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "item.updated",
                new { ItemId = item.Id, Name = item.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated item with ID {ItemId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Items.AnyAsync(i => i.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update item - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete an item
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var item = await _db.Items.FindAsync(new object[] { id }, cancellationToken);
        if (item == null)
        {
            _logger.LogWarning("Item with ID {ItemId} not found for deletion", id);
            return NotFound();
        }

        var itemId = item.Id;
        var itemName = item.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Items.Remove(item);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "item.deleted",
                new { ItemId = itemId, Name = itemName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted item with ID {ItemId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete item - transaction rolled back");
            throw;
        }
    }
}
