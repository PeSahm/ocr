from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    APP_NAME: str = "OCR API"
    DEBUG_MODE: bool = False

    class Config:
        env_file = ".env"

settings = Settings()