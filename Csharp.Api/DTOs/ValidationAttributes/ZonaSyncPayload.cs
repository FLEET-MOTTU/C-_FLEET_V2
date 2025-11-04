using System.Text.Json.Serialization;
using System;

namespace Csharp.Api.DTOs.ValidationAttributes
{
    public class ZonaSyncPayload
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;
        
        [JsonPropertyName("pateoId")]
        public Guid PateoId { get; set; }
        
        [JsonPropertyName("criadoPorId")]
        public Guid CriadoPorId { get; set; }
        
        [JsonPropertyName("coordenadasWKT")]
        public string CoordenadasWKT { get; set; } = string.Empty;
    }
}