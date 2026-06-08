using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TestProject1
{
    public class AuthApiLoginTests : IClassFixture<WebApplicationFactory<Program>>// IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public AuthApiLoginTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Login_WithValidCredentials_Returns200()
        {
            // Arrange
            var loginRequest = new
            {
                username = "MasterAdmin",
                password = "yaad"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithEmptyBody_Returns400()
        {
            // Arrange
            var loginRequest = new { };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }


    }

}
