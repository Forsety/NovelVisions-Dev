# models/schemas/story_schemas.py
"""
Pydantic schemas для Story API.
"""

from typing import Optional, Dict, Any, List
from datetime import datetime
from pydantic import BaseModel, Field


class StoryCreateRequest(BaseModel):
    """Запрос на создание истории"""
    title: str = Field(..., min_length=1, max_length=500)
    author: Optional[str] = None
    description: Optional[str] = None
    genre: Optional[str] = None
    language: str = Field(default="en", max_length=10)
    external_book_id: Optional[str] = None
    default_style: Optional[str] = None
    default_model: str = Field(default="midjourney")
    visualization_mode: str = Field(default="per_page")
    generation_settings: Optional[Dict[str, Any]] = None
    metadata: Optional[Dict[str, Any]] = None

    class Config:
        json_schema_extra = {
            "example": {
                "title": "The Lord of the Rings",
                "author": "J.R.R. Tolkien",
                "genre": "fantasy",
                "default_style": "fantasy",
                "default_model": "midjourney"
            }
        }


class StoryUpdateRequest(BaseModel):
    """Запрос на обновление истории"""
    title: Optional[str] = Field(None, min_length=1, max_length=500)
    author: Optional[str] = None
    description: Optional[str] = None
    genre: Optional[str] = None
    default_style: Optional[str] = None
    default_model: Optional[str] = None
    visualization_mode: Optional[str] = None
    generation_settings: Optional[Dict[str, Any]] = None
    metadata: Optional[Dict[str, Any]] = None


class StoryResponse(BaseModel):
    """Ответ с информацией об истории"""
    id: str
    user_id: str
    external_book_id: Optional[str]
    title: str
    author: Optional[str]
    description: Optional[str]
    genre: Optional[str]
    language: str
    default_style: Optional[str]
    default_model: str
    visualization_mode: str
    generation_settings: Optional[Dict[str, Any]]
    metadata: Optional[Dict[str, Any]]
    total_pages: int
    total_chapters: int
    generated_images_count: int
    is_active: bool
    created_at: Optional[datetime]
    updated_at: Optional[datetime]

    class Config:
        from_attributes = True


class StoryListResponse(BaseModel):
    """Ответ со списком историй"""
    items: List[StoryResponse]
    total: int
    skip: int
    limit: int
    has_more: bool