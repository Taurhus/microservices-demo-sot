using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerService.Models;

namespace PlayerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly PlayerDb _db;
        public PlayerController(PlayerDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var players = await _db.Players.ToListAsync();
            return Ok(players);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var player = await _db.Players.FindAsync(id);
            if (player == null) return NotFound();
            return Ok(player);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Player player)
        {
            _db.Players.Add(player);
            await _db.SaveChangesAsync();
            return Created($"/players/{player.Id}", player);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Player player)
        {
            if (id != player.Id) return BadRequest();
            var existing = await _db.Players.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = player.Name;
            // Update other properties as needed
            try {
                await _db.SaveChangesAsync();
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating player: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var player = await _db.Players.FindAsync(id);
            if (player == null) return NotFound();
            try {
                _db.Players.Remove(player);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting player: {ex.Message}");
            }
        }
    }
}
