# models/schemas/request/character_request.py
"""
Request schemas для Character endpoints.
РЕФАКТОРИНГ: story_id → book_id
"""

from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field


class CharacterCreateRequest(BaseModel):
    """Запрос на создание персонажа."""
    
    # Обязательные поля
    book_id: str = Field(..., description="ID книги из Catalog.API")
    name: str = Field(..., min_length=1, max_length=200)
    
    # Описание
    description: Optional[str] = Field(None, max_length=2000)
    role: Optional[str] = Field(None, max_length=100)  # protagonist, antagonist, etc.
    
    # Физические характеристики
    gender: Optional[str] = Field(None, max_length=50)
    age: Optional[str] = Field(None, max_length=50)
    height: Optional[str] = Field(None, max_length=50)
    build: Optional[str] = Field(None, max_length=100)
    
    # Внешность
    appearance: Optional[str] = Field(None, max_length=2000)
    hair: Optional[str] = Field(None, max_length=200)
    eyes: Optional[str] = Field(None, max_length=100)
    skin: Optional[str] = Field(None, max_length=100)
    facial_features: Optional[str] = Field(None, max_length=500)
    distinguishing_features: Optional[str] = Field(None, max_length=500)
    
    # Одежда
    default_clothing: Optional[str] = Field(None, max_length=500)
    accessories: Optional[str] = Field(None, max_length=500)
    
    # Дополнительные атрибуты
    aliases: Optional[List[str]] = Field(default_factory=list)
    attributes: Optional[Dict[str, Any]] = Field(default_factory=dict)
    
    # Референсы
    reference_image_url: Optional[str] = Field(None, max_length=500)
    
    class Config:
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "name": "Harry Potter",
                "description": "The Boy Who Lived, a young wizard with a lightning scar",
                "gender": "male",
                "age": "teenager",
                "hair": "messy black hair",
                "eyes": "bright green eyes",
                "distinguishing_features": "lightning bolt scar on forehead, round glasses",
                "default_clothing": "Hogwarts robes, Gryffindor scarf"
            }
        }


class CharacterUpdateRequest(BaseModel):
    """Запрос на обновление персонажа."""
    
    name: Optional[str] = Field(None, min_length=1, max_length=200)
    description: Optional[str] = Field(None, max_length=2000)
    role: Optional[str] = Field(None, max_length=100)
    
    gender: Optional[str] = Field(None, max_length=50)
    age: Optional[str] = Field(None, max_length=50)
    height: Optional[str] = Field(None, max_length=50)
    build: Optional[str] = Field(None, max_length=100)
    
    appearance: Optional[str] = Field(None, max_length=2000)
    hair: Optional[str] = Field(None, max_length=200)
    eyes: Optional[str] = Field(None, max_length=100)
    skin: Optional[str] = Field(None, max_length=100)
    facial_features: Optional[str] = Field(None, max_length=500)
    distinguishing_features: Optional[str] = Field(None, max_length=500)
    
    default_clothing: Optional[str] = Field(None, max_length=500)
    accessories: Optional[str] = Field(None, max_length=500)
    
    aliases: Optional[List[str]] = None
    attributes: Optional[Dict[str, Any]] = None
    
    reference_image_url: Optional[str] = Field(None, max_length=500)
    base_prompt: Optional[str] = Field(None, max_length=2000)
    
    # Флаг фиксации описания
    is_established: Optional[bool] = None


class CharacterPromptRequest(BaseModel):
    """Запрос на генерацию промпта для персонажа."""
    
    # Контекст сцены
    action: Optional[str] = Field(None, description="Что делает персонаж")
    emotion: Optional[str] = Field(None, description="Эмоция персонажа")
    pose: Optional[str] = Field(None, description="Поза персонажа")
    scene_context: Optional[str] = Field(None, description="Контекст сцены")
    
    # Целевая модель
    target_model: str = Field(default="dalle3", description="AI модель для генерации")
    style: Optional[str] = Field(None, description="Стиль изображения")
    
    class Config:
        json_schema_extra = {
            "example": {
                "action": "casting a spell",
                "emotion": "determined",
                "pose": "pointing wand forward",
                "scene_context": "dark corridor of Hogwarts",
                "target_model": "midjourney",
                "style": "fantasy illustration"
            }
        }


class CharacterSearchRequest(BaseModel):
    """Запрос на поиск персонажей."""
    
    book_id: str = Field(..., description="ID книги")
    query: Optional[str] = Field(None, description="Поисковый запрос по имени")
    role: Optional[str] = Field(None, description="Фильтр по роли")
    importance_min: Optional[int] = Field(None, ge=1, le=10)
    
    page: int = Field(default=1, ge=1)
    page_size: int = Field(default=20, ge=1, le=100)