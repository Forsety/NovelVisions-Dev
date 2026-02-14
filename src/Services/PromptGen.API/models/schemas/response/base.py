# models/schemas/response/base.py
"""
Base response schemas.
"""

from typing import TypeVar, Generic, Optional, List, Any, Dict
from pydantic import BaseModel, Field
from datetime import datetime

T = TypeVar('T')


class SuccessResponse(BaseModel, Generic[T]):
    """Стандартный успешный response."""
    
    success: bool = True
    message: str = "Operation completed successfully"
    data: Optional[T] = None
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    
    class Config:
        from_attributes = True


class ErrorResponse(BaseModel):
    """Стандартный error response."""
    
    success: bool = False
    error: str
    error_code: Optional[str] = None
    details: Optional[Dict[str, Any]] = None
    timestamp: datetime = Field(default_factory=datetime.utcnow)


class PaginationMeta(BaseModel):
    """Метаданные пагинации."""
    
    page: int
    page_size: int
    total_items: int
    total_pages: int
    has_next: bool
    has_prev: bool


class PaginatedResponse(BaseModel, Generic[T]):
    """Response с пагинацией."""
    
    success: bool = True
    message: str = "Data retrieved successfully"
    data: List[T] = []
    pagination: PaginationMeta
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    
    class Config:
        from_attributes = True
    
    @classmethod
    def create(
        cls,
        items: List[T],
        page: int,
        page_size: int,
        total_items: int,
        message: str = "Data retrieved successfully"
    ) -> 'PaginatedResponse[T]':
        total_pages = (total_items + page_size - 1) // page_size if page_size > 0 else 0
        
        return cls(
            message=message,
            data=items,
            pagination=PaginationMeta(
                page=page,
                page_size=page_size,
                total_items=total_items,
                total_pages=total_pages,
                has_next=page < total_pages,
                has_prev=page > 1
            )
        )


class HealthResponse(BaseModel):
    """Health check response."""
    
    status: str = "healthy"
    service: str = "promptgen-api"
    version: str = "1.0.0"
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    dependencies: Dict[str, str] = {}