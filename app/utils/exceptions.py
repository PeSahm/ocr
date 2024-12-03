"""
Custom exceptions for the OCR API
"""
from fastapi import HTTPException

class OCRException(HTTPException):
    def __init__(self, detail: str):
        super().__init__(status_code=400, detail=detail)

class ImageProcessingError(OCRException):
    pass

class InvalidBase64Error(OCRException):
    pass