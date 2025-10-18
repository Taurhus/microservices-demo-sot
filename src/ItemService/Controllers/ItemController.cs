using ItemService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItemService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ItemDb _db;
        public ItemController(ItemDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var items = await _db.Items.ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Item item)
        {
            _db.Items.Add(item);
            await _db.SaveChangesAsync();
            return Created($"/items/{item.Id}", item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Item item)
        {
            if (id != item.Id) return BadRequest();
            _db.Entry(item).State = EntityState.Modified;
            try {
                await _db.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!_db.Items.Any(e => e.Id == id)) return NotFound();
                else throw;
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating item: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();
            try {
                _db.Items.Remove(item);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting item: {ex.Message}");
            }
        }
    }
}
