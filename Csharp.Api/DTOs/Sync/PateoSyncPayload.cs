using System;
using System.Text.Json.Serialization;

namespace Csharp.Api.DTOs.Sync
{
    /// <summary>Pátio replicado do Java (para sync de dados).</summary>
    public class PateoSyncPayload
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("plantaBaixaUrl")] public string? PlantaBaixaUrl { get; set; }
        [JsonPropertyName("plantaLargura")] public int? PlantaLargura { get; set; }
        [JsonPropertyName("plantaAltura")] public int? PlantaAltura { get; set; }
        [JsonPropertyName("gerenciadoPorId")] public Guid GerenciadoPorId { get; set; }
    }
}
