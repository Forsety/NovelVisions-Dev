# models/domain/scene.py
"""
Domain model для Scene (Сцена/Локация).
РЕФАКТОРИНГ: story_id → book_id (внешний ID из Catalog.API, НЕ FK)
"""

from datetime import datetime
from typing import Optional, Dict, Any, List
from sqlalchemy import Column, String, Text, DateTime, JSON, Integer, Index
import uuid

from models.database.base import Base


class Scene(Base):
    """
    Модель сцены/локации для консистентности фонов и окружения.
    
    Scene представляет место действия с его визуальными характеристиками.
    Привязан к book_id - внешнему ID книги из Catalog.API.
    """
    
    __tablename__ = "scenes"
    
    # ===========================================
    # PRIMARY KEYS & EXTERNAL REFERENCES
    # ===========================================
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    
    # book_id - внешний ID из Catalog.API (НЕ FK, просто GUID для группировки)
    book_id = Column(String(36), nullable=False, index=True)
    
    # ===========================================
    # BASIC INFORMATION
    # ===========================================
    
    name = Column(String(300), nullable=False)  # "Winterfell Castle", "The Dark Forest"
    aliases = Column(JSON, default=list)  # Альтернативные названия
    description = Column(Text, nullable=True)  # Полное описание
    
    # ===========================================
    # LOCATION TYPE
    # ===========================================
    
    location_type = Column(String(100), nullable=True)  # interior, exterior, mixed
    setting_type = Column(String(100), nullable=True)  # castle, forest, city, space, etc.
    
    # ===========================================
    # VISUAL CHARACTERISTICS
    # ===========================================
    
    architecture = Column(Text, nullable=True)  # Архитектурный стиль
    materials = Column(Text, nullable=True)  # Камень, дерево, металл, стекло
    colors = Column(JSON, default=list)  # Доминирующие цвета ["gray", "gold", "crimson"]
    textures = Column(Text, nullable=True)  # Текстуры поверхностей
    
    # ===========================================
    # ATMOSPHERE & MOOD
    # ===========================================
    
    atmosphere = Column(Text, nullable=True)  # "mysterious", "warm and cozy", "ominous"
    mood = Column(String(100), nullable=True)  # "dark", "bright", "melancholic"
    
    # ===========================================
    # LIGHTING
    # ===========================================
    
    default_lighting = Column(String(200), nullable=True)  # "candlelight", "moonlight", "harsh sunlight"
    light_sources = Column(JSON, default=list)  # ["torches", "windows", "fireplace"]
    shadow_intensity = Column(String(50), nullable=True)  # "soft", "harsh", "dramatic"
    
    # ===========================================
    # WEATHER & TIME
    # ===========================================
    
    default_weather = Column(String(100), nullable=True)  # "rainy", "snowy", "clear"
    time_period = Column(String(100), nullable=True)  # "medieval", "victorian", "futuristic"
    typical_time_of_day = Column(String(50), nullable=True)  # "night", "dawn", "midday"
    season = Column(String(50), nullable=True)  # "winter", "autumn"
    
    # ===========================================
    # ENVIRONMENT DETAILS
    # ===========================================
    
    key_elements = Column(JSON, default=list)  # ["throne", "fireplace", "stained glass"]
    decorations = Column(Text, nullable=True)  # Декор и украшения
    furniture = Column(Text, nullable=True)  # Мебель (для интерьеров)
    vegetation = Column(Text, nullable=True)  # Растительность (для экстерьеров)
    props = Column(JSON, default=list)  # Важные объекты в сцене
    
    # ===========================================
    # SCALE & COMPOSITION
    # ===========================================
    
    scale = Column(String(100), nullable=True)  # "grand", "intimate", "vast", "claustrophobic"
    camera_suggestions = Column(JSON, default=list)  # ["wide shot", "low angle"]
    
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
    reference_prompt = Column(Text, nullable=True)  # Эталонный промпт
    base_prompt = Column(Text, nullable=True)  # Базовый промпт для всех генераций
    
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
    #     "sounds": ["wind howling", "fire crackling"],
    #     "smells": ["smoke", "old books"],
    #     "hazards": ["trap door", "crumbling floor"]
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
        Index('ix_scenes_book_id_name', 'book_id', 'name'),
        Index('ix_scenes_setting_type', 'setting_type'),
    )
    
    # ===========================================
    # METHODS
    # ===========================================
    
    def __repr__(self):
        return f"<Scene(id={self.id}, name={self.name}, book_id={self.book_id})>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "book_id": self.book_id,
            "name": self.name,
            "aliases": self.aliases or [],
            "description": self.description,
            "location_type": self.location_type,
            "setting_type": self.setting_type,
            "architecture": self.architecture,
            "materials": self.materials,
            "colors": self.colors or [],
            "textures": self.textures,
            "atmosphere": self.atmosphere,
            "mood": self.mood,
            "default_lighting": self.default_lighting,
            "light_sources": self.light_sources or [],
            "default_weather": self.default_weather,
            "time_period": self.time_period,
            "typical_time_of_day": self.typical_time_of_day,
            "season": self.season,
            "key_elements": self.key_elements or [],
            "decorations": self.decorations,
            "furniture": self.furniture,
            "vegetation": self.vegetation,
            "scale": self.scale,
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
        Генерирует фрагмент промпта для сцены.
        """
        parts = []
        
        # Тип локации
        if self.setting_type:
            parts.append(self.setting_type)
        
        # Название/описание
        if self.name:
            parts.append(self.name)
        
        # Архитектура
        if self.architecture:
            parts.append(f"{self.architecture} architecture")
        
        # Атмосфера
        if self.atmosphere:
            parts.append(f"{self.atmosphere} atmosphere")
        
        # Освещение
        if self.default_lighting:
            parts.append(f"{self.default_lighting}")
        
        # Погода
        if self.default_weather:
            parts.append(f"{self.default_weather} weather")
        
        # Время суток
        if self.typical_time_of_day:
            parts.append(self.typical_time_of_day)
        
        # Ключевые элементы
        if self.key_elements:
            elements = ", ".join(self.key_elements[:3])  # Максимум 3
            parts.append(f"with {elements}")
        
        # Полное описание (приоритет)
        if self.description and len(self.description) > 50:
            return self.description
        
        return ", ".join(parts) if parts else self.name or "scene"
    
    def update_appearance_stats(self, page_number: int, chapter_number: int = None):
        """Обновляет статистику появлений"""
        self.appearance_count = (self.appearance_count or 0) + 1
        
        if self.first_appearance_page is None:
            self.first_appearance_page = page_number
            self.first_appearance_chapter = chapter_number
        
        self.updated_at = datetime.utcnow()
    
    def mark_as_established(self):
        """Помечает сцену как 'установленную'"""
        self.is_established = '1'
        self.updated_at = datetime.utcnow()