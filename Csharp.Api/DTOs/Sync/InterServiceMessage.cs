using System.Text.Json;
using System.Text.Json.Serialization;

namespace Csharp.Api.DTOs.Sync
{
    /// <summary>Envelope genérico vindo do Java; roteia pelo eventType.</summary>
    public class InterServiceMessage
    {
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}
