using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class LocationServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdLocationId;

        public LocationServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetLocations_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/locations");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetLocationById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/locations/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateLocation_ReturnsCreated()
        {
            var newLocation = new { Name = "Test Location", Type = "Outpost", Region = "The Shores of Plenty" };
            var response = await _client.PostAsJsonAsync("api/locations", newLocation);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<LocationResponse>();
            Assert.NotNull(created);
            _createdLocationId = created.Id;
        }

        [Fact]
        public async Task UpdateLocation_ReturnsNoContent()
        {
            if (_createdLocationId == 0)
            {
                await CreateLocation_ReturnsCreated();
            }
            var updateLocation = new { Id = _createdLocationId, Name = "Updated Location", Type = "Island", Region = "The Wilds", Latitude = (decimal?)25.5, Longitude = (decimal?)-30.2, HasMerchant = true, HasShipwright = false, HasWeaponsmith = false, HasTavern = true, IsActive = true };
            var response = await _client.PutAsJsonAsync($"api/locations/{_createdLocationId}", updateLocation);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_ReturnsNoContent()
        {
            if (_createdLocationId == 0)
            {
                await CreateLocation_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/locations/{_createdLocationId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetLocationById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/locations/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateLocation_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidLocation = new { Type = "Outpost" }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/locations", invalidLocation);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateLocation_WithNonExistentId_ReturnsNotFound()
        {
            var updateLocation = new { Id = 99999, Name = "Updated Location", Type = "Island", Region = "The Wilds", Latitude = (decimal?)25.5, Longitude = (decimal?)-30.2, HasMerchant = true, HasShipwright = false, HasWeaponsmith = false, HasTavern = true, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/locations/99999", updateLocation);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateLocation_WithIdMismatch_ReturnsBadRequest()
        {
            var updateLocation = new { Id = 1, Name = "Updated Location", Type = "Island", Region = "The Wilds", Latitude = (decimal?)25.5, Longitude = (decimal?)-30.2, HasMerchant = true, HasShipwright = false, HasWeaponsmith = false, HasTavern = true, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/locations/2", updateLocation); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/locations/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class LocationResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
