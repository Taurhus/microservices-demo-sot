using LocationService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly LocationDb _db;
        public LocationController(LocationDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var locations = await _db.Locations.ToListAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var location = await _db.Locations.FindAsync(id);
            if (location == null) return NotFound();
            return Ok(location);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Location location)
        {
            _db.Locations.Add(location);
            await _db.SaveChangesAsync();
            return Created($"/locations/{location.Id}", location);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Location location)
        {
            if (id != location.Id) return BadRequest();
            _db.Entry(location).State = EntityState.Modified;
            try {
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!_db.Locations.Any(e => e.Id == id)) return NotFound();
                else throw;
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating location: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _db.Locations.FindAsync(id);
            if (location == null) return NotFound();
            try {
                _db.Locations.Remove(location);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting location: {ex.Message}");
            }
        }
    }
}