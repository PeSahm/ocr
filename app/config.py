"""
Application configuration using Pydantic settings management
"""
from pydantic import BaseSettings

class Settings(BaseSettings):
    APP_NAME: str = "OCR API"
    DEBUG_MODE: bool = False
    HOST: str = "0.0.0.0"
    PORT: int = 8000

    class Config:
        env_file = ".env"
        case_sensitive = False

# Create a global instance
config = Settings()