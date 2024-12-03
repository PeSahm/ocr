#!/bin/bash
set -e

# Start Uvicorn
exec uvicorn app.main:app \
    --host 0.0.0.0 \
    --port 8000 \
    --workers 2 \
    --timeout-keep-alive 120 \
    --log-level info