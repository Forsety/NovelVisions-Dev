# api/v1/dependencies.py
"""
Dependency Injection для FastAPI endpoints.
"""

from typing import Optional, AsyncGenerator, List
from functools import lru_cache

from fastapi import Depends, HTTPException, Header, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from sqlalchemy.ext.asyncio import AsyncSession

# Используем python-jose вместо jwt
from jose import jwt, JWTError

from app.config import settings
from models.database.session import get_db
from services.storage.cache_service import CacheService, cache_service
from services.storage.vector_store import VectorStore
from services.ai.openai_service import OpenAIService
from core.engines.prompt_enhancer import PromptEnhancer
from core.engines.style_engine import StyleEngine
from core.templates.template_engine import TemplateEngine
from core.generators.base_generator import GeneratorFactory


# === Security ===

security = HTTPBearer(auto_error=False)


async def get_current_user(
    credentials: Optional[HTTPAuthorizationCredentials] = Depends(security),
    x_user_id: Optional[str] = Header(None, alias="X-User-Id")
) -> dict:
    """
    Получает текущего пользователя из JWT токена или заголовка.
    """
    # Для development/testing - разрешаем X-User-Id header
    if getattr(settings, 'DEVELOPMENT_MODE', False) and x_user_id:
        return {
            "user_id": x_user_id,
            "roles": ["user"],
            "is_authenticated": True
        }
    
    if not credentials:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Not authenticated",
            headers={"WWW-Authenticate": "Bearer"}
        )
    
    try:
        # Декодируем JWT
        jwt_secret = getattr(settings, 'JWT_SECRET', 'secret')
        jwt_algorithm = getattr(settings, 'JWT_ALGORITHM', 'HS256')
        
        payload = jwt.decode(
            credentials.credentials,
            jwt_secret,
            algorithms=[jwt_algorithm]
        )
        
        user_id = payload.get("sub") or payload.get("user_id")
        if not user_id:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Invalid token: no user_id"
            )
        
        return {
            "user_id": user_id,
            "email": payload.get("email"),
            "roles": payload.get("roles", ["user"]),
            "is_authenticated": True
        }
        
    except JWTError as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=f"Invalid token: {str(e)}"
        )


async def get_optional_user(
    credentials: Optional[HTTPAuthorizationCredentials] = Depends(security),
    x_user_id: Optional[str] = Header(None, alias="X-User-Id")
) -> Optional[dict]:
    """Получает пользователя если токен предоставлен, иначе None"""
    try:
        return await get_current_user(credentials, x_user_id)
    except HTTPException:
        return None


def require_roles(*required_roles: str):
    """Dependency для проверки ролей."""
    async def role_checker(
        user: dict = Depends(get_current_user)
    ) -> dict:
        user_roles = set(user.get("roles", []))
        required = set(required_roles)
        
        if not user_roles.intersection(required):
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"Required roles: {required_roles}"
            )
        
        return user
    
    return role_checker


# === Database ===

async def get_database() -> AsyncGenerator[AsyncSession, None]:
    """Dependency для получения сессии БД"""
    async for session in get_db():
        yield session


# === Cache ===

async def get_redis_cache() -> CacheService:
    """Dependency для получения кэша"""
    return cache_service


# === Vector Store ===

_vector_store: Optional[VectorStore] = None

async def get_vector_store() -> VectorStore:
    """Dependency для получения векторного хранилища"""
    global _vector_store
    
    if _vector_store is None:
        backend = getattr(settings, 'VECTOR_STORE_BACKEND', 'memory')
        _vector_store = VectorStore(backend=backend)
    
    return _vector_store


# === AI Services ===

_openai_service: Optional[OpenAIService] = None

async def get_openai_service() -> OpenAIService:
    """Dependency для OpenAI сервиса"""
    global _openai_service
    
    if _openai_service is None:
        _openai_service = OpenAIService()
    
    return _openai_service


# === Engines ===

async def get_prompt_enhancer(
    db: AsyncSession = Depends(get_database),
    cache: CacheService = Depends(get_redis_cache)
) -> PromptEnhancer:
    """Dependency для PromptEnhancer"""
    return PromptEnhancer(db=db, cache=cache)


@lru_cache()
def get_style_engine() -> StyleEngine:
    """Dependency для StyleEngine (кэшируется)"""
    return StyleEngine()


@lru_cache()
def get_template_engine() -> TemplateEngine:
    """Dependency для TemplateEngine (кэшируется)"""
    return TemplateEngine()


@lru_cache()
def get_generator_factory() -> GeneratorFactory:
    """Dependency для GeneratorFactory (кэшируется)"""
    return GeneratorFactory()


# === Pagination ===

class PaginationParams:
    """Параметры пагинации"""
    
    def __init__(
        self,
        skip: int = 0,
        limit: int = 20,
        max_limit: int = 100
    ):
        self.skip = max(0, skip)
        self.limit = min(max(1, limit), max_limit)


def get_pagination(
    skip: int = 0,
    limit: int = 20
) -> PaginationParams:
    """Dependency для пагинации"""
    return PaginationParams(skip=skip, limit=limit)


# === Rate Limiting ===

class RateLimiter:
    """Простой rate limiter на основе кэша"""
    
    def __init__(
        self,
        requests_per_minute: int = 60,
        cache: Optional[CacheService] = None
    ):
        self.rpm = requests_per_minute
        self.cache = cache or cache_service
    
    async def check(self, key: str) -> bool:
        """Проверяет лимит"""
        cache_key = f"ratelimit:{key}"
        
        count = await self.cache.incr(cache_key)
        
        if count == 1:
            await self.cache.expire(cache_key, 60)
        
        return count <= self.rpm
    
    async def get_remaining(self, key: str) -> int:
        """Возвращает оставшееся количество запросов"""
        cache_key = f"ratelimit:{key}"
        count_str = await self.cache.get(cache_key)
        count = int(count_str) if count_str else 0
        return max(0, self.rpm - count)


def rate_limit(requests_per_minute: int = 60):
    """Dependency для rate limiting."""
    limiter = RateLimiter(requests_per_minute)
    
    async def check_rate_limit(
        user: dict = Depends(get_current_user)
    ) -> None:
        user_id = user.get("user_id", "anonymous")
        
        if not await limiter.check(user_id):
            remaining = await limiter.get_remaining(user_id)
            raise HTTPException(
                status_code=status.HTTP_429_TOO_MANY_REQUESTS,
                detail=f"Rate limit exceeded. Remaining: {remaining}",
                headers={"Retry-After": "60"}
            )
    
    return check_rate_limit