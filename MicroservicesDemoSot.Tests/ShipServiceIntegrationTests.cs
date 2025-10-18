using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    public class ShipServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdShipId;

        public ShipServiceIntegrationTests()
        {
            // Assumes ShipService is running on localhost:5002 (adjust as needed)
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5002/") };
        }

        [Fact]
        public async Task GetShips_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/ship");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetShipById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/ship/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateShip_ReturnsCreated()
        {
            var newShip = new { Name = "Test Ship" };
            var response = await _client.PostAsJsonAsync("api/ship", newShip);
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
            var updateShip = new { Id = _createdShipId, Name = "Updated Ship" };
            var response = await _client.PutAsJsonAsync($"api/ship/{_createdShipId}", updateShip);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteShip_ReturnsNoContent()
        {
            if (_createdShipId == 0)
            {
                await CreateShip_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/ship/{_createdShipId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private class ShipResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
