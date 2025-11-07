# EasyOCR Web API

EasyOCR (Deep Learning based OCR) as a Web API using .NET and Python.

**Ready to Use** - Just execute `docker-compose up -d --build`

## Features

- 🚀 Fast deep learning-based OCR using EasyOCR
- 🔄 Persistent model caching (models downloaded once and stored locally)
- 🐳 Optimized Docker build with layer caching
- 📦 Supports both file upload and base64 image input

## Quick Start

```bash
# Clone and start
git clone <your-repo-url>
cd ocr
docker-compose up -d --build
```

The API will be available at:
- .NET API: `http://localhost:8080`
- EasyOCR Server: `http://localhost:5001`

## Docker Optimizations

This project includes several Docker optimizations:

1. **Layer Caching**: Dependencies are installed in separate layers for better caching
2. **Model Persistence**: EasyOCR models are cached in `./easyocr_models/` volume
3. **Minimal Dependencies**: Only essential libraries are installed
4. **.dockerignore**: Excludes unnecessary files from build context

### Volume Mapping

The `easyocr_models` folder is mounted as a volume to persist downloaded models between container restarts. This means:
- First run: Models are downloaded (~100-200MB)
- Subsequent runs: Models are loaded from local cache (much faster!)

## API Endpoints

### OCR from File Upload
```bash
curl --location 'http://localhost:8080/ocr/captcha-easy' \
--form 'file=@"/path/to/your-image.png"'
```

### OCR from Base64
```bash
curl --location 'http://localhost:8080/ocr/captcha-easy-base64' \
--header 'Content-Type: application/json' \
--data '{
  "base64": "data:image/jpeg;base64,/9j/4AAQSkZJRg..."
}'
```

## How It Works

1. **EasyOCR Server** runs on port 5001 as a Flask application
   - Loads the model once at startup
   - Keeps the model in memory for fast inference
   
2. **.NET API** runs on port 8080
   - Receives OCR requests via HTTP
   - Forwards images to the EasyOCR server
   - Returns recognized text

## Development

### Build and Run Locally
```bash
docker-compose up -d --build
```

### View Logs
```bash
docker-compose logs -f api
```

### Stop Services
```bash
docker-compose down
```

## Requirements

- Docker and Docker Compose
- At least 2GB of free disk space (for models)
- Linux containers support
