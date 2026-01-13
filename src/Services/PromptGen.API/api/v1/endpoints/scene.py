from typing import List, Optional
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession

from api.v1.dependencies import get_database, get_current_user, get_redis_cache
from api.responses import SuccessResponse, PaginatedResponse
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
from core.managers.scene_manager import SceneManager

router = APIRouter()


@router.post("/", response_model=SuccessResponse[SceneResponse])
async def create_scene(
    request: SceneCreateRequest,
    db: AsyncSession = Depends(get_database),
    user: dict = Depends(get_current_user),
    cache = Depends(get_redis_cache)
):
    """Create a new scene"""
    try:
        manager = SceneManager(db, cache)
        
        scene = await manager.create(
            name=request.name,
            description=request.description,
            location=request.location,
            time_of_day=request.time_of_day,
            weather=request.weather,
            lighting=request.lighting,
            atmosphere=request.atmosphere,
            objects=request.objects,
            user_id=user["user_id"]
        )
        
        return SuccessResponse(
            message="Scene created successfully",
            data=scene
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get("/{scene_id}", response_model=SuccessResponse[SceneDetailResponse])
async def get_scene(
    scene_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Get scene by ID"""
    try:
        manager = SceneManager(db, cache)
        
        scene = await manager.get(scene_id)
        if not scene:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Scene not found"
            )
        
        return SuccessResponse(
            message="Scene retrieved successfully",
            data=scene
        )
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.put("/{scene_id}", response_model=SuccessResponse[SceneResponse])
async def update_scene(
    scene_id: str,
    request: SceneUpdateRequest,
    db: AsyncSession = Depends(get_database),
    user: dict = Depends(get_current_user),
    cache = Depends(get_redis_cache)
):
    """Update a scene"""
    try:
        manager = SceneManager(db, cache)
        
        scene = await manager.update(
            scene_id=scene_id,
            updates=request.dict(exclude_unset=True),
            user_id=user["user_id"]
        )
        
        return SuccessResponse(
            message="Scene updated successfully",
            data=scene
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.post("/{scene_id}/prompt", response_model=SuccessResponse[ScenePromptResponse])
async def generate_scene_prompt(
    scene_id: str,
    request: ScenePromptRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Generate a prompt for the scene"""
    try:
        manager = SceneManager(db, cache)
        
        prompt = await manager.generate_prompt(
            scene_id=scene_id,
            camera_angle=request.camera_angle,
            focus=request.focus,
            mood=request.mood,
            characters=request.characters,
            target_model=request.target_model
        )
        
        return SuccessResponse(
            message="Scene prompt generated successfully",
            data=prompt
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )
