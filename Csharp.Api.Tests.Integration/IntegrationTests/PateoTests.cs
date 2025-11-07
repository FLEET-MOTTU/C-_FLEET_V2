using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Csharp.Api.Tests.Integration
{
    public class PateoTests : IClassFixture<FleetApiFactory>
    {
        private readonly HttpClient _client;

        public PateoTests(FleetApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_MeuPateo_ShouldReturnOkAndContainZones()
        {
            // Arrange
            var url = "/api/v2/pateo/meu-pateo";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var txt = await response.Content.ReadAsStringAsync();
                Assert.True(false, $"GetMeuPateo failed: {(int)response.StatusCode} - {txt}");
            }
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal("Patio teste C#", body.GetProperty("nome").GetString());
        }
    }
}
