# models/schemas/request/scene_request.py
"""
Request schemas для Scene endpoints.
РЕФАКТОРИНГ: story_id → book_id
"""

from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field


class SceneCreateRequest(BaseModel):
    """Запрос на создание сцены/локации."""
    
    # Обязательные поля
    book_id: str = Field(..., description="ID книги из Catalog.API")
    name: str = Field(..., min_length=1, max_length=300)
    
    # Описание
    description: Optional[str] = Field(None, max_length=2000)
    
    # Тип локации
    location_type: Optional[str] = Field(None, max_length=100)  # interior, exterior, mixed
    setting_type: Optional[str] = Field(None, max_length=100)  # castle, forest, city
    
    # Визуальные характеристики
    architecture: Optional[str] = Field(None, max_length=500)
    materials: Optional[str] = Field(None, max_length=300)
    colors: Optional[List[str]] = Field(default_factory=list)
    textures: Optional[str] = Field(None, max_length=300)
    
    # Атмосфера
    atmosphere: Optional[str] = Field(None, max_length=300)
    mood: Optional[str] = Field(None, max_length=100)
    
    # Освещение
    lighting: Optional[str] = Field(None, max_length=200)  # default_lighting
    light_sources: Optional[List[str]] = Field(default_factory=list)
    
    # Погода и время
    weather: Optional[str] = Field(None, max_length=100)  # default_weather
    time_of_day: Optional[str] = Field(None, max_length=50)
    time_period: Optional[str] = Field(None, max_length=100)
    season: Optional[str] = Field(None, max_length=50)
    
    # Детали окружения
    key_elements: Optional[List[str]] = Field(default_factory=list)
    decorations: Optional[str] = Field(None, max_length=500)
    furniture: Optional[str] = Field(None, max_length=500)
    vegetation: Optional[str] = Field(None, max_length=500)
    
    # Масштаб
    scale: Optional[str] = Field(None, max_length=100)
    
    # Альтернативные названия
    aliases: Optional[List[str]] = Field(default_factory=list)
    
    # Дополнительные атрибуты
    attributes: Optional[Dict[str, Any]] = Field(default_factory=dict)
    
    # Референс
    reference_image_url: Optional[str] = Field(None, max_length=500)
    
    class Config:
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "name": "The Great Hall of Hogwarts",
                "description": "A magnificent dining hall with floating candles and enchanted ceiling",
                "location_type": "interior",
                "setting_type": "castle",
                "architecture": "medieval gothic",
                "materials": "ancient stone, dark wood",
                "colors": ["warm gold", "deep brown", "silver"],
                "atmosphere": "magical and grand",
                "lighting": "candlelight with enchanted ceiling showing night sky",
                "key_elements": ["four long house tables", "high table for professors", "floating candles"],
                "scale": "grand"
            }
        }


class SceneUpdateRequest(BaseModel):
    """Запрос на обновление сцены."""
    
    name: Optional[str] = Field(None, min_length=1, max_length=300)
    description: Optional[str] = Field(None, max_length=2000)
    
    location_type: Optional[str] = Field(None, max_length=100)
    setting_type: Optional[str] = Field(None, max_length=100)
    
    architecture: Optional[str] = Field(None, max_length=500)
    materials: Optional[str] = Field(None, max_length=300)
    colors: Optional[List[str]] = None
    textures: Optional[str] = Field(None, max_length=300)
    
    atmosphere: Optional[str] = Field(None, max_length=300)
    mood: Optional[str] = Field(None, max_length=100)
    
    lighting: Optional[str] = Field(None, max_length=200)
    light_sources: Optional[List[str]] = None
    
    weather: Optional[str] = Field(None, max_length=100)
    time_of_day: Optional[str] = Field(None, max_length=50)
    time_period: Optional[str] = Field(None, max_length=100)
    season: Optional[str] = Field(None, max_length=50)
    
    key_elements: Optional[List[str]] = None
    decorations: Optional[str] = Field(None, max_length=500)
    furniture: Optional[str] = Field(None, max_length=500)
    vegetation: Optional[str] = Field(None, max_length=500)
    
    scale: Optional[str] = Field(None, max_length=100)
    aliases: Optional[List[str]] = None
    attributes: Optional[Dict[str, Any]] = None
    
    reference_image_url: Optional[str] = Field(None, max_length=500)
    base_prompt: Optional[str] = Field(None, max_length=2000)
    
    is_established: Optional[bool] = None


class ScenePromptRequest(BaseModel):
    """Запрос на генерацию промпта для сцены."""
    
    # Переопределения для конкретной генерации
    time_of_day: Optional[str] = Field(None, description="Время суток")
    weather: Optional[str] = Field(None, description="Погода")
    lighting: Optional[str] = Field(None, description="Освещение")
    
    # Персонажи в сцене
    characters: Optional[List[str]] = Field(None, description="Имена персонажей")
    action: Optional[str] = Field(None, description="Что происходит в сцене")
    
    # Целевая модель
    target_model: str = Field(default="dalle3")
    style: Optional[str] = Field(None)
    
    # Композиция
    camera_angle: Optional[str] = Field(None, description="Угол камеры")
    shot_type: Optional[str] = Field(None, description="Тип кадра: wide, medium, close-up")
    
    class Config:
        json_schema_extra = {
            "example": {
                "time_of_day": "evening",
                "lighting": "candlelight with moonlight through windows",
                "characters": ["Harry", "Hermione", "Ron"],
                "action": "having dinner",
                "target_model": "midjourney",
                "shot_type": "wide shot"
            }
        }


class SceneSearchRequest(BaseModel):
    """Запрос на поиск сцен."""
    
    book_id: str = Field(..., description="ID книги")
    query: Optional[str] = Field(None, description="Поисковый запрос по имени")
    location_type: Optional[str] = Field(None)
    setting_type: Optional[str] = Field(None)
    
    page: int = Field(default=1, ge=1)
    page_size: int = Field(default=20, ge=1, le=100)