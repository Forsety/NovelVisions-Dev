# src/Services/PromptGen.API/models/schemas/request/visualization_request.py
"""
Request schemas для визуализации - интеграция с Catalog.API
"""
from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field, validator
from enum import Enum


class TargetModel(str, Enum):
    """Поддерживаемые AI модели"""
    MIDJOURNEY = "midjourney"
    DALLE3 = "dalle3"
    STABLE_DIFFUSION = "stable-diffusion"
    FLUX = "flux"


class VisualizationStyle(str, Enum):
    """Стили визуализации"""
    REALISTIC = "realistic"
    ANIME = "anime"
    MANGA = "manga"
    FANTASY = "fantasy"
    OIL_PAINTING = "oil-painting"
    WATERCOLOR = "watercolor"
    COMIC = "comic"
    CINEMATIC = "cinematic"


class GeneratePromptsRequest(BaseModel):
    """
    Запрос на генерацию промптов для страницы книги.
    Вызывается из Visualization.API.
    """
    
    # Идентификаторы из Catalog.API
    book_id: str = Field(..., description="ID книги из Catalog.API")
    chapter_id: str = Field(..., description="ID главы")
    page_id: str = Field(..., description="ID страницы")
    
    # Контент страницы
    page_content: str = Field(
        ..., 
        min_length=1, 
        max_length=50000,
        description="Текст страницы для анализа"
    )
    page_number: int = Field(..., ge=1, description="Номер страницы")
    chapter_number: int = Field(..., ge=1, description="Номер главы")
    
    # Настройки генерации
    target_model: TargetModel = Field(
        default=TargetModel.DALLE3, 
        description="Целевая AI модель"
    )
    style: Optional[VisualizationStyle] = Field(
        None, 
        description="Стиль визуализации"
    )
    
    # Контекст книги (опционально, для consistency)
    book_title: Optional[str] = Field(None, description="Название книги")
    book_genre: Optional[str] = Field(None, description="Жанр книги")
    author_hint: Optional[str] = Field(
        None, 
        max_length=1000,
        description="Подсказка автора для визуализации"
    )
    
    # Дополнительные параметры
    max_prompts: int = Field(
        default=1, 
        ge=1, 
        le=5,
        description="Максимум промптов на страницу"
    )
    maintain_consistency: bool = Field(
        default=True, 
        description="Поддерживать консистентность персонажей"
    )
    include_negative_prompt: bool = Field(
        default=True,
        description="Включить негативный промпт"
    )
    
    class Config:
        use_enum_values = True


class EnhancePromptRequest(BaseModel):
    """
    Запрос на улучшение существующего промпта.
    """
    
    prompt: str = Field(
        ..., 
        min_length=1, 
        max_length=2000,
        description="Исходный промпт"
    )
    target_model: TargetModel = Field(default=TargetModel.DALLE3)
    style: Optional[VisualizationStyle] = None
    
    # Контекст для consistency
    book_id: Optional[str] = Field(None, description="ID книги для контекста")
    character_names: Optional[List[str]] = Field(
        None, 
        description="Имена персонажей в сцене"
    )
    
    class Config:
        use_enum_values = True


class CharacterConsistencyRequest(BaseModel):
    """
    Запрос данных консистентности персонажа.
    """
    
    book_id: str = Field(..., description="ID книги")
    character_name: str = Field(..., min_length=1, max_length=100)
    
    # Опциональное описание для создания/обновления
    appearance: Optional[str] = Field(None, max_length=1000)
    clothing: Optional[str] = Field(None, max_length=500)
    distinguishing_features: Optional[str] = Field(None, max_length=500)


class BatchGenerateRequest(BaseModel):
    """
    Пакетная генерация промптов для нескольких страниц.
    """
    
    book_id: str = Field(..., description="ID книги")
    pages: List[Dict[str, Any]] = Field(
        ..., 
        min_items=1, 
        max_items=20,
        description="Список страниц с контентом"
    )
    target_model: TargetModel = Field(default=TargetModel.DALLE3)
    style: Optional[VisualizationStyle] = None
    maintain_consistency: bool = Field(default=True)
    
    class Config:
        use_enum_values = True