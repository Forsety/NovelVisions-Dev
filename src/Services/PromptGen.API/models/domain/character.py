# models/domain/character.py
"""
Domain model для Character (Персонаж).
РЕФАКТОРИНГ: story_id → book_id (внешний ID из Catalog.API, НЕ FK)
"""

from datetime import datetime
from typing import Optional, Dict, Any, List
from sqlalchemy import Column, String, Text, DateTime, JSON, Integer, Float, Index
from sqlalchemy.orm import relationship
import uuid

from models.database.base import Base


class Character(Base):
    """
    Модель персонажа для обеспечения консистентности при генерации изображений.
    
    Character хранит все визуальные характеристики персонажа.
    Привязан к book_id - внешнему ID книги из Catalog.API.
    """
    
    __tablename__ = "characters"
    
    # ===========================================
    # PRIMARY KEYS & EXTERNAL REFERENCES
    # ===========================================
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    
    # book_id - внешний ID из Catalog.API (НЕ FK, просто GUID для группировки)
    book_id = Column(String(36), nullable=False, index=True)
    
    # ===========================================
    # BASIC INFORMATION
    # ===========================================
    
    name = Column(String(200), nullable=False)
    aliases = Column(JSON, default=list)  # Альтернативные имена ["Jon", "Lord Snow"]
    description = Column(Text, nullable=True)  # Общее описание
    role = Column(String(100), nullable=True)  # protagonist, antagonist, supporting, etc.
    
    # ===========================================
    # PHYSICAL CHARACTERISTICS
    # ===========================================
    
    gender = Column(String(50), nullable=True)
    age = Column(String(50), nullable=True)  # "young adult", "elderly", "30s"
    height = Column(String(50), nullable=True)  # "tall", "average", "short"
    build = Column(String(100), nullable=True)  # "athletic", "slim", "muscular"
    ethnicity = Column(String(100), nullable=True)
    
    # ===========================================
    # FACIAL FEATURES
    # ===========================================
    
    face_shape = Column(String(100), nullable=True)
    skin = Column(String(100), nullable=True)  # Тон кожи
    hair = Column(String(200), nullable=True)  # Цвет и стиль волос
    eyes = Column(String(100), nullable=True)  # Цвет глаз
    facial_features = Column(Text, nullable=True)  # Особенности лица (шрамы, родинки)
    
    # ===========================================
    # FULL APPEARANCE (для промпта)
    # ===========================================
    
    appearance = Column(Text, nullable=True)  # Полное описание внешности для промпта
    
    # ===========================================
    # CLOTHING & STYLE
    # ===========================================
    
    default_clothing = Column(Text, nullable=True)  # Типичная одежда
    accessories = Column(Text, nullable=True)  # Аксессуары (украшения, очки)
    distinguishing_features = Column(Text, nullable=True)  # Особые приметы
    
    # ===========================================
    # TRACKING & ANALYTICS
    # ===========================================
    
    first_appearance_page = Column(Integer, nullable=True)
    first_appearance_chapter = Column(Integer, nullable=True)
    appearance_count = Column(Integer, default=0)  # Сколько раз появлялся
    importance = Column(Integer, default=5)  # 1-10, важность персонажа
    
    # ===========================================
    # REFERENCE & CONSISTENCY
    # ===========================================
    
    reference_image_url = Column(String(500), nullable=True)  # URL референса
    reference_prompt = Column(Text, nullable=True)  # Эталонный промпт
    base_prompt = Column(Text, nullable=True)  # Базовый промпт для всех генераций
    
    # ===========================================
    # EMBEDDINGS (для similarity search)
    # ===========================================
    
    embedding_vector = Column(JSON, nullable=True)  # Vector embedding для поиска
    
    # ===========================================
    # CONSISTENCY FLAGS
    # ===========================================
    
    is_established = Column(String(1), default='0')  # '1' если описание зафиксировано
    generation_count = Column(Integer, default=0)  # Сколько раз генерировались изображения
    
    # ===========================================
    # ADDITIONAL ATTRIBUTES (flexible JSON)
    # ===========================================
    
    attributes = Column(JSON, default=dict)
    # Пример:
    # {
    #     "personality_traits": ["brave", "kind"],
    #     "voice": "deep and commanding",
    #     "mannerisms": "often crosses arms",
    #     "relationships": {"Harry": "friend", "Voldemort": "enemy"}
    # }
    
    # ===========================================
    # TIMESTAMPS
    # ===========================================
    
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, onupdate=datetime.utcnow)
    
    # ===========================================
    # INDEXES
    # ===========================================
    
    __table_args__ = (
        Index('ix_characters_book_id_name', 'book_id', 'name'),
        Index('ix_characters_importance', 'importance'),
    )
    
    # ===========================================
    # METHODS
    # ===========================================
    
    def __repr__(self):
        return f"<Character(id={self.id}, name={self.name}, book_id={self.book_id})>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "book_id": self.book_id,
            "name": self.name,
            "aliases": self.aliases or [],
            "description": self.description,
            "role": self.role,
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
            "first_appearance_page": self.first_appearance_page,
            "first_appearance_chapter": self.first_appearance_chapter,
            "appearance_count": self.appearance_count,
            "importance": self.importance,
            "reference_image_url": self.reference_image_url,
            "base_prompt": self.base_prompt,
            "is_established": self.is_established == '1',
            "generation_count": self.generation_count,
            "attributes": self.attributes or {},
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None
        }
    
    def to_prompt_fragment(self) -> str:
        """
        Генерирует фрагмент промпта для персонажа.
        Используется ConsistencyEngine для поддержания консистентности.
        """
        parts = []
        
        # Имя и роль
        if self.name:
            parts.append(self.name)
        
        # Физические характеристики
        if self.age:
            parts.append(self.age)
        if self.gender:
            parts.append(self.gender)
        if self.build:
            parts.append(f"{self.build} build")
        
        # Лицо и волосы
        if self.hair:
            parts.append(f"{self.hair} hair")
        if self.eyes:
            parts.append(f"{self.eyes} eyes")
        if self.skin:
            parts.append(f"{self.skin} skin")
        
        # Особенности
        if self.distinguishing_features:
            parts.append(self.distinguishing_features)
        
        # Одежда
        if self.default_clothing:
            parts.append(f"wearing {self.default_clothing}")
        
        # Полное описание (если есть, имеет приоритет)
        if self.appearance:
            return self.appearance
        
        return ", ".join(parts) if parts else self.name or "character"
    
    def update_appearance_stats(self, page_number: int, chapter_number: int = None):
        """Обновляет статистику появлений"""
        self.appearance_count = (self.appearance_count or 0) + 1
        
        if self.first_appearance_page is None:
            self.first_appearance_page = page_number
            self.first_appearance_chapter = chapter_number
        
        self.updated_at = datetime.utcnow()
    
    def mark_as_established(self):
        """Помечает персонажа как 'установленного' (описание зафиксировано)"""
        self.is_established = '1'
        self.updated_at = datetime.utcnow()
    
    def increment_generation_count(self):
        """Увеличивает счётчик генераций"""
        self.generation_count = (self.generation_count or 0) + 1
        self.updated_at = datetime.utcnow()