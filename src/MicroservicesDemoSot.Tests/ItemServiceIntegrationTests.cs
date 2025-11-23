using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class ItemServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdItemId;

        public ItemServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetItems_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/items");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetItemById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/items/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateItem_ReturnsCreated()
        {
            var newItem = new { Name = "Test Item", Description = "A test item", Category = "Consumable", Rarity = "Common" };
            var response = await _client.PostAsJsonAsync("api/items", newItem);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ItemResponse>();
            Assert.NotNull(created);
            _createdItemId = created.Id;
        }

        [Fact]
        public async Task UpdateItem_ReturnsNoContent()
        {
            if (_createdItemId == 0)
            {
                await CreateItem_ReturnsCreated();
            }
            var updateItem = new { Id = _createdItemId, Name = "Updated Item", Description = "Updated description", Category = "Weapon", Rarity = "Rare", BaseValue = (decimal?)50.0, IsStackable = false, MaxStackSize = (int?)null, IsActive = true };
            var response = await _client.PutAsJsonAsync($"api/items/{_createdItemId}", updateItem);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_ReturnsNoContent()
        {
            if (_createdItemId == 0)
            {
                await CreateItem_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/items/{_createdItemId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetItemById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/items/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateItem_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidItem = new { Description = "A test item" }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/items", invalidItem);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateItem_WithNonExistentId_ReturnsNotFound()
        {
            var updateItem = new { Id = 99999, Name = "Updated Item", Description = "Updated", Category = "Weapon", Rarity = "Rare", BaseValue = (decimal?)50.0, IsStackable = false, MaxStackSize = (int?)null, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/items/99999", updateItem);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateItem_WithIdMismatch_ReturnsBadRequest()
        {
            var updateItem = new { Id = 1, Name = "Updated Item", Description = "Updated", Category = "Weapon", Rarity = "Rare", BaseValue = (decimal?)50.0, IsStackable = false, MaxStackSize = (int?)null, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/items/2", updateItem); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/items/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class ItemResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
