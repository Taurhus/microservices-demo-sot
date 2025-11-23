using EventService.DTOs;
using EventService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace EventService.Controllers;

[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventController : ControllerBase
{
    private readonly EventDb _db;
    private readonly ILogger<EventController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public EventController(
        EventDb db, 
        ILogger<EventController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all events with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var events = await _db.Events
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventDto(
                e.Id, 
                e.Name, 
                e.Description, 
                e.Type, 
                e.Difficulty, 
                e.MinPlayers, 
                e.MaxPlayers, 
                e.EstimatedDurationMinutes, 
                e.IsActive, 
                e.IntroducedDate))
            .ToListAsync(cancellationToken);

        return Ok(events);
    }

    /// <summary>
    /// Get an event by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var ev = await _db.Events
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EventDto(
                e.Id, 
                e.Name, 
                e.Description, 
                e.Type, 
                e.Difficulty, 
                e.MinPlayers, 
                e.MaxPlayers, 
                e.EstimatedDurationMinutes, 
                e.IsActive, 
                e.IntroducedDate))
            .FirstOrDefaultAsync(cancellationToken);

        if (ev == null)
        {
            _logger.LogWarning("Event with ID {EventId} not found", id);
            return NotFound();
        }

        return Ok(ev);
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDto>> Post(
        [FromBody] CreateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        EventEntity ev;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            ev = new EventEntity 
            { 
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Difficulty = dto.Difficulty,
                MinPlayers = dto.MinPlayers,
                MaxPlayers = dto.MaxPlayers,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                IsActive = dto.IsActive,
                IntroducedDate = dto.IntroducedDate
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "event.created",
                new { EventId = ev.Id, Name = ev.Name, Type = ev.Type, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created event with ID {EventId}", ev.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create event - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = ev.Id }, new EventDto(
            ev.Id, 
            ev.Name, 
            ev.Description, 
            ev.Type, 
            ev.Difficulty, 
            ev.MinPlayers, 
            ev.MaxPlayers, 
            ev.EstimatedDurationMinutes, 
            ev.IsActive, 
            ev.IntroducedDate));
    }

    /// <summary>
    /// Update an existing event
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateEventDto dto,
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

        var ev = await _db.Events.FindAsync(new object[] { id }, cancellationToken);
        if (ev == null)
        {
            _logger.LogWarning("Event with ID {EventId} not found for update", id);
            return NotFound();
        }

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.Type = dto.Type;
        ev.Difficulty = dto.Difficulty;
        ev.MinPlayers = dto.MinPlayers;
        ev.MaxPlayers = dto.MaxPlayers;
        ev.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
        ev.IsActive = dto.IsActive;
        ev.IntroducedDate = dto.IntroducedDate;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "event.updated",
                new { EventId = ev.Id, Name = ev.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated event with ID {EventId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Events.AnyAsync(e => e.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update event - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete an event
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var ev = await _db.Events.FindAsync(new object[] { id }, cancellationToken);
        if (ev == null)
        {
            _logger.LogWarning("Event with ID {EventId} not found for deletion", id);
            return NotFound();
        }

        var eventId = ev.Id;
        var eventName = ev.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Events.Remove(ev);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "event.deleted",
                new { EventId = eventId, Name = eventName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted event with ID {EventId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete event - transaction rolled back");
            throw;
        }
    }
}
