# services/storage/database_service.py
"""
Сервис для работы с базой данных.
"""

from typing import Optional, List, Type, TypeVar, Generic, Dict, Any
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, update, delete, func, and_, or_
from sqlalchemy.orm import selectinload

from models.database.base import Base
from models.database.session import db_manager


T = TypeVar('T', bound=Base)


class DatabaseService(Generic[T]):
    """
    Универсальный сервис для работы с моделями.
    """
    
    def __init__(self, model: Type[T]):
        self.model = model
    
    async def get_by_id(
        self, 
        session: AsyncSession, 
        id: str,
        load_relations: Optional[List[str]] = None
    ) -> Optional[T]:
        """Получает запись по ID"""
        
        query = select(self.model).where(self.model.id == id)
        
        # Загружаем связи если указаны
        if load_relations:
            for relation in load_relations:
                if hasattr(self.model, relation):
                    query = query.options(selectinload(getattr(self.model, relation)))
        
        result = await session.execute(query)
        return result.scalar_one_or_none()
    
    async def get_all(
        self,
        session: AsyncSession,
        skip: int = 0,
        limit: int = 100,
        filters: Optional[Dict[str, Any]] = None,
        order_by: Optional[str] = None,
        descending: bool = False
    ) -> List[T]:
        """Получает список записей с пагинацией и фильтрацией"""
        
        query = select(self.model)
        
        # Применяем фильтры
        if filters:
            conditions = []
            for key, value in filters.items():
                if hasattr(self.model, key):
                    conditions.append(getattr(self.model, key) == value)
            if conditions:
                query = query.where(and_(*conditions))
        
        # Сортировка
        if order_by and hasattr(self.model, order_by):
            order_column = getattr(self.model, order_by)
            if descending:
                order_column = order_column.desc()
            query = query.order_by(order_column)
        
        # Пагинация
        query = query.offset(skip).limit(limit)
        
        result = await session.execute(query)
        return list(result.scalars().all())
    
    async def get_by_user(
        self,
        session: AsyncSession,
        user_id: str,
        skip: int = 0,
        limit: int = 100
    ) -> List[T]:
        """Получает записи пользователя"""
        
        if not hasattr(self.model, 'user_id'):
            return []
        
        query = (
            select(self.model)
            .where(self.model.user_id == user_id)
            .offset(skip)
            .limit(limit)
        )
        
        result = await session.execute(query)
        return list(result.scalars().all())
    
    async def get_by_story(
        self,
        session: AsyncSession,
        story_id: str,
        skip: int = 0,
        limit: int = 100
    ) -> List[T]:
        """Получает записи для истории"""
        
        if not hasattr(self.model, 'story_id'):
            return []
        
        query = (
            select(self.model)
            .where(self.model.story_id == story_id)
            .offset(skip)
            .limit(limit)
        )
        
        result = await session.execute(query)
        return list(result.scalars().all())
    
    async def create(self, session: AsyncSession, **kwargs: Any) -> T:
        """Создаёт новую запись"""
        
        instance = self.model(**kwargs)
        session.add(instance)
        await session.flush()
        await session.refresh(instance)
        return instance
    
    async def update(
        self,
        session: AsyncSession,
        id: str,
        **kwargs: Any
    ) -> Optional[T]:
        """Обновляет запись"""
        
        instance = await self.get_by_id(session, id)
        if not instance:
            return None
        
        for key, value in kwargs.items():
            if hasattr(instance, key):
                setattr(instance, key, value)
        
        await session.flush()
        await session.refresh(instance)
        return instance
    
    async def delete(self, session: AsyncSession, id: str) -> bool:
        """Удаляет запись"""
        
        query = delete(self.model).where(self.model.id == id)
        result = await session.execute(query)
        return result.rowcount > 0
    
    async def count(
        self,
        session: AsyncSession,
        filters: Optional[Dict[str, Any]] = None
    ) -> int:
        """Считает количество записей"""
        
        query = select(func.count(self.model.id))
        
        if filters:
            conditions = []
            for key, value in filters.items():
                if hasattr(self.model, key):
                    conditions.append(getattr(self.model, key) == value)
            if conditions:
                query = query.where(and_(*conditions))
        
        result = await session.execute(query)
        return result.scalar() or 0
    
    async def exists(self, session: AsyncSession, id: str) -> bool:
        """Проверяет существование записи"""
        
        query = select(func.count(self.model.id)).where(self.model.id == id)
        result = await session.execute(query)
        return (result.scalar() or 0) > 0
    
    async def search(
        self,
        session: AsyncSession,
        search_field: str,
        search_term: str,
        limit: int = 20
    ) -> List[T]:
        """Поиск по текстовому полю"""
        
        if not hasattr(self.model, search_field):
            return []
        
        field = getattr(self.model, search_field)
        query = (
            select(self.model)
            .where(field.ilike(f"%{search_term}%"))
            .limit(limit)
        )
        
        result = await session.execute(query)
        return list(result.scalars().all())


# Инициализация базы данных
async def init_database() -> None:
    """Инициализирует базу данных при старте приложения"""
    
    # Импортируем все модели для регистрации
    from models.domain.story import Story
    from models.domain.character import Character
    from models.domain.scene import Scene
    from models.domain.object import StoryObject
    from models.domain.prompt_history import PromptHistory
    
    # Создаём таблицы
    await db_manager.init_db()
    
    print("Database initialized successfully")


async def close_database() -> None:
    """Закрывает подключения при остановке приложения"""
    await db_manager.close()
    print("Database connections closed")