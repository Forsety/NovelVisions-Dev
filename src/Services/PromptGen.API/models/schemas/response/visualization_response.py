# src/Services/PromptGen.API/models/schemas/response/visualization_response.py
"""
Response schemas для визуализации
"""
from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field
from datetime import datetime


class GeneratedPrompt(BaseModel):
    """Сгенерированный промпт"""
    
    prompt: str = Field(..., description="Основной промпт")
    negative_prompt: Optional[str] = Field(None, description="Негативный промпт")
    
    # Метаданные
    moment_description: str = Field(..., description="Описание визуального момента")
    moment_type: str = Field(..., description="Тип момента: action, emotion, establishing, reveal")
    importance: str = Field(..., description="Важность: high, medium, low")
    
    # Элементы сцены
    characters: List[str] = Field(default_factory=list, description="Персонажи в сцене")
    scene_elements: List[str] = Field(default_factory=list, description="Элементы сцены")
    
    # Параметры для модели
    suggested_aspect_ratio: str = Field(default="1:1", description="Рекомендуемое соотношение сторон")
    suggested_parameters: Optional[Dict[str, Any]] = Field(None, description="Параметры для модели")


class GeneratePromptsResponse(BaseModel):
    """
    Ответ на запрос генерации промптов.
    """
    
    # Идентификаторы
    book_id: str
    chapter_id: str
    page_id: str
    page_number: int
    
    # Сгенерированные промпты
    prompts: List[GeneratedPrompt] = Field(..., description="Список промптов")
    
    # Анализ страницы
    analysis: Dict[str, Any] = Field(..., description="Результат анализа текста")
    
    # Контекст персонажей
    character_context: Dict[str, Dict] = Field(
        default_factory=dict, 
        description="Данные консистентности персонажей"
    )
    
    # Метаданные
    target_model: str
    style: Optional[str]
    processing_time_ms: int
    
    class Config:
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "chapter_id": "550e8400-e29b-41d4-a716-446655440001",
                "page_id": "550e8400-e29b-41d4-a716-446655440002",
                "page_number": 42,
                "prompts": [
                    {
                        "prompt": "A majestic elderly wizard with long silver beard...",
                        "negative_prompt": "blurry, low quality, deformed",
                        "moment_description": "Gandalf reveals his true power",
                        "moment_type": "reveal",
                        "importance": "high",
                        "characters": ["Gandalf"],
                        "scene_elements": ["ancient tower", "magical light"],
                        "suggested_aspect_ratio": "16:9"
                    }
                ],
                "analysis": {
                    "mood": "dramatic",
                    "setting": "fantasy castle",
                    "key_actions": ["magic spell", "confrontation"]
                },
                "target_model": "dalle3",
                "processing_time_ms": 1250
            }
        }


class EnhancePromptResponse(BaseModel):
    """Ответ на улучшение промпта"""
    
    original_prompt: str
    enhanced_prompt: str
    negative_prompt: Optional[str]
    
    improvements: List[str] = Field(
        default_factory=list, 
        description="Список улучшений"
    )
    target_model: str
    style: Optional[str]


class CharacterConsistencyResponse(BaseModel):
    """Данные консистентности персонажа"""
    
    book_id: str
    character_name: str
    
    # Визуальное описание
    appearance_prompt: str = Field(..., description="Промпт внешности")
    clothing_prompt: Optional[str] = Field(None, description="Промпт одежды")
    
    # Атрибуты
    attributes: Dict[str, str] = Field(
        default_factory=dict,
        description="Визуальные атрибуты"
    )
    
    # Статус
    is_established: bool = Field(
        default=False, 
        description="Установлена ли baseline консистентность"
    )
    generation_count: int = Field(
        default=0, 
        description="Количество генераций с этим персонажем"
    )
    
    created_at: datetime
    updated_at: Optional[datetime]


class BatchGenerateResponse(BaseModel):
    """Ответ на пакетную генерацию"""
    
    book_id: str
    total_pages: int
    successful: int
    failed: int
    
    results: List[GeneratePromptsResponse]
    errors: List[Dict[str, Any]] = Field(default_factory=list)
    
    total_processing_time_ms: int