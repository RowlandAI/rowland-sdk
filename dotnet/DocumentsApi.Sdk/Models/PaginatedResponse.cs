using System.Text.Json.Serialization;

namespace DocumentsApi.Sdk.Models
{
    public class PaginatedResponse<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("has_next")]
        public bool HasNext { get; set; }

        [JsonPropertyName("has_previous")]
        public bool HasPrevious { get; set; }
    }
}
