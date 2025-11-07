#!/usr/bin/env python3
"""
EasyOCR HTTP Server - Keeps the model loaded in memory for faster processing
"""
from flask import Flask, request, jsonify
import easyocr
import numpy as np
from PIL import Image
import io
import base64

app = Flask(__name__)

# Initialize EasyOCR once at startup (takes ~2-3 seconds)
print("Loading EasyOCR model...")
reader = easyocr.Reader(['en'], gpu=False, verbose=False)
print("EasyOCR model loaded and ready!")

@app.route('/health', methods=['GET'])
def health():
    return jsonify({"status": "healthy"}), 200

@app.route('/ocr', methods=['POST'])
def ocr():
    try:
        data = request.get_json()
        
        # Decode base64 image
        image_data = base64.b64decode(data['base64'].split(',')[-1])
        img = Image.open(io.BytesIO(image_data))
        
        # Convert to RGB if needed
        if img.mode != 'RGB':
            img = img.convert('RGB')
        
        # Resize if too large
        max_size = 800
        if max(img.size) > max_size:
            ratio = max_size / max(img.size)
            new_size = tuple([int(x * ratio) for x in img.size])
            img = img.resize(new_size, Image.Resampling.LANCZOS)
        
        img_array = np.array(img)
        
        # Fast OCR with optimized parameters
        result = reader.readtext(
            img_array,
            allowlist='0123456789',
            detail=0,
            paragraph=False,
            batch_size=1,
            width_ths=0.7,
            decoder='greedy'
        )
        
        text = ''.join(result).strip()
        return jsonify({"text": text}), 200
        
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5001, threaded=True)
