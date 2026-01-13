# src/Services/PromptGen.API/models/domain/book_context.py
"""
Domain models для контекста книги - заменяет старый story.py
"""
from typing import Optional, Dict, Any, List
from dataclasses import dataclass, field
from datetime import datetime
from enum import Enum


class VisualizationMode(str, Enum):
    """Режимы визуализации (синхронизировано с Catalog.API)"""
    NONE = "None"
    PER_PAGE = "PerPage"
    PER_CHAPTER = "PerChapter"
    USER_SELECTED = "UserSelected"
    AUTHOR_DEFINED = "AuthorDefined"


@dataclass
class CharacterProfile:
    """Профиль персонажа для консистентности"""
    
    name: str
    book_id: str
    
    # Визуальные атрибуты
    appearance: str = ""
    hair: Optional[str] = None
    eyes: Optional[str] = None
    age: Optional[str] = None
    build: Optional[str] = None
    clothing: Optional[str] = None
    distinguishing_features: Optional[str] = None
    
    # Генерация
    base_prompt: Optional[str] = None
    negative_prompt: Optional[str] = None
    
    # Метаданные
    generation_count: int = 0
    is_established: bool = False
    created_at: datetime = field(default_factory=datetime.utcnow)
    updated_at: Optional[datetime] = None
    
    def to_prompt_fragment(self) -> str:
        """Конвертирует профиль в фрагмент промпта"""
        parts = []
        
        if self.appearance:
            parts.append(self.appearance)
        
        if self.hair:
            parts.append(f"{self.hair} hair")
        
        if self.eyes:
            parts.append(f"{self.eyes} eyes")
        
        if self.age:
            parts.append(f"{self.age}")
        
        if self.build:
            parts.append(f"{self.build} build")
        
        if self.clothing:
            parts.append(f"wearing {self.clothing}")
        
        if self.distinguishing_features:
            parts.append(self.distinguishing_features)
        
        return ", ".join(parts) if parts else self.name
    
    def to_dict(self) -> Dict[str, Any]:
        return {
            "name": self.name,
            "book_id": self.book_id,
            "appearance": self.appearance,
            "hair": self.hair,
            "eyes": self.eyes,
            "age": self.age,
            "build": self.build,
            "clothing": self.clothing,
            "distinguishing_features": self.distinguishing_features,
            "base_prompt": self.base_prompt,
            "generation_count": self.generation_count,
            "is_established": self.is_established
        }


@dataclass
class SceneContext:
    """Контекст сцены"""
    
    name: str
    book_id: str
    
    # Описание
    description: str = ""
    location: Optional[str] = None
    time_of_day: Optional[str] = None
    weather: Optional[str] = None
    lighting: Optional[str] = None
    atmosphere: Optional[str] = None
    
    def to_prompt_fragment(self) -> str:
        """Конвертирует контекст в фрагмент промпта"""
        parts = []
        
        if self.location:
            parts.append(self.location)
        
        if self.time_of_day:
            parts.append(f"{self.time_of_day} lighting")
        
        if self.weather:
            parts.append(f"{self.weather} weather")
        
        if self.atmosphere:
            parts.append(f"{self.atmosphere} atmosphere")
        
        if self.description:
            parts.append(self.description)
        
        return ", ".join(parts) if parts else self.name


@dataclass
class BookContext:
    """
    Полный контекст книги для генерации промптов.
    Заменяет старый Story model.
    """
    
    # Идентификаторы (из Catalog.API)
    book_id: str
    
    # Метаданные книги
    title: Optional[str] = None
    genre: Optional[str] = None
    style: Optional[str] = None
    
    # Настройки визуализации
    visualization_mode: VisualizationMode = VisualizationMode.PER_PAGE
    preferred_style: Optional[str] = None
    preferred_provider: Optional[str] = None
    
    # Персонажи книги
    characters: Dict[str, CharacterProfile] = field(default_factory=dict)
    
    # Сцены/локации
    scenes: Dict[str, SceneContext] = field(default_factory=dict)
    
    # История генераций для consistency
    generation_history: List[Dict[str, Any]] = field(default_factory=list)
    
    # Текущая позиция чтения
    current_chapter: int = 1
    current_page: int = 1
    
    def get_character(self, name: str) -> Optional[CharacterProfile]:
        """Получить профиль персонажа по имени (case-insensitive)"""
        name_lower = name.lower()
        for char_name, profile in self.characters.items():
            if char_name.lower() == name_lower:
                return profile
        return None
    
    def add_character(self, profile: CharacterProfile) -> None:
        """Добавить персонажа"""
        self.characters[profile.name] = profile
    
    def get_scene(self, name: str) -> Optional[SceneContext]:
        """Получить контекст сцены"""
        return self.scenes.get(name)
    
    def add_scene(self, scene: SceneContext) -> None:
        """Добавить сцену"""
        self.scenes[scene.name] = scene
    
    def to_dict(self) -> Dict[str, Any]:
        return {
            "book_id": self.book_id,
            "title": self.title,
            "genre": self.genre,
            "style": self.style,
            "visualization_mode": self.visualization_mode.value,
            "characters": {k: v.to_dict() for k, v in self.characters.items()},
            "current_chapter": self.current_chapter,
            "current_page": self.current_page
        }