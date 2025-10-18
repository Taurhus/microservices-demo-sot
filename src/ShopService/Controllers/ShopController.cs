using ShopService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShopService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private readonly ShopDb _db;
        public ShopController(ShopDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var shops = await _db.Shops.ToListAsync();
            return Ok(shops);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var shop = await _db.Shops.FindAsync(id);
            if (shop == null) return NotFound();
            return Ok(shop);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Shop shop)
        {
            try {
                _db.Shops.Add(shop);
                await _db.SaveChangesAsync();
                return Created($"/shops/{shop.Id}", shop);
            } catch (Exception ex) {
                return StatusCode(500, $"Error creating shop: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Shop shop)
        {
            if (id != shop.Id) return BadRequest();
            var existing = await _db.Shops.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = shop.Name;
            // Update other properties as needed
            try {
                await _db.SaveChangesAsync();
            } catch (Exception ex) {
                return StatusCode(500, $"Error updating shop: {ex.Message}");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var shop = await _db.Shops.FindAsync(id);
            if (shop == null) return NotFound();
            try {
                _db.Shops.Remove(shop);
                await _db.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error deleting shop: {ex.Message}");
            }
        }
    }
}
