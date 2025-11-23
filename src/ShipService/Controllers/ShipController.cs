using ShipService.DTOs;
using ShipService;
using ShipService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace ShipService.Controllers;

[ApiController]
[Route("api/ships")]
[Produces("application/json")]
public class ShipController : ControllerBase
{
    private readonly ShipDb _db;
    private readonly ILogger<ShipController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public ShipController(
        ShipDb db, 
        ILogger<ShipController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all ships with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShipDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ShipDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var ships = await _db.Ships
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ShipDto(
                s.Id, 
                s.Name, 
                s.Type, 
                s.Description, 
                s.MaxCrewSize, 
                s.CannonCount, 
                s.MastCount, 
                s.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(ships);
    }

    /// <summary>
    /// Get a ship by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var ship = await _db.Ships
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ShipDto(
                s.Id, 
                s.Name, 
                s.Type, 
                s.Description, 
                s.MaxCrewSize, 
                s.CannonCount, 
                s.MastCount, 
                s.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (ship == null)
        {
            _logger.LogWarning("Ship with ID {ShipId} not found", id);
            return NotFound();
        }

        return Ok(ship);
    }

    /// <summary>
    /// Create a new ship
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShipDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShipDto>> Post(
        [FromBody] CreateShipDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Ship ship;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            ship = new Ship 
            { 
                Name = dto.Name,
                Type = dto.Type,
                Description = dto.Description,
                MaxCrewSize = dto.MaxCrewSize,
                CannonCount = dto.CannonCount,
                MastCount = dto.MastCount,
                IsActive = dto.IsActive
            };
            _db.Ships.Add(ship);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "ship.created",
                new { ShipId = ship.Id, Name = ship.Name, Type = ship.Type, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created ship with ID {ShipId}", ship.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create ship - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = ship.Id }, new ShipDto(
            ship.Id, 
            ship.Name, 
            ship.Type, 
            ship.Description, 
            ship.MaxCrewSize, 
            ship.CannonCount, 
            ship.MastCount, 
            ship.IsActive));
    }

    /// <summary>
    /// Update an existing ship
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateShipDto dto,
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

        var ship = await _db.Ships.FindAsync(new object[] { id }, cancellationToken);
        if (ship == null)
        {
            _logger.LogWarning("Ship with ID {ShipId} not found for update", id);
            return NotFound();
        }

        ship.Name = dto.Name;
        ship.Type = dto.Type;
        ship.Description = dto.Description;
        ship.MaxCrewSize = dto.MaxCrewSize;
        ship.CannonCount = dto.CannonCount;
        ship.MastCount = dto.MastCount;
        ship.IsActive = dto.IsActive;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "ship.updated",
                new { ShipId = ship.Id, Name = ship.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated ship with ID {ShipId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Ships.AnyAsync(s => s.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update ship - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a ship
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var ship = await _db.Ships.FindAsync(new object[] { id }, cancellationToken);
        if (ship == null)
        {
            _logger.LogWarning("Ship with ID {ShipId} not found for deletion", id);
            return NotFound();
        }

        var shipId = ship.Id;
        var shipName = ship.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Ships.Remove(ship);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "ship.deleted",
                new { ShipId = shipId, Name = shipName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted ship with ID {ShipId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete ship - transaction rolled back");
            throw;
        }
    }
}
