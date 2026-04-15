import time

import pytest

from rowland import DocumentsApiClient


class TestBasicApiOperations:
    document_id: str | None = None

    def _wait_for_document_processing(
        self, client: DocumentsApiClient, doc_id: str, max_wait: int = 300
    ) -> str:
        """
        Wait for document to finish processing.
        Returns the final status ('success' or 'failed').
        """
        start_time = time.time()

        while time.time() - start_time < max_wait:
            doc = client.get_document(doc_id)
            print(f"Document {doc_id} status: {doc.status}")

            if doc.status in ["success", "failed"]:
                return doc.status

            time.sleep(5)

        doc = client.get_document(doc_id)
        pytest.fail(f"Document timed out after {max_wait}s. Final status: {doc.status}")

    def test_health_check(self, client: DocumentsApiClient) -> None:
        """Test API health endpoint."""
        health = client.get_health()
        assert health is not None

    def test_upload_pdf_document(
        self, client: DocumentsApiClient, sample_pdf_path: str
    ) -> None:
        """Test uploading a PDF Division Order Title Opinion."""
        with open(sample_pdf_path, "rb") as f:
            doc = client.upload_document(f, "Division Order Title Opinion.pdf")

        assert doc.id is not None
        assert doc.name == "Division Order Title Opinion.pdf"
        assert doc.folder_id is None
        assert doc.file_size == 108523
        assert doc.mime_type == "application/pdf"
        assert doc.owner_id is None
        assert doc.owner_organization_id is not None
        assert doc.status in {"queued", "processing"}
        assert doc.created_at is not None
        assert doc.updated_at is not None

        TestBasicApiOperations.document_id = doc.id

    @pytest.mark.slow
    def test_document_processing_completion(self, client: DocumentsApiClient) -> None:
        """Test that document processing completes successfully."""
        assert self.document_id is not None, "Upload test must run first"

        final_status = self._wait_for_document_processing(client, self.document_id)

        assert final_status == "success", (
            f"Document processing failed with status: {final_status}"
        )

    def test_get_document(self, client: DocumentsApiClient) -> None:
        """Test retrieving a document by ID."""
        assert self.document_id is not None, "Upload test must run first"

        doc = client.get_document(self.document_id)
        print(doc)

        assert doc.id == self.document_id
        assert doc.name == "Division Order Title Opinion.pdf"
        assert doc.folder_id is None
        assert doc.file_size == 108523
        assert doc.mime_type == "application/pdf"
        assert doc.owner_id is None
        assert doc.owner_organization_id is not None
        assert doc.summary is not None
        assert doc.status == "success"
        assert doc.document_type == "title_opinion"
        assert doc.created_at is not None
        assert doc.updated_at is not None

    def test_get_document_extractions(self, client: DocumentsApiClient) -> None:
        """Test retrieving document extraction results."""
        assert self.document_id is not None, "Upload test must run first"

        extraction = client.get_document_extractions(self.document_id)
        print(extraction)

        assert extraction.document_id == self.document_id
        assert extraction.document_name == "Division Order Title Opinion.pdf"
        assert extraction.extraction_id is not None
        assert extraction.consolidated_objects is not None
        assert isinstance(extraction.consolidated_objects, list)
        assert len(extraction.consolidated_objects) > 0
        assert extraction.total_objects_found is not None
        assert extraction.total_objects_found > 0
        assert extraction.created_at is not None
        assert extraction.updated_at is not None

    def test_list_documents(self, client: DocumentsApiClient) -> None:
        """Test listing documents with pagination."""
        response = client.get_documents(offset=0, limit=10)
        print(f"All documents: {response}")

        assert response is not None
        assert response.total >= 1
        assert len(response.items) <= 10

        for doc in response.items:
            assert doc.id is not None
            assert doc.name is not None
            assert doc.file_size is not None
            assert doc.mime_type is not None
            assert doc.status in {"queued", "processing", "success", "failed"}
            assert doc.created_at is not None
            assert doc.updated_at is not None

    def test_delete_document(self, client: DocumentsApiClient) -> None:
        """Test deleting a document by ID."""
        assert self.document_id is not None, "Upload test must run first"

        response = client.delete_document(self.document_id)
        assert response == {"detail": "Document deleted successfully"}

        with pytest.raises(Exception) as exc_info:
            client.get_document(self.document_id)

        error_message = str(exc_info.value).lower()
        assert "404" in error_message or "not found" in error_message
