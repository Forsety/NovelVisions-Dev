# models/schemas/response/visualization_response.py
"""
Response schemas для Visualization endpoints.
Главные responses для взаимодействия с Visualization.API.
"""

from typing import Optional, Dict, Any, List
from pydantic import BaseModel
from datetime import datetime


class GeneratedPrompt(BaseModel):
    """Один сгенерированный промпт."""
    
    # Промпт
    prompt: str
    negative_prompt: Optional[str] = None
    
    # Метаданные
    scene_description: Optional[str] = None
    scene_type: Optional[str] = None  # action, dialogue, establishing
    importance: str = "medium"  # high, medium, low
    
    # Персонажи и объекты в сцене
    characters: List[str] = []
    objects: List[str] = []
    location: Optional[str] = None
    
    # Параметры для AI модели
    parameters: Dict[str, Any] = {}
    # Пример для Midjourney:
    # {
    #     "aspect_ratio": "16:9",
    #     "quality": 2,
    #     "stylize": 750
    # }


class GeneratePromptsResponse(BaseModel):
    """
    Главный response генерации промптов.
    Возвращается на запрос от Visualization.API.
    """
    
    # Идентификаторы
    book_id: str
    page_id: Optional[str] = None
    page_number: Optional[int] = None
    
    # Сгенерированные промпты
    prompts: List[GeneratedPrompt]
    
    # Параметры генерации
    target_model: str
    style: Optional[str] = None
    
    # Консистентность
    consistency_data: Dict[str, Any] = {}
    # Содержит данные о персонажах для отслеживания:
    # {
    #     "characters_used": ["Harry", "Hermione"],
    #     "new_characters": ["Luna"],
    #     "scenes_used": ["Great Hall"]
    # }
    
    # Метрики
    processing_time_ms: int = 0
    tokens_used: int = 0
    
    # Timestamp
    generated_at: datetime = None
    
    class Config:
        json_schema_extra = {
            "example": {
                "book_id": "550e8400-e29b-41d4-a716-446655440000",
                "page_id": "660e8400-e29b-41d4-a716-446655440001",
                "page_number": 42,
                "prompts": [
                    {
                        "prompt": "Harry Potter, teenage boy with messy black hair, round glasses, lightning scar on forehead, pointing holly wand, red spell light, Great Hall of Hogwarts, candlelight, dramatic lighting, fantasy illustration style",
                        "negative_prompt": "blurry, low quality, distorted",
                        "scene_description": "Harry casting Expelliarmus",
                        "scene_type": "action",
                        "importance": "high",
                        "characters": ["Harry Potter"],
                        "location": "Great Hall",
                        "parameters": {"aspect_ratio": "16:9"}
                    }
                ],
                "target_model": "dalle3",
                "style": "fantasy",
                "consistency_data": {
                    "characters_used": ["Harry Potter"],
                    "new_characters": []
                },
                "processing_time_ms": 1250
            }
        }


class EnhancePromptResponse(BaseModel):
    """Response для улучшения промпта."""
    
    # Промпты
    original_prompt: str
    enhanced_prompt: str
    negative_prompt: Optional[str] = None
    
    # Улучшения
    improvements: List[str] = []
    # Пример: ["Added lighting details", "Added composition", "Added style modifiers"]
    
    # Параметры
    target_model: str
    style: Optional[str] = None
    
    # Метрики
    length_increase_percent: float = 0


class CharacterConsistencyResponse(BaseModel):
    """Response с данными консистентности персонажа."""
    
    book_id: str
    character_name: str
    
    # Промпты
    appearance_prompt: str  # Готовый фрагмент для вставки в промпт
    clothing_prompt: Optional[str] = None
    
    # Атрибуты
    attributes: Dict[str, str] = {}
    # {
    #     "hair": "messy black hair",
    #     "eyes": "bright green eyes",
    #     "age": "teenager",
    #     "distinguishing_features": "lightning scar, round glasses"
    # }
    
    # Статус
    is_established: bool = False
    generation_count: int = 0


class BatchGenerateResponse(BaseModel):
    """Response для пакетной генерации."""
    
    book_id: str
    
    # Результаты по страницам
    results: List[Dict[str, Any]] = []
    # Каждый элемент:
    # {
    #     "page_id": "...",
    #     "page_number": 1,
    #     "prompts": [...],
    #     "status": "success" | "error",
    #     "error": null | "error message"
    # }
    
    # Статистика
    total_pages: int = 0
    successful: int = 0
    failed: int = 0
    
    # Консистентность
    consistency_data: Dict[str, Any] = {}
    
    # Метрики
    total_processing_time_ms: int = 0


class AnalyzeTextResponse(BaseModel):
    """Response для анализа текста."""
    
    book_id: str
    
    # Извлечённые элементы
    characters: List[Dict[str, Any]] = []
    scenes: List[Dict[str, Any]] = []
    objects: List[Dict[str, Any]] = []
    
    # Анализ
    plot_points: List[str] = []
    mood: Dict[str, str] = {}
    themes: List[str] = []
    
    # Рекомендации
    suggested_visuals: List[str] = []
    narrative_style: Dict[str, Any] = {}