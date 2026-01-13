# src/Services/PromptGen.API/api/v1/endpoints/visualization.py
"""
Visualization API endpoints - главные эндпоинты для Visualization.API
"""
import logging
from typing import Optional, List
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession

from models.schemas.request.visualization_request import (
    GeneratePromptsRequest,
    EnhancePromptRequest,
    CharacterConsistencyRequest,
    BatchGenerateRequest
)
from models.schemas.response.visualization_response import (
    GeneratePromptsResponse,
    EnhancePromptResponse,
    CharacterConsistencyResponse,
    BatchGenerateResponse
)
from models.schemas.response.base import SuccessResponse, ErrorResponse
from core.managers.visualization_manager import VisualizationManager
from services.storage.cache_service import get_redis_cache
from services.storage.database_service import get_database

logger = logging.getLogger(__name__)

router = APIRouter()


@router.post(
    "/generate-prompts",
    response_model=SuccessResponse[GeneratePromptsResponse],
    responses={
        400: {"model": ErrorResponse},
        500: {"model": ErrorResponse}
    },
    summary="Generate visualization prompts for a page",
    description="""
    Главный эндпоинт для генерации промптов визуализации.
    Вызывается из Visualization.API при создании задания на генерацию изображения.
    
    Процесс:
    1. Анализ текста страницы
    2. Извлечение персонажей и сцен
    3. Поддержание консистентности персонажей
    4. Генерация оптимизированных промптов для целевой модели
    """
)
async def generate_prompts(
    request: GeneratePromptsRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """
    Генерация промптов для страницы книги.
    
    - **book_id**: ID книги из Catalog.API
    - **page_content**: Текст страницы для анализа
    - **target_model**: Целевая AI модель (dalle3, midjourney, stable-diffusion, flux)
    - **style**: Стиль визуализации
    - **maintain_consistency**: Поддерживать консистентность персонажей
    """
    try:
        manager = VisualizationManager(db, cache)
        result = await manager.generate_prompts(request)
        
        return SuccessResponse(
            message="Prompts generated successfully",
            data=result
        )
    
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        logger.exception(f"Error generating prompts: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to generate prompts: {str(e)}"
        )


@router.post(
    "/enhance",
    response_model=SuccessResponse[EnhancePromptResponse],
    summary="Enhance an existing prompt",
    description="Улучшить существующий промпт для целевой модели"
)
async def enhance_prompt(
    request: EnhancePromptRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """
    Улучшение существующего промпта.
    
    - **prompt**: Исходный промпт
    - **target_model**: Целевая модель
    - **book_id**: ID книги для контекста персонажей (опционально)
    """
    try:
        manager = VisualizationManager(db, cache)
        result = await manager.enhance_prompt(request)
        
        return SuccessResponse(
            message="Prompt enhanced successfully",
            data=result
        )
    
    except Exception as e:
        logger.exception(f"Error enhancing prompt: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get(
    "/character-consistency/{book_id}/{character_name}",
    response_model=SuccessResponse[CharacterConsistencyResponse],
    responses={404: {"model": ErrorResponse}},
    summary="Get character consistency data",
    description="Получить данные консистентности персонажа для книги"
)
async def get_character_consistency(
    book_id: str,
    character_name: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """
    Получить данные консистентности персонажа.
    
    Используется Visualization.API для поддержания визуальной
    консистентности персонажа между генерациями.
    """
    try:
        manager = VisualizationManager(db, cache)
        result = await manager.get_character_consistency(book_id, character_name)
        
        if not result:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Character '{character_name}' not found in book {book_id}"
            )
        
        return SuccessResponse(
            message="Character consistency data retrieved",
            data=result
        )
    
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error getting character consistency: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.post(
    "/character-consistency",
    response_model=SuccessResponse[CharacterConsistencyResponse],
    summary="Create or update character consistency",
    description="Создать или обновить данные консистентности персонажа"
)
async def set_character_consistency(
    request: CharacterConsistencyRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """
    Создать или обновить профиль персонажа для консистентности.
    
    Позволяет авторам задать фиксированное описание персонажа.
    """
    try:
        manager = VisualizationManager(db, cache)
        
        # Получить или создать контекст книги
        from models.domain.book_context import BookContext, CharacterProfile
        
        book_context = await manager._get_book_context(request.book_id)
        if not book_context:
            book_context = BookContext(book_id=request.book_id)
        
        # Создать/обновить профиль
        profile = CharacterProfile(
            name=request.character_name,
            book_id=request.book_id,
            appearance=request.appearance or "",
            clothing=request.clothing,
            distinguishing_features=request.distinguishing_features,
            is_established=True
        )
        
        book_context.add_character(profile)
        await manager._save_book_context(book_context)
        
        result = await manager.get_character_consistency(
            request.book_id, 
            request.character_name
        )
        
        return SuccessResponse(
            message="Character consistency data saved",
            data=result
        )
    
    except Exception as e:
        logger.exception(f"Error saving character consistency: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.post(
    "/batch-generate",
    response_model=SuccessResponse[BatchGenerateResponse],
    summary="Batch generate prompts for multiple pages",
    description="Пакетная генерация промптов для нескольких страниц"
)
async def batch_generate_prompts(
    request: BatchGenerateRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """
    Пакетная генерация промптов для нескольких страниц.
    
    Оптимизировано для генерации всей главы или книги.
    """
    import time
    start_time = time.time()
    
    try:
        manager = VisualizationManager(db, cache)
        
        results = []
        errors = []
        
        for page_data in request.pages:
            try:
                page_request = GeneratePromptsRequest(
                    book_id=request.book_id,
                    chapter_id=page_data.get("chapter_id", ""),
                    page_id=page_data.get("page_id", ""),
                    page_content=page_data["content"],
                    page_number=page_data["page_number"],
                    chapter_number=page_data.get("chapter_number", 1),
                    target_model=request.target_model,
                    style=request.style,
                    maintain_consistency=request.maintain_consistency
                )
                
                result = await manager.generate_prompts(page_request)
                results.append(result)
                
            except Exception as e:
                errors.append({
                    "page_number": page_data.get("page_number"),
                    "error": str(e)
                })
        
        total_time = int((time.time() - start_time) * 1000)
        
        return SuccessResponse(
            message=f"Batch generation completed: {len(results)} successful, {len(errors)} failed",
            data=BatchGenerateResponse(
                book_id=request.book_id,
                total_pages=len(request.pages),
                successful=len(results),
                failed=len(errors),
                results=results,
                errors=errors,
                total_processing_time_ms=total_time
            )
        )
    
    except Exception as e:
        logger.exception(f"Error in batch generation: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get(
    "/health",
    summary="Health check",
    description="Проверка доступности сервиса"
)
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "service": "PromptGen.API",
        "version": "2.0.0"
    }