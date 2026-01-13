from typing import List
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession

from api.v1.dependencies import get_database, get_redis_cache
from api.responses import SuccessResponse
from models.schemas.response.style_response import (
    StyleResponse,
    StylePresetResponse
)
from core.engines.style_engine import StyleEngine

router = APIRouter()


@router.get("/presets", response_model=SuccessResponse[List[StylePresetResponse]])
async def get_style_presets(
    category: Optional[str] = None,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Get available style presets"""
    try:
        engine = StyleEngine(db, cache)
        
        presets = await engine.get_presets(category)
        
        return SuccessResponse(
            message="Style presets retrieved successfully",
            data=presets
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get("/{style_id}", response_model=SuccessResponse[StyleResponse])
async def get_style(
    style_id: str,
    db: AsyncSession = Depends(get_database),
    cache = Depends(get_redis_cache)
):
    """Get style details"""
    try:
        engine = StyleEngine(db, cache)
        
        style = await engine.get_style(style_id)
        if not style:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Style not found"
            )
        
        return SuccessResponse(
            message="Style retrieved successfully",
            data=style
        )
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )
