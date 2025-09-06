from collections.abc import Generator
import os
from pathlib import Path

import pytest

from rowland import DocumentsApiClient


@pytest.fixture(scope="session")
def api_key() -> str:
    """Get API key from environment variable."""
    key = os.getenv("ROWLAND_API_KEY")
    if not key:
        pytest.skip("ROWLAND_API_KEY environment variable not set")
    return key


@pytest.fixture(scope="session")
def client(api_key: str) -> Generator[DocumentsApiClient, None, None]:
    """Create a client instance for integration tests."""
    with DocumentsApiClient(api_key=api_key) as client:
        yield client


@pytest.fixture
def sample_pdf_path() -> str:
    """Path to sample PDF for testing."""
    return str(
        Path(__file__).parent.parent / "fixtures" / "Division Order Title Opinion.pdf"
    )
