using ShipService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShipService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipController : ControllerBase
    {
        private readonly ShipDb _db;
        public ShipController(ShipDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var ships = await _db.Ships.ToListAsync();
            return Ok(ships);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var ship = await _db.Ships.FindAsync(id);
            if (ship == null) return NotFound();
            return Ok(ship);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Ship ship)
        {
            _db.Ships.Add(ship);
            await _db.SaveChangesAsync();
            return Created($"/ships/{ship.Id}", ship);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Ship ship)
        {
            if (id != ship.Id) return BadRequest();
            _db.Entry(ship).State = EntityState.Modified;
            try {
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!_db.Ships.Any(e => e.Id == id)) return NotFound();
                else throw;
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating ship: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ship = await _db.Ships.FindAsync(id);
            if (ship == null) return NotFound();
            try {
                _db.Ships.Remove(ship);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting ship: {ex.Message}");
            }
        }
    }
}
