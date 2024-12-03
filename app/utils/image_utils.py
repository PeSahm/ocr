"""
Image processing utilities
"""
import base64
from typing import Optional
from .exceptions import InvalidBase64Error

def decode_base64(base64_string: str) -> Optional[bytes]:
    """
    Decode base64 string to bytes.
    
    Args:
        base64_string: The base64 encoded string
        
    Returns:
        bytes: The decoded binary data
        
    Raises:
        InvalidBase64Error: If the string is not valid base64
    """
    try:
        # Remove data URL prefix if present
        if ',' in base64_string:
            base64_string = base64_string.split(',')[1]
        return base64.b64decode(base64_string)
    except Exception as e:
        raise InvalidBase64Error(f"Invalid base64 string: {str(e)}")