# models/schemas/request/visualization_request.py
"""
Request schemas для Visualization endpoints.
Главные endpoints для взаимодействия с Visualization.API.
"""

from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field
from enum import Enum


class TargetModel(str, Enum):
    """Поддерживаемые AI модели для генерации изображений."""
    DALLE3 = "dalle3"
    MIDJOURNEY = "midjourney"
    STABLE_DIFFUSION = "stable-diffusion"
    FLUX = "flux"


class VisualizationStyle(str, Enum):
    """Предустановленные стили визуализации."""
    REALISTIC = "realistic"
    FANTASY = "fantasy"
    MANGA = "manga"
    ANIME = "anime"
    COMIC = "comic"
    PAINTERLY = "painterly"
    SKETCH = "sketch"
    CINEMATIC = "cinematic"
    WATERCOLOR = "watercolor"
    OIL_PAINTING = "oil_painting"


class GeneratePromptsRequest(BaseModel):
    """
    Главный запрос генерации промптов.
    Вызывается из Visualization.API при создании задания на генерацию.
    """
    
    # =========================================
    # ОБЯЗАТЕЛЬНЫЕ ПОЛЯ
    # =========================================
    
    book_id: str = Field(..., description="ID книги из Catalog.API")
    page_content: str = Field(
        ..., 
        min_length=1, 
        max_length=10000,
        description="Текст страницы для визуализации"
    )
    
    # =========================================
    # ИДЕНТИФИКАТОРЫ СТРАНИЦЫ (опционально)
    # =========================================
    
    page_id: Optional[str] = Field(None, description="ID страницы из Catalog.API")
    chapter_id: Optional[str] = Field(None, description="ID главы")
    page_number: Optional[int] = Field(None, ge=1, description="Номер страницы")
    chapter_number: Optional[int] = Field(None, ge=1, description="Номер главы")
    
    # =========================================
    # КОНТЕКСТ
    # =========================================
    
    context_before: Optional[str] = Field(
        None, 
        max_length=5000,
        description="Текст до текущей страницы (для контекста)"
    )
    context_after: Optional[str] = Field(
        None, 
        max_length=5000,
        description="Текст после текущей страницы"
    )
    
    # =========================================
    # НАСТРОЙКИ ГЕНЕРАЦИИ
    # =========================================
    
    target_model: str = Field(
        default="dalle3",
        description="Целевая AI модель (dalle3, midjourney, stable-diffusion, flux)"
    )
    
    # Принимаем любую строку для style (не enum)
    style: Optional[str] = Field(
        None, 
        description="Стиль визуализации"
    )
    
    custom_style: Optional[str] = Field(
        None, 
        max_length=200,
        description="Кастомный стиль (если не из предустановленных)"
    )
    
    max_prompts: int = Field(
        default=1, 
        ge=1, 
        le=5,
        description="Максимальное количество промптов"
    )
    
    # =========================================
    # КОНСИСТЕНТНОСТЬ
    # =========================================
    
    maintain_consistency: bool = Field(
        default=True,
        description="Поддерживать консистентность персонажей"
    )
    known_characters: Optional[List[str]] = Field(
        None,
        description="Список известных персонажей в книге"
    )
    
    # =========================================
    # ДОПОЛНИТЕЛЬНЫЕ ПАРАМЕТРЫ
    # =========================================
    
    parameters: Optional[Dict[str, Any]] = Field(
        default_factory=dict,
        description="Дополнительные параметры генерации"
    )
    
    # Метаданные книги (опционально, для контекста)
    book_title: Optional[str] = Field(None, description="Название книги")
    book_genre: Optional[str] = Field(None, description="Жанр книги")
    
    class Config:
        # Не использовать enum values автоматически - принимаем строки
        use_enum_values = True
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "page_id": "660e8400-e29b-41d4-a716-446655440001",
                "page_content": "Harry raised his wand, his eyes fixed on the towering figure of Voldemort. The dark wizard's red eyes gleamed with malice as lightning crackled overhead.",
                "target_model": "dalle3",
                "style": "fantasy",
                "maintain_consistency": True,
                "max_prompts": 1
            }
        }


class EnhancePromptRequest(BaseModel):
    """
    Запрос на улучшение существующего промпта.
    """
    
    prompt: str = Field(..., min_length=1, max_length=5000, description="Исходный промпт")
    target_model: str = Field(
        default="dalle3",
        description="Целевая AI модель"
    )
    book_id: Optional[str] = Field(None, description="ID книги для контекста персонажей")
    style: Optional[str] = Field(None, description="Желаемый стиль")
    parameters: Optional[Dict[str, Any]] = Field(default_factory=dict)
    
    class Config:
        json_schema_extra = {
            "example": {
                "prompt": "A wizard casting a spell",
                "target_model": "midjourney",
                "style": "fantasy"
            }
        }


class CharacterConsistencyRequest(BaseModel):
    """
    Запрос на сохранение данных консистентности персонажа.
    """
    
    book_id: str = Field(..., description="ID книги")
    character_name: str = Field(..., min_length=1, max_length=100, description="Имя персонажа")
    appearance: Optional[str] = Field(None, max_length=2000, description="Описание внешности")
    clothing: Optional[str] = Field(None, max_length=1000, description="Одежда")
    distinguishing_features: Optional[List[str]] = Field(None, description="Отличительные черты")
    reference_image_url: Optional[str] = Field(None, description="URL референсного изображения")
    
    class Config:
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "character_name": "Harry Potter",
                "appearance": "Young man with messy black hair, round glasses, thin build",
                "distinguishing_features": ["lightning bolt scar on forehead", "bright green eyes"]
            }
        }


class BatchGenerateRequest(BaseModel):
    """
    Запрос на пакетную генерацию промптов.
    """
    
    book_id: str = Field(..., description="ID книги")
    pages: List[Dict[str, Any]] = Field(
        ..., 
        min_length=1,
        max_length=50,
        description="Список страниц для генерации"
    )
    target_model: str = Field(default="dalle3")
    style: Optional[str] = Field(None)
    maintain_consistency: bool = Field(default=True)
    
    class Config:
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "pages": [
                    {"page_number": 1, "content": "Chapter 1 text..."},
                    {"page_number": 2, "content": "Chapter 1 continued..."}
                ],
                "target_model": "dalle3",
                "style": "fantasy"
            }
        }