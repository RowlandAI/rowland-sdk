using System.Text.Json.Serialization;

namespace DocumentsApi.Sdk.Models
{
    public class DocumentExtractionResponse
    {
        [JsonPropertyName("document_id")]
        public string DocumentId { get; set; } = string.Empty;

        [JsonPropertyName("document_name")]
        public string DocumentName { get; set; } = string.Empty;

        [JsonPropertyName("extraction_id")]
        public string ExtractionId { get; set; } = string.Empty;

        [JsonPropertyName("consolidated_objects")]
        public List<Dictionary<string, object>>? ConsolidatedObjects { get; set; }

        [JsonPropertyName("total_objects_found")]
        public int? TotalObjectsFound { get; set; }

        [JsonPropertyName("review_notes")]
        public string? ReviewNotes { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
