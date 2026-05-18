"""Rowland Documents API Python SDK."""

from .client import DocumentsApiClient
from .exceptions import RowlandAuthenticationError, RowlandError, RowlandHTTPError
from .models import (
    Document,
    DocumentExtractionResponse,
    ProcessingStatus,
    DocumentType,
    PaginatedResponse,
)

__version__ = "1.0.0"
__all__ = [
    "DocumentsApiClient",
    "Document",
    "DocumentExtractionResponse",
    "ProcessingStatus",
    "DocumentType",
    "PaginatedResponse",
    "RowlandError",
    "RowlandHTTPError",
    "RowlandAuthenticationError",
]
