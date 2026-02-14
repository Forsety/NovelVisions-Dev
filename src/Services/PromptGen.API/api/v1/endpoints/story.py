# api/v1/endpoints/story.py
"""
API endpoints для работы с историями/книгами.
"""

from typing import Optional, List
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession

from api.responses import SuccessResponse, PaginatedResponse
from api.v1.dependencies import (
    get_current_user,
    get_database,
    get_pagination,
    PaginationParams
)
from models.schemas.story_schemas import (
    StoryCreateRequest,
    StoryUpdateRequest,
    StoryResponse,
    StoryListResponse
)
from models.domain.story import Story
from services.storage.database_service import DatabaseService


router = APIRouter(prefix="/stories", tags=["Stories"])


# === Endpoints ===

@router.post(
    "",
    response_model=SuccessResponse[StoryResponse],
    status_code=status.HTTP_201_CREATED,
    summary="Создать историю"
)
async def create_story(
    request: StoryCreateRequest,
    user: dict = Depends(get_current_user),
    db: AsyncSession = Depends(get_database)
):
    """Создаёт новую историю/книгу"""
    
    service = DatabaseService(Story)
    
    story = await service.create(
        db,
        user_id=user["user_id"],
        title=request.title,
        author=request.author,
        description=request.description,
        genre=request.genre,
        language=request.language,
        external_book_id=request.external_book_id,
        default_style=request.default_style,
        default_model=request.default_model,
        visualization_mode=request.visualization_mode,
        generation_settings=request.generation_settings or {},
        metadata=request.metadata or {}
    )
    
    return SuccessResponse(data=StoryResponse.model_validate(story))


@router.get(
    "",
    response_model=SuccessResponse[StoryListResponse],
    summary="Список историй"
)
async def list_stories(
    user: dict = Depends(get_current_user),
    db: AsyncSession = Depends(get_database),
    pagination: PaginationParams = Depends(get_pagination),
    genre: Optional[str] = Query(None, description="Фильтр по жанру"),
    is_active: bool = Query(True, description="Только активные")
):
    """Получает список историй пользователя"""
    
    service = DatabaseService(Story)
    
    filters = {"user_id": user["user_id"]}
    if genre:
        filters["genre"] = genre
    if is_active is not None:
        filters["is_active"] = is_active
    
    stories = await service.get_all(
        db,
        skip=pagination.skip,
        limit=pagination.limit,
        filters=filters,
        order_by="created_at",
        descending=True
    )
    
    total = await service.count(db, filters=filters)
    
    return SuccessResponse(data=StoryListResponse(
        items=[StoryResponse.model_validate(s) for s in stories],
        total=total,
        skip=pagination.skip,
        limit=pagination.limit,
        has_more=(pagination.skip + len(stories)) < total
    ))


@router.get(
    "/{story_id}",
    response_model=SuccessResponse[StoryResponse],
    summary="Получить историю"
)
async def get_story(
    story_id: str,
    user: dict = Depends(get_current_user),
    db: AsyncSession = Depends(get_database)
):
    """Получает историю по ID"""
    
    service = DatabaseService(Story)
    story = await service.get_by_id(db, story_id)
    
    if not story:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Story not found: {story_id}"
        )
    
    # Проверяем владельца
    if story.user_id != user["user_id"]:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Access denied"
        )
    
    return SuccessResponse(data=StoryResponse.model_validate(story))


@router.put(
    "/{story_id}",
    response_model=SuccessResponse[StoryResponse],
    summary="Обновить историю"
)
async def update_story(
    story_id: str,
    request: StoryUpdateRequest,
    user: dict = Depends(get_current_user),
    db: AsyncSession = Depends(get_database)
):
    """Обновляет историю"""
    
    service = DatabaseService(Story)
    story = await service.get_by_id(db, story_id)
    
    if not story:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Story not found: {story_id}"
        )
    
    if story.user_id != user["user_id"]:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Access denied"
        )
    
    # Обновляем только переданные поля
    update_data = request.model_dump(exclude_unset=True)
    
    updated_story = await service.update(db, story_id, **update_data)
    
    return SuccessResponse(data=StoryResponse.model_validate(updated_story))


@router.delete(
    "/{story_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Удалить историю"
)
async def delete_story(
    story_id: str,
    user: dict = Depends(get_current_user),
    db: AsyncSession = Depends(get_database)
):
    """Удаляет историю"""
    
    service = DatabaseService(Story)
    story = await service.get_by_id(db, story_id)
    
    if not story:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Story not found: {story_id}"
        )
    
    if story.user_id != user["user_id"]:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Access denied"
        )
    
    await service.delete(db, story_id)