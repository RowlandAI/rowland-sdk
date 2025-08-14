using System.Text.Json.Serialization;

namespace DocumentsApi.Sdk.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentStatus
    {
        [JsonPropertyName("queued")]
        Queued,

        [JsonPropertyName("processing")]
        Processing,

        [JsonPropertyName("success")]
        Success,

        [JsonPropertyName("failed")]
        Failed
    }
}
