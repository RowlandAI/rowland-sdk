using System.Text.Json.Serialization;

namespace DocumentsApi.Sdk.Models
{
    public class Document
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("folder_id")]
        public string? FolderId { get; set; }

        [JsonPropertyName("s3_key")]
        public string? S3Key { get; set; }

        [JsonPropertyName("file_size")]
        public long? FileSize { get; set; }

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        [JsonPropertyName("owner_id")]
        public string? OwnerId { get; set; }

        [JsonPropertyName("owner_organization_id")]
        public string? OwnerOrganizationId { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("status")]
        public DocumentStatus Status { get; set; }

        [JsonPropertyName("document_type")]
        public DocumentType DocumentType { get; set; } = DocumentType.Other;

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
