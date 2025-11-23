using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class PlayerServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdPlayerId;

        public PlayerServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetPlayers_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/players");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetPlayerById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/players/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreatePlayer_ReturnsCreated()
        {
            var newPlayer = new { Name = "Test Player", Gamertag = "TestGamer123" };
            var response = await _client.PostAsJsonAsync("api/players", newPlayer);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<PlayerResponse>();
            Assert.NotNull(created);
            _createdPlayerId = created.Id;
        }

        [Fact]
        public async Task UpdatePlayer_ReturnsNoContent()
        {
            // Create a new player
            var newPlayer = new { Name = "Player To Update", Gamertag = "UpdateGamer123" };
            var createResponse = await _client.PostAsJsonAsync("api/players", newPlayer);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<PlayerResponse>();
            Assert.NotNull(created);
            await Task.Delay(100); // Ensure commit
            var updatePlayer = new { Id = created.Id, Name = "Updated Player", Gamertag = "UpdatedGamer123", Gold = 1000L, Renown = 50, IsPirateLegend = false, LastLoginDate = (DateTime?)null, Platform = "Steam" };
            var response = await _client.PutAsJsonAsync($"api/players/{created.Id}", updatePlayer);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeletePlayer_ReturnsNoContent()
        {
            if (_createdPlayerId == 0)
            {
                await CreatePlayer_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/players/{_createdPlayerId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetPlayerById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/players/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreatePlayer_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidPlayer = new { }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/players", invalidPlayer);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePlayer_WithNonExistentId_ReturnsNotFound()
        {
            var updatePlayer = new { Id = 99999, Name = "Updated Player", Gamertag = "UpdatedGT", Gold = 1000L, Renown = 50, IsPirateLegend = false, Platform = "PC" };
            var response = await _client.PutAsJsonAsync("api/players/99999", updatePlayer);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePlayer_WithIdMismatch_ReturnsBadRequest()
        {
            var updatePlayer = new { Id = 1, Name = "Updated Player", Gamertag = "UpdatedGT", Gold = 1000L, Renown = 50, IsPirateLegend = false, Platform = "PC" };
            var response = await _client.PutAsJsonAsync("api/players/2", updatePlayer); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeletePlayer_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/players/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class PlayerResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
