FROM python:3.9-slim as builder

WORKDIR /app

# Install build dependencies
RUN apt-get update && apt-get install -y \
    --no-install-recommends \
    build-essential \
    python3-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Download EasyOCR models during build
RUN mkdir -p /root/.EasyOCR/model && \
    python3 -c "import easyocr; reader = easyocr.Reader(['en'], gpu=False)"

# Final stage
FROM python:3.9-slim

WORKDIR /app

# Install runtime dependencies
RUN apt-get update && apt-get install -y \
    --no-install-recommends \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender1 \
    curl \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean autoclean \
    && apt-get autoremove -y

# Copy Python packages and binaries from builder
COPY --from=builder /usr/local/lib/python3.9/site-packages /usr/local/lib/python3.9/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin
COPY --from=builder /root/.EasyOCR /home/appuser/.EasyOCR

# Copy application code
COPY ./app /app/app
COPY entrypoint.sh .

# Make entrypoint executable and fix line endings
RUN chmod +x entrypoint.sh && \
    sed -i 's/\r$//' entrypoint.sh && \
    # Create non-root user
    useradd -m appuser && \
    chown -R appuser:appuser /app /home/appuser/.EasyOCR

USER appuser

EXPOSE 8000

ENTRYPOINT ["./entrypoint.sh"]