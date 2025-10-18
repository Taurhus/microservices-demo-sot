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
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5005/") };
        }

        [Fact]
        public async Task GetEvents_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/event");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetEventById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/event/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateEvent_ReturnsCreatedAndStoresId()
        {
            var newEvent = new { Name = "Test Event" };
            var response = await _client.PostAsJsonAsync("api/event", newEvent);
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
            var updateEvent = new { Id = 1, Name = "Updated Event" };
            var response = await _client.PutAsJsonAsync("api/event/1", updateEvent);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_DeletesCreatedEvent()
        {
            if (_createdEventId == 0)
            {
                await CreateEvent_ReturnsCreatedAndStoresId();
            }
            var delResponse = await _client.DeleteAsync($"api/event/{_createdEventId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, delResponse.StatusCode);
        }

        private class EventDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
