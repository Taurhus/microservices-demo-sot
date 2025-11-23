using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerService.DTOs;
using PlayerService.Models;
using Shared.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace PlayerService.Controllers;

[ApiController]
[Route("api/players")]
[Produces("application/json")]
public class PlayerController : ControllerBase
{
    private readonly PlayerDb _db;
    private readonly ILogger<PlayerController> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITransactionalMessagePublisher _transactionalMessagePublisher;

    public PlayerController(
        PlayerDb db, 
        ILogger<PlayerController> logger, 
        IMessagePublisher messagePublisher,
        ITransactionalMessagePublisher transactionalMessagePublisher)
    {
        _db = db;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _transactionalMessagePublisher = transactionalMessagePublisher;
    }

    /// <summary>
    /// Get all players with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlayerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> Get(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var players = await _db.Players
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlayerDto(
                p.Id, 
                p.Name, 
                p.Gamertag, 
                p.Gold, 
                p.Renown, 
                p.IsPirateLegend, 
                p.LastLoginDate, 
                p.CreatedDate, 
                p.Platform))
            .ToListAsync(cancellationToken);

        return Ok(players);
    }

    /// <summary>
    /// Get a player by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDto>> Get(int id, CancellationToken cancellationToken = default)
    {
        var player = await _db.Players
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PlayerDto(
                p.Id, 
                p.Name, 
                p.Gamertag, 
                p.Gold, 
                p.Renown, 
                p.IsPirateLegend, 
                p.LastLoginDate, 
                p.CreatedDate, 
                p.Platform))
            .FirstOrDefaultAsync(cancellationToken);

        if (player == null)
        {
            _logger.LogWarning("Player with ID {PlayerId} not found", id);
            return NotFound();
        }

        return Ok(player);
    }

    /// <summary>
    /// Create a new player
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlayerDto>> Post(
        [FromBody] CreatePlayerDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Player player;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            player = new Player 
            { 
                Name = dto.Name,
                Gamertag = dto.Gamertag,
                Gold = dto.Gold,
                Renown = dto.Renown,
                IsPirateLegend = dto.IsPirateLegend,
                Platform = dto.Platform,
                CreatedDate = DateTime.UtcNow
            };
            _db.Players.Add(player);
            await _db.SaveChangesAsync(cancellationToken);

            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "player.created",
                new { PlayerId = player.Id, Name = player.Name, Gamertag = player.Gamertag, CreatedAt = DateTime.UtcNow },
                cancellationToken);

            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Created player with ID {PlayerId}", player.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create player - transaction rolled back");
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = player.Id }, new PlayerDto(
            player.Id, 
            player.Name, 
            player.Gamertag, 
            player.Gold, 
            player.Renown, 
            player.IsPirateLegend, 
            player.LastLoginDate, 
            player.CreatedDate, 
            player.Platform));
    }

    /// <summary>
    /// Update an existing player
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] UpdatePlayerDto dto,
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

        var player = await _db.Players.FindAsync(new object[] { id }, cancellationToken);
        if (player == null)
        {
            _logger.LogWarning("Player with ID {PlayerId} not found for update", id);
            return NotFound();
        }

        player.Name = dto.Name;
        player.Gamertag = dto.Gamertag;
        player.Gold = dto.Gold;
        player.Renown = dto.Renown;
        player.IsPirateLegend = dto.IsPirateLegend;
        player.LastLoginDate = dto.LastLoginDate;
        player.Platform = dto.Platform;

        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "player.updated",
                new { PlayerId = player.Id, Name = player.Name, UpdatedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Updated player with ID {PlayerId}", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (!await _db.Players.AnyAsync(p => p.Id == id, cancellationToken))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update player - transaction rolled back");
            throw;
        }
    }

    /// <summary>
    /// Delete a player
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var player = await _db.Players.FindAsync(new object[] { id }, cancellationToken);
        if (player == null)
        {
            _logger.LogWarning("Player with ID {PlayerId} not found for deletion", id);
            return NotFound();
        }

        var playerId = player.Id;
        var playerName = player.Name;
        
        // Use a database transaction to ensure atomicity
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Players.Remove(player);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Save event to outbox in the same transaction
            await _transactionalMessagePublisher.SaveEventToOutboxAsync(_db, "seaofthieves.events",
                "player.deleted",
                new { PlayerId = playerId, Name = playerName, DeletedAt = DateTime.UtcNow },
                cancellationToken);
            
            // Save outbox event
            await _db.SaveChangesAsync(cancellationToken);
            
            // Commit transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted player with ID {PlayerId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to delete player - transaction rolled back");
            throw;
        }
    }
}
