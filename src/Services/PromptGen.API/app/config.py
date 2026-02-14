# app/config.py
"""
Конфигурация приложения через Pydantic Settings.
Поддержка SQLite (default), PostgreSQL, SQL Server.
"""

from typing import Optional, List
from pydantic_settings import BaseSettings
from pydantic import Field
from functools import lru_cache
import os


class Settings(BaseSettings):
    """Настройки приложения"""
    
    # === App ===
    APP_NAME: str = "PromptGen.API"
    APP_VERSION: str = "1.0.0"
    DEBUG: bool = False
    DEVELOPMENT_MODE: bool = True
    TESTING: bool = False
    HOST: str = "0.0.0.0"
    PORT: int = 8000
    WORKERS: int = 1
    
    # === Database ===
    # Тип БД: sqlite (default), postgresql, mssql
    DB_TYPE: str = "sqlite"
    
    # SQLite (по умолчанию для разработки)
    DATABASE_URL: Optional[str] = "sqlite+aiosqlite:///./data/promptgen.db"
    DB_PATH: str = "./data/promptgen.db"
    
    # PostgreSQL / SQL Server параметры
    DB_HOST: str = "localhost"
    DB_PORT: int = 5432
    DB_NAME: str = "promptgen"
    DB_USER: str = "postgres"
    DB_PASSWORD: str = "postgres"
    DB_DRIVER: str = "ODBC+Driver+17+for+SQL+Server"  # Для SQL Server
    
    # Connection pool (игнорируется для SQLite)
    DB_POOL_SIZE: int = 5
    DB_MAX_OVERFLOW: int = 10
    DB_POOL_TIMEOUT: int = 30
    DB_POOL_RECYCLE: int = 1800
    DB_ECHO: bool = False
    
    # === Redis (опционально) ===
    REDIS_ENABLED: bool = False
    REDIS_URL: Optional[str] = None
    REDIS_HOST: str = "localhost"
    REDIS_PORT: int = 6379
    REDIS_DB: int = 0
    REDIS_PASSWORD: Optional[str] = None
    CACHE_TTL: int = 3600
    
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
    RATE_LIMIT_ENABLED: bool = True
    RATE_LIMIT_PER_MINUTE: int = 60
    RATE_LIMIT_REQUESTS: int = 100
    RATE_LIMIT_PERIOD: int = 60
    
    # === AI Configuration ===
    DEFAULT_AI_PROVIDER: str = "openai"
    MAX_PROMPT_LENGTH: int = 2000
    MAX_ENHANCED_LENGTH: int = 500
    SUPPORTED_MODELS: List[str] = ["midjourney", "dalle3", "stable-diffusion", "flux"]
    
    # === Logging ===
    LOG_LEVEL: str = "INFO"
    LOG_FORMAT: str = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
    LOG_FILE: str = "logs/promptgen.log"
    
    # === Catalog.API Integration ===
    CATALOG_API_URL: str = "http://localhost:5100"
    CATALOG_API_TIMEOUT: int = 30
    
    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
        case_sensitive = True
        extra = "ignore"  # Игнорировать неизвестные поля


@lru_cache()
def get_settings() -> Settings:
    """Кэшированное получение настроек"""
    return Settings()


# Глобальный экземпляр
settings = get_settings()


def ensure_directories():
    """Создаёт необходимые директории"""
    dirs = [
        "data",
        "logs", 
        "storage",
        "storage/chroma",
        "uploads",
        "temp"
    ]
    for d in dirs:
        os.makedirs(d, exist_ok=True)