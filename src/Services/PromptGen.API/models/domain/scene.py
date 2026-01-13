# models/domain/scene.py
"""
Domain model для Scene (Сцена/Локация).
"""

from datetime import datetime
from typing import Optional, Dict, Any, List
from sqlalchemy import Column, String, Text, DateTime, JSON, ForeignKey, Integer
from sqlalchemy.orm import relationship
import uuid

from models.database.base import Base


class Scene(Base):
    """
    Модель сцены/локации.
    
    Scene представляет место действия с его визуальными характеристиками.
    Используется для консистентности фонов и окружения.
    """
    
    __tablename__ = "scenes"
    
    # Первичные ключи
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    story_id = Column(String(36), ForeignKey("stories.id"), nullable=True, index=True)
    user_id = Column(String(36), nullable=False, index=True)
    
    # Основная информация
    name = Column(String(300), nullable=False)  # "Winterfell Castle", "The Dark Forest"
    aliases = Column(JSON, default=list)  # Альтернативные названия
    description = Column(Text, nullable=True)  # Полное описание
    
    # Тип локации
    location_type = Column(String(100), nullable=True)  # interior, exterior, mixed
    setting_type = Column(String(100), nullable=True)  # castle, forest, city, etc.
    
    # Визуальные характеристики
    architecture = Column(Text, nullable=True)  # Архитектурный стиль
    materials = Column(Text, nullable=True)  # Камень, дерево, металл
    colors = Column(JSON, default=list)  # Доминирующие цвета
    textures = Column(Text, nullable=True)  # Текстуры поверхностей
    
    # Атмосфера
    atmosphere = Column(Text, nullable=True)  # Атмосфера места
    default_lighting = Column(String(200), nullable=True)  # Типичное освещение
    default_weather = Column(String(100), nullable=True)  # Типичная погода
    time_period = Column(String(100), nullable=True)  # Эпоха/период
    
    # Детали окружения
    key_elements = Column(JSON, default=list)  # Ключевые элементы ["throne", "fireplace"]
    decorations = Column(Text, nullable=True)  # Декор и украшения
    furniture = Column(Text, nullable=True)  # Мебель (для интерьеров)
    vegetation = Column(Text, nullable=True)  # Растительность (для экстерьеров)
    
    # Размер и масштаб
    scale = Column(String(100), nullable=True)  # "grand", "intimate", "vast"
    
    # Отслеживание появлений
    first_appearance_page = Column(Integer, nullable=True)
    first_appearance_chapter = Column(Integer, nullable=True)
    appearance_count = Column(Integer, default=0)
    
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
    story = relationship("Story", back_populates="scenes")
    
    def __repr__(self):
        return f"<Scene(id={self.id}, name='{self.name}')>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "story_id": self.story_id,
            "name": self.name,
            "aliases": self.aliases,
            "description": self.description,
            "location_type": self.location_type,
            "setting_type": self.setting_type,
            "architecture": self.architecture,
            "materials": self.materials,
            "colors": self.colors,
            "atmosphere": self.atmosphere,
            "default_lighting": self.default_lighting,
            "default_weather": self.default_weather,
            "time_period": self.time_period,
            "key_elements": self.key_elements,
            "decorations": self.decorations,
            "scale": self.scale,
            "first_appearance_page": self.first_appearance_page,
            "appearance_count": self.appearance_count,
            "reference_image_url": self.reference_image_url,
            "created_at": self.created_at.isoformat() if self.created_at else None
        }
    
    def get_visual_prompt(self) -> str:
        """Генерирует описание сцены для промпта"""
        
        parts = []
        
        # Тип и название
        if self.setting_type:
            parts.append(self.setting_type)
        
        # Архитектура и материалы
        if self.architecture:
            parts.append(self.architecture)
        if self.materials:
            parts.append(f"made of {self.materials}")
        
        # Атмосфера
        if self.atmosphere:
            parts.append(self.atmosphere)
        
        # Освещение
        if self.default_lighting:
            parts.append(f"{self.default_lighting} lighting")
        
        # Масштаб
        if self.scale:
            parts.append(f"{self.scale} scale")
        
        # Ключевые элементы
        if self.key_elements:
            elements = ", ".join(self.key_elements[:5])  # Первые 5
            parts.append(f"featuring {elements}")
        
        # Декор
        if self.decorations:
            parts.append(self.decorations)
        
        # Эпоха
        if self.time_period:
            parts.append(f"{self.time_period} era")
        
        if not parts and self.description:
            return self.description
        
        return ", ".join(filter(None, parts)) if parts else self.name
    
    def get_consistency_data(self) -> Dict[str, Any]:
        """Возвращает данные для проверки консистентности"""
        return {
            "name": self.name,
            "architecture": self.architecture,
            "materials": self.materials,
            "colors": self.colors,
            "atmosphere": self.atmosphere,
            "lighting": self.default_lighting,
            "key_elements": self.key_elements,
            "reference_prompt": self.reference_prompt
        }