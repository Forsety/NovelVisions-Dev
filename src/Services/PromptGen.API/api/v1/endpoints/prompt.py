# api/v1/endpoints/prompt.py
"""
API endpoints для работы с промптами.

Основные операции:
- Улучшение промпта
- Генерация для страницы
- Получение истории
"""

from typing import Optional, List, Dict, Any
from fastapi import APIRouter, Depends, HTTPException, status, Query, Body
from pydantic import BaseModel, Field

from api.responses import SuccessResponse, ErrorResponse, PaginatedResponse
from api.v1.dependencies import (
    get_current_user,
    get_database,
    get_redis_cache,
    get_prompt_enhancer,
    get_style_engine,
    get_generator_factory,
    get_pagination,
    PaginationParams,
    rate_limit
)
from core.engines.prompt_enhancer import PromptEnhancer, EnhancementContext, EnhancedPrompt
from core.engines.style_engine import StyleEngine
from core.generators.base_generator import GeneratorFactory


router = APIRouter(prefix="/prompts", tags=["Prompts"])


# === Request/Response Models ===

class EnhancePromptRequest(BaseModel):
    """Запрос на улучшение промпта"""
    text: str = Field(..., min_length=1, max_length=5000, description="Текст для улучшения")
    target_model: str = Field(default="midjourney", description="Целевая модель")
    style: Optional[str] = Field(default=None, description="Художественный стиль")
    story_id: Optional[str] = Field(default=None, description="ID истории для консистентности")
    page_number: Optional[int] = Field(default=None, description="Номер страницы")
    maintain_consistency: bool = Field(default=True, description="Поддерживать консистентность")
    parameters: Optional[Dict[str, Any]] = Field(default=None, description="Дополнительные параметры")
    
    # Контекст персонажей и сцен
    characters: Optional[Dict[str, Dict]] = Field(default=None, description="Информация о персонажах")
    scenes: Optional[Dict[str, Dict]] = Field(default=None, description="Информация о сценах")

    class Config:
        json_schema_extra = {
            "example": {
                "text": "The knight raised his sword against the dragon",
                "target_model": "midjourney",
                "style": "fantasy",
                "parameters": {
                    "aspect": "16:9",
                    "quality": "high"
                }
            }
        }


class EnhancePromptResponse(BaseModel):
    """Ответ с улучшенным промптом"""
    original: str
    enhanced: str
    negative_prompt: Optional[str]
    model: str
    style: Optional[str]
    scene_type: str
    quality_score: int
    parameters: Dict[str, Any]
    entities: Dict[str, List]
    composition: Dict[str, str]
    warnings: List[str]


class QuickEnhanceRequest(BaseModel):
    """Быстрое улучшение без контекста"""
    text: str = Field(..., min_length=1, max_length=2000)
    model: str = Field(default="midjourney")
    style: Optional[str] = None


class BatchEnhanceRequest(BaseModel):
    """Пакетное улучшение промптов"""
    texts: List[str] = Field(..., min_items=1, max_items=20)
    target_model: str = Field(default="midjourney")
    style: Optional[str] = None
    story_id: Optional[str] = None


class StyleInfo(BaseModel):
    """Информация о стиле"""
    id: str
    name: str
    category: str
    description: str
    tags: List[str]


class GeneratorInfo(BaseModel):
    """Информация о генераторе"""
    name: str
    display_name: str
    max_prompt_length: int
    supports_negative: bool
    capabilities: List[str]


# === Endpoints ===

@router.post(
    "/enhance",
    response_model=SuccessResponse[EnhancePromptResponse],
    summary="Улучшить промпт",
    description="Улучшает текст из книги в детальный промпт для AI генерации изображений"
)
async def enhance_prompt(
    request: EnhancePromptRequest,
    user: dict = Depends(get_current_user),
    enhancer: PromptEnhancer = Depends(get_prompt_enhancer),
    _: None = Depends(rate_limit(30))  # 30 запросов в минуту
):
    """
    Улучшает промпт с полным анализом и форматированием.
    
    Workflow:
    1. Анализ текста (тип сцены, персонажи, настроение)
    2. Улучшение через GPT-4
    3. Проверка консистентности
    4. Применение стиля
    5. Форматирование под модель
    """
    try:
        # Создаём контекст
        context = EnhancementContext(
            story_id=request.story_id,
            page_number=request.page_number,
            characters=request.characters or {},
            scenes=request.scenes or {},
            style=request.style,
            target_model=request.target_model,
            maintain_consistency=request.maintain_consistency,
            custom_parameters=request.parameters
        )
        
        # Улучшаем промпт
        result = await enhancer.enhance(request.text, context)
        
        # Формируем ответ
        response_data = EnhancePromptResponse(
            original=result.original,
            enhanced=result.enhanced,
            negative_prompt=result.negative_prompt,
            model=result.model,
            style=result.style,
            scene_type=result.scene_type.value,
            quality_score=result.quality_score,
            parameters=result.parameters,
            entities=result.entities,
            composition=result.composition,
            warnings=result.warnings
        )
        
        return SuccessResponse(data=response_data)
        
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Enhancement failed: {str(e)}"
        )


@router.post(
    "/quick",
    response_model=SuccessResponse[dict],
    summary="Быстрое улучшение",
    description="Быстрое улучшение без полного анализа"
)
async def quick_enhance(
    request: QuickEnhanceRequest,
    user: dict = Depends(get_current_user),
    enhancer: PromptEnhancer = Depends(get_prompt_enhancer)
):
    """Быстрое улучшение промпта"""
    try:
        enhanced = await enhancer.quick_enhance(
            text=request.text,
            model=request.model,
            style=request.style
        )
        
        return SuccessResponse(data={
            "original": request.text,
            "enhanced": enhanced,
            "model": request.model,
            "style": request.style
        })
        
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.post(
    "/batch",
    response_model=SuccessResponse[List[EnhancePromptResponse]],
    summary="Пакетное улучшение",
    description="Улучшает несколько промптов за один запрос"
)
async def batch_enhance(
    request: BatchEnhanceRequest,
    user: dict = Depends(get_current_user),
    enhancer: PromptEnhancer = Depends(get_prompt_enhancer),
    _: None = Depends(rate_limit(10))  # 10 batch запросов в минуту
):
    """Пакетное улучшение нескольких промптов"""
    try:
        context = EnhancementContext(
            story_id=request.story_id,
            style=request.style,
            target_model=request.target_model
        )
        
        results = await enhancer.enhance_batch(request.texts, context)
        
        response_data = [
            EnhancePromptResponse(
                original=r.original,
                enhanced=r.enhanced,
                negative_prompt=r.negative_prompt,
                model=r.model,
                style=r.style,
                scene_type=r.scene_type.value,
                quality_score=r.quality_score,
                parameters=r.parameters,
                entities=r.entities,
                composition=r.composition,
                warnings=r.warnings
            )
            for r in results
        ]
        
        return SuccessResponse(data=response_data)
        
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )


@router.get(
    "/styles",
    response_model=SuccessResponse[List[StyleInfo]],
    summary="Список стилей",
    description="Возвращает список доступных художественных стилей"
)
async def list_styles(
    category: Optional[str] = Query(None, description="Фильтр по категории"),
    style_engine: StyleEngine = Depends(get_style_engine)
):
    """Получает список доступных стилей"""
    if category:
        from core.engines.style_engine import StyleCategory
        try:
            cat = StyleCategory(category)
            styles = style_engine.get_styles_by_category(cat)
        except ValueError:
            styles = []
    else:
        styles = style_engine.get_all_styles()
    
    response_data = [
        StyleInfo(
            id=s.id,
            name=s.name,
            category=s.category.value,
            description=s.description,
            tags=s.tags
        )
        for s in styles
    ]
    
    return SuccessResponse(data=response_data)


@router.get(
    "/styles/{style_id}",
    response_model=SuccessResponse[StyleInfo],
    summary="Информация о стиле"
)
async def get_style(
    style_id: str,
    style_engine: StyleEngine = Depends(get_style_engine)
):
    """Получает информацию о конкретном стиле"""
    style = style_engine.get_style(style_id)
    
    if not style:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Style not found: {style_id}"
        )
    
    return SuccessResponse(data=StyleInfo(
        id=style.id,
        name=style.name,
        category=style.category.value,
        description=style.description,
        tags=style.tags
    ))


@router.get(
    "/generators",
    response_model=SuccessResponse[List[GeneratorInfo]],
    summary="Список генераторов",
    description="Возвращает список доступных AI моделей"
)
async def list_generators(
    factory: GeneratorFactory = Depends(get_generator_factory)
):
    """Получает список доступных генераторов"""
    generators = ["midjourney", "dalle3", "stable-diffusion", "flux"]
    
    response_data = []
    for name in generators:
        try:
            info = factory.get_generator_info(name)
            response_data.append(GeneratorInfo(**info))
        except:
            pass
    
    return SuccessResponse(data=response_data)


@router.get(
    "/generators/{generator_name}",
    response_model=SuccessResponse[GeneratorInfo],
    summary="Информация о генераторе"
)
async def get_generator(
    generator_name: str,
    factory: GeneratorFactory = Depends(get_generator_factory)
):
    """Получает информацию о генераторе"""
    try:
        info = factory.get_generator_info(generator_name)
        return SuccessResponse(data=GeneratorInfo(**info))
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=str(e)
        )


@router.post(
    "/format",
    response_model=SuccessResponse[dict],
    summary="Форматировать промпт",
    description="Форматирует готовый промпт под конкретную модель"
)
async def format_prompt(
    prompt: str = Body(..., embed=True),
    model: str = Body(default="midjourney", embed=True),
    style: Optional[str] = Body(default=None, embed=True),
    parameters: Optional[Dict[str, Any]] = Body(default=None, embed=True),
    factory: GeneratorFactory = Depends(get_generator_factory)
):
    """Форматирует промпт для конкретной модели без улучшения"""
    try:
        generator = factory.get_generator(model)
        
        formatted = await generator.generate(
            text=prompt,
            style=style,
            parameters=parameters
        )
        
        negative = None
        if generator.supports_negative:
            negative = generator.get_negative_prompt(style=style)
        
        return SuccessResponse(data={
            "original": prompt,
            "formatted": formatted,
            "negative_prompt": negative,
            "model": model,
            "parameters": generator.get_parameters()
        })
        
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=str(e)
        )