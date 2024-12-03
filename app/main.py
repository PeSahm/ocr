"""
FastAPI application initialization and configuration
"""
import logging
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from .config import config
from .routers import ocr, health

# Configure logging
logging.basicConfig(
    level=logging.DEBUG if config.DEBUG_MODE else logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

def create_app():
    logger.info("Initializing FastAPI application")
    app = FastAPI(
        title=config.APP_NAME,
        description="OCR API using EasyOCR",
        version="1.0.0",
        docs_url="/api/docs",
        redoc_url="/api/redoc"
    )
    
    logger.info("Configuring CORS")
    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )
    
    logger.info("Including routers")
    app.include_router(health.router, prefix="/api", tags=["health"])
    app.include_router(ocr.router, prefix="/api/v1", tags=["ocr"])
    
    logger.info("Application startup complete")
    return app

app = create_app()

@app.on_event("startup")
async def startup_event():
    logger.info("Application starting up")
    logger.info(f"Server running at http://{config.HOST}:{config.PORT}")
    logger.info(f"Documentation available at http://{config.HOST}:{config.PORT}/api/docs")