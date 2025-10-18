using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    public class ShopServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdShopId;

        public ShopServiceIntegrationTests()
        {
            // Assumes ShopService is running on localhost:5008 (adjust as needed)
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5008/") };
        }

        [Fact]
        public async Task GetShops_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/shop");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetShopById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/shop/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateShop_ReturnsCreated()
        {
            var newShop = new { Name = "Test Shop" };
            var response = await _client.PostAsJsonAsync("api/shop", newShop);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ShopResponse>();
            Assert.NotNull(created);
            _createdShopId = created.Id;
        }

        [Fact]
        public async Task UpdateShop_ReturnsNoContent()
        {
            if (_createdShopId == 0)
            {
                await CreateShop_ReturnsCreated();
            }
                // Create a new shop
                var newShop = new { Name = "Shop To Update" };
                var createResponse = await _client.PostAsJsonAsync("api/shop", newShop);
                Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
                var created = await createResponse.Content.ReadFromJsonAsync<ShopResponse>();
                Assert.NotNull(created);
                await Task.Delay(100); // Ensure commit
                var updateShop = new { Id = created.Id, Name = "Updated Shop" };
                var response = await _client.PutAsJsonAsync($"api/shop/{created.Id}", updateShop);
                Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteShop_ReturnsNoContent()
        {
            if (_createdShopId == 0)
            {
                await CreateShop_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/shop/{_createdShopId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private class ShopResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
