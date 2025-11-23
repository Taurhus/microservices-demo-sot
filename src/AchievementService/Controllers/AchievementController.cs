using AchievementService.DTOs;
using AchievementService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace AchievementService.Controllers;

[ApiController]
[Route("api/achievements")]
[Produces("application/json")]
public class AchievementController : ControllerBase
{
    private readonly AchievementDb _db;
    private readonly ILogger<AchievementController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public AchievementController(
        AchievementDb db, 
        ILogger<AchievementController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all achievements with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AchievementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AchievementDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var achievements = await _db.Achievements
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AchievementDto(
                a.Id, 
                a.PlayerId, 
                a.Name, 
                a.Description, 
                a.Category, 
                a.Rarity, 
                a.UnlockedDate, 
                a.Progress, 
                a.RequiredProgress, 
                a.Notes))
            .ToListAsync(cancellationToken);

        return Ok(achievements);
    }

    /// <summary>
    /// Get an achievement by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AchievementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AchievementDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var achievement = await _db.Achievements
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AchievementDto(
                a.Id, 
                a.PlayerId, 
                a.Name, 
                a.Description, 
                a.Category, 
                a.Rarity, 
                a.UnlockedDate, 
                a.Progress, 
                a.RequiredProgress, 
                a.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        if (achievement == null)
        {
            _logger.LogWarning("Achievement with ID {AchievementId} not found", id);
            return NotFound();
        }

        return Ok(achievement);
    }

    /// <summary>
    /// Get achievements by player ID
    /// </summary>
    [HttpGet("player/{playerId}")]
    [ProducesResponseType(typeof(IEnumerable<AchievementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AchievementDto>>> GetByPlayerId(
        int playerId,
        CancellationToken cancellationToken = default)
    {
        var achievements = await _db.Achievements
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.UnlockedDate)
            .Select(a => new AchievementDto(
                a.Id, 
                a.PlayerId, 
                a.Name, 
                a.Description, 
                a.Category, 
                a.Rarity, 
                a.UnlockedDate, 
                a.Progress, 
                a.RequiredProgress, 
                a.Notes))
            .ToListAsync(cancellationToken);

        return Ok(achievements);
    }

    /// <summary>
    /// Create a new achievement
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AchievementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AchievementDto>> Post(
        [FromBody] CreateAchievementDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Achievement achievement;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            achievement = new Achievement 
            { 
                PlayerId = dto.PlayerId,
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Rarity = dto.Rarity,
                Progress = dto.Progress,
                RequiredProgress = dto.RequiredProgress,
                Notes = dto.Notes,
                UnlockedDate = dto.Progress >= dto.RequiredProgress ? DateTime.UtcNow : DateTime.MinValue
            };
            _db.Achievements.Add(achievement);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "achievement.created",
                new { AchievementId = achievement.Id, PlayerId = achievement.PlayerId, Name = achievement.Name, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created achievement with ID {AchievementId}", achievement.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create achievement - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = achievement.Id }, new AchievementDto(
            achievement.Id, 
            achievement.PlayerId, 
            achievement.Name, 
            achievement.Description, 
            achievement.Category, 
            achievement.Rarity, 
            achievement.UnlockedDate, 
            achievement.Progress, 
            achievement.RequiredProgress, 
            achievement.Notes));
    }

    /// <summary>
    /// Update an existing achievement
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateAchievementDto dto,
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

        var achievement = await _db.Achievements.FindAsync(new object[] { id }, cancellationToken);
        if (achievement == null)
        {
            _logger.LogWarning("Achievement with ID {AchievementId} not found for update", id);
            return NotFound();
        }

        achievement.PlayerId = dto.PlayerId;
        achievement.Name = dto.Name;
        achievement.Description = dto.Description;
        achievement.Category = dto.Category;
        achievement.Rarity = dto.Rarity;
        achievement.UnlockedDate = dto.UnlockedDate;
        achievement.Progress = dto.Progress;
        achievement.RequiredProgress = dto.RequiredProgress;
        achievement.Notes = dto.Notes;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "achievement.updated",
                new { AchievementId = achievement.Id, PlayerId = achievement.PlayerId, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated achievement with ID {AchievementId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Achievements.AnyAsync(a => a.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update achievement - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete an achievement
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var achievement = await _db.Achievements.FindAsync(new object[] { id }, cancellationToken);
        if (achievement == null)
        {
            _logger.LogWarning("Achievement with ID {AchievementId} not found for deletion", id);
            return NotFound();
        }

        var achievementId = achievement.Id;
        var playerId = achievement.PlayerId;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Achievements.Remove(achievement);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "achievement.deleted",
                new { AchievementId = achievementId, PlayerId = playerId, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted achievement with ID {AchievementId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete achievement - transaction rolled back");
            throw;
        }
    }
}

