using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentsApi.Sdk.Models;

namespace DocumentsApi.Sdk.Clients
{
    public class DocumentsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        private static string GetMimeTypeFromFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }


        public DocumentsApiClient(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DictionaryKeyPolicy = null,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
            };
        }

        public async Task<Document> UploadDocumentAsync(
            Stream fileStream,
            string fileName,
            string? userId = null,
            string? organizationId = null,
            string? folderId = null,
            string? webhookUrl = null,
            string? webhookSecret = null,
            CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);

            var mimeType = GetMimeTypeFromFileName(fileName);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

            content.Add(fileContent, "file", fileName);

            if (userId != null) content.Add(new StringContent(userId), "user_id");
            if (organizationId != null) content.Add(new StringContent(organizationId), "organization_id");
            if (folderId != null) content.Add(new StringContent(folderId), "folder_id");
            if (webhookUrl != null) content.Add(new StringContent(webhookUrl), "webhook_url");
            if (webhookSecret != null) content.Add(new StringContent(webhookSecret), "webhook_secret");

            var response = await _httpClient.PostAsync("/v0/documents", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Upload failed with status {response.StatusCode}: {errorContent}");
            }
            // response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Document>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize upload response");
        }

        public async Task<PaginatedResponse<Document>> GetDocumentsAsync(
            int offset = 0,
            int limit = 50,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
                $"/v0/documents?offset={offset}&limit={limit}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<PaginatedResponse<Document>>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task<Document> GetDocumentAsync(
            string documentId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
                $"/v0/documents/{documentId}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Document>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Document not found");
        }
        public async Task<Dictionary<string, string>> GetHealthAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize health response");
        }

        public async Task<Document> UpdateDocumentContentAsync(
            string documentUuid,
            string pageUpdates,
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(pageUpdates), "page_updates");
            if (userId != null) content.Add(new StringContent(userId), "user_id");

            var response = await _httpClient.PostAsync($"/v0/documents/{documentUuid}/update", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Update failed with status {response.StatusCode}: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Document>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize update response");
        }

        public async Task<Dictionary<string, string>> DeleteDocumentAsync(
            string documentId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync($"/v0/documents/{documentId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize delete response");
        }

        public async Task<DocumentExtractionResponse> GetDocumentExtractionsAsync(
            string documentId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
                $"/v0/documents/{documentId}/extractions",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<DocumentExtractionResponse>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize extractions response");
        }
    }
}
