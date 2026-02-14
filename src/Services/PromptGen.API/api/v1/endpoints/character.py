# api/v1/endpoints/character.py
"""
Character API endpoints.
РЕФАКТОРИНГ: story_id → book_id
"""

import logging
from typing import List, Optional
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession

from services.storage.database_service import get_database
from services.storage.cache_service import get_redis_cache
from models.schemas.request.character_request import (
    CharacterCreateRequest,
    CharacterUpdateRequest,
    CharacterPromptRequest,
    CharacterSearchRequest
)
from models.schemas.response.character_response import (
    CharacterResponse,
    CharacterDetailResponse,
    CharacterPromptResponse
)
from models.schemas.response.base import SuccessResponse, PaginatedResponse
from core.managers.character_manager import CharacterManager

logger = logging.getLogger(__name__)

router = APIRouter()


# ===========================================
# CREATE
# ===========================================

@router.post(
    "/",
    response_model=SuccessResponse[CharacterResponse],
    status_code=status.HTTP_201_CREATED,
    summary="Create a new character",
    description="Создание нового персонажа для книги. book_id - ID книги из Catalog.API."
)
async def create_character(
    request: CharacterCreateRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Создать нового персонажа."""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.create(
            book_id=request.book_id,
            name=request.name,
            description=request.description,
            role=request.role,
            gender=request.gender,
            age=request.age,
            height=request.height,
            build=request.build,
            appearance=request.appearance,
            hair=request.hair,
            eyes=request.eyes,
            skin=request.skin,
            facial_features=request.facial_features,
            distinguishing_features=request.distinguishing_features,
            default_clothing=request.default_clothing,
            accessories=request.accessories,
            aliases=request.aliases,
            attributes=request.attributes,
            reference_image_url=request.reference_image_url
        )
        
        return SuccessResponse(
            message="Character created successfully",
            data=character
        )
        
    except Exception as e:
        logger.exception(f"Error creating character: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# READ
# ===========================================

@router.get(
    "/{character_id}",
    response_model=SuccessResponse[CharacterDetailResponse],
    summary="Get character by ID"
)
async def get_character(
    character_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Получить персонажа по ID."""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.get(character_id)
        if not character:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Character with ID {character_id} not found"
            )
        
        return SuccessResponse(
            message="Character retrieved successfully",
            data=character
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error getting character: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get(
    "/book/{book_id}",
    response_model=PaginatedResponse[CharacterResponse],
    summary="List characters by book"
)
async def list_characters_by_book(
    book_id: str,
    page: int = Query(default=1, ge=1),
    page_size: int = Query(default=20, ge=1, le=100),
    search: Optional[str] = Query(None, description="Search by name"),
    role: Optional[str] = Query(None, description="Filter by role"),
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Получить список персонажей книги."""
    try:
        manager = CharacterManager(db, cache)
        
        result = await manager.list_by_book(
            book_id=book_id,
            page=page,
            page_size=page_size,
            search=search,
            role=role
        )
        
        return result
        
    except Exception as e:
        logger.exception(f"Error listing characters: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get(
    "/book/{book_id}/by-name/{name}",
    response_model=SuccessResponse[CharacterDetailResponse],
    summary="Get character by name in book"
)
async def get_character_by_name(
    book_id: str,
    name: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Найти персонажа по имени в книге."""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.get_by_name(book_id, name)
        if not character:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Character '{name}' not found in book {book_id}"
            )
        
        return SuccessResponse(
            message="Character found",
            data=character
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error finding character by name: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# UPDATE
# ===========================================

@router.put(
    "/{character_id}",
    response_model=SuccessResponse[CharacterResponse],
    summary="Update character"
)
async def update_character(
    character_id: str,
    request: CharacterUpdateRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Обновить персонажа."""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.update(
            character_id=character_id,
            **request.model_dump(exclude_unset=True)
        )
        
        if not character:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Character with ID {character_id} not found"
            )
        
        return SuccessResponse(
            message="Character updated successfully",
            data=character
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error updating character: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.patch(
    "/{character_id}/establish",
    response_model=SuccessResponse[CharacterResponse],
    summary="Mark character as established"
)
async def establish_character(
    character_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Пометить персонажа как 'установленного' (описание зафиксировано)."""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.mark_established(character_id)
        if not character:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Character with ID {character_id} not found"
            )
        
        return SuccessResponse(
            message="Character marked as established",
            data=character
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error establishing character: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# DELETE
# ===========================================

@router.delete(
    "/{character_id}",
    response_model=SuccessResponse,
    summary="Delete character"
)
async def delete_character(
    character_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Удалить персонажа."""
    try:
        manager = CharacterManager(db, cache)
        
        deleted = await manager.delete(character_id)
        if not deleted:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Character with ID {character_id} not found"
            )
        
        return SuccessResponse(
            message="Character deleted successfully"
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error deleting character: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# PROMPT GENERATION
# ===========================================

@router.post(
    "/{character_id}/prompt",
    response_model=SuccessResponse[CharacterPromptResponse],
    summary="Generate prompt for character"
)
async def generate_character_prompt(
    character_id: str,
    request: CharacterPromptRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Сгенерировать промпт для персонажа с учётом консистентности."""
    try:
        manager = CharacterManager(db, cache)
        
        prompt = await manager.generate_prompt(
            character_id=character_id,
            action=request.action,
            emotion=request.emotion,
            pose=request.pose,
            scene_context=request.scene_context,
            target_model=request.target_model,
            style=request.style
        )
        
        if not prompt:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Character with ID {character_id} not found"
            )
        
        return SuccessResponse(
            message="Character prompt generated successfully",
            data=prompt
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error generating character prompt: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# BULK OPERATIONS
# ===========================================

@router.delete(
    "/book/{book_id}/all",
    response_model=SuccessResponse,
    summary="Delete all characters in book"
)
async def delete_all_characters_in_book(
    book_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Удалить всех персонажей книги."""
    try:
        manager = CharacterManager(db, cache)
        
        count = await manager.delete_by_book(book_id)
        
        return SuccessResponse(
            message=f"Deleted {count} characters from book {book_id}"
        )
        
    except Exception as e:
        logger.exception(f"Error deleting characters: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )