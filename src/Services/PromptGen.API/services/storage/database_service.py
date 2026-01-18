# services/storage/database_service.py
"""
Database service for PromptGen.API.
Поддержка SQLite, PostgreSQL, SQL Server.
"""

from typing import TypeVar, Generic, List, Optional, Type, Any
from sqlalchemy import select, func, text
from sqlalchemy.ext.asyncio import AsyncSession, AsyncEngine, create_async_engine, async_sessionmaker
from sqlalchemy.orm import selectinload
from sqlalchemy.pool import StaticPool, NullPool, QueuePool
from contextlib import asynccontextmanager
import os

from models.database.base import Base
from app.config import settings

T = TypeVar("T", bound=Any)


class DatabaseManager:
    """
    Менеджер подключений к базе данных.
    Поддерживает SQLite, PostgreSQL, SQL Server.
    """
    
    _instance: Optional['DatabaseManager'] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
            cls._instance._initialized = False
        return cls._instance
    
    def __init__(self):
        if self._initialized:
            return
            
        self._engine: Optional[AsyncEngine] = None
        self._session_factory: Optional[async_sessionmaker] = None
        self._initialized = True
    
    @property
    def engine(self) -> AsyncEngine:
        """Возвращает engine, создавая при необходимости."""
        if self._engine is None:
            self._engine = self._create_engine()
        return self._engine
    
    @property
    def session_factory(self) -> async_sessionmaker:
        """Возвращает фабрику сессий."""
        if self._session_factory is None:
            self._session_factory = async_sessionmaker(
                bind=self.engine,
                class_=AsyncSession,
                expire_on_commit=False,
                autocommit=False,
                autoflush=False,
            )
        return self._session_factory
    
    def _create_engine(self) -> AsyncEngine:
        """Создаёт engine с учётом типа БД"""
        
        database_url = self._get_database_url()
        is_sqlite = "sqlite" in database_url
        
        if is_sqlite:
            # SQLite: StaticPool для async, check_same_thread=False
            return create_async_engine(
                database_url,
                echo=settings.DEBUG,
                poolclass=StaticPool,
                connect_args={"check_same_thread": False}
            )
        else:
            # PostgreSQL / SQL Server
            return create_async_engine(
                database_url,
                echo=settings.DEBUG,
                pool_size=settings.DB_POOL_SIZE,
                max_overflow=settings.DB_MAX_OVERFLOW,
                pool_pre_ping=True,
                pool_recycle=settings.DB_POOL_RECYCLE,
            )
    
    def _get_database_url(self) -> str:
        """Формирует URL базы данных"""
        
        # Используем готовый URL если есть
        if settings.DATABASE_URL:
            url = settings.DATABASE_URL
            
            # Конвертируем в async версию
            if url.startswith("postgresql://"):
                return url.replace("postgresql://", "postgresql+asyncpg://")
            elif url.startswith("sqlite:///"):
                return url.replace("sqlite:///", "sqlite+aiosqlite:///")
            
            return url
        
        # Собираем из параметров
        db_type = getattr(settings, 'DB_TYPE', 'sqlite').lower()
        
        if db_type == 'sqlite':
            db_path = getattr(settings, 'DB_PATH', './data/promptgen.db')
            os.makedirs(os.path.dirname(db_path) or '.', exist_ok=True)
            return f"sqlite+aiosqlite:///{db_path}"
        
        elif db_type == 'postgresql':
            return f"postgresql+asyncpg://{settings.DB_USER}:{settings.DB_PASSWORD}@{settings.DB_HOST}:{settings.DB_PORT}/{settings.DB_NAME}"
        
        elif db_type == 'mssql':
            driver = getattr(settings, 'DB_DRIVER', 'ODBC+Driver+17+for+SQL+Server')
            return f"mssql+aioodbc://{settings.DB_USER}:{settings.DB_PASSWORD}@{settings.DB_HOST}/{settings.DB_NAME}?driver={driver}"
        
        # Default: SQLite
        return "sqlite+aiosqlite:///./data/promptgen.db"
    
    async def get_session(self):
        """Dependency для FastAPI - генератор сессий."""
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
    async def session_scope(self):
        """Контекстный менеджер для сессий."""
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
        """Инициализация базы данных (создание таблиц)."""
        async with self.engine.begin() as conn:
            await conn.run_sync(Base.metadata.create_all)
    
    async def drop_db(self):
        """Удаление всех таблиц (для тестов)."""
        async with self.engine.begin() as conn:
            await conn.run_sync(Base.metadata.drop_all)
    
    async def health_check(self) -> bool:
        """Проверка подключения к БД."""
        try:
            async with self.session_factory() as session:
                await session.execute(text("SELECT 1"))
                return True
        except Exception as e:
            print(f"Database health check failed: {e}")
            return False
    
    async def close(self):
        """Закрытие всех подключений."""
        if self._engine:
            await self._engine.dispose()
            self._engine = None
            self._session_factory = None


# Глобальный экземпляр
db_manager = DatabaseManager()


# Dependency для FastAPI
async def get_database():
    """Dependency для получения сессии БД."""
    async for session in db_manager.get_session():
        yield session


class BaseRepository(Generic[T]):
    """
    Базовый репозиторий для CRUD операций.
    """
    
    def __init__(self, model: Type[T]):
        self.model = model
    
    async def get_by_id(self, session: AsyncSession, id: str) -> Optional[T]:
        """Получить по ID."""
        result = await session.execute(
            select(self.model).where(self.model.id == id)
        )
        return result.scalar_one_or_none()
    
    async def get_all(
        self,
        session: AsyncSession,
        skip: int = 0,
        limit: int = 100
    ) -> List[T]:
        """Получить список с пагинацией."""
        result = await session.execute(
            select(self.model).offset(skip).limit(limit)
        )
        return list(result.scalars().all())
    
    async def create(self, session: AsyncSession, obj: T) -> T:
        """Создать запись."""
        session.add(obj)
        await session.flush()
        await session.refresh(obj)
        return obj
    
    async def update(self, session: AsyncSession, obj: T) -> T:
        """Обновить запись."""
        await session.flush()
        await session.refresh(obj)
        return obj
    
    async def delete(self, session: AsyncSession, obj: T) -> None:
        """Удалить запись."""
        await session.delete(obj)
        await session.flush()
    
    async def count(self, session: AsyncSession) -> int:
        """Подсчёт записей."""
        result = await session.execute(
            select(func.count()).select_from(self.model)
        )
        return result.scalar_one()