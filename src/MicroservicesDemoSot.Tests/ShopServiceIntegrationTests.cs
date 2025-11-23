using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class ShopServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdShopId;

        public ShopServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetShops_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/shops");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetShopById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/shops/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateShop_ReturnsCreated()
        {
            var newShop = new { Name = "Test Shop", Type = "Merchant", LocationName = "Plunder Outpost", Description = "A test shop" };
            var response = await _client.PostAsJsonAsync("api/shops", newShop);
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
            var newShop = new { Name = "Shop To Update", Type = "Weaponsmith", LocationName = "Sanctuary Outpost", Description = "A shop to update" };
            var createResponse = await _client.PostAsJsonAsync("api/shops", newShop);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<ShopResponse>();
            Assert.NotNull(created);
            await Task.Delay(100); // Ensure commit
            var updateShop = new { Id = created.Id, Name = "Updated Shop", Type = "Shipwright", LocationName = "Galleon's Grave Outpost", Description = "Updated description", IsActive = true };
            var response = await _client.PutAsJsonAsync($"api/shops/{created.Id}", updateShop);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteShop_ReturnsNoContent()
        {
            if (_createdShopId == 0)
            {
                await CreateShop_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/shops/{_createdShopId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetShopById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/shops/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateShop_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidShop = new { Type = "Merchant" }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/shops", invalidShop);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateShop_WithNonExistentId_ReturnsNotFound()
        {
            var updateShop = new { Id = 99999, Name = "Updated Shop", Type = "Shipwright", LocationName = "Galleon's Grave Outpost", Description = "Updated description", IsActive = true };
            var response = await _client.PutAsJsonAsync("api/shops/99999", updateShop);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateShop_WithIdMismatch_ReturnsBadRequest()
        {
            var updateShop = new { Id = 1, Name = "Updated Shop", Type = "Shipwright", LocationName = "Galleon's Grave Outpost", Description = "Updated description", IsActive = true };
            var response = await _client.PutAsJsonAsync("api/shops/2", updateShop); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteShop_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/shops/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class ShopResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
