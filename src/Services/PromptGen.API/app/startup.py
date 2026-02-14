
import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.middleware.gzip import GZipMiddleware
from slowapi import _rate_limit_exceeded_handler
from slowapi.errors import RateLimitExceeded

from app.config import settings
from api.v1.endpoints import prompt, character, scene, story, style
from services.storage.database_service import init_db
from services.storage.cache_service import init_cache
from utils.logger import setup_logging


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan events"""
    # Startup
    setup_logging(settings.LOG_LEVEL, settings.LOG_FILE)
    logger = logging.getLogger(__name__)
    logger.info(f"Starting {settings.APP_NAME} v{settings.APP_VERSION}")
    
    # Initialize database
    await init_db()
    logger.info("Database initialized")
    
    # Initialize cache
    await init_cache()
    logger.info("Cache service initialized")
    
    yield
    
    # Shutdown
    logger.info("Shutting down application")


def create_application() -> FastAPI:
    """Create FastAPI application"""
    
    app = FastAPI(
        title=settings.APP_NAME,
        version=settings.APP_VERSION,
        debug=settings.DEBUG,
        lifespan=lifespan,
        openapi_url=f"{settings.API_PREFIX}/openapi.json",
        docs_url=f"{settings.API_PREFIX}/docs",
        redoc_url=f"{settings.API_PREFIX}/redoc"
    )
    
    # Add middlewares
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.CORS_ORIGINS,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )
    
    app.add_middleware(GZipMiddleware, minimum_size=1000)
    
    # Include routers
    app.include_router(
        prompt.router,
        prefix=f"{settings.API_PREFIX}/prompts",
        tags=["prompts"]
    )
    
    app.include_router(
        character.router,
        prefix=f"{settings.API_PREFIX}/characters",
        tags=["characters"]
    )
    
    app.include_router(
        scene.router,
        prefix=f"{settings.API_PREFIX}/scenes",
        tags=["scenes"]
    )
    
    app.include_router(
        story.router,
        prefix=f"{settings.API_PREFIX}/stories",
        tags=["stories"]
    )
    
    app.include_router(
        style.router,
        prefix=f"{settings.API_PREFIX}/styles",
        tags=["styles"]
    )
    
    # Add exception handlers
    if settings.RATE_LIMIT_ENABLED:
        from slowapi import Limiter
        from slowapi.util import get_remote_address
        
        limiter = Limiter(
            key_func=get_remote_address,
            default_limits=[f"{settings.RATE_LIMIT_REQUESTS}/{settings.RATE_LIMIT_PERIOD}seconds"]
        )
        app.state.limiter = limiter
        app.add_exception_handler(RateLimitExceeded, _rate_limit_exceeded_handler)
    
    return app
