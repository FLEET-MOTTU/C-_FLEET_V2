using System.Text.Json;
using System.Text.Json.Serialization;

namespace Csharp.Api.DTOs
{
    /**
     * O "envelope" genérico que o Java envia.
     * O campo 'Data' é desserializado como um JsonElement genérico
     * para que possamos decidir qual DTO específica (Funcionario, Pateo, Zona)
     * usar com base no 'EventType'.
     */
    public class InterServiceMessage
    {
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}