"""
OCR endpoint router
"""
from fastapi import APIRouter, Depends
from ..models.ocr import ImageRequest, ImageResponse, ErrorResponse
from ..services.image_processor import ImageProcessor
from ..utils.exceptions import OCRException

router = APIRouter()

async def get_processor() -> ImageProcessor:
    """Dependency to get ImageProcessor instance."""
    return ImageProcessor()

@router.post("/ocr",
    response_model=ImageResponse,
    responses={
        400: {"model": ErrorResponse},
        500: {"model": ErrorResponse}
    },
    description="Extract text from a base64 encoded image using OCR"
)
async def ocr_endpoint(
    request: ImageRequest,
    processor: ImageProcessor = Depends(get_processor)
):
    """
    Extract text from a base64 encoded image.
    
    Args:
        request: Request containing base64 encoded image
        processor: ImageProcessor instance
        
    Returns:
        ImageResponse: Response containing extracted text
        
    Raises:
        OCRException: If image processing fails
    """
    result = processor.process_image(request.base64_string)
    if "error" in result:
        raise OCRException(result["error"])
    return ImageResponse(text=result["text"])