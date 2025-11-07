#!/usr/bin/env python3
import sys
import os
from paddleocr import PaddleOCR

# Initialize PaddleOCR with minimal parameters
ocr = PaddleOCR(lang='en')

def recognize_digits(image_path):
    try:
        result = ocr.ocr(image_path, cls=False)
        
        if result and result[0]:
            # Extract only digits from all detected text
            digits = ''
            for line in result[0]:
                text = line[1][0]  # Get text from result
                # Keep only digits
                digits += ''.join(filter(str.isdigit, text))
            
            return digits.strip()
        return ''
    except Exception as e:
        print(f"Error: {str(e)}", file=sys.stderr)
        return ''

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print("Usage: paddle_ocr.py <image_path>", file=sys.stderr)
        sys.exit(1)
    
    image_path = sys.argv[1]
    result = recognize_digits(image_path)
    print(result, end='')
