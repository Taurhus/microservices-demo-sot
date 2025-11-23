using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class CrewServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdCrewId;

        public CrewServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetCrews_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/crews");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetCrewById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/crews/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateCrew_ReturnsCreatedAndStoresId()
        {
            var newCrew = new { Name = "Test Crew", ShipId = 1, MaxMembers = 4, CurrentMembers = 1, Status = "Active", Notes = "Test" };
            var response = await _client.PostAsJsonAsync("api/crews", newCrew);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<CrewDto>();
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            _createdCrewId = created.Id;
        }

        [Fact]
        public async Task UpdateCrew_ReturnsNoContent()
        {
            var updateCrew = new { Id = 1, Name = "Updated Crew", ShipId = 1, MaxMembers = 4, CurrentMembers = 2, Status = "Active", Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/crews/1", updateCrew);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCrew_DeletesCreatedCrew()
        {
            if (_createdCrewId == 0)
            {
                await CreateCrew_ReturnsCreatedAndStoresId();
            }
            var delResponse = await _client.DeleteAsync($"api/crews/{_createdCrewId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, delResponse.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetCrewById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/crews/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateCrew_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidCrew = new { ShipId = 1 }; // Missing required Name field
            var response = await _client.PostAsJsonAsync("api/crews", invalidCrew);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCrew_WithNonExistentId_ReturnsNotFound()
        {
            var updateCrew = new { Id = 99999, Name = "Updated Crew", ShipId = 1, MaxMembers = 4, CurrentMembers = 2, Status = "Active", Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/crews/99999", updateCrew);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCrew_WithIdMismatch_ReturnsBadRequest()
        {
            var updateCrew = new { Id = 1, Name = "Updated Crew", ShipId = 1, MaxMembers = 4, CurrentMembers = 2, Status = "Active", Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/crews/2", updateCrew); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCrew_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/crews/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class CrewDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}

