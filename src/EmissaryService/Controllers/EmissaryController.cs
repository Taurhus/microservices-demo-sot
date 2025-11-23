using EmissaryService.DTOs;
using EmissaryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace EmissaryService.Controllers;

[ApiController]
[Route("api/emissaries")]
[Produces("application/json")]
public class EmissaryController : ControllerBase
{
    private readonly EmissaryDb _db;
    private readonly ILogger<EmissaryController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public EmissaryController(
        EmissaryDb db, 
        ILogger<EmissaryController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all emissaries with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmissaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmissaryDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var emissaries = await _db.Emissaries
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmissaryDto(
                e.Id, 
                e.PlayerId, 
                e.FactionName, 
                e.Level, 
                e.IsActive, 
                e.RaisedDate, 
                e.LoweredDate, 
                e.Value, 
                e.Notes))
            .ToListAsync(cancellationToken);

        return Ok(emissaries);
    }

    /// <summary>
    /// Get an emissary by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmissaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmissaryDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var emissary = await _db.Emissaries
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EmissaryDto(
                e.Id, 
                e.PlayerId, 
                e.FactionName, 
                e.Level, 
                e.IsActive, 
                e.RaisedDate, 
                e.LoweredDate, 
                e.Value, 
                e.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        if (emissary == null)
        {
            _logger.LogWarning("Emissary with ID {EmissaryId} not found", id);
            return NotFound();
        }

        return Ok(emissary);
    }

    /// <summary>
    /// Get emissaries by player ID
    /// </summary>
    [HttpGet("player/{playerId}")]
    [ProducesResponseType(typeof(IEnumerable<EmissaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmissaryDto>>> GetByPlayerId(
        int playerId,
        CancellationToken cancellationToken = default)
    {
        var emissaries = await _db.Emissaries
            .AsNoTracking()
            .Where(e => e.PlayerId == playerId)
            .OrderByDescending(e => e.IsActive)
            .ThenByDescending(e => e.Level)
            .Select(e => new EmissaryDto(
                e.Id, 
                e.PlayerId, 
                e.FactionName, 
                e.Level, 
                e.IsActive, 
                e.RaisedDate, 
                e.LoweredDate, 
                e.Value, 
                e.Notes))
            .ToListAsync(cancellationToken);

        return Ok(emissaries);
    }

    /// <summary>
    /// Create a new emissary
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmissaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmissaryDto>> Post(
        [FromBody] CreateEmissaryDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Emissary emissary;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            emissary = new Emissary 
            { 
                PlayerId = dto.PlayerId,
                FactionName = dto.FactionName,
                Level = dto.Level,
                IsActive = dto.IsActive,
                Value = dto.Value,
                Notes = dto.Notes,
                RaisedDate = dto.IsActive ? DateTime.UtcNow : null
            };
            _db.Emissaries.Add(emissary);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "emissary.created",
                new { EmissaryId = emissary.Id, PlayerId = emissary.PlayerId, FactionName = emissary.FactionName, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created emissary with ID {EmissaryId}", emissary.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create emissary - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = emissary.Id }, new EmissaryDto(
            emissary.Id, 
            emissary.PlayerId, 
            emissary.FactionName, 
            emissary.Level, 
            emissary.IsActive, 
            emissary.RaisedDate, 
            emissary.LoweredDate, 
            emissary.Value, 
            emissary.Notes));
    }

    /// <summary>
    /// Update an existing emissary
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateEmissaryDto dto,
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

        var emissary = await _db.Emissaries.FindAsync(new object[] { id }, cancellationToken);
        if (emissary == null)
        {
            _logger.LogWarning("Emissary with ID {EmissaryId} not found for update", id);
            return NotFound();
        }

        emissary.PlayerId = dto.PlayerId;
        emissary.FactionName = dto.FactionName;
        emissary.Level = dto.Level;
        emissary.IsActive = dto.IsActive;
        emissary.RaisedDate = dto.RaisedDate;
        emissary.LoweredDate = dto.LoweredDate;
        emissary.Value = dto.Value;
        emissary.Notes = dto.Notes;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "emissary.updated",
                new { EmissaryId = emissary.Id, PlayerId = emissary.PlayerId, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated emissary with ID {EmissaryId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Emissaries.AnyAsync(e => e.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update emissary - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete an emissary
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var emissary = await _db.Emissaries.FindAsync(new object[] { id }, cancellationToken);
        if (emissary == null)
        {
            _logger.LogWarning("Emissary with ID {EmissaryId} not found for deletion", id);
            return NotFound();
        }

        var emissaryId = emissary.Id;
        var playerId = emissary.PlayerId;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Emissaries.Remove(emissary);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "emissary.deleted",
                new { EmissaryId = emissaryId, PlayerId = playerId, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted emissary with ID {EmissaryId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete emissary - transaction rolled back");
            throw;
        }
    }
}

