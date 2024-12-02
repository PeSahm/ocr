# OCR API

A FastAPI-based web API that performs OCR (Optical Character Recognition) on base64-encoded images using EasyOCR.

## Features

- Base64 image processing
- Text extraction using EasyOCR
- Dockerized application
- RESTful API endpoints

## Setup and Running

1. Build and run with Docker Compose:
   ```bash
   docker-compose up --build
   ```

2. The API will be available at: http://localhost:8000

3. API Documentation available at:
   - Swagger UI: http://localhost:8000/docs
   - ReDoc: http://localhost:8000/redoc

## API Usage

Send a POST request to `/api/v1/ocr` with a JSON body:

```json
{
    "base64_string": "your_base64_encoded_image_string"
}
```

The API will return the extracted text:

```json
{
    "text": "extracted text from the image"
}
```