using ShopService.DTOs;
using ShopService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Controllers;

[ApiController]
[Route("api/shops")]
[Produces("application/json")]
public class ShopController : ControllerBase
{
    private readonly ShopDb _db;
    private readonly ILogger<ShopController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public ShopController(
        ShopDb db, 
        ILogger<ShopController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all shops with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ShopDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var shops = await _db.Shops
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ShopDto(
                s.Id, 
                s.Name, 
                s.Type, 
                s.LocationName, 
                s.Description, 
                s.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(shops);
    }

    /// <summary>
    /// Get a shop by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShopDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShopDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var shop = await _db.Shops
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ShopDto(
                s.Id, 
                s.Name, 
                s.Type, 
                s.LocationName, 
                s.Description, 
                s.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (shop == null)
        {
            _logger.LogWarning("Shop with ID {ShopId} not found", id);
            return NotFound();
        }

        return Ok(shop);
    }

    /// <summary>
    /// Create a new shop
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShopDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShopDto>> Post(
        [FromBody] CreateShopDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Shop shop;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            shop = new Shop 
            { 
                Name = dto.Name,
                Type = dto.Type,
                LocationName = dto.LocationName,
                Description = dto.Description,
                IsActive = dto.IsActive
            };
            _db.Shops.Add(shop);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "shop.created",
                new { ShopId = shop.Id, Name = shop.Name, Type = shop.Type, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created shop with ID {ShopId}", shop.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create shop - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = shop.Id }, new ShopDto(
            shop.Id, 
            shop.Name, 
            shop.Type, 
            shop.LocationName, 
            shop.Description, 
            shop.IsActive));
    }

    /// <summary>
    /// Update an existing shop
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateShopDto dto,
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

        var shop = await _db.Shops.FindAsync(new object[] { id }, cancellationToken);
        if (shop == null)
        {
            _logger.LogWarning("Shop with ID {ShopId} not found for update", id);
            return NotFound();
        }

        shop.Name = dto.Name;
        shop.Type = dto.Type;
        shop.LocationName = dto.LocationName;
        shop.Description = dto.Description;
        shop.IsActive = dto.IsActive;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "shop.updated",
                new { ShopId = shop.Id, Name = shop.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated shop with ID {ShopId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Shops.AnyAsync(s => s.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update shop - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a shop
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var shop = await _db.Shops.FindAsync(new object[] { id }, cancellationToken);
        if (shop == null)
        {
            _logger.LogWarning("Shop with ID {ShopId} not found for deletion", id);
            return NotFound();
        }

        var shopId = shop.Id;
        var shopName = shop.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Shops.Remove(shop);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "shop.deleted",
                new { ShopId = shopId, Name = shopName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted shop with ID {ShopId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete shop - transaction rolled back");
            throw;
        }
    }
}
