# app/config.py
"""
Конфигурация приложения через Pydantic Settings.
"""

from typing import Optional, List
from pydantic_settings import BaseSettings
from pydantic import Field
from functools import lru_cache


class Settings(BaseSettings):
    """Настройки приложения"""
    
    # === App ===
    APP_NAME: str = "PromptGen.API"
    APP_VERSION: str = "1.0.0"
    DEBUG: bool = False
    DEVELOPMENT_MODE: bool = False
    HOST: str = "0.0.0.0"
    PORT: int = 8000
    WORKERS: int = 1
    
    # === Database ===
    DATABASE_URL: Optional[str] = None
    DB_HOST: str = "localhost"
    DB_PORT: int = 5432
    DB_NAME: str = "promptgen"
    DB_USER: str = "postgres"
    DB_PASSWORD: str = "postgres"
    DB_POOL_SIZE: int = 5
    DB_MAX_OVERFLOW: int = 10
    DB_POOL_TIMEOUT: int = 30
    DB_POOL_RECYCLE: int = 1800
    DB_ECHO: bool = False
    
    # === Redis ===
    REDIS_URL: Optional[str] = None
    REDIS_HOST: str = "localhost"
    REDIS_PORT: int = 6379
    REDIS_DB: int = 0
    REDIS_PASSWORD: Optional[str] = None
    
    # === Vector Store ===
    VECTOR_STORE_BACKEND: str = "memory"  # memory, chroma, qdrant
    CHROMA_PERSIST_DIR: str = "./storage/chroma"
    QDRANT_HOST: str = "localhost"
    QDRANT_PORT: int = 6333
    
    # === OpenAI ===
    OPENAI_API_KEY: Optional[str] = None
    OPENAI_MODEL: str = "gpt-4-turbo-preview"
    OPENAI_EMBEDDING_MODEL: str = "text-embedding-3-small"
    
    # === Anthropic ===
    ANTHROPIC_API_KEY: Optional[str] = None
    ANTHROPIC_MODEL: str = "claude-3-sonnet-20240229"
    
    # === JWT ===
    JWT_SECRET: str = "your-secret-key-change-in-production"
    JWT_ALGORITHM: str = "HS256"
    JWT_EXPIRE_MINUTES: int = 60
    
    # === CORS ===
    CORS_ORIGINS: List[str] = ["*"]
    
    # === Rate Limiting ===
    RATE_LIMIT_PER_MINUTE: int = 60
    
    # === Logging ===
    LOG_LEVEL: str = "INFO"
    LOG_FORMAT: str = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
    
    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
        case_sensitive = True


@lru_cache()
def get_settings() -> Settings:
    """Кэшированное получение настроек"""
    return Settings()


# Глобальный экземпляр
settings = get_settings()