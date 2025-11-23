using CrewService.DTOs;
using CrewService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace CrewService.Controllers;

[ApiController]
[Route("api/crews")]
[Produces("application/json")]
public class CrewController : ControllerBase
{
    private readonly CrewDb _db;
    private readonly ILogger<CrewController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public CrewController(
        CrewDb db, 
        ILogger<CrewController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all crews with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CrewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CrewDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var crews = await _db.Crews
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CrewDto(
                c.Id, 
                c.Name, 
                c.ShipId, 
                c.MaxMembers, 
                c.CurrentMembers, 
                c.Status, 
                c.CreatedDate, 
                c.LastActivityDate, 
                c.Notes))
            .ToListAsync(cancellationToken);

        return Ok(crews);
    }

    /// <summary>
    /// Get a crew by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CrewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CrewDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var crew = await _db.Crews
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CrewDto(
                c.Id, 
                c.Name, 
                c.ShipId, 
                c.MaxMembers, 
                c.CurrentMembers, 
                c.Status, 
                c.CreatedDate, 
                c.LastActivityDate, 
                c.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        if (crew == null)
        {
            _logger.LogWarning("Crew with ID {CrewId} not found", id);
            return NotFound();
        }

        return Ok(crew);
    }

    /// <summary>
    /// Create a new crew
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CrewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CrewDto>> Post(
        [FromBody] CreateCrewDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Crew crew;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            crew = new Crew 
            { 
                Name = dto.Name,
                ShipId = dto.ShipId,
                MaxMembers = dto.MaxMembers,
                CurrentMembers = dto.CurrentMembers,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            _db.Crews.Add(crew);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "crew.created",
                new { CrewId = crew.Id, Name = crew.Name, ShipId = crew.ShipId, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created crew with ID {CrewId}", crew.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create crew - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = crew.Id }, new CrewDto(
            crew.Id, 
            crew.Name, 
            crew.ShipId, 
            crew.MaxMembers, 
            crew.CurrentMembers, 
            crew.Status, 
            crew.CreatedDate, 
            crew.LastActivityDate, 
            crew.Notes));
    }

    /// <summary>
    /// Update an existing crew
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdateCrewDto dto,
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

        var crew = await _db.Crews.FindAsync(new object[] { id }, cancellationToken);
        if (crew == null)
        {
            _logger.LogWarning("Crew with ID {CrewId} not found for update", id);
            return NotFound();
        }

        crew.Name = dto.Name;
        crew.ShipId = dto.ShipId;
        crew.MaxMembers = dto.MaxMembers;
        crew.CurrentMembers = dto.CurrentMembers;
        crew.Status = dto.Status;
        crew.LastActivityDate = dto.LastActivityDate ?? DateTime.UtcNow;
        crew.Notes = dto.Notes;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "crew.updated",
                new { CrewId = crew.Id, Name = crew.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated crew with ID {CrewId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Crews.AnyAsync(c => c.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update crew - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a crew
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var crew = await _db.Crews.FindAsync(new object[] { id }, cancellationToken);
        if (crew == null)
        {
            _logger.LogWarning("Crew with ID {CrewId} not found for deletion", id);
            return NotFound();
        }

        var crewId = crew.Id;
        var crewName = crew.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Crews.Remove(crew);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "crew.deleted",
                new { CrewId = crewId, Name = crewName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted crew with ID {CrewId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete crew - transaction rolled back");
            throw;
        }
    }
}

