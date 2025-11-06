using System;
using System.Text.Json.Serialization;

namespace Csharp.Api.DTOs.Sync
{
    /// <summary>Funcionario replicado do Java (para sync de dados).</summary>
    public class FuncionarioSyncPayload
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("nome")] public string Nome { get; set; } = string.Empty;
        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
        [JsonPropertyName("telefone")] public string Telefone { get; set; } = string.Empty;
        [JsonPropertyName("cargo")] public string Cargo { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("pateoId")] public Guid PateoId { get; set; }
        [JsonPropertyName("fotoUrl")] public string? FotoUrl { get; set; }
    }
}
