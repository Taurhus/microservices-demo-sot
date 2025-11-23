using QuestService.DTOs;
using QuestService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace QuestService.Controllers;

[ApiController]
[Route("api/quests")]
[Produces("application/json")]
public class QuestController : ControllerBase
{
    private readonly QuestDb _db;
    private readonly ILogger<QuestController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public QuestController(
        QuestDb db, 
        ILogger<QuestController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all quests with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QuestDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var quests = await _db.Quests
            .AsNoTracking()
            .OrderBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuestDto(
                q.Id, 
                q.Name, 
                q.Description, 
                q.FactionName, 
                q.Type, 
                q.RequiredReputationLevel, 
                q.EstimatedDurationMinutes, 
                q.GoldReward, 
                q.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(quests);
    }

    /// <summary>
    /// Get a quest by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var quest = await _db.Quests
            .AsNoTracking()
            .Where(q => q.Id == id)
            .Select(q => new QuestDto(
                q.Id, 
                q.Name, 
                q.Description, 
                q.FactionName, 
                q.Type, 
                q.RequiredReputationLevel, 
                q.EstimatedDurationMinutes, 
                q.GoldReward, 
                q.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (quest == null)
        {
            _logger.LogWarning("Quest with ID {QuestId} not found", id);
            return NotFound();
        }

        return Ok(quest);
    }

    /// <summary>
    /// Create a new quest
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(QuestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuestDto>> Post(
        [FromBody] CreateQuestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Quest quest;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            quest = new Quest 
            { 
                Name = dto.Name,
                Description = dto.Description,
                FactionName = dto.FactionName,
                Type = dto.Type,
                RequiredReputationLevel = dto.RequiredReputationLevel,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                GoldReward = dto.GoldReward,
                IsActive = dto.IsActive
            };
            _db.Quests.Add(quest);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "quest.created",
                new { QuestId = quest.Id, Name = quest.Name, Type = quest.Type, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created quest with ID {QuestId}", quest.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create quest - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = quest.Id }, new QuestDto(
            quest.Id, 
            quest.Name, 
            quest.Description, 
            quest.FactionName, 
            quest.Type, 
            quest.RequiredReputationLevel, 
            quest.EstimatedDurationMinutes, 
            quest.GoldReward, 
            quest.IsActive));
    }

    /// <summary>
    /// Update an existing quest
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateQuestDto dto,
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

        var quest = await _db.Quests.FindAsync(new object[] { id }, cancellationToken);
        if (quest == null)
        {
            _logger.LogWarning("Quest with ID {QuestId} not found for update", id);
            return NotFound();
        }

        quest.Name = dto.Name;
        quest.Description = dto.Description;
        quest.FactionName = dto.FactionName;
        quest.Type = dto.Type;
        quest.RequiredReputationLevel = dto.RequiredReputationLevel;
        quest.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
        quest.GoldReward = dto.GoldReward;
        quest.IsActive = dto.IsActive;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "quest.updated",
                new { QuestId = quest.Id, Name = quest.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated quest with ID {QuestId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Quests.AnyAsync(q => q.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update quest - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a quest
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var quest = await _db.Quests.FindAsync(new object[] { id }, cancellationToken);
        if (quest == null)
        {
            _logger.LogWarning("Quest with ID {QuestId} not found for deletion", id);
            return NotFound();
        }

        var questId = quest.Id;
        var questName = quest.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Quests.Remove(quest);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "quest.deleted",
                new { QuestId = questId, Name = questName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted quest with ID {QuestId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete quest - transaction rolled back");
            throw;
        }
    }
}
