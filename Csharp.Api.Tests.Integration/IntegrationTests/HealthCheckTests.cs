using Xunit;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Csharp.Api.Tests.Integration
{
    public class HealthCheckTests : IClassFixture<FleetApiFactory>
    {
        private readonly HttpClient _client;
        private readonly FleetApiFactory _factory;

        public HealthCheckTests(FleetApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(); // HttpClient que conversa com o servidor na memória
        }

        [Fact]
        public async Task Get_HealthCheck_ShouldReturnOk()
        {
            // Arrange
            var url = "/health";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // status code = 2xx
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}