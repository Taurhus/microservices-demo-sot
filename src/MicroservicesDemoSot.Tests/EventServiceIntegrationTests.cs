using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class EventServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdEventId;

        public EventServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetEvents_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/events");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetEventById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/events/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateEvent_ReturnsCreatedAndStoresId()
        {
            var newEvent = new { 
                Name = "Test Event", 
                Description = "Test event description", 
                Type = "WorldEvent", 
                Difficulty = "Normal", 
                MinPlayers = 1, 
                MaxPlayers = 4, 
                EstimatedDurationMinutes = 20, 
                IsActive = true, 
                IntroducedDate = "2020-01-01" 
            };
            var response = await _client.PostAsJsonAsync("api/events", newEvent);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<EventDto>();
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            _createdEventId = created.Id;
        }

        [Fact]
        public async Task UpdateEvent_ReturnsNoContent()
        {
            // Update seeded data (id=1) as before
            var updateEvent = new { 
                Id = 1, 
                Name = "Updated Event", 
                Description = "Updated description", 
                Type = "WorldEvent", 
                Difficulty = "Normal", 
                MinPlayers = 1, 
                MaxPlayers = 4, 
                EstimatedDurationMinutes = 20, 
                IsActive = true, 
                IntroducedDate = "2020-01-01" 
            };
            var response = await _client.PutAsJsonAsync("api/events/1", updateEvent);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_DeletesCreatedEvent()
        {
            if (_createdEventId == 0)
            {
                await CreateEvent_ReturnsCreatedAndStoresId();
            }
            var delResponse = await _client.DeleteAsync($"api/events/{_createdEventId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, delResponse.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetEventById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/events/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidEvent = new { Description = "Test event description" }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/events", invalidEvent);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_WithNonExistentId_ReturnsNotFound()
        {
            var updateEvent = new { Id = 99999, Name = "Updated Event", Description = "Updated", Type = "WorldEvent", Difficulty = "Normal", MinPlayers = 1, MaxPlayers = 4, EstimatedDurationMinutes = 20, IsActive = true, IntroducedDate = "2020-01-01" };
            var response = await _client.PutAsJsonAsync("api/events/99999", updateEvent);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_WithIdMismatch_ReturnsBadRequest()
        {
            var updateEvent = new { Id = 1, Name = "Updated Event", Description = "Updated", Type = "WorldEvent", Difficulty = "Normal", MinPlayers = 1, MaxPlayers = 4, EstimatedDurationMinutes = 20, IsActive = true, IntroducedDate = "2020-01-01" };
            var response = await _client.PutAsJsonAsync("api/events/2", updateEvent); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/events/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class EventDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
