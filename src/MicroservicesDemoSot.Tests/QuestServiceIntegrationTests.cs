using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class QuestServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdQuestId;

        public QuestServiceIntegrationTests()
        {
            // Assumes QuestService is running on localhost:5003 (adjust as needed)
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5003/") };
        }

        [Fact]
        public async Task GetQuests_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/quest");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetQuestById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/quest/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateQuest_ReturnsCreated()
        {
            var newQuest = new { Name = "Test Quest" };
            var response = await _client.PostAsJsonAsync("api/quest", newQuest);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<QuestResponse>();
            Assert.NotNull(created);
            _createdQuestId = created.Id;
        }

        [Fact]
        public async Task UpdateQuest_ReturnsNoContent()
        {
            if (_createdQuestId == 0)
            {
                await CreateQuest_ReturnsCreated();
            }
            var updateQuest = new { Id = _createdQuestId, Name = "Updated Quest" };
            var response = await _client.PutAsJsonAsync($"api/quest/{_createdQuestId}", updateQuest);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteQuest_ReturnsNoContent()
        {
            if (_createdQuestId == 0)
            {
                await CreateQuest_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/quest/{_createdQuestId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private class QuestResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
