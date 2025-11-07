using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Csharp.Api.Tests.Integration
{
    public class MotosIntegrationTests : IClassFixture<FleetApiFactory>
    {
        private readonly HttpClient _client;

        public MotosIntegrationTests(FleetApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UpsertPorPlaca_CreateThenGetAndPredict_ShouldSucceed()
        {
            // Arrange
            var upsertUrl = "/api/v2/motos/upsert-por-placa";
            var placa = "AAA1234";

            var dto = new
            {
                placa = placa,
                modelo = "ModeloSport100",
                statusMoto = "PendenteColeta",
                codigoUnicoTag = "TAG-INT-01",
                zonaId = (string?)null
            };

            // Act - Upsert
            var upsertResponse = await _client.PostAsJsonAsync(upsertUrl, dto);
            if (!upsertResponse.IsSuccessStatusCode)
            {
                var txt = await upsertResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Upsert failed: {(int)upsertResponse.StatusCode} - {txt}");
            }
            Assert.Equal(HttpStatusCode.OK, upsertResponse.StatusCode);

            var created = await upsertResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            string? id = created.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(id)) throw new Xunit.Sdk.XunitException("Upsert response did not contain an 'id'.");

            // Act - Get by placa
            var getByPlacaUrl = $"/api/v2/motos/por-placa/{placa}";
            var getResp = await _client.GetAsync(getByPlacaUrl);
            if (!getResp.IsSuccessStatusCode)
            {
                var txt = await getResp.Content.ReadAsStringAsync();
                throw new Xunit.Sdk.XunitException($"Get by placa failed: {(int)getResp.StatusCode} - {txt}");
            }
            var moto = await getResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal(placa, moto.GetProperty("placa").GetString());

            // Act - Predict
            var predictUrl = $"/api/v2/motos/{id}/prever-vistoria";
            var predResp = await _client.GetAsync(predictUrl);
            if (!predResp.IsSuccessStatusCode)
            {
                var txt = await predResp.Content.ReadAsStringAsync();
                throw new Xunit.Sdk.XunitException($"Predict failed: {(int)predResp.StatusCode} - {txt}");
            }
            var pred = await predResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.True(pred.TryGetProperty("precisaReparoComplexo", out var prop), "Response missing 'precisaReparoComplexo' property");
            Assert.True(prop.ValueKind == System.Text.Json.JsonValueKind.True || prop.ValueKind == System.Text.Json.JsonValueKind.False || prop.ValueKind == System.Text.Json.JsonValueKind.Null);
        }
    }
}
