using LocationService.DTOs;
using LocationService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace LocationService.Controllers;

[ApiController]
[Route("api/locations")]
[Produces("application/json")]
public class LocationController : ControllerBase
{
    private readonly LocationDb _db;
    private readonly ILogger<LocationController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public LocationController(
        LocationDb db, 
        ILogger<LocationController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all locations with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LocationDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var locations = await _db.Locations
            .AsNoTracking()
            .OrderBy(l => l.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LocationDto(
                l.Id, 
                l.Name, 
                l.Type, 
                l.Region, 
                l.Latitude, 
                l.Longitude, 
                l.HasMerchant, 
                l.HasShipwright, 
                l.HasWeaponsmith, 
                l.HasTavern, 
                l.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(locations);
    }

    /// <summary>
    /// Get a location by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocationDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var location = await _db.Locations
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LocationDto(
                l.Id, 
                l.Name, 
                l.Type, 
                l.Region, 
                l.Latitude, 
                l.Longitude, 
                l.HasMerchant, 
                l.HasShipwright, 
                l.HasWeaponsmith, 
                l.HasTavern, 
                l.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (location == null)
        {
            _logger.LogWarning("Location with ID {LocationId} not found", id);
            return NotFound();
        }

        return Ok(location);
    }

    /// <summary>
    /// Create a new location
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocationDto>> Post(
        [FromBody] CreateLocationDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Location location;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            location = new Location 
            { 
                Name = dto.Name,
                Type = dto.Type,
                Region = dto.Region,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                HasMerchant = dto.HasMerchant,
                HasShipwright = dto.HasShipwright,
                HasWeaponsmith = dto.HasWeaponsmith,
                HasTavern = dto.HasTavern,
                IsActive = dto.IsActive
            };
            _db.Locations.Add(location);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "location.created",
                new { LocationId = location.Id, Name = location.Name, Type = location.Type, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created location with ID {LocationId}", location.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create location - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = location.Id }, new LocationDto(
            location.Id, 
            location.Name, 
            location.Type, 
            location.Region, 
            location.Latitude, 
            location.Longitude, 
            location.HasMerchant, 
            location.HasShipwright, 
            location.HasWeaponsmith, 
            location.HasTavern, 
            location.IsActive));
    }

    /// <summary>
    /// Update an existing location
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateLocationDto dto,
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

        var location = await _db.Locations.FindAsync(new object[] { id }, cancellationToken);
        if (location == null)
        {
            _logger.LogWarning("Location with ID {LocationId} not found for update", id);
            return NotFound();
        }

        location.Name = dto.Name;
        location.Type = dto.Type;
        location.Region = dto.Region;
        location.Latitude = dto.Latitude;
        location.Longitude = dto.Longitude;
        location.HasMerchant = dto.HasMerchant;
        location.HasShipwright = dto.HasShipwright;
        location.HasWeaponsmith = dto.HasWeaponsmith;
        location.HasTavern = dto.HasTavern;
        location.IsActive = dto.IsActive;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "location.updated",
                new { LocationId = location.Id, Name = location.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated location with ID {LocationId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Locations.AnyAsync(l => l.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update location - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a location
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var location = await _db.Locations.FindAsync(new object[] { id }, cancellationToken);
        if (location == null)
        {
            _logger.LogWarning("Location with ID {LocationId} not found for deletion", id);
            return NotFound();
        }

        var locationId = location.Id;
        var locationName = location.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Locations.Remove(location);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "location.deleted",
                new { LocationId = locationId, Name = locationName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted location with ID {LocationId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete location - transaction rolled back");
            throw;
        }
    }
}