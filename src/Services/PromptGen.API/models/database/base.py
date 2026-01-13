# models/database/base.py
"""
SQLAlchemy Base и общие миксины для всех моделей.
"""

from datetime import datetime
from typing import Any, Dict
from sqlalchemy import Column, DateTime, event
from sqlalchemy.ext.declarative import declarative_base, declared_attr
from sqlalchemy.orm import Session


class CustomBase:
    """
    Базовый класс с общей функциональностью для всех моделей.
    """
    
    @declared_attr
    def __tablename__(cls) -> str:
        """Автоматическое имя таблицы из имени класса"""
        # CamelCase -> snake_case
        name = cls.__name__
        return ''.join(
            ['_' + c.lower() if c.isupper() else c for c in name]
        ).lstrip('_')
    
    def to_dict(self) -> Dict[str, Any]:
        """Базовая конвертация в словарь"""
        result = {}
        for column in self.__table__.columns:
            value = getattr(self, column.name)
            if isinstance(value, datetime):
                value = value.isoformat()
            result[column.name] = value
        return result
    
    def update_from_dict(self, data: Dict[str, Any]):
        """Обновление из словаря"""
        for key, value in data.items():
            if hasattr(self, key):
                setattr(self, key, value)
    
    def __repr__(self) -> str:
        """Строковое представление"""
        class_name = self.__class__.__name__
        if hasattr(self, 'id'):
            return f"<{class_name}(id={self.id})>"
        return f"<{class_name}>"


# Создаём Base
Base = declarative_base(cls=CustomBase)


class TimestampMixin:
    """Миксин для временных меток"""
    
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)


class SoftDeleteMixin:
    """Миксин для мягкого удаления"""
    
    deleted_at = Column(DateTime, nullable=True)
    is_deleted = Column(DateTime, default=False)
    
    def soft_delete(self):
        """Мягкое удаление"""
        self.deleted_at = datetime.utcnow()
        self.is_deleted = True
    
    def restore(self):
        """Восстановление"""
        self.deleted_at = None
        self.is_deleted = False


# Событие для автоматического обновления updated_at
@event.listens_for(Session, "before_flush")
def before_flush(session, flush_context, instances):
    """Автоматическое обновление updated_at перед сохранением"""
    for instance in session.dirty:
        if hasattr(instance, 'updated_at'):
            instance.updated_at = datetime.utcnow()