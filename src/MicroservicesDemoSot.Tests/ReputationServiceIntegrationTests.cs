using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class ReputationServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdReputationId;

        public ReputationServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetReputations_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/reputations");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetReputationById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/reputations/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateReputation_ReturnsCreatedAndStoresId()
        {
            // Use a unique FactionName with timestamp to avoid unique constraint violation
            // Seed data has: PlayerId=1 (Gold Hoarders, Order of Souls), PlayerId=2 (Merchant Alliance), PlayerId=3 (none)
            var uniqueFactionName = $"Test Faction {DateTime.UtcNow.Ticks}";
            var newReputation = new { PlayerId = 3, FactionName = uniqueFactionName, Level = 1, TotalReputation = 0L, Notes = "Test" };
            var response = await _client.PostAsJsonAsync("api/reputations", newReputation);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ReputationDto>();
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            _createdReputationId = created.Id;
        }

        [Fact]
        public async Task UpdateReputation_ReturnsNoContent()
        {
            // First create a reputation to update (use PlayerId = 3 with a unique FactionName to avoid unique constraint)
            var uniqueFactionName = $"Update Test Faction {DateTime.UtcNow.Ticks}";
            var newReputation = new { PlayerId = 3, FactionName = uniqueFactionName, Level = 1, TotalReputation = 0L, Notes = "Initial" };
            var createResponse = await _client.PostAsJsonAsync("api/reputations", newReputation);
            if (createResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var created = await createResponse.Content.ReadFromJsonAsync<ReputationDto>();
                Assert.NotNull(created);
                await Task.Delay(100);
                var updateReputation = new { Id = created.Id, PlayerId = 3, FactionName = uniqueFactionName, Level = 2, TotalReputation = 100L, LastUpdated = DateTime.UtcNow, Notes = "Updated" };
                var response = await _client.PutAsJsonAsync($"api/reputations/{created.Id}", updateReputation);
                Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task DeleteReputation_DeletesCreatedReputation()
        {
            if (_createdReputationId == 0)
            {
                await CreateReputation_ReturnsCreatedAndStoresId();
            }
            var delResponse = await _client.DeleteAsync($"api/reputations/{_createdReputationId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, delResponse.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetReputationById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/reputations/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateReputation_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidReputation = new { PlayerId = 1 }; // Missing required FactionName field
            var response = await _client.PostAsJsonAsync("api/reputations", invalidReputation);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateReputation_WithNonExistentId_ReturnsNotFound()
        {
            var uniqueFactionName = $"Update Test Faction {DateTime.UtcNow.Ticks}";
            var updateReputation = new { Id = 99999, PlayerId = 3, FactionName = uniqueFactionName, Level = 2, TotalReputation = 100L, LastUpdated = DateTime.UtcNow, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/reputations/99999", updateReputation);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateReputation_WithIdMismatch_ReturnsBadRequest()
        {
            var uniqueFactionName = $"Update Test Faction {DateTime.UtcNow.Ticks}";
            var updateReputation = new { Id = 1, PlayerId = 1, FactionName = uniqueFactionName, Level = 2, TotalReputation = 100L, LastUpdated = DateTime.UtcNow, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/reputations/2", updateReputation); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteReputation_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/reputations/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class ReputationDto
        {
            public int Id { get; set; }
            public int PlayerId { get; set; }
        }
    }
}

