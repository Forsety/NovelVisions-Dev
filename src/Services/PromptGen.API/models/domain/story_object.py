# models/domain/story_object.py
"""
Domain model для StoryObject (Объект в истории).
РЕФАКТОРИНГ: story_id → book_id (внешний ID из Catalog.API, НЕ FK)
"""

from datetime import datetime
from typing import Optional, Dict, Any, List
from sqlalchemy import Column, String, Text, DateTime, JSON, Integer, Index
import uuid

from models.database.base import Base


class StoryObject(Base):
    """
    Модель объекта в истории для консистентности артефактов.
    
    StoryObject представляет значимый объект (артефакт, оружие, предмет),
    который появляется в истории и должен выглядеть консистентно.
    Привязан к book_id - внешнему ID книги из Catalog.API.
    """
    
    __tablename__ = "story_objects"
    
    # ===========================================
    # PRIMARY KEYS & EXTERNAL REFERENCES
    # ===========================================
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    
    # book_id - внешний ID из Catalog.API (НЕ FK, просто GUID для группировки)
    book_id = Column(String(36), nullable=False, index=True)
    
    # ===========================================
    # BASIC INFORMATION
    # ===========================================
    
    name = Column(String(300), nullable=False)  # "Elder Wand", "The One Ring"
    aliases = Column(JSON, default=list)  # ["Deathstick", "Wand of Destiny"]
    description = Column(Text, nullable=True)  # Полное описание
    
    # ===========================================
    # CATEGORIZATION
    # ===========================================
    
    category = Column(String(100), nullable=True)  # weapon, artifact, vehicle, tool, etc.
    subcategory = Column(String(100), nullable=True)  # sword, ring, car, wand, etc.
    
    # ===========================================
    # VISUAL CHARACTERISTICS
    # ===========================================
    
    appearance = Column(Text, nullable=True)  # Полное описание внешнего вида
    materials = Column(Text, nullable=True)  # Из чего сделан: "gold and silver", "ancient wood"
    colors = Column(JSON, default=list)  # ["gold", "emerald green"]
    size = Column(String(100), nullable=True)  # "handheld", "massive", "tiny"
    shape = Column(String(100), nullable=True)  # "cylindrical", "ornate", "angular"
    
    # ===========================================
    # DETAILS & MARKINGS
    # ===========================================
    
    details = Column(Text, nullable=True)  # Гравировки, украшения, узоры
    markings = Column(Text, nullable=True)  # Символы, надписи, руны
    texture = Column(String(100), nullable=True)  # "smooth", "rough", "crystalline"
    condition = Column(String(100), nullable=True)  # "pristine", "worn", "ancient", "damaged"
    
    # ===========================================
    # SPECIAL PROPERTIES
    # ===========================================
    
    special_properties = Column(JSON, default=list)  # ["glowing", "magical aura", "humming"]
    effects = Column(Text, nullable=True)  # Визуальные эффекты: "emits soft blue light"
    aura = Column(String(100), nullable=True)  # "menacing", "holy", "corrupted"
    
    # ===========================================
    # OWNERSHIP
    # ===========================================
    
    owner_character_id = Column(String(36), nullable=True)  # ID персонажа-владельца
    owner_name = Column(String(200), nullable=True)  # Имя владельца (денормализация)
    
    # ===========================================
    # TRACKING & ANALYTICS
    # ===========================================
    
    first_appearance_page = Column(Integer, nullable=True)
    first_appearance_chapter = Column(Integer, nullable=True)
    appearance_count = Column(Integer, default=0)
    importance = Column(Integer, default=5)  # 1-10
    
    # ===========================================
    # REFERENCE & CONSISTENCY
    # ===========================================
    
    reference_image_url = Column(String(500), nullable=True)
    reference_prompt = Column(Text, nullable=True)
    base_prompt = Column(Text, nullable=True)
    
    # ===========================================
    # EMBEDDINGS
    # ===========================================
    
    embedding_vector = Column(JSON, nullable=True)
    
    # ===========================================
    # CONSISTENCY FLAGS
    # ===========================================
    
    is_established = Column(String(1), default='0')
    generation_count = Column(Integer, default=0)
    
    # ===========================================
    # ADDITIONAL ATTRIBUTES
    # ===========================================
    
    attributes = Column(JSON, default=dict)
    # Пример:
    # {
    #     "powers": ["invisibility", "protection"],
    #     "history": "forged in ancient times",
    #     "curse": "corrupts the bearer"
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
        Index('ix_story_objects_book_id_name', 'book_id', 'name'),
        Index('ix_story_objects_category', 'category'),
        Index('ix_story_objects_owner', 'owner_character_id'),
    )
    
    # ===========================================
    # METHODS
    # ===========================================
    
    def __repr__(self):
        return f"<StoryObject(id={self.id}, name={self.name}, book_id={self.book_id})>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "book_id": self.book_id,
            "name": self.name,
            "aliases": self.aliases or [],
            "description": self.description,
            "category": self.category,
            "subcategory": self.subcategory,
            "appearance": self.appearance,
            "materials": self.materials,
            "colors": self.colors or [],
            "size": self.size,
            "shape": self.shape,
            "details": self.details,
            "markings": self.markings,
            "texture": self.texture,
            "condition": self.condition,
            "special_properties": self.special_properties or [],
            "effects": self.effects,
            "aura": self.aura,
            "owner_character_id": self.owner_character_id,
            "owner_name": self.owner_name,
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
        Генерирует фрагмент промпта для объекта.
        """
        parts = []
        
        # Название
        if self.name:
            parts.append(self.name)
        
        # Материалы
        if self.materials:
            parts.append(f"made of {self.materials}")
        
        # Размер и форма
        if self.size:
            parts.append(f"{self.size}")
        
        # Цвета
        if self.colors:
            colors_str = " and ".join(self.colors[:2])
            parts.append(f"{colors_str}")
        
        # Детали
        if self.details:
            parts.append(self.details)
        
        # Эффекты
        if self.effects:
            parts.append(self.effects)
        
        # Состояние
        if self.condition:
            parts.append(f"{self.condition} condition")
        
        # Полное описание (приоритет)
        if self.appearance:
            return self.appearance
        
        return ", ".join(parts) if parts else self.name or "object"
    
    def update_appearance_stats(self, page_number: int, chapter_number: int = None):
        """Обновляет статистику появлений"""
        self.appearance_count = (self.appearance_count or 0) + 1
        
        if self.first_appearance_page is None:
            self.first_appearance_page = page_number
            self.first_appearance_chapter = chapter_number
        
        self.updated_at = datetime.utcnow()
    
    def set_owner(self, character_id: str, character_name: str = None):
        """Устанавливает владельца объекта"""
        self.owner_character_id = character_id
        self.owner_name = character_name
        self.updated_at = datetime.utcnow()