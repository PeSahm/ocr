#!/usr/bin/env python3
import sys
import os
import easyocr
import numpy as np
from PIL import Image

# Initialize EasyOCR with English language (cache the reader globally)
reader = easyocr.Reader(['en'], gpu=False, verbose=False)

def recognize_digits(image_path):
    try:
        # Load and preprocess image for better speed
        img = Image.open(image_path)
        
        # Convert to RGB if needed
        if img.mode != 'RGB':
            img = img.convert('RGB')
        
        # Resize if image is too large (speeds up processing significantly)
        max_size = 800
        if max(img.size) > max_size:
            ratio = max_size / max(img.size)
            new_size = tuple([int(x * ratio) for x in img.size])
            img = img.resize(new_size, Image.Resampling.LANCZOS)
        
        # Convert to numpy array
        img_array = np.array(img)
        
        # Use optimized parameters for speed
        # allowlist only digits, detail=0 for faster processing
        # paragraph=False to treat as single line
        result = reader.readtext(
            img_array, 
            allowlist='0123456789',
            detail=0,
            paragraph=False,
            batch_size=1,
            width_ths=0.7,
            decoder='greedy'  # Greedy decoder is faster than beamsearch
        )
        
        # Join all detected text
        digits = ''.join(result)
        return digits.strip()
    except Exception as e:
        print(f"Error: {str(e)}", file=sys.stderr)
        return ''

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print("Usage: easy_ocr.py <image_path>", file=sys.stderr)
        sys.exit(1)
    
    image_path = sys.argv[1]
    result = recognize_digits(image_path)
    print(result, end='')
