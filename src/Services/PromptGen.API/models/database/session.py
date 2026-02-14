# models/database/session.py
"""
Асинхронная сессия SQLAlchemy и управление подключениями.
Поддержка: SQLite, PostgreSQL, SQL Server
"""

from typing import AsyncGenerator, Optional
from contextlib import asynccontextmanager
import os

from sqlalchemy.ext.asyncio import (
    AsyncSession, 
    AsyncEngine,
    create_async_engine,
    async_sessionmaker
)
from sqlalchemy.pool import NullPool, QueuePool, StaticPool
from sqlalchemy import text, event

from app.config import settings


class DatabaseManager:
    """
    Менеджер базы данных.
    
    Управляет подключениями, сессиями и пулом соединений.
    Поддерживает SQLite, PostgreSQL и SQL Server.
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
        """Создаёт асинхронный engine с учётом типа БД"""
        
        database_url = self._get_database_url()
        is_sqlite = database_url.startswith("sqlite")
        is_testing = getattr(settings, 'TESTING', False)
        
        # Параметры зависят от типа БД
        if is_sqlite:
            # SQLite: используем StaticPool для async
            engine = create_async_engine(
                database_url,
                echo=getattr(settings, 'DB_ECHO', False),
                poolclass=StaticPool,
                connect_args={"check_same_thread": False}
            )
            
            # Включаем foreign keys для SQLite
            @event.listens_for(engine.sync_engine, "connect")
            def set_sqlite_pragma(dbapi_connection, connection_record):
                cursor = dbapi_connection.cursor()
                cursor.execute("PRAGMA foreign_keys=ON")
                cursor.close()
                
        elif is_testing:
            # Тестовый режим: NullPool
            engine = create_async_engine(
                database_url,
                echo=getattr(settings, 'DB_ECHO', False),
                poolclass=NullPool
            )
        else:
            # PostgreSQL/SQL Server: QueuePool с настройками
            pool_size = getattr(settings, 'DB_POOL_SIZE', 5)
            max_overflow = getattr(settings, 'DB_MAX_OVERFLOW', 10)
            pool_timeout = getattr(settings, 'DB_POOL_TIMEOUT', 30)
            pool_recycle = getattr(settings, 'DB_POOL_RECYCLE', 1800)
            
            engine = create_async_engine(
                database_url,
                echo=getattr(settings, 'DB_ECHO', False),
                poolclass=QueuePool,
                pool_size=pool_size,
                max_overflow=max_overflow,
                pool_timeout=pool_timeout,
                pool_recycle=pool_recycle,
                pool_pre_ping=True
            )
        
        return engine
    
    def _get_database_url(self) -> str:
        """Формирует URL базы данных"""
        
        # 1. Пробуем получить готовый URL из настроек
        if hasattr(settings, 'DATABASE_URL') and settings.DATABASE_URL:
            url = settings.DATABASE_URL
            
            # Конвертируем sync URL в async если нужно
            if url.startswith("postgresql://"):
                url = url.replace("postgresql://", "postgresql+asyncpg://")
            elif url.startswith("sqlite:///"):
                url = url.replace("sqlite:///", "sqlite+aiosqlite:///")
            
            return url
        
        # 2. Проверяем тип БД из настроек
        db_type = getattr(settings, 'DB_TYPE', 'sqlite').lower()
        
        if db_type == 'sqlite':
            # SQLite - файл в папке data
            db_path = getattr(settings, 'DB_PATH', './data/promptgen.db')
            # Создаём папку если не существует
            os.makedirs(os.path.dirname(db_path), exist_ok=True)
            return f"sqlite+aiosqlite:///{db_path}"
        
        elif db_type == 'postgresql':
            # PostgreSQL
            db_host = getattr(settings, 'DB_HOST', 'localhost')
            db_port = getattr(settings, 'DB_PORT', 5432)
            db_name = getattr(settings, 'DB_NAME', 'promptgen')
            db_user = getattr(settings, 'DB_USER', 'postgres')
            db_password = getattr(settings, 'DB_PASSWORD', 'postgres')
            return f"postgresql+asyncpg://{db_user}:{db_password}@{db_host}:{db_port}/{db_name}"
        
        elif db_type == 'mssql':
            # SQL Server
            db_host = getattr(settings, 'DB_HOST', 'localhost')
            db_name = getattr(settings, 'DB_NAME', 'PromptGenDb')
            db_user = getattr(settings, 'DB_USER', 'sa')
            db_password = getattr(settings, 'DB_PASSWORD', '')
            driver = getattr(settings, 'DB_DRIVER', 'ODBC+Driver+17+for+SQL+Server')
            return f"mssql+aioodbc://{db_user}:{db_password}@{db_host}/{db_name}?driver={driver}"
        
        else:
            # По умолчанию SQLite
            return "sqlite+aiosqlite:///./data/promptgen.db"
    
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