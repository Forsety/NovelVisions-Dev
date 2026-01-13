# src/Services/PromptGen.API/models/schemas/response/base.py
"""
Base response schemas
"""
from typing import TypeVar, Generic, Optional, List, Any
from pydantic import BaseModel, Field

T = TypeVar("T")


class SuccessResponse(BaseModel, Generic[T]):
    """Успешный ответ"""
    
    success: bool = True
    message: str
    data: Optional[T] = None


class ErrorResponse(BaseModel):
    """Ответ с ошибкой"""
    
    success: bool = False
    error: str
    details: Optional[List[str]] = None
    code: Optional[str] = None


class PaginatedResponse(BaseModel, Generic[T]):
    """Пагинированный ответ"""
    
    items: List[T]
    total: int
    page: int
    page_size: int
    total_pages: int
    
    @property
    def has_next(self) -> bool:
        return self.page < self.total_pages
    
    @property
    def has_previous(self) -> bool:
        return self.page > 1