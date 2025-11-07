# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image with EasyOCR dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install Python and system dependencies for EasyOCR
RUN apt update && \
    apt install -y \
    python3 python3-pip \
    libglib2.0-0 libgomp1 libgl1-mesa-glx && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

# Install EasyOCR and Flask
RUN python3 -m pip install --no-cache-dir easyocr flask pillow --break-system-packages

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
RUN printf '#!/bin/bash\nset -euo pipefail\npython3 /app/PythonOCR/easyocr_server.py &\nexec dotnet TesseractApi.dll\n' > /app/start.sh && chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]

# Optional: Add a healthcheck to verify the app's availability
#HEALTHCHECK --interval=30s --timeout=10s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1
