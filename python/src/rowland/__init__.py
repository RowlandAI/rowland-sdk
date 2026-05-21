"""Rowland Documents API Python SDK."""

from importlib.metadata import PackageNotFoundError
from importlib.metadata import version as _pkg_version

from .client import DocumentsApiClient
from .exceptions import RowlandAuthenticationError, RowlandError, RowlandHTTPError
from .models import (
    Document,
    DocumentExtractionResponse,
    DocumentType,
    PaginatedResponse,
    ProcessingStatus,
)

try:
    __version__ = _pkg_version("rowland-sdk")
except PackageNotFoundError:
    __version__ = "0.0.0+unknown"

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
