using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    public class FactionServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdFactionId;

        public FactionServiceIntegrationTests()
        {
            // Assumes FactionService is running on localhost:5004 (adjust as needed)
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5004/") };
        }

        [Fact]
        public async Task GetFactions_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/faction");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetFactionById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/faction/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateFaction_ReturnsCreated()
        {
            var newFaction = new { Name = "Test Faction" };
            var response = await _client.PostAsJsonAsync("api/faction", newFaction);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<FactionResponse>();
            Assert.NotNull(created);
            _createdFactionId = created.Id;
        }

        [Fact]
        public async Task UpdateFaction_ReturnsNoContent()
        {
            if (_createdFactionId == 0)
            {
                await CreateFaction_ReturnsCreated();
            }
            // Create a new faction
            var newFaction = new { Name = "Faction To Update" };
            var createResponse = await _client.PostAsJsonAsync("api/faction", newFaction);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<FactionResponse>();
            Assert.NotNull(created);
            await Task.Delay(100); // Ensure commit
            var updateFaction = new { Id = created.Id, Name = "Updated Faction" };
            var response = await _client.PutAsJsonAsync($"api/faction/{created.Id}", updateFaction);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteFaction_ReturnsNoContent()
        {
            if (_createdFactionId == 0)
            {
                await CreateFaction_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/faction/{_createdFactionId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private class FactionResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
