using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class AchievementServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdAchievementId;

        public AchievementServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetAchievements_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/achievements");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetAchievementById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/achievements/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateAchievement_ReturnsCreatedAndStoresId()
        {
            var newAchievement = new { PlayerId = 1, Name = "Test Achievement", Description = "Test", Category = "Combat", Rarity = "Common", Progress = 0, RequiredProgress = 100, Notes = "Test" };
            var response = await _client.PostAsJsonAsync("api/achievements", newAchievement);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<AchievementDto>();
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            _createdAchievementId = created.Id;
        }

        [Fact]
        public async Task UpdateAchievement_ReturnsNoContent()
        {
            var updateAchievement = new { Id = 1, PlayerId = 1, Name = "Updated Achievement", Description = "Updated", Category = "Combat", Rarity = "Common", Progress = 50, RequiredProgress = 100, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/achievements/1", updateAchievement);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAchievement_DeletesCreatedAchievement()
        {
            if (_createdAchievementId == 0)
            {
                await CreateAchievement_ReturnsCreatedAndStoresId();
            }
            var delResponse = await _client.DeleteAsync($"api/achievements/{_createdAchievementId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, delResponse.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetAchievementById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/achievements/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateAchievement_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidAchievement = new { PlayerId = 1 }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/achievements", invalidAchievement);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateAchievement_WithNonExistentId_ReturnsNotFound()
        {
            var updateAchievement = new { Id = 99999, PlayerId = 1, Name = "Updated Achievement", Description = "Updated", Category = "Combat", Rarity = "Common", Progress = 50, RequiredProgress = 100, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/achievements/99999", updateAchievement);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateAchievement_WithIdMismatch_ReturnsBadRequest()
        {
            var updateAchievement = new { Id = 1, PlayerId = 1, Name = "Updated Achievement", Description = "Updated", Category = "Combat", Rarity = "Common", Progress = 50, RequiredProgress = 100, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/achievements/2", updateAchievement); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAchievement_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/achievements/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class AchievementDto
        {
            public int Id { get; set; }
            public int PlayerId { get; set; }
        }
    }
}

