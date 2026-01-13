# models/domain/character.py
"""
Domain model для Character (Персонаж).
"""

from datetime import datetime
from typing import Optional, Dict, Any, List
from sqlalchemy import Column, String, Text, DateTime, JSON, ForeignKey, Integer, Float
from sqlalchemy.orm import relationship
import uuid

from models.database.base import Base


class Character(Base):
    """
    Модель персонажа.
    
    Character хранит все визуальные характеристики персонажа
    для обеспечения консистентности при генерации изображений.
    """
    
    __tablename__ = "characters"
    
    # Первичные ключи
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    story_id = Column(String(36), ForeignKey("stories.id"), nullable=True, index=True)
    user_id = Column(String(36), nullable=False, index=True)
    
    # Основная информация
    name = Column(String(200), nullable=False)
    aliases = Column(JSON, default=list)  # Альтернативные имена ["Jon", "Lord Snow"]
    description = Column(Text, nullable=True)  # Общее описание
    
    # Визуальные характеристики - физические
    gender = Column(String(50), nullable=True)
    age = Column(String(50), nullable=True)  # "young adult", "elderly", "30s"
    height = Column(String(50), nullable=True)  # "tall", "average", "short"
    build = Column(String(100), nullable=True)  # "athletic", "slim", "muscular"
    
    # Визуальные характеристики - лицо
    face_shape = Column(String(100), nullable=True)
    skin = Column(String(100), nullable=True)  # Тон кожи
    hair = Column(String(200), nullable=True)  # Цвет и стиль волос
    eyes = Column(String(100), nullable=True)  # Цвет глаз
    facial_features = Column(Text, nullable=True)  # Особенности лица
    
    # Внешность - полное описание
    appearance = Column(Text, nullable=True)  # Полное описание внешности для промпта
    
    # Одежда и стиль
    default_clothing = Column(Text, nullable=True)
    accessories = Column(Text, nullable=True)
    distinguishing_features = Column(Text, nullable=True)  # Шрамы, татуировки
    
    # Характер (влияет на выражения и позы)
    personality = Column(Text, nullable=True)
    typical_expressions = Column(JSON, default=list)  # ["stern", "thoughtful"]
    typical_poses = Column(JSON, default=list)  # ["standing tall", "arms crossed"]
    
    # Роль и важность
    role = Column(String(100), nullable=True)  # protagonist, antagonist, supporting
    importance = Column(Integer, default=5)  # 1-10, влияет на детализацию
    
    # Отслеживание появлений
    first_appearance_page = Column(Integer, nullable=True)
    first_appearance_chapter = Column(Integer, nullable=True)
    appearance_count = Column(Integer, default=0)
    
    # Референсы для консистентности
    reference_image_url = Column(String(500), nullable=True)  # URL референса
    reference_prompt = Column(Text, nullable=True)  # Промпт который дал хороший результат
    seed_value = Column(Integer, nullable=True)  # Seed для воспроизводимости
    
    # Эмбеддинг для поиска
    embedding_vector = Column(JSON, nullable=True)  # Вектор для консистентности
    
    # Дополнительные атрибуты
    attributes = Column(JSON, default=dict)
    
    # Временные метки
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Связи
    story = relationship("Story", back_populates="characters")
    
    def __repr__(self):
        return f"<Character(id={self.id}, name='{self.name}')>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "story_id": self.story_id,
            "name": self.name,
            "aliases": self.aliases,
            "description": self.description,
            "gender": self.gender,
            "age": self.age,
            "height": self.height,
            "build": self.build,
            "face_shape": self.face_shape,
            "skin": self.skin,
            "hair": self.hair,
            "eyes": self.eyes,
            "facial_features": self.facial_features,
            "appearance": self.appearance,
            "default_clothing": self.default_clothing,
            "accessories": self.accessories,
            "distinguishing_features": self.distinguishing_features,
            "personality": self.personality,
            "typical_expressions": self.typical_expressions,
            "typical_poses": self.typical_poses,
            "role": self.role,
            "importance": self.importance,
            "first_appearance_page": self.first_appearance_page,
            "appearance_count": self.appearance_count,
            "reference_image_url": self.reference_image_url,
            "created_at": self.created_at.isoformat() if self.created_at else None
        }
    
    def get_visual_prompt(self, include_clothing: bool = True) -> str:
        """
        Генерирует описание персонажа для промпта.
        
        Args:
            include_clothing: Включать ли одежду
            
        Returns:
            Строка описания для промпта
        """
        parts = []
        
        # Базовые характеристики
        if self.gender:
            parts.append(self.gender)
        if self.age:
            parts.append(self.age)
        if self.build:
            parts.append(self.build)
        
        # Внешность
        if self.hair:
            parts.append(f"{self.hair} hair")
        if self.eyes:
            parts.append(f"{self.eyes} eyes")
        if self.skin:
            parts.append(f"{self.skin} skin")
        
        # Особенности
        if self.facial_features:
            parts.append(self.facial_features)
        if self.distinguishing_features:
            parts.append(self.distinguishing_features)
        
        # Одежда
        if include_clothing and self.default_clothing:
            parts.append(f"wearing {self.default_clothing}")
        if self.accessories:
            parts.append(self.accessories)
        
        # Если есть готовое полное описание - используем его
        if not parts and self.appearance:
            return self.appearance
        
        if not parts and self.description:
            return self.description
        
        return ", ".join(filter(None, parts)) if parts else self.name
    
    def get_consistency_data(self) -> Dict[str, Any]:
        """Возвращает данные для проверки консистентности"""
        return {
            "name": self.name,
            "hair": self.hair,
            "eyes": self.eyes,
            "skin": self.skin,
            "build": self.build,
            "distinguishing_features": self.distinguishing_features,
            "reference_prompt": self.reference_prompt,
            "seed": self.seed_value
        }
    
    def increment_appearance(self):
        """Увеличивает счётчик появлений"""
        self.appearance_count = (self.appearance_count or 0) + 1
        self.updated_at = datetime.utcnow()