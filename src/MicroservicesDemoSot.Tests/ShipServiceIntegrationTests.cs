using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class ShipServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdShipId;

        public ShipServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetShips_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/ships");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetShipById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/ships/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateShip_ReturnsCreated()
        {
            var newShip = new { Name = "Test Ship", Type = "Sloop", Description = "A test ship", MaxCrewSize = 2, CannonCount = 2, MastCount = 1 };
            var response = await _client.PostAsJsonAsync("api/ships", newShip);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ShipResponse>();
            Assert.NotNull(created);
            _createdShipId = created.Id;
        }

        [Fact]
        public async Task UpdateShip_ReturnsNoContent()
        {
            if (_createdShipId == 0)
            {
                await CreateShip_ReturnsCreated();
            }
            var updateShip = new { Id = _createdShipId, Name = "Updated Ship", Type = "Brigantine", Description = "Updated description", MaxCrewSize = 3, CannonCount = 4, MastCount = 2, IsActive = true };
            var response = await _client.PutAsJsonAsync($"api/ships/{_createdShipId}", updateShip);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteShip_ReturnsNoContent()
        {
            if (_createdShipId == 0)
            {
                await CreateShip_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/ships/{_createdShipId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetShipById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/ships/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateShip_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidShip = new { Type = "Sloop" }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/ships", invalidShip);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateShip_WithNonExistentId_ReturnsNotFound()
        {
            var updateShip = new { Id = 99999, Name = "Updated Ship", Type = "Brigantine", Description = "Updated", MaxCrewSize = 3, CannonCount = 4, MastCount = 2, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/ships/99999", updateShip);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateShip_WithIdMismatch_ReturnsBadRequest()
        {
            var updateShip = new { Id = 1, Name = "Updated Ship", Type = "Brigantine", Description = "Updated", MaxCrewSize = 3, CannonCount = 4, MastCount = 2, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/ships/2", updateShip); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteShip_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/ships/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class ShipResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
