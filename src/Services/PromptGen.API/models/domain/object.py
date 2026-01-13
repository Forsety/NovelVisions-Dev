# models/domain/object.py
"""
Domain model для StoryObject (Объект в истории).
"""

from datetime import datetime
from typing import Optional, Dict, Any, List
from sqlalchemy import Column, String, Text, DateTime, JSON, ForeignKey, Integer
from sqlalchemy.orm import relationship
import uuid

from models.database.base import Base


class StoryObject(Base):
    """
    Модель объекта в истории.
    
    StoryObject представляет значимый объект (артефакт, оружие, предмет),
    который появляется в истории и должен выглядеть консистентно.
    """
    
    __tablename__ = "story_objects"
    
    # Первичные ключи
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    story_id = Column(String(36), ForeignKey("stories.id"), nullable=True, index=True)
    user_id = Column(String(36), nullable=False, index=True)
    
    # Основная информация
    name = Column(String(300), nullable=False)
    aliases = Column(JSON, default=list)
    description = Column(Text, nullable=True)
    
    # Категория объекта
    category = Column(String(100), nullable=True)  # weapon, artifact, vehicle, etc.
    subcategory = Column(String(100), nullable=True)  # sword, ring, car, etc.
    
    # Визуальные характеристики
    appearance = Column(Text, nullable=True)  # Полное описание внешнего вида
    materials = Column(Text, nullable=True)  # Из чего сделан
    colors = Column(JSON, default=list)  # Цвета
    size = Column(String(100), nullable=True)  # Размер
    shape = Column(String(100), nullable=True)  # Форма
    
    # Детали
    details = Column(Text, nullable=True)  # Гравировки, украшения
    markings = Column(Text, nullable=True)  # Символы, надписи
    condition = Column(String(100), nullable=True)  # Состояние: new, worn, ancient
    
    # Особые свойства
    special_properties = Column(JSON, default=list)  # ["glowing", "magical aura"]
    effects = Column(Text, nullable=True)  # Визуальные эффекты
    
    # Владелец (если есть)
    owner_character_id = Column(String(36), nullable=True)
    
    # Отслеживание
    first_appearance_page = Column(Integer, nullable=True)
    first_appearance_chapter = Column(Integer, nullable=True)
    appearance_count = Column(Integer, default=0)
    importance = Column(Integer, default=5)  # 1-10
    
    # Референсы
    reference_image_url = Column(String(500), nullable=True)
    reference_prompt = Column(Text, nullable=True)
    
    # Эмбеддинг
    embedding_vector = Column(JSON, nullable=True)
    
    # Дополнительные атрибуты
    attributes = Column(JSON, default=dict)
    
    # Временные метки
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Связи
    story = relationship("Story", back_populates="objects")
    
    def __repr__(self):
        return f"<StoryObject(id={self.id}, name='{self.name}')>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "story_id": self.story_id,
            "name": self.name,
            "aliases": self.aliases,
            "description": self.description,
            "category": self.category,
            "subcategory": self.subcategory,
            "appearance": self.appearance,
            "materials": self.materials,
            "colors": self.colors,
            "size": self.size,
            "shape": self.shape,
            "details": self.details,
            "markings": self.markings,
            "condition": self.condition,
            "special_properties": self.special_properties,
            "effects": self.effects,
            "owner_character_id": self.owner_character_id,
            "first_appearance_page": self.first_appearance_page,
            "appearance_count": self.appearance_count,
            "importance": self.importance,
            "reference_image_url": self.reference_image_url,
            "created_at": self.created_at.isoformat() if self.created_at else None
        }
    
    def get_visual_prompt(self) -> str:
        """Генерирует описание объекта для промпта"""
        
        parts = []
        
        # Тип
        if self.subcategory:
            parts.append(self.subcategory)
        elif self.category:
            parts.append(self.category)
        
        # Материалы
        if self.materials:
            parts.append(f"made of {self.materials}")
        
        # Размер и форма
        if self.size:
            parts.append(self.size)
        if self.shape:
            parts.append(f"{self.shape} shaped")
        
        # Детали
        if self.details:
            parts.append(f"with {self.details}")
        if self.markings:
            parts.append(f"bearing {self.markings}")
        
        # Состояние
        if self.condition:
            parts.append(f"{self.condition} condition")
        
        # Особые свойства
        if self.special_properties:
            props = ", ".join(self.special_properties[:3])
            parts.append(props)
        
        # Эффекты
        if self.effects:
            parts.append(self.effects)
        
        if not parts and self.appearance:
            return self.appearance
        
        if not parts and self.description:
            return self.description
        
        return ", ".join(filter(None, parts)) if parts else self.name