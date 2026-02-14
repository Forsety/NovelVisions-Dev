# api/responses.py
"""
Унифицированные модели ответов API.
"""

from typing import Generic, TypeVar, Optional, List, Any, Dict
from pydantic import BaseModel, Field
from datetime import datetime


T = TypeVar('T')


class SuccessResponse(BaseModel, Generic[T]):
    """Успешный ответ"""
    success: bool = True
    data: T
    message: Optional[str] = None
    timestamp: datetime = Field(default_factory=datetime.utcnow)


class ErrorResponse(BaseModel):
    """Ответ с ошибкой"""
    success: bool = False
    error: str
    error_code: Optional[str] = None
    details: Optional[Dict[str, Any]] = None
    timestamp: datetime = Field(default_factory=datetime.utcnow)


class PaginatedResponse(BaseModel, Generic[T]):
    """Пагинированный ответ"""
    success: bool = True
    data: List[T]
    total: int
    skip: int
    limit: int
    has_more: bool
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    
    @classmethod
    def create(
        cls,
        items: List[T],
        total: int,
        skip: int,
        limit: int
    ) -> "PaginatedResponse[T]":
        return cls(
            data=items,
            total=total,
            skip=skip,
            limit=limit,
            has_more=(skip + len(items)) < total
        )


class ValidationErrorDetail(BaseModel):
    """Детали ошибки валидации"""
    field: str
    message: str
    value: Optional[Any] = None


class ValidationErrorResponse(BaseModel):
    """Ответ с ошибками валидации"""
    success: bool = False
    error: str = "Validation error"
    errors: List[ValidationErrorDetail]
    timestamp: datetime = Field(default_factory=datetime.utcnow)


class HealthResponse(BaseModel):
    """Ответ health check"""
    status: str  # healthy, degraded, unhealthy
    version: str
    services: Dict[str, bool]
    timestamp: datetime = Field(default_factory=datetime.utcnow)