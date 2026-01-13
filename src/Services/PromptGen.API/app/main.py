# app/main.py
"""
Точка входа FastAPI приложения.
"""

from contextlib import asynccontextmanager
from fastapi import FastAPI, Request, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from fastapi.exceptions import RequestValidationError
import time

from app.config import settings
from api.responses import ErrorResponse, ValidationErrorResponse, ValidationErrorDetail


# Lifespan для startup/shutdown
@asynccontextmanager
async def lifespan(app: FastAPI):
    """Lifecycle events"""
    # Startup
    print("Starting PromptGen.API...")
    
    # Инициализация базы данных
    from services.storage.database_service import init_database
    await init_database()
    
    # Прогрев кэша
    from services.storage.cache_service import cache_service
    await cache_service.health_check()
    
    print("PromptGen.API started successfully")
    
    yield
    
    # Shutdown
    print("Shutting down PromptGen.API...")
    
    from services.storage.database_service import close_database
    await close_database()
    
    await cache_service.close()
    
    print("PromptGen.API stopped")


# Создаём приложение
app = FastAPI(
    title="PromptGen.API",
    description="AI-Powered Prompt Generation Service for NovelVision",
    version=getattr(settings, 'APP_VERSION', '1.0.0'),
    docs_url="/docs",
    redoc_url="/redoc",
    openapi_url="/openapi.json",
    lifespan=lifespan
)


# === Middleware ===

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=getattr(settings, 'CORS_ORIGINS', ["*"]),
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"]
)


# Request timing middleware
@app.middleware("http")
async def add_process_time_header(request: Request, call_next):
    start_time = time.time()
    response = await call_next(request)
    process_time = time.time() - start_time
    response.headers["X-Process-Time"] = str(round(process_time * 1000, 2))
    return response


# === Exception handlers ===

@app.exception_handler(RequestValidationError)
async def validation_exception_handler(request: Request, exc: RequestValidationError):
    """Обработка ошибок валидации"""
    errors = []
    for error in exc.errors():
        errors.append(ValidationErrorDetail(
            field=".".join(str(loc) for loc in error["loc"]),
            message=error["msg"],
            value=error.get("input")
        ))
    
    return JSONResponse(
        status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
        content=ValidationErrorResponse(errors=errors).model_dump()
    )


@app.exception_handler(Exception)
async def general_exception_handler(request: Request, exc: Exception):
    """Общий обработчик ошибок"""
    return JSONResponse(
        status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
        content=ErrorResponse(
            error=str(exc),
            error_code="INTERNAL_ERROR"
        ).model_dump()
    )


# === Routes ===

# Health endpoints
from api.v1.endpoints.health import router as health_router
app.include_router(health_router)

# API v1
from api.v1.endpoints.prompt import router as prompt_router
app.include_router(prompt_router, prefix="/api/v1")

# Дополнительные роутеры (когда будут готовы)
# from api.v1.endpoints.story import router as story_router
# from api.v1.endpoints.character import router as character_router
# from api.v1.endpoints.scene import router as scene_router
# from api.v1.endpoints.style import router as style_router

# app.include_router(story_router, prefix="/api/v1")
# app.include_router(character_router, prefix="/api/v1")
# app.include_router(scene_router, prefix="/api/v1")
# app.include_router(style_router, prefix="/api/v1")


# === Root endpoint ===

@app.get("/")
async def root():
    """Root endpoint"""
    return {
        "service": "PromptGen.API",
        "version": getattr(settings, 'APP_VERSION', '1.0.0'),
        "status": "running",
        "docs": "/docs"
    }


# === Запуск ===

if __name__ == "__main__":
    import uvicorn
    
    uvicorn.run(
        "app.main:app",
        host=getattr(settings, 'HOST', '0.0.0.0'),
        port=getattr(settings, 'PORT', 8000),
        reload=getattr(settings, 'DEBUG', False),
        workers=getattr(settings, 'WORKERS', 1)
    )