# models/database/base.py
"""
SQLAlchemy Base class for all models.
"""

from sqlalchemy.orm import DeclarativeBase, declared_attr
from sqlalchemy import Column, DateTime
from datetime import datetime


class Base(DeclarativeBase):
    """
    Базовый класс для всех моделей SQLAlchemy.
    """
    
    @declared_attr.directive
    def __tablename__(cls) -> str:
        """Автоматическое имя таблицы из имени класса"""
        return cls.__name__.lower() + "s"
    
    # Общие поля для всех моделей
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)