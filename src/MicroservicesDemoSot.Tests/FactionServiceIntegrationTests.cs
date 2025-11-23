using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class FactionServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdFactionId;

        public FactionServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetFactions_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/factions");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetFactionById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/factions/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateFaction_ReturnsCreated()
        {
            var newFaction = new { Name = "Test Faction", Description = "A test faction", Type = "Trading Company", Headquarters = "Plunder Outpost", MaxReputationLevel = 75 };
            var response = await _client.PostAsJsonAsync("api/factions", newFaction);
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
            var newFaction = new { Name = "Faction To Update", Description = "A faction to update", Type = "Trading Company", Headquarters = "Sanctuary Outpost", MaxReputationLevel = 75 };
            var createResponse = await _client.PostAsJsonAsync("api/factions", newFaction);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<FactionResponse>();
            Assert.NotNull(created);
            await Task.Delay(100); // Ensure commit
            var updateFaction = new { Id = created.Id, Name = "Updated Faction", Description = "Updated description", Type = "Trading Company", Headquarters = "Galleon's Grave Outpost", MaxReputationLevel = 75, IsActive = true, IntroducedDate = (DateTime?)null };
            var response = await _client.PutAsJsonAsync($"api/factions/{created.Id}", updateFaction);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteFaction_ReturnsNoContent()
        {
            if (_createdFactionId == 0)
            {
                await CreateFaction_ReturnsCreated();
            }
            var response = await _client.DeleteAsync($"api/factions/{_createdFactionId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetFactionById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/factions/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateFaction_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidFaction = new { Description = "A test faction" }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/factions", invalidFaction);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateFaction_WithNonExistentId_ReturnsNotFound()
        {
            var updateFaction = new { Id = 99999, Name = "Updated Faction", Description = "Updated", Type = "Trading Company", Headquarters = "Galleon's Grave Outpost", MaxReputationLevel = 75, IsActive = true, IntroducedDate = (DateTime?)null };
            var response = await _client.PutAsJsonAsync("api/factions/99999", updateFaction);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateFaction_WithIdMismatch_ReturnsBadRequest()
        {
            var updateFaction = new { Id = 1, Name = "Updated Faction", Description = "Updated", Type = "Trading Company", Headquarters = "Galleon's Grave Outpost", MaxReputationLevel = 75, IsActive = true, IntroducedDate = (DateTime?)null };
            var response = await _client.PutAsJsonAsync("api/factions/2", updateFaction); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteFaction_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/factions/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class FactionResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
