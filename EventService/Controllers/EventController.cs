using EventService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventDb _db;
        public EventController(EventDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var events = await _db.Events.ToListAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            return Ok(ev);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EventEntity ev)
        {
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            return Created($"/events/{ev.Id}", ev);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] EventEntity ev)
        {
            if (id != ev.Id) return BadRequest();
            _db.Entry(ev).State = EntityState.Modified;
            try {
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!_db.Events.Any(e => e.Id == id)) return NotFound();
                else throw;
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating event: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            try {
                _db.Events.Remove(ev);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting event: {ex.Message}");
            }
        }
    }
}
