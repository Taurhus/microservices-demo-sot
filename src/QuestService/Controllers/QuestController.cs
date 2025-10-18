using QuestService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuestService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestController : ControllerBase
    {
        private readonly QuestDb _db;
        public QuestController(QuestDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var quests = await _db.Quests.ToListAsync();
            return Ok(quests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var quest = await _db.Quests.FindAsync(id);
            if (quest == null) return NotFound();
            return Ok(quest);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Quest quest)
        {
            _db.Quests.Add(quest);
            await _db.SaveChangesAsync();
            return Created($"/quests/{quest.Id}", quest);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Quest quest)
        {
            if (id != quest.Id) return BadRequest();
            _db.Entry(quest).State = EntityState.Modified;
            try {
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!_db.Quests.Any(e => e.Id == id)) return NotFound();
                else throw;
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating quest: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var quest = await _db.Quests.FindAsync(id);
            if (quest == null) return NotFound();
            try {
                _db.Quests.Remove(quest);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting quest: {ex.Message}");
            }
        }
    }
}
