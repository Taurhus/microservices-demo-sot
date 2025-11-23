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
            // Use API Gateway instead of calling service directly
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetQuests_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/quests");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetQuestById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/quests/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateQuest_ReturnsCreated()
        {
            var newQuest = new { Name = "Test Quest", Description = "A test quest", FactionName = "Gold Hoarders", Type = "Voyage" };
            var response = await _client.PostAsJsonAsync("api/quests", newQuest);
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
            var updateQuest = new { Id = _createdQuestId, Name = "Updated Quest", Description = "Updated description", FactionName = "Gold Hoarders", Type = "Voyage", RequiredReputationLevel = 5, EstimatedDurationMinutes = 45, GoldReward = (decimal?)100.0, IsActive = true };
            var response = await _client.PutAsJsonAsync($"api/quests/{_createdQuestId}", updateQuest);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteQuest_ReturnsNoContent()
        {
            if (_createdQuestId == 0)
            {
                await CreateQuest_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/quests/{_createdQuestId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetQuestById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/quests/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateQuest_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidQuest = new { Description = "A test quest" }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/quests", invalidQuest);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateQuest_WithNonExistentId_ReturnsNotFound()
        {
            var updateQuest = new { Id = 99999, Name = "Updated Quest", Description = "Updated", FactionName = "Gold Hoarders", Type = "Voyage", RequiredReputationLevel = 5, EstimatedDurationMinutes = 45, GoldReward = (decimal?)100.0, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/quests/99999", updateQuest);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateQuest_WithIdMismatch_ReturnsBadRequest()
        {
            var updateQuest = new { Id = 1, Name = "Updated Quest", Description = "Updated", FactionName = "Gold Hoarders", Type = "Voyage", RequiredReputationLevel = 5, EstimatedDurationMinutes = 45, GoldReward = (decimal?)100.0, IsActive = true };
            var response = await _client.PutAsJsonAsync("api/quests/2", updateQuest); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteQuest_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/quests/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class QuestResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
