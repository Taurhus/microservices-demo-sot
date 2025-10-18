using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    public class PlayerServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdPlayerId;

        public PlayerServiceIntegrationTests()
        {
            // Assumes PlayerService is running on localhost:5001 (adjust as needed)
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5001/") };
        }

        [Fact]
        public async Task GetPlayers_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/player");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetPlayerById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/player/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreatePlayer_ReturnsCreated()
        {
            var newPlayer = new { Name = "Test Player" };
            var response = await _client.PostAsJsonAsync("api/player", newPlayer);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<PlayerResponse>();
            Assert.NotNull(created);
            _createdPlayerId = created.Id;
        }

        [Fact]
        public async Task UpdatePlayer_ReturnsNoContent()
        {
            // Create a new player
            var newPlayer = new { Name = "Player To Update" };
            var createResponse = await _client.PostAsJsonAsync("api/player", newPlayer);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<PlayerResponse>();
            Assert.NotNull(created);
            await Task.Delay(100); // Ensure commit
            var updatePlayer = new { Id = created.Id, Name = "Updated Player" };
            var response = await _client.PutAsJsonAsync($"api/player/{created.Id}", updatePlayer);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeletePlayer_ReturnsNoContent()
        {
            if (_createdPlayerId == 0)
            {
                await CreatePlayer_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/player/{_createdPlayerId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private class PlayerResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
