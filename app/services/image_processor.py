"""
Image processing service using EasyOCR
"""
import easyocr
from typing import Dict, Union
from ..utils.image_utils import decode_base64
from ..utils.exceptions import ImageProcessingError

class ImageProcessor:
    def __init__(self):
        """Initialize the OCR reader with English language support."""
        try:
            # Enable downloads in case models are missing
            self.reader = easyocr.Reader(['en'], gpu=False, download_enabled=True)
        except Exception as e:
            raise ImageProcessingError(f"Failed to initialize OCR reader: {str(e)}")
    
    def extract_text(self, image_data: bytes) -> str:
        """
        Extract text from image using EasyOCR.
        
        Args:
            image_data: Binary image data
            
        Returns:
            str: Extracted text from the image
            
        Raises:
            ImageProcessingError: If text extraction fails
        """
        try:
            result = self.reader.readtext(image_data, detail=0)
            return " ".join(result) if result else ""
        except Exception as e:
            raise ImageProcessingError(f"Text extraction failed: {str(e)}")
    
    def process_image(self, base64_string: str) -> Dict[str, str]:
        """
        Process the image and extract text.
        
        Args:
            base64_string: Base64 encoded image string
            
        Returns:
            dict: Dictionary containing extracted text or error message
        """
        try:
            binary_data = decode_base64(base64_string)
            text = self.extract_text(binary_data)
            return {"text": text}
        except Exception as e:
            return {"error": str(e)}