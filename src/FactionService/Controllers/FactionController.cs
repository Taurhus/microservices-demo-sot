using FactionService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FactionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactionController : ControllerBase
    {
        private readonly FactionDb _db;
        public FactionController(FactionDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var factions = await _db.Factions.ToListAsync();
            return Ok(factions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var faction = await _db.Factions.FindAsync(id);
            if (faction == null) return NotFound();
            return Ok(faction);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Faction faction)
        {
            _db.Factions.Add(faction);
            await _db.SaveChangesAsync();
            return Created($"/factions/{faction.Id}", faction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Faction faction)
        {
            if (id != faction.Id) return BadRequest();
            var existing = await _db.Factions.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = faction.Name;
            // Update other properties as needed
            try {
                await _db.SaveChangesAsync();
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating faction: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var faction = await _db.Factions.FindAsync(id);
            if (faction == null) return NotFound();
            try {
                _db.Factions.Remove(faction);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting faction: {ex.Message}");
            }
        }
    }
}
