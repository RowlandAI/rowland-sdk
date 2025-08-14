using DocumentsApi.Sdk.Clients;

Console.WriteLine("Documents API SDK Example");
Console.WriteLine("========================");

var apiBaseUrl = "https://documents.rowland.ai";
var apiKey = "your-api-key-here"; // Replace with your actual API key

var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri(apiBaseUrl);
var client = new DocumentsApiClient(httpClient, apiKey);

try
{
    // 1. Test health endpoint first
    Console.WriteLine("1. Testing health endpoint...");
    var health = await client.GetHealthAsync();
    Console.WriteLine($"Health check: {health["status"]}");
    Console.WriteLine();

    // 2. Upload a document (optional - uncomment to test)
    // Console.WriteLine("2. Uploading a document...");
    // using var fileStream = File.OpenRead("/path/to/your/test-document.pdf"); // Replace with your actual file path
    // var uploadedDoc = await client.UploadDocumentAsync(
    //     fileStream,
    //     "file.pdf" // Replace with your actual file name
    // );
    // Console.WriteLine($"Uploaded document: {uploadedDoc.Id} - {uploadedDoc.Name}");
    // Console.WriteLine($"Status: {uploadedDoc.Status}, Type: {uploadedDoc.DocumentType}");
    // Console.WriteLine();


    // 3. List current documents
    Console.WriteLine("3. Fetching documents...");
    var documents = await client.GetDocumentsAsync(offset: 0, limit: 10);
    Console.WriteLine($"Found {documents.Total} total documents");
    Console.WriteLine($"Showing {documents.Items.Count} documents (offset: {documents.Offset}, limit: {documents.Limit})");
    Console.WriteLine($"Has next page: {documents.HasNext}");
    Console.WriteLine($"Has previous page: {documents.HasPrevious}");
    Console.WriteLine();

    if (documents.Items.Any())
    {
        Console.WriteLine("Documents:");
        Console.WriteLine("----------");

        var firstDocumentId = string.Empty;

        foreach (var doc in documents.Items)
        {
            Console.WriteLine($"• {doc.Name} (ID: {doc.Id})");
            Console.WriteLine($"  Type: {doc.DocumentType}");
            Console.WriteLine($"  Status: {doc.Status}");
            Console.WriteLine($"  File Size: {doc.FileSize?.ToString() ?? "N/A"} bytes");
            Console.WriteLine($"  Created: {doc.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
            Console.WriteLine();

            // Remember first document ID for later examples
            if (string.IsNullOrEmpty(firstDocumentId))
                firstDocumentId = doc.Id;
        }

        // 4. Get a specific document
        if (!string.IsNullOrEmpty(firstDocumentId))
        {
            Console.WriteLine($"4. Getting specific document: {firstDocumentId}");
            var specificDoc = await client.GetDocumentAsync(firstDocumentId);
            Console.WriteLine($"Retrieved: {specificDoc.Name}");
            Console.WriteLine($"Summary: {specificDoc.Summary ?? "No summary available"}");
            Console.WriteLine();

            // 5. Get document extractions
            Console.WriteLine($"5. Getting extractions for document: {firstDocumentId}");
            try
            {
                var extractions = await client.GetDocumentExtractionsAsync(firstDocumentId);
                Console.WriteLine($"Extractions found for: {extractions.DocumentName}");
                Console.WriteLine($"Extraction ID: {extractions.ExtractionId}");
                Console.WriteLine($"Total objects found: {extractions.TotalObjectsFound ?? 0}");
                Console.WriteLine($"Review notes: {extractions.ReviewNotes ?? "None"}");

                if (extractions.ConsolidatedObjects?.Any() == true)
                {
                    Console.WriteLine($"Consolidated objects: {extractions.ConsolidatedObjects.Count} items");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get extractions: {ex.Message}");
            }
            Console.WriteLine();

            // 6. Delete document (commented out for safety)
            // Console.WriteLine($"6. Deleting document: {firstDocumentId}");
            // var deleteResult = await client.DeleteDocumentAsync(firstDocumentId);
            // Console.WriteLine($"Delete result: {string.Join(", ", deleteResult.Select(kv => $"{kv.Key}: {kv.Value}"))}");
            // Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("No documents found. Try uploading one first!");
    }

    Console.WriteLine("All API methods tested successfully!");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
    Console.WriteLine("Check your API URL, API key, and network connection.");
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Unauthorized: Check your API key.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
finally
{
    httpClient.Dispose();
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
