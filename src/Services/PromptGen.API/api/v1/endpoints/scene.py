# api/v1/endpoints/scene.py
"""
Scene API endpoints.
РЕФАКТОРИНГ: story_id → book_id
"""

import logging
from typing import List, Optional
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession

from services.storage.database_service import get_database
from services.storage.cache_service import get_redis_cache
from models.schemas.request.scene_request import (
    SceneCreateRequest,
    SceneUpdateRequest,
    ScenePromptRequest
)
from models.schemas.response.scene_response import (
    SceneResponse,
    SceneDetailResponse,
    ScenePromptResponse
)
from models.schemas.response.base import SuccessResponse, PaginatedResponse
from core.managers.scene_manager import SceneManager

logger = logging.getLogger(__name__)

router = APIRouter()


# ===========================================
# CREATE
# ===========================================

@router.post(
    "/",
    response_model=SuccessResponse[SceneResponse],
    status_code=status.HTTP_201_CREATED,
    summary="Create a new scene",
    description="Создание новой сцены/локации для книги. book_id - ID книги из Catalog.API."
)
async def create_scene(
    request: SceneCreateRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Создать новую сцену/локацию."""
    try:
        manager = SceneManager(db, cache)
        
        scene = await manager.create(
            book_id=request.book_id,
            name=request.name,
            description=request.description,
            location_type=request.location_type,
            setting_type=request.setting_type,
            architecture=request.architecture,
            materials=request.materials,
            colors=request.colors,
            textures=request.textures,
            atmosphere=request.atmosphere,
            mood=request.mood,
            default_lighting=request.lighting,
            light_sources=request.light_sources,
            default_weather=request.weather,
            typical_time_of_day=request.time_of_day,
            time_period=request.time_period,
            season=request.season,
            key_elements=request.key_elements,
            decorations=request.decorations,
            furniture=request.furniture,
            vegetation=request.vegetation,
            scale=request.scale,
            aliases=request.aliases,
            attributes=request.attributes,
            reference_image_url=request.reference_image_url
        )
        
        return SuccessResponse(
            message="Scene created successfully",
            data=scene
        )
        
    except Exception as e:
        logger.exception(f"Error creating scene: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# READ
# ===========================================

@router.get(
    "/{scene_id}",
    response_model=SuccessResponse[SceneDetailResponse],
    summary="Get scene by ID"
)
async def get_scene(
    scene_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Получить сцену по ID."""
    try:
        manager = SceneManager(db, cache)
        
        scene = await manager.get(scene_id)
        if not scene:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Scene with ID {scene_id} not found"
            )
        
        return SuccessResponse(
            message="Scene retrieved successfully",
            data=scene
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error getting scene: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get(
    "/book/{book_id}",
    response_model=PaginatedResponse[SceneResponse],
    summary="List scenes by book"
)
async def list_scenes_by_book(
    book_id: str,
    page: int = Query(default=1, ge=1),
    page_size: int = Query(default=20, ge=1, le=100),
    search: Optional[str] = Query(None, description="Search by name"),
    location_type: Optional[str] = Query(None, description="Filter by location type"),
    setting_type: Optional[str] = Query(None, description="Filter by setting type"),
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Получить список сцен книги."""
    try:
        manager = SceneManager(db, cache)
        
        result = await manager.list_by_book(
            book_id=book_id,
            page=page,
            page_size=page_size,
            search=search,
            location_type=location_type,
            setting_type=setting_type
        )
        
        return result
        
    except Exception as e:
        logger.exception(f"Error listing scenes: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get(
    "/book/{book_id}/by-name/{name}",
    response_model=SuccessResponse[SceneDetailResponse],
    summary="Get scene by name in book"
)
async def get_scene_by_name(
    book_id: str,
    name: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Найти сцену по имени в книге."""
    try:
        manager = SceneManager(db, cache)
        
        scene = await manager.get_by_name(book_id, name)
        if not scene:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Scene '{name}' not found in book {book_id}"
            )
        
        return SuccessResponse(
            message="Scene found",
            data=scene
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error finding scene by name: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# UPDATE
# ===========================================

@router.put(
    "/{scene_id}",
    response_model=SuccessResponse[SceneResponse],
    summary="Update scene"
)
async def update_scene(
    scene_id: str,
    request: SceneUpdateRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Обновить сцену."""
    try:
        manager = SceneManager(db, cache)
        
        # Map request fields to model fields
        update_data = request.model_dump(exclude_unset=True)
        
        # Remap field names if needed
        if 'lighting' in update_data:
            update_data['default_lighting'] = update_data.pop('lighting')
        if 'weather' in update_data:
            update_data['default_weather'] = update_data.pop('weather')
        if 'time_of_day' in update_data:
            update_data['typical_time_of_day'] = update_data.pop('time_of_day')
        
        scene = await manager.update(scene_id=scene_id, **update_data)
        
        if not scene:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Scene with ID {scene_id} not found"
            )
        
        return SuccessResponse(
            message="Scene updated successfully",
            data=scene
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error updating scene: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.patch(
    "/{scene_id}/establish",
    response_model=SuccessResponse[SceneResponse],
    summary="Mark scene as established"
)
async def establish_scene(
    scene_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Пометить сцену как 'установленную' (описание зафиксировано)."""
    try:
        manager = SceneManager(db, cache)
        
        scene = await manager.mark_established(scene_id)
        if not scene:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Scene with ID {scene_id} not found"
            )
        
        return SuccessResponse(
            message="Scene marked as established",
            data=scene
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error establishing scene: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# DELETE
# ===========================================

@router.delete(
    "/{scene_id}",
    response_model=SuccessResponse,
    summary="Delete scene"
)
async def delete_scene(
    scene_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Удалить сцену."""
    try:
        manager = SceneManager(db, cache)
        
        deleted = await manager.delete(scene_id)
        if not deleted:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Scene with ID {scene_id} not found"
            )
        
        return SuccessResponse(
            message="Scene deleted successfully"
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error deleting scene: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


# ===========================================
# PROMPT GENERATION
# ===========================================

@router.post(
    "/{scene_id}/prompt",
    response_model=SuccessResponse[ScenePromptResponse],
    summary="Generate prompt for scene"
)
async def generate_scene_prompt(
    scene_id: str,
    request: ScenePromptRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Сгенерировать промпт для сцены с учётом консистентности."""
    try:
        manager = SceneManager(db, cache)
        
        prompt = await manager.generate_prompt(
            scene_id=scene_id,
            time_of_day=request.time_of_day,
            weather=request.weather,
            lighting=request.lighting,
            characters=request.characters,
            action=request.action,
            target_model=request.target_model,
            style=request.style,
            camera_angle=request.camera_angle,
            shot_type=request.shot_type
        )
        
        if not prompt:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"Scene with ID {scene_id} not found"
            )
        
        return SuccessResponse(
            message="Scene prompt generated successfully",
            data=prompt
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.exception(f"Error generating scene prompt: {e}")
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
    summary="Delete all scenes in book"
)
async def delete_all_scenes_in_book(
    book_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Удалить все сцены книги."""
    try:
        manager = SceneManager(db, cache)
        
        count = await manager.delete_by_book(book_id)
        
        return SuccessResponse(
            message=f"Deleted {count} scenes from book {book_id}"
        )
        
    except Exception as e:
        logger.exception(f"Error deleting scenes: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )