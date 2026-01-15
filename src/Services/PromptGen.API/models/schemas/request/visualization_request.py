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
    
    target_model: TargetModel = Field(
        default=TargetModel.DALLE3,
        description="Целевая AI модель"
    )
    style: Optional[VisualizationStyle] = Field(
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
    
    class Config:
        use_enum_values = True
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "page_id": "660e8400-e29b-41d4-a716-446655440001",
                "page_content": "Harry raised his wand, his eyes fixed on the towering figure of Voldemort. 'Expelliarmus!' he shouted, a jet of red light bursting from his wand.",
                "page_number": 42,
                "chapter_number": 7,
                "target_model": "dalle3",
                "style": "fantasy",
                "maintain_consistency": True,
                "max_prompts": 1
            }
        }


class EnhancePromptRequest(BaseModel):
    """
    Запрос на улучшение существующего промпта.
    Добавляет детали, стилизацию и параметры для целевой модели.
    """
    
    prompt: str = Field(
        ..., 
        min_length=1, 
        max_length=2000,
        description="Исходный промпт для улучшения"
    )
    target_model: TargetModel = Field(default=TargetModel.DALLE3)
    style: Optional[VisualizationStyle] = None
    custom_style: Optional[str] = Field(None, max_length=200)
    
    # Контекст для консистентности
    book_id: Optional[str] = Field(None, description="ID книги для контекста")
    character_names: Optional[List[str]] = Field(
        None, 
        description="Имена персонажей в сцене для консистентности"
    )
    scene_name: Optional[str] = Field(
        None,
        description="Название локации для консистентности"
    )
    
    class Config:
        use_enum_values = True


class CharacterConsistencyRequest(BaseModel):
    """
    Запрос на создание/обновление данных консистентности персонажа.
    Позволяет авторам зафиксировать описание персонажа.
    """
    
    book_id: str = Field(..., description="ID книги")
    character_name: str = Field(..., min_length=1, max_length=200)
    
    # Описание внешности
    appearance: Optional[str] = Field(None, max_length=1000)
    hair: Optional[str] = Field(None, max_length=200)
    eyes: Optional[str] = Field(None, max_length=100)
    age: Optional[str] = Field(None, max_length=50)
    build: Optional[str] = Field(None, max_length=100)
    
    # Одежда и особенности
    clothing: Optional[str] = Field(None, max_length=500)
    distinguishing_features: Optional[str] = Field(None, max_length=500)
    accessories: Optional[str] = Field(None, max_length=300)
    
    class Config:
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "character_name": "Harry Potter",
                "appearance": "A thin young man with messy jet-black hair",
                "hair": "messy jet-black hair",
                "eyes": "bright green eyes, almond-shaped like his mother's",
                "age": "teenager, about 17",
                "build": "thin but athletic from Quidditch",
                "distinguishing_features": "lightning bolt scar on forehead, round glasses"
            }
        }


class BatchGenerateRequest(BaseModel):
    """
    Пакетная генерация промптов для нескольких страниц.
    Оптимизировано для генерации всей главы или книги.
    """
    
    book_id: str = Field(..., description="ID книги")
    
    pages: List[Dict[str, Any]] = Field(
        ..., 
        min_length=1, 
        max_length=20,
        description="Список страниц с контентом"
    )
    # Каждый элемент pages:
    # {
    #     "page_id": "...",
    #     "page_number": 1,
    #     "content": "текст страницы"
    # }
    
    target_model: TargetModel = Field(default=TargetModel.DALLE3)
    style: Optional[VisualizationStyle] = None
    maintain_consistency: bool = Field(default=True)
    
    class Config:
        use_enum_values = True


class AnalyzeTextRequest(BaseModel):
    """
    Запрос на анализ текста для извлечения персонажей, сцен и объектов.
    """
    
    book_id: str = Field(..., description="ID книги")
    text: str = Field(..., min_length=1, max_length=20000)
    
    extract_characters: bool = Field(default=True)
    extract_scenes: bool = Field(default=True)
    extract_objects: bool = Field(default=True)
    
    # Опции
    save_extracted: bool = Field(
        default=False, 
        description="Сохранить извлечённые элементы в БД"
    )