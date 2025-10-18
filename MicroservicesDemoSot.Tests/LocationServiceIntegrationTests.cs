using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    public class LocationServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdLocationId;

        public LocationServiceIntegrationTests()
        {
            // Assumes LocationService is running on localhost:5007 (adjust as needed)
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5007/") };
        }

        [Fact]
        public async Task GetLocations_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/location");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetLocationById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/location/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateLocation_ReturnsCreated()
        {
            var newLocation = new { Name = "Test Location" };
            var response = await _client.PostAsJsonAsync("api/location", newLocation);
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
            var updateLocation = new { Id = _createdLocationId, Name = "Updated Location" };
            var response = await _client.PutAsJsonAsync($"api/location/{_createdLocationId}", updateLocation);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_ReturnsNoContent()
        {
            if (_createdLocationId == 0)
            {
                await CreateLocation_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/location/{_createdLocationId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private class LocationResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
