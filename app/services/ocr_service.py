import base64
import easyocr

def process_image(base64_string: str) -> str:
    """Process the image and extract text using EasyOCR."""
    try:
        binary_data = base64.b64decode(base64_string)
        reader = easyocr.Reader(['en'], gpu=False)
        result = reader.readtext(binary_data, detail=0)
        return "".join(result)
    except Exception as e:
        return str(e)