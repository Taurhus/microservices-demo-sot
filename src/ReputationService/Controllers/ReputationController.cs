using ReputationService.DTOs;
using ReputationService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace ReputationService.Controllers;

[ApiController]
[Route("api/reputations")]
[Produces("application/json")]
public class ReputationController : ControllerBase
{
    private readonly ReputationDb _db;
    private readonly ILogger<ReputationController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public ReputationController(
        ReputationDb db, 
        ILogger<ReputationController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all reputations with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReputationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReputationDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var reputations = await _db.Reputations
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReputationDto(
                r.Id, 
                r.PlayerId, 
                r.FactionName, 
                r.Level, 
                r.TotalReputation, 
                r.LastUpdated, 
                r.Notes))
            .ToListAsync(cancellationToken);

        return Ok(reputations);
    }

    /// <summary>
    /// Get a reputation by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReputationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReputationDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var reputation = await _db.Reputations
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReputationDto(
                r.Id, 
                r.PlayerId, 
                r.FactionName, 
                r.Level, 
                r.TotalReputation, 
                r.LastUpdated, 
                r.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        if (reputation == null)
        {
            _logger.LogWarning("Reputation with ID {ReputationId} not found", id);
            return NotFound();
        }

        return Ok(reputation);
    }

    /// <summary>
    /// Get reputations by player ID
    /// </summary>
    [HttpGet("player/{playerId}")]
    [ProducesResponseType(typeof(IEnumerable<ReputationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReputationDto>>> GetByPlayerId(
        int playerId,
        CancellationToken cancellationToken = default)
    {
        var reputations = await _db.Reputations
            .AsNoTracking()
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.Level)
            .Select(r => new ReputationDto(
                r.Id, 
                r.PlayerId, 
                r.FactionName, 
                r.Level, 
                r.TotalReputation, 
                r.LastUpdated, 
                r.Notes))
            .ToListAsync(cancellationToken);

        return Ok(reputations);
    }

    /// <summary>
    /// Create a new reputation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReputationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReputationDto>> Post(
        [FromBody] CreateReputationDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Reputation reputation;
        
        // Use a database transaction to ensure atomicity between database save and event publishing
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            reputation = new Reputation 
            { 
                PlayerId = dto.PlayerId,
                FactionName = dto.FactionName,
                Level = dto.Level,
                TotalReputation = dto.TotalReputation,
                Notes = dto.Notes,
                LastUpdated = DateTime.UtcNow
            };
            _db.Reputations.Add(reputation);
            
            // Save to get the ID (transaction not committed yet)
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            // If this fails, the entire transaction will rollback
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "reputation.created",
                new { ReputationId = reputation.Id, PlayerId = reputation.PlayerId, FactionName = reputation.FactionName, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event (both reputation and event are in the same transaction)
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction - both reputation and event are saved atomically
            // If commit fails, both will be rolled back
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created reputation with ID {ReputationId}", reputation.Id);
        }
        catch (Exception ex)
        {
            // Rollback transaction if anything fails
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create reputation - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = reputation.Id }, new ReputationDto(
            reputation.Id, 
            reputation.PlayerId, 
            reputation.FactionName, 
            reputation.Level, 
            reputation.TotalReputation, 
            reputation.LastUpdated, 
            reputation.Notes));
    }

    /// <summary>
    /// Update an existing reputation
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateReputationDto dto,
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

        var reputation = await _db.Reputations.FindAsync(new object[] { id }, cancellationToken);
        if (reputation == null)
        {
            _logger.LogWarning("Reputation with ID {ReputationId} not found for update", id);
            return NotFound();
        }

        reputation.PlayerId = dto.PlayerId;
        reputation.FactionName = dto.FactionName;
        reputation.Level = dto.Level;
        reputation.TotalReputation = dto.TotalReputation;
        reputation.LastUpdated = DateTime.UtcNow;
        reputation.Notes = dto.Notes;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "reputation.updated",
                new { ReputationId = reputation.Id, PlayerId = reputation.PlayerId, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated reputation with ID {ReputationId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Reputations.AnyAsync(r => r.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update reputation - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a reputation
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var reputation = await _db.Reputations.FindAsync(new object[] { id }, cancellationToken);
        if (reputation == null)
        {
            _logger.LogWarning("Reputation with ID {ReputationId} not found for deletion", id);
            return NotFound();
        }

        var reputationId = reputation.Id;
        var playerId = reputation.PlayerId;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Reputations.Remove(reputation);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "reputation.deleted",
                new { ReputationId = reputationId, PlayerId = playerId, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted reputation with ID {ReputationId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete reputation - transaction rolled back");
            throw;
        }
    }
}

