using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MicroservicesDemoSot.Tests
{
    [Xunit.Collection("IntegrationTestCollection")]
    public class EmissaryServiceIntegrationTests
    {
        private readonly HttpClient _client;
        private static int _createdEmissaryId;

        public EmissaryServiceIntegrationTests()
        {
            // Use API Gateway instead of calling service directly
            _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000/") };
        }

        [Fact]
        public async Task GetEmissaries_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/emissaries");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task GetEmissaryById_ReturnsSuccess()
        {
            var response = await _client.GetAsync("api/emissaries/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }

        [Fact]
        public async Task CreateEmissary_ReturnsCreatedAndStoresId()
        {
            var newEmissary = new { PlayerId = 1, FactionName = "Gold Hoarders", Level = 1, IsActive = true, Value = 0, Notes = "Test" };
            var response = await _client.PostAsJsonAsync("api/emissaries", newEmissary);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<EmissaryDto>();
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            _createdEmissaryId = created.Id;
        }

        [Fact]
        public async Task UpdateEmissary_ReturnsNoContent()
        {
            var updateEmissary = new { Id = 1, PlayerId = 1, FactionName = "Gold Hoarders", Level = 2, IsActive = true, Value = 0, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/emissaries/1", updateEmissary);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEmissary_DeletesCreatedEmissary()
        {
            if (_createdEmissaryId == 0)
            {
                await CreateEmissary_ReturnsCreatedAndStoresId();
            }
            var delResponse = await _client.DeleteAsync($"api/emissaries/{_createdEmissaryId}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, delResponse.StatusCode);
        }

        // Negative Tests
        [Fact]
        public async Task GetEmissaryById_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/emissaries/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateEmissary_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidEmissary = new { PlayerId = 1 }; // Missing required FactionName field
            var response = await _client.PostAsJsonAsync("api/emissaries", invalidEmissary);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmissary_WithNonExistentId_ReturnsNotFound()
        {
            var updateEmissary = new { Id = 99999, PlayerId = 1, FactionName = "Gold Hoarders", Level = 2, IsActive = true, Value = 0, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/emissaries/99999", updateEmissary);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmissary_WithIdMismatch_ReturnsBadRequest()
        {
            var updateEmissary = new { Id = 1, PlayerId = 1, FactionName = "Gold Hoarders", Level = 2, IsActive = true, Value = 0, Notes = "Updated" };
            var response = await _client.PutAsJsonAsync("api/emissaries/2", updateEmissary); // ID mismatch
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEmissary_WithNonExistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/emissaries/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        private class EmissaryDto
        {
            public int Id { get; set; }
            public int PlayerId { get; set; }
        }
    }
}

