# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image with Tesseract OCR and dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install Python, Tesseract, and system dependencies
RUN apt update && \
    apt install -y \
    tesseract-ocr libtesseract-dev tesseract-ocr-eng tesseract-ocr-fas libleptonica-dev \
    python3 python3-pip python3-venv \
    libglib2.0-0 libsm6 libxext6 libxrender-dev libgomp1 libgl1-mesa-glx && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

# Create symlink for leptonica library
RUN ln -s /usr/lib/x86_64-linux-gnu/libleptonica.so.6 /usr/lib/x86_64-linux-gnu/libleptonica-1.82.0.so || true && \
    ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /usr/lib/x86_64-linux-gnu/libtesseract50.so || true

# Install Python OCR libraries
RUN python3 -m pip install --no-cache-dir paddlepaddle paddleocr easyocr flask pillow --break-system-packages

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TesseractApi/TesseractApi.csproj", "TesseractApi/"]
RUN dotnet restore "TesseractApi/TesseractApi.csproj"
COPY . .
WORKDIR "/src/TesseractApi"
RUN dotnet build "TesseractApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TesseractApi.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 5001

COPY --from=publish /app/publish .

# Copy Python OCR scripts
COPY TesseractApi/PythonOCR/ ./PythonOCR/
RUN chmod +x ./PythonOCR/*.py

# Create startup script to run both .NET API and EasyOCR server
RUN echo '#!/bin/bash\npython3 /app/PythonOCR/easyocr_server.py &\ndotnet TesseractApi.dll' > /app/start.sh && chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]

# Optional: Add a healthcheck to verify the app's availability
#HEALTHCHECK --interval=30s --timeout=10s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1
