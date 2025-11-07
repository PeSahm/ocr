# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image with EasyOCR dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install Python and system dependencies for EasyOCR (separate layer for better caching)
RUN apt update && \
    apt install -y \
    python3 python3-pip \
    libglib2.0-0 libgomp1 libgl1-mesa-glx && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

# Install Python packages in a separate layer for better caching
RUN python3 -m pip install --no-cache-dir flask pillow numpy --break-system-packages

# Install EasyOCR separately (large download, benefits from caching)
RUN python3 -m pip install --no-cache-dir easyocr --break-system-packages

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only project file first for better caching of restore layer
COPY ["TesseractApi/TesseractApi.csproj", "TesseractApi/"]
RUN dotnet restore "TesseractApi/TesseractApi.csproj"

# Copy rest of the source code
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

# Create directory for EasyOCR model cache
RUN mkdir -p /root/.EasyOCR/model

# Create startup script to run both .NET API and EasyOCR server
RUN printf '#!/bin/bash\nset -euo pipefail\npython3 /app/PythonOCR/easyocr_server.py &\nexec dotnet TesseractApi.dll\n' > /app/start.sh && chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]
