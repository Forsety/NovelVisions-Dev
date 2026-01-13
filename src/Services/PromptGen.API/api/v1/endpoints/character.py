from typing import List, Optional
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession

from api.v1.dependencies import get_database, get_current_user, get_redis_cache
from api.responses import SuccessResponse, PaginatedResponse
from models.schemas.request.character_request import (
    CharacterCreateRequest,
    CharacterUpdateRequest,
    CharacterPromptRequest
)
from models.schemas.response.character_response import (
    CharacterResponse,
    CharacterDetailResponse,
    CharacterPromptResponse
)
from core.managers.character_manager import CharacterManager

router = APIRouter()


@router.post("/", response_model=SuccessResponse[CharacterResponse])
async def create_character(
    request: CharacterCreateRequest,
    db: AsyncSession = Depends(get_database),
    user: dict = Depends(get_current_user),
    cache = Depends(get_redis_cache)
):
    """Create a new character"""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.create(
            name=request.name,
            description=request.description,
            appearance=request.appearance,
            personality=request.personality,
            clothing=request.clothing,
            attributes=request.attributes,
            user_id=user["user_id"]
        )
        
        return SuccessResponse(
            message="Character created successfully",
            data=character
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get("/{character_id}", response_model=SuccessResponse[CharacterDetailResponse])
async def get_character(
    character_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Get character by ID"""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.get(character_id)
        if not character:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Character not found"
            )
        
        return SuccessResponse(
            message="Character retrieved successfully",
            data=character
        )
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.put("/{character_id}", response_model=SuccessResponse[CharacterResponse])
async def update_character(
    character_id: str,
    request: CharacterUpdateRequest,
    db: AsyncSession = Depends(get_database),
    user: dict = Depends(get_current_user),
    cache = Depends(get_redis_cache)
):
    """Update a character"""
    try:
        manager = CharacterManager(db, cache)
        
        character = await manager.update(
            character_id=character_id,
            updates=request.dict(exclude_unset=True),
            user_id=user["user_id"]
        )
        
        return SuccessResponse(
            message="Character updated successfully",
            data=character
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.delete("/{character_id}")
async def delete_character(
    character_id: str,
    db: AsyncSession = Depends(get_database),
    user: dict = Depends(get_current_user),
    cache = Depends(get_redis_cache)
):
    """Delete a character"""
    try:
        manager = CharacterManager(db, cache)
        
        await manager.delete(character_id, user["user_id"])
        
        return SuccessResponse(
            message="Character deleted successfully"
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get("/", response_model=PaginatedResponse[CharacterResponse])
async def list_characters(
    page: int = 1,
    page_size: int = 20,
    search: Optional[str] = None,
    db: AsyncSession = Depends(get_database),
    user: dict = Depends(get_current_user),
    cache = Depends(get_redis_cache)
):
    """List user's characters"""
    try:
        manager = CharacterManager(db, cache)
        
        result = await manager.list(
            user_id=user["user_id"],
            page=page,
            page_size=page_size,
            search=search
        )
        
        return result
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.post("/{character_id}/prompt", response_model=SuccessResponse[CharacterPromptResponse])
async def generate_character_prompt(
    character_id: str,
    request: CharacterPromptRequest,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Generate a prompt for the character"""
    try:
        manager = CharacterManager(db, cache)
        
        prompt = await manager.generate_prompt(
            character_id=character_id,
            action=request.action,
            emotion=request.emotion,
            pose=request.pose,
            scene_context=request.scene_context,
            target_model=request.target_model
        )
        
        return SuccessResponse(
            message="Character prompt generated successfully",
            data=prompt
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )
