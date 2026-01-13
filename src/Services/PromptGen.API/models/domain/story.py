# models/domain/story.py
"""
Domain model для Story (История/Книга).
"""

from datetime import datetime
from typing import Optional, List, Dict, Any
from sqlalchemy import Column, String, Text, DateTime, JSON, Integer, Boolean
from sqlalchemy.orm import relationship
import uuid

from models.database.base import Base


class Story(Base):
    """
    Модель истории/книги.
    
    Story представляет книгу или историю, для которой генерируются
    визуализации. Содержит метаданные и настройки генерации.
    """
    
    __tablename__ = "stories"
    
    # Первичные ключи
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    user_id = Column(String(36), nullable=False, index=True)
    
    # Внешние связи
    external_book_id = Column(String(36), nullable=True, index=True)  # ID из Catalog.API
    
    # Основная информация
    title = Column(String(500), nullable=False)
    author = Column(String(300), nullable=True)
    description = Column(Text, nullable=True)
    genre = Column(String(100), nullable=True)
    language = Column(String(10), default="en")
    
    # Настройки визуализации
    default_style = Column(String(100), nullable=True)  # Стиль по умолчанию
    default_model = Column(String(50), default="midjourney")  # AI модель по умолчанию
    visualization_mode = Column(String(50), default="per_page")  # per_page, per_chapter, selected
    
    # Настройки генерации (JSON)
    generation_settings = Column(JSON, default=dict)
    # Пример:
    # {
    #     "aspect_ratio": "16:9",
    #     "quality": "high",
    #     "maintain_consistency": true,
    #     "auto_detect_scenes": true
    # }
    
    # Метаданные (JSON)
    metadata = Column(JSON, default=dict)
    # Пример:
    # {
    #     "cover_image_url": "...",
    #     "isbn": "...",
    #     "publication_year": 2024
    # }
    
    # Статистика
    total_pages = Column(Integer, default=0)
    total_chapters = Column(Integer, default=0)
    generated_images_count = Column(Integer, default=0)
    
    # Статус
    is_active = Column(Boolean, default=True)
    is_processing = Column(Boolean, default=False)
    
    # Временные метки
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    last_generated_at = Column(DateTime, nullable=True)
    
    # Связи с другими моделями
    characters = relationship(
        "Character", 
        back_populates="story", 
        cascade="all, delete-orphan",
        lazy="dynamic"
    )
    scenes = relationship(
        "Scene", 
        back_populates="story", 
        cascade="all, delete-orphan",
        lazy="dynamic"
    )
    objects = relationship(
        "StoryObject", 
        back_populates="story", 
        cascade="all, delete-orphan",
        lazy="dynamic"
    )
    prompt_history = relationship(
        "PromptHistory", 
        back_populates="story", 
        cascade="all, delete-orphan",
        lazy="dynamic"
    )
    
    def __repr__(self):
        return f"<Story(id={self.id}, title='{self.title[:30]}...')>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "user_id": self.user_id,
            "external_book_id": self.external_book_id,
            "title": self.title,
            "author": self.author,
            "description": self.description,
            "genre": self.genre,
            "language": self.language,
            "default_style": self.default_style,
            "default_model": self.default_model,
            "visualization_mode": self.visualization_mode,
            "generation_settings": self.generation_settings,
            "metadata": self.metadata,
            "total_pages": self.total_pages,
            "total_chapters": self.total_chapters,
            "generated_images_count": self.generated_images_count,
            "is_active": self.is_active,
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None
        }
    
    def get_settings(self, key: str, default: Any = None) -> Any:
        """Получает настройку генерации"""
        return self.generation_settings.get(key, default) if self.generation_settings else default
    
    def update_settings(self, **kwargs):
        """Обновляет настройки генерации"""
        if self.generation_settings is None:
            self.generation_settings = {}
        self.generation_settings.update(kwargs)