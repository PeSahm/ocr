# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image with Tesseract OCR and dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt update && \
    apt install -y tesseract-ocr libtesseract-dev wget tesseract-ocr-eng && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

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
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TesseractApi.dll"]

# Optional: Add a healthcheck to verify the app's availability
#HEALTHCHECK --interval=30s --timeout=10s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1
