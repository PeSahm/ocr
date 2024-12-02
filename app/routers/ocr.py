from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from ..services.ocr_service import process_image

router = APIRouter()

class ImageRequest(BaseModel):
    base64_string: str

class ImageResponse(BaseModel):
    text: str

@router.post("/ocr", response_model=ImageResponse)
async def ocr_endpoint(request: ImageRequest):
    try:
        result = process_image(request.base64_string)
        return ImageResponse(text=result)
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))