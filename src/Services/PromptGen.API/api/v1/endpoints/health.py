# api/v1/endpoints/health.py
"""
Health check endpoints.
"""

from fastapi import APIRouter, Depends
from typing import Dict

from api.responses import HealthResponse
from api.v1.dependencies import get_redis_cache, get_database, get_openai_service
from services.storage.cache_service import CacheService
from models.database.session import db_manager
from app.config import settings


router = APIRouter(tags=["Health"])


@router.get(
    "/health",
    response_model=HealthResponse,
    summary="Health Check",
    description="Проверка здоровья сервиса"
)
async def health_check():
    """Проверяет состояние всех компонентов"""
    
    services = {}
    overall_status = "healthy"
    
    # Проверка базы данных
    try:
        db_healthy = await db_manager.health_check()
        services["database"] = db_healthy
        if not db_healthy:
            overall_status = "degraded"
    except Exception:
        services["database"] = False
        overall_status = "degraded"
    
    # Проверка Redis
    try:
        cache = CacheService()
        redis_healthy = await cache.health_check()
        services["redis"] = redis_healthy
        if not redis_healthy:
            overall_status = "degraded"
    except Exception:
        services["redis"] = False
        overall_status = "degraded"
    
    # Проверка OpenAI
    try:
        from services.ai.openai_service import OpenAIService
        openai_service = OpenAIService()
        openai_healthy = await openai_service.health_check()
        services["openai"] = openai_healthy
        if not openai_healthy:
            overall_status = "degraded"
    except Exception:
        services["openai"] = False
        # OpenAI не критичен для работы
    
    # Если критичные сервисы недоступны - unhealthy
    if not services.get("database", False):
        overall_status = "unhealthy"
    
    return HealthResponse(
        status=overall_status,
        version=getattr(settings, 'APP_VERSION', '1.0.0'),
        services=services
    )


@router.get(
    "/health/live",
    summary="Liveness Probe",
    description="Kubernetes liveness probe"
)
async def liveness():
    """Простая проверка что сервис запущен"""
    return {"status": "alive"}


@router.get(
    "/health/ready",
    summary="Readiness Probe",
    description="Kubernetes readiness probe"
)
async def readiness():
    """Проверка готовности принимать трафик"""
    
    # Проверяем только критичные зависимости
    db_ready = await db_manager.health_check()
    
    if not db_ready:
        return {"status": "not_ready", "reason": "database unavailable"}
    
    return {"status": "ready"}