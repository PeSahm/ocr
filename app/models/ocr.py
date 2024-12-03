"""
OCR API data models
"""
from pydantic import BaseModel, Field

class ImageRequest(BaseModel):
    base64_string: str = Field(
        ...,
        description="Base64 encoded image string",
        example="data:image/jpeg;base64,/9j/4AAQSkZJRg..."
    )

class ImageResponse(BaseModel):
    text: str = Field(
        ...,
        description="Extracted text from the image",
        example="Hello World"
    )

class ErrorResponse(BaseModel):
    error: str = Field(
        ...,
        description="Error message",
        example="Invalid base64 string"
    )