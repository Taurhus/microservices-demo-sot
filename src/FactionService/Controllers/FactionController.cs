using FactionService.DTOs;
using FactionService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace FactionService.Controllers;

[ApiController]
[Route("api/factions")]
[Produces("application/json")]
public class FactionController : ControllerBase
{
    private readonly FactionDb _db;
    private readonly ILogger<FactionController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public FactionController(
        FactionDb db, 
        ILogger<FactionController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all factions with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FactionDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var factions = await _db.Factions
            .AsNoTracking()
            .OrderBy(f => f.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FactionDto(
                f.Id, 
                f.Name, 
                f.Description, 
                f.Type, 
                f.Headquarters, 
                f.MaxReputationLevel, 
                f.IsActive, 
                f.IntroducedDate))
            .ToListAsync(cancellationToken);

        return Ok(factions);
    }

    /// <summary>
    /// Get a faction by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FactionDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var faction = await _db.Factions
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FactionDto(
                f.Id, 
                f.Name, 
                f.Description, 
                f.Type, 
                f.Headquarters, 
                f.MaxReputationLevel, 
                f.IsActive, 
                f.IntroducedDate))
            .FirstOrDefaultAsync(cancellationToken);

        if (faction == null)
        {
            _logger.LogWarning("Faction with ID {FactionId} not found", id);
            return NotFound();
        }

        return Ok(faction);
    }

    /// <summary>
    /// Create a new faction
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FactionDto>> Post(
        [FromBody] CreateFactionDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Faction faction;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            faction = new Faction 
            { 
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Headquarters = dto.Headquarters,
                MaxReputationLevel = dto.MaxReputationLevel,
                IsActive = dto.IsActive,
                IntroducedDate = dto.IntroducedDate
            };
            _db.Factions.Add(faction);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "faction.created",
                new { FactionId = faction.Id, Name = faction.Name, Type = faction.Type, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created faction with ID {FactionId}", faction.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create faction - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = faction.Id }, new FactionDto(
            faction.Id, 
            faction.Name, 
            faction.Description, 
            faction.Type, 
            faction.Headquarters, 
            faction.MaxReputationLevel, 
            faction.IsActive, 
            faction.IntroducedDate));
    }

    /// <summary>
    /// Update an existing faction
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateFactionDto dto,
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

        var faction = await _db.Factions.FindAsync(new object[] { id }, cancellationToken);
        if (faction == null)
        {
            _logger.LogWarning("Faction with ID {FactionId} not found for update", id);
            return NotFound();
        }

        faction.Name = dto.Name;
        faction.Description = dto.Description;
        faction.Type = dto.Type;
        faction.Headquarters = dto.Headquarters;
        faction.MaxReputationLevel = dto.MaxReputationLevel;
        faction.IsActive = dto.IsActive;
        faction.IntroducedDate = dto.IntroducedDate;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "faction.updated",
                new { FactionId = faction.Id, Name = faction.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated faction with ID {FactionId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Factions.AnyAsync(f => f.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update faction - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a faction
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var faction = await _db.Factions.FindAsync(new object[] { id }, cancellationToken);
        if (faction == null)
        {
            _logger.LogWarning("Faction with ID {FactionId} not found for deletion", id);
            return NotFound();
        }

        var factionId = faction.Id;
        var factionName = faction.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Factions.Remove(faction);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "faction.deleted",
                new { FactionId = factionId, Name = factionName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted faction with ID {FactionId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete faction - transaction rolled back");
            throw;
        }
    }
}
