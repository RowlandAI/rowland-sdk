# Rowland SDK

<p align="center">
  <img src="assets/images/rowland_logo.png" alt="Rowland Logo" width="400">
</p>

Enterprise client libraries and developer tools for integrating with Rowland's AI-first platform APIs.

![Version](https://img.shields.io/badge/version-0.0.1-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Python](https://img.shields.io/badge/python-3.8+-blue)
![.NET](https://img.shields.io/badge/.net-9.0+-purple)

## Overview

The Rowland SDK provides production-ready client libraries, tools, and utilities to accelerate integration with Rowland's APIs. Built for enterprise developers who need to integrate AI-powered document processing and workflow automation into their applications.

These SDKs handle authentication, request/response serialization, error handling, and provide strongly-typed interfaces for all Rowland API endpoints. Whether you're building internal tools, customer-facing applications, or system integrations, these libraries provide the foundation for robust Rowland API integration.

## Available SDKs

| Language | Package | Documentation | Installation |
|----------|---------|---------------|--------------|
| **Python** | [`rowland`](https://pypi.org/project/rowland/) | [Docs](./python/) | `pip install rowland` |
| **C# (.NET)** | [`Rowland.Sdk`](https://www.nuget.org/packages/Rowland.Sdk/) | [Docs](./dotnet/) | `dotnet add package Rowland.Sdk` |

## Quick Integration Examples

### Python
```python
from rowland import DocumentsApiClient

# Initialize client with your API key
with DocumentsApiClient(api_key="your-api-key") as client:
    # Upload document for processing
    with open("contract.pdf", "rb") as f:
        document = client.upload_document(f, "contract.pdf")
    
    # Monitor processing status
    print(f"Processing document: {document.id}")
    
    # Retrieve structured data when complete
    if document.status == "success":
        extractions = client.get_document_extractions(document.id)
        # Use extracted data in your application logic
        process_contract_data(extractions.consolidated_objects)
```

### C# (.NET)
```csharp
using DocumentsApi.Sdk.Clients;

var client = new DocumentsApiClient(httpClient, "your-api-key");

// Upload document for processing
using var fileStream = File.OpenRead("contract.pdf");
var document = await client.UploadDocumentAsync(fileStream, "contract.pdf");

// Monitor processing status
Console.WriteLine($"Processing document: {document.Id}");

// Retrieve structured data when complete
if (document.Status == DocumentStatus.Success)
{
    var extractions = await client.GetDocumentExtractionsAsync(document.Id);
    // Integrate extracted data into your business logic
    await ProcessContractDataAsync(extractions.ConsolidatedObjects);
}
```

## Rowland API Capabilities

These SDKs provide access to Rowland's enterprise APIs:

### Documents API
Programmatically process energy industry documents and extract structured data.

- **Document Upload & Processing** - Submit documents for AI analysis
- **Status Monitoring** - Track processing progress in real-time
- **Data Extraction** - Retrieve structured results in JSON format
- **Document Management** - List, retrieve, and delete processed documents
- **Webhook Integration** - Receive notifications when processing completes

### Workflows API
Execute complex multi-step business processes programmatically.

- **Workflow Execution** - Run pre-built or custom automation workflows
- **Progress Monitoring** - Track multi-step workflow execution
- **Custom Logic** - Deploy conditional branching and business rules
- **Template Library** - Access industry-specific workflow templates

## SDK Features

### Production-Ready
- Comprehensive error handling and retry logic
- Connection pooling and request optimization
- Configurable timeouts and rate limiting
- Detailed logging and debugging support

### Enterprise Security
- Secure API key authentication
- TLS encryption for all communications
- Request signing and validation
- Audit logging capabilities

### Developer Experience
- Strongly-typed models and responses
- Comprehensive IntelliSense/autocomplete support
- Detailed error messages and status codes
- Extensive code examples and documentation

### Integration Support
- Webhook handler utilities
- Async/await support (where applicable)
- Pagination helpers for large datasets
- Bulk operation support

## Authentication & Configuration

Obtain your API credentials from the [Rowland Developer Portal](https://app.rowland.ai) and configure authentication:

```bash
# Set as environment variable (recommended)
export ROWLAND_API_KEY="your-api-key-here"
export ROWLAND_BASE_URL="https://api.rowland.ai"  # Optional: defaults to production
```

```python
# Python configuration
client = DocumentsApiClient(
    api_key="your-api-key",
    base_url="https://api.rowland.ai",  # Optional
    timeout=30,  # Optional: request timeout in seconds
    max_retries=3  # Optional: retry failed requests
)
```

```csharp
// C# configuration
var httpClient = new HttpClient()
{
    BaseAddress = new Uri("https://api.rowland.ai"),
    Timeout = TimeSpan.FromSeconds(30)
};
var client = new DocumentsApiClient(httpClient, "your-api-key");
```

## Integration Patterns

### Synchronous Processing
For real-time applications requiring immediate results:

```python
# Upload and wait for processing
document = client.upload_document(file_stream, filename)
while document.status == "processing":
    time.sleep(2)
    document = client.get_document(document.id)

# Process results immediately
if document.status == "success":
    handle_results(client.get_document_extractions(document.id))
```

### Asynchronous Processing with Webhooks
For scalable applications processing large volumes:

```python
# Upload with webhook notification
document = client.upload_document(
    file_stream, 
    filename,
    webhook_url="https://yourapp.com/webhooks/rowland"
)

# Your webhook handler receives notification when complete
# Then retrieve results: client.get_document_extractions(document_id)
```

### Batch Processing
For high-volume document processing:

```python
# Upload multiple documents
documents = []
for file_path in document_files:
    with open(file_path, "rb") as f:
        doc = client.upload_document(f, os.path.basename(file_path))
        documents.append(doc)

# Monitor batch completion
completed = client.wait_for_completion([doc.id for doc in documents])
```

## Developer Resources

- **[API Documentation](https://docs.rowland.ai)** - Complete API reference

## Enterprise Support

- **Developer Portal**: [app.rowland.ai](https://app.rowland.ai)
- **API Status & Monitoring**: [status.rowland.ai](https://status.rowland.ai)
- **Integration Issues**: [GitHub Issues](https://github.com/RowlandAI/rowland-sdk/issues)

## Contributing

We welcome contributions from enterprise developers!

## License

Licensed under the MIT License - see [LICENSE](LICENSE) for details.

---

**Enterprise-grade SDKs for seamless Rowland API integration.**  
*Built by developers, for developers.*
