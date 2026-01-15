# services/storage/database_service.py
"""
Database service for PromptGen.API.

РЕФАКТОРИНГ: Удалены импорты Story - книги теперь в Catalog.API.
"""

from typing import TypeVar, Generic, List, Optional, Dict, Any, Type
from sqlalchemy import select, func, text
from sqlalchemy.ext.asyncio import AsyncSession, AsyncEngine, create_async_engine, async_sessionmaker
from sqlalchemy.orm import selectinload
from contextlib import asynccontextmanager
from asyncio import current_task

from models.database.base import Base
from app.config import settings

T = TypeVar("T", bound=Any)


class DatabaseManager:
    """
    Менеджер подключений к базе данных.
    Поддерживает асинхронные операции с PostgreSQL.
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
            self._engine = create_async_engine(
                settings.DATABASE_URL,
                echo=settings.DEBUG,
                pool_size=5,
                max_overflow=10,
                pool_pre_ping=True,
                pool_recycle=3600,
            )
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
    
    async def get_session(self):
        """
        Dependency для FastAPI - генератор сессий.
        
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
    async def session_scope(self):
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
    """
    Dependency для получения сессии БД.
    
    Использование в FastAPI:
        @app.get("/items")
        async def get_items(db: AsyncSession = Depends(get_database)):
            ...
    """
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
    
    async def get_by_book_id(
        self,
        session: AsyncSession,
        book_id: str,
        skip: int = 0,
        limit: int = 100
    ) -> List[T]:
        """Получить все записи по book_id."""
        result = await session.execute(
            select(self.model)
            .where(self.model.book_id == book_id)
            .offset(skip)
            .limit(limit)
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
    
    async def delete(self, session: AsyncSession, id: str) -> bool:
        """Удалить по ID."""
        obj = await self.get_by_id(session, id)
        if obj:
            await session.delete(obj)
            return True
        return False
    
    async def count(self, session: AsyncSession) -> int:
        """Подсчитать общее количество."""
        result = await session.execute(
            select(func.count(self.model.id))
        )
        return result.scalar() or 0
    
    async def count_by_book_id(self, session: AsyncSession, book_id: str) -> int:
        """Подсчитать количество по book_id."""
        result = await session.execute(
            select(func.count(self.model.id))
            .where(self.model.book_id == book_id)
        )
        return result.scalar() or 0
    
    async def exists(self, session: AsyncSession, id: str) -> bool:
        """Проверить существование записи."""
        result = await session.execute(
            select(func.count(self.model.id)).where(self.model.id == id)
        )
        return (result.scalar() or 0) > 0
    
    async def search(
        self,
        session: AsyncSession,
        search_field: str,
        search_term: str,
        book_id: Optional[str] = None,
        limit: int = 20
    ) -> List[T]:
        """Поиск по текстовому полю."""
        if not hasattr(self.model, search_field):
            return []
        
        field = getattr(self.model, search_field)
        query = select(self.model).where(field.ilike(f"%{search_term}%"))
        
        if book_id and hasattr(self.model, 'book_id'):
            query = query.where(self.model.book_id == book_id)
        
        query = query.limit(limit)
        
        result = await session.execute(query)
        return list(result.scalars().all())


# ===========================================
# Инициализация базы данных
# ===========================================

async def init_database() -> None:
    """
    Инициализирует базу данных при старте приложения.
    Импортирует все модели для регистрации в metadata.
    """
    
    # Импортируем все модели для регистрации
    from models.domain.character import Character
    from models.domain.scene import Scene
    from models.domain.story_object import StoryObject
    from models.domain.prompt_history import PromptHistory
    
    # NOTE: Story и User больше не импортируются!
    
    # Создаём таблицы
    await db_manager.init_db()
    
    print("Database initialized successfully")
    print("Tables created: characters, scenes, story_objects, prompt_history")


async def close_database() -> None:
    """Закрывает подключения при остановке приложения."""
    await db_manager.close()
    print("Database connections closed")