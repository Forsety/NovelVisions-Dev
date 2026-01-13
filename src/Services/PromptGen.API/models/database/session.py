# models/database/session.py
"""
Асинхронная сессия SQLAlchemy и управление подключениями.
"""

from typing import AsyncGenerator, Optional
from contextlib import asynccontextmanager

from sqlalchemy.ext.asyncio import (
    AsyncSession, 
    AsyncEngine,
    create_async_engine,
    async_sessionmaker
)
from sqlalchemy.pool import NullPool, QueuePool
from sqlalchemy import text

from app.config import settings


class DatabaseManager:
    """
    Менеджер базы данных.
    
    Управляет подключениями, сессиями и пулом соединений.
    """
    
    _instance: Optional["DatabaseManager"] = None
    _engine: Optional[AsyncEngine] = None
    _session_factory: Optional[async_sessionmaker] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    @property
    def engine(self) -> AsyncEngine:
        """Получает или создаёт engine"""
        if self._engine is None:
            self._engine = self._create_engine()
        return self._engine
    
    @property
    def session_factory(self) -> async_sessionmaker:
        """Получает или создаёт session factory"""
        if self._session_factory is None:
            self._session_factory = async_sessionmaker(
                bind=self.engine,
                class_=AsyncSession,
                expire_on_commit=False,
                autocommit=False,
                autoflush=False
            )
        return self._session_factory
    
    def _create_engine(self) -> AsyncEngine:
        """Создаёт асинхронный engine"""
        
        # Получаем URL базы данных
        database_url = self._get_database_url()
        
        # Параметры пула
        pool_size = getattr(settings, 'DB_POOL_SIZE', 5)
        max_overflow = getattr(settings, 'DB_MAX_OVERFLOW', 10)
        pool_timeout = getattr(settings, 'DB_POOL_TIMEOUT', 30)
        pool_recycle = getattr(settings, 'DB_POOL_RECYCLE', 1800)
        
        # Определяем тип пула
        # Для тестов используем NullPool
        if getattr(settings, 'TESTING', False):
            pool_class = NullPool
            pool_kwargs = {}
        else:
            pool_class = QueuePool
            pool_kwargs = {
                "pool_size": pool_size,
                "max_overflow": max_overflow,
                "pool_timeout": pool_timeout,
                "pool_recycle": pool_recycle,
                "pool_pre_ping": True  # Проверка соединения перед использованием
            }
        
        engine = create_async_engine(
            database_url,
            echo=getattr(settings, 'DB_ECHO', False),
            poolclass=pool_class,
            **pool_kwargs
        )
        
        return engine
    
    def _get_database_url(self) -> str:
        """Формирует URL базы данных"""
        
        # Пробуем получить готовый URL
        if hasattr(settings, 'DATABASE_URL') and settings.DATABASE_URL:
            url = settings.DATABASE_URL
            # Конвертируем в async версию если нужно
            if url.startswith("postgresql://"):
                url = url.replace("postgresql://", "postgresql+asyncpg://")
            return url
        
        # Собираем из отдельных параметров
        db_host = getattr(settings, 'DB_HOST', 'localhost')
        db_port = getattr(settings, 'DB_PORT', 5432)
        db_name = getattr(settings, 'DB_NAME', 'promptgen')
        db_user = getattr(settings, 'DB_USER', 'postgres')
        db_password = getattr(settings, 'DB_PASSWORD', 'postgres')
        
        return f"postgresql+asyncpg://{db_user}:{db_password}@{db_host}:{db_port}/{db_name}"
    
    async def get_session(self) -> AsyncGenerator[AsyncSession, None]:
        """
        Генератор сессий для dependency injection.
        
        Использование:
            async for session in db_manager.get_session():
                # работа с session
        """
        async with self.session_factory() as session:
            try:
                yield session
                await session.commit()
            except Exception:
                await session.rollback()
                raise
            finally:
                await session.close()
    
    @asynccontextmanager
    async def session_scope(self) -> AsyncGenerator[AsyncSession, None]:
        """
        Контекстный менеджер для сессий.
        
        Использование:
            async with db_manager.session_scope() as session:
                # работа с session
        """
        session = self.session_factory()
        try:
            yield session
            await session.commit()
        except Exception:
            await session.rollback()
            raise
        finally:
            await session.close()
    
    async def init_db(self):
        """Инициализация базы данных (создание таблиц)"""
        from models.database.base import Base
        
        async with self.engine.begin() as conn:
            await conn.run_sync(Base.metadata.create_all)
    
    async def drop_db(self):
        """Удаление всех таблиц (для тестов)"""
        from models.database.base import Base
        
        async with self.engine.begin() as conn:
            await conn.run_sync(Base.metadata.drop_all)
    
    async def health_check(self) -> bool:
        """Проверка подключения к БД"""
        try:
            async with self.session_factory() as session:
                await session.execute(text("SELECT 1"))
                return True
        except Exception as e:
            print(f"Database health check failed: {e}")
            return False
    
    async def close(self):
        """Закрытие всех подключений"""
        if self._engine:
            await self._engine.dispose()
            self._engine = None
            self._session_factory = None


# Глобальный экземпляр
db_manager = DatabaseManager()


# Функция для dependency injection
async def get_db() -> AsyncGenerator[AsyncSession, None]:
    """
    Dependency для получения сессии БД.
    
    Использование в FastAPI:
        @app.get("/items")
        async def get_items(db: AsyncSession = Depends(get_db)):
            ...
    """
    async for session in db_manager.get_session():
        yield session