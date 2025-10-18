using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    public class ItemServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdItemId;

        public ItemServiceIntegrationTests()
        {
            // Assumes ItemService is running on localhost:5006 (adjust as needed)
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5006/") };
        }

        [Fact]
        public async Task GetItems_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/item");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetItemById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/item/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateItem_ReturnsCreated()
        {
            var newItem = new { Name = "Test Item" };
            var response = await _client.PostAsJsonAsync("api/item", newItem);
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
            var updateItem = new { Id = _createdItemId, Name = "Updated Item" };
            var response = await _client.PutAsJsonAsync($"api/item/{_createdItemId}", updateItem);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_ReturnsNoContent()
        {
            if (_createdItemId == 0)
            {
                await CreateItem_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/item/{_createdItemId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private class ItemResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
