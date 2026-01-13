# src/Services/PromptGen.API/core/managers/visualization_manager.py
"""
Visualization Manager - главный менеджер для генерации промптов визуализации.
Интегрируется с Catalog.API и Visualization.API.
"""
import json
import time
import logging
from typing import Dict, List, Optional, Any
from datetime import datetime

from sqlalchemy.ext.asyncio import AsyncSession

from models.schemas.request.visualization_request import (
    GeneratePromptsRequest, 
    EnhancePromptRequest,
    TargetModel,
    VisualizationStyle
)
from models.schemas.response.visualization_response import (
    GeneratePromptsResponse,
    GeneratedPrompt,
    EnhancePromptResponse,
    CharacterConsistencyResponse
)
from models.domain.book_context import BookContext, CharacterProfile, SceneContext
from services.storage.cache_service import CacheService
from services.ai.openai_service import OpenAIService
from core.engines.prompt_enhancer import PromptEnhancer
from core.engines.consistency_engine import ConsistencyEngine
from core.engines.context_analyzer import ContextAnalyzer

logger = logging.getLogger(__name__)


class VisualizationManager:
    """
    Менеджер визуализации - координирует генерацию промптов для Visualization.API
    """
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.ai_service = OpenAIService()
        self.prompt_enhancer = PromptEnhancer(db, cache)
        self.consistency_engine = ConsistencyEngine(db, cache)
        self.context_analyzer = ContextAnalyzer(db, cache)
        
        # Настройки по умолчанию для разных моделей
        self.model_defaults = {
            "dalle3": {
                "max_length": 4000,
                "style_suffix": ", highly detailed, professional quality",
                "negative_default": "blurry, low quality, distorted, deformed, ugly, bad anatomy"
            },
            "midjourney": {
                "max_length": 6000,
                "style_suffix": " --q 2 --s 750",
                "aspect_ratios": {"portrait": "--ar 2:3", "landscape": "--ar 3:2", "square": "--ar 1:1"}
            },
            "stable-diffusion": {
                "max_length": 380,
                "style_suffix": ", masterpiece, best quality, highly detailed",
                "negative_default": "lowres, bad anatomy, bad hands, text, error, missing fingers"
            },
            "flux": {
                "max_length": 2000,
                "style_suffix": ", ultra high quality, photorealistic",
                "negative_default": "blurry, low resolution, artifacts"
            }
        }
    
    async def generate_prompts(
        self,
        request: GeneratePromptsRequest
    ) -> GeneratePromptsResponse:
        """
        Главный метод генерации промптов для страницы.
        Вызывается из Visualization.API.
        """
        start_time = time.time()
        
        logger.info(
            f"Generating prompts for book={request.book_id}, "
            f"page={request.page_number}, model={request.target_model}"
        )
        
        # 1. Получить или создать контекст книги
        book_context = await self._get_or_create_book_context(request)
        
        # 2. Анализ текста страницы
        page_analysis = await self._analyze_page(request.page_content)
        
        # 3. Извлечь персонажей из текста
        extracted_characters = await self._extract_characters(
            request.page_content, 
            page_analysis
        )
        
        # 4. Обновить профили персонажей
        for char_name in extracted_characters:
            if char_name not in book_context.characters:
                # Создать новый профиль
                profile = await self._create_character_profile(
                    char_name, 
                    request.page_content,
                    request.book_id
                )
                book_context.add_character(profile)
        
        # 5. Определить визуальные моменты
        visual_moments = await self._identify_visual_moments(
            request.page_content,
            page_analysis,
            request.max_prompts
        )
        
        # 6. Генерация промптов для каждого момента
        prompts = []
        for moment in visual_moments:
            prompt = await self._generate_moment_prompt(
                moment=moment,
                book_context=book_context,
                request=request,
                page_analysis=page_analysis
            )
            prompts.append(prompt)
        
        # 7. Сохранить контекст в кэш
        await self._save_book_context(book_context)
        
        processing_time = int((time.time() - start_time) * 1000)
        
        return GeneratePromptsResponse(
            book_id=request.book_id,
            chapter_id=request.chapter_id,
            page_id=request.page_id,
            page_number=request.page_number,
            prompts=prompts,
            analysis=page_analysis,
            character_context={
                name: profile.to_dict() 
                for name, profile in book_context.characters.items()
                if name in extracted_characters
            },
            target_model=request.target_model,
            style=request.style,
            processing_time_ms=processing_time
        )
    
    async def enhance_prompt(
        self,
        request: EnhancePromptRequest
    ) -> EnhancePromptResponse:
        """Улучшить существующий промпт"""
        
        # Получить контекст книги если указан
        character_context = {}
        if request.book_id and request.character_names:
            book_context = await self._get_book_context(request.book_id)
            if book_context:
                for name in request.character_names:
                    profile = book_context.get_character(name)
                    if profile:
                        character_context[name] = profile.to_prompt_fragment()
        
        # Улучшить промпт
        enhanced = await self.prompt_enhancer.enhance(
            text=request.prompt,
            model=request.target_model,
            style=request.style,
            character_context=character_context
        )
        
        # Добавить негативный промпт
        model_config = self.model_defaults.get(request.target_model, {})
        negative_prompt = model_config.get("negative_default", "")
        
        return EnhancePromptResponse(
            original_prompt=request.prompt,
            enhanced_prompt=enhanced["enhanced"],
            negative_prompt=negative_prompt,
            improvements=enhanced.get("improvements", []),
            target_model=request.target_model,
            style=request.style
        )
    
    async def get_character_consistency(
        self,
        book_id: str,
        character_name: str
    ) -> Optional[CharacterConsistencyResponse]:
        """Получить данные консистентности персонажа"""
        
        book_context = await self._get_book_context(book_id)
        if not book_context:
            return None
        
        profile = book_context.get_character(character_name)
        if not profile:
            return None
        
        return CharacterConsistencyResponse(
            book_id=book_id,
            character_name=character_name,
            appearance_prompt=profile.to_prompt_fragment(),
            clothing_prompt=profile.clothing,
            attributes={
                "hair": profile.hair or "",
                "eyes": profile.eyes or "",
                "age": profile.age or "",
                "build": profile.build or "",
                "distinguishing_features": profile.distinguishing_features or ""
            },
            is_established=profile.is_established,
            generation_count=profile.generation_count,
            created_at=profile.created_at,
            updated_at=profile.updated_at
        )
    
    # === Private Methods ===
    
    async def _get_or_create_book_context(
        self, 
        request: GeneratePromptsRequest
    ) -> BookContext:
        """Получить или создать контекст книги"""
        
        # Попробовать из кэша
        context = await self._get_book_context(request.book_id)
        
        if not context:
            # Создать новый контекст
            context = BookContext(
                book_id=request.book_id,
                title=request.book_title,
                genre=request.book_genre,
                preferred_style=request.style
            )
        
        # Обновить текущую позицию
        context.current_chapter = request.chapter_number
        context.current_page = request.page_number
        
        return context
    
    async def _get_book_context(self, book_id: str) -> Optional[BookContext]:
        """Получить контекст книги из кэша"""
        
        cache_key = f"book_context:{book_id}"
        cached = await self.cache.get(cache_key)
        
        if not cached:
            return None
        
        try:
            data = json.loads(cached)
            context = BookContext(
                book_id=data["book_id"],
                title=data.get("title"),
                genre=data.get("genre"),
                style=data.get("style")
            )
            
            # Восстановить персонажей
            for name, char_data in data.get("characters", {}).items():
                profile = CharacterProfile(
                    name=name,
                    book_id=book_id,
                    appearance=char_data.get("appearance", ""),
                    hair=char_data.get("hair"),
                    eyes=char_data.get("eyes"),
                    age=char_data.get("age"),
                    build=char_data.get("build"),
                    clothing=char_data.get("clothing"),
                    distinguishing_features=char_data.get("distinguishing_features"),
                    generation_count=char_data.get("generation_count", 0),
                    is_established=char_data.get("is_established", False)
                )
                context.add_character(profile)
            
            return context
        except Exception as e:
            logger.error(f"Failed to parse book context: {e}")
            return None
    
    async def _save_book_context(self, context: BookContext) -> None:
        """Сохранить контекст книги в кэш"""
        
        cache_key = f"book_context:{context.book_id}"
        await self.cache.set(
            cache_key, 
            json.dumps(context.to_dict()),
            expire=86400  # 24 часа
        )
    
    async def _analyze_page(self, text: str) -> Dict[str, Any]:
        """Анализ текста страницы"""
        
        system_prompt = """Analyze this text and extract:
        - mood: overall emotional tone
        - setting: location/environment
        - key_actions: main actions happening
        - time_of_day: if mentioned
        - weather: if mentioned
        - atmosphere: descriptive atmosphere
        Return as JSON."""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=text[:3000],  # Ограничить длину
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return {
                "mood": "neutral",
                "setting": "unspecified",
                "key_actions": [],
                "atmosphere": "general"
            }
    
    async def _extract_characters(
        self, 
        text: str,
        analysis: Dict[str, Any]
    ) -> List[str]:
        """Извлечь имена персонажей из текста"""
        
        system_prompt = """Extract all character names mentioned in this text.
        Return as JSON array of strings with just the names.
        Include only proper character names, not pronouns or generic terms."""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=text[:3000],
            response_format="json"
        )
        
        try:
            names = json.loads(response)
            return [n.strip() for n in names if isinstance(n, str) and n.strip()]
        except:
            return []
    
    async def _create_character_profile(
        self,
        name: str,
        context_text: str,
        book_id: str
    ) -> CharacterProfile:
        """Создать профиль персонажа на основе контекста"""
        
        system_prompt = f"""Based on this text, extract visual details about the character "{name}".
        Return JSON with these fields (use null if not mentioned):
        - appearance: general description
        - hair: hair color and style
        - eyes: eye color
        - age: approximate age or age group
        - build: body type/build
        - clothing: what they're wearing
        - distinguishing_features: any unique features"""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=context_text[:2000],
            response_format="json"
        )
        
        try:
            data = json.loads(response)
            return CharacterProfile(
                name=name,
                book_id=book_id,
                appearance=data.get("appearance") or f"{name}",
                hair=data.get("hair"),
                eyes=data.get("eyes"),
                age=data.get("age"),
                build=data.get("build"),
                clothing=data.get("clothing"),
                distinguishing_features=data.get("distinguishing_features")
            )
        except:
            return CharacterProfile(name=name, book_id=book_id)
    
    async def _identify_visual_moments(
        self,
        text: str,
        analysis: Dict[str, Any],
        max_moments: int = 1
    ) -> List[Dict[str, Any]]:
        """Определить ключевые визуальные моменты"""
        
        system_prompt = f"""Identify the most visually impactful moments in this text.
        Return JSON array with maximum {max_moments} moments.
        Each moment should have:
        - description: what's happening visually
        - type: action, emotion, establishing, reveal, dialogue
        - importance: high, medium, low
        - characters: list of character names involved
        - scene_elements: key visual elements (objects, environment details)
        - suggested_composition: portrait, landscape, or square"""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=f"Text: {text[:3000]}\n\nAnalysis: {json.dumps(analysis)}",
            response_format="json"
        )
        
        try:
            moments = json.loads(response)
            return moments[:max_moments]
        except:
            return [{
                "description": "Scene from the text",
                "type": "establishing",
                "importance": "medium",
                "characters": [],
                "scene_elements": [],
                "suggested_composition": "square"
            }]
    
    async def _generate_moment_prompt(
        self,
        moment: Dict[str, Any],
        book_context: BookContext,
        request: GeneratePromptsRequest,
        page_analysis: Dict[str, Any]
    ) -> GeneratedPrompt:
        """Генерация промпта для визуального момента"""
        
        parts = []
        
        # 1. Основное описание момента
        parts.append(moment["description"])
        
        # 2. Добавить описания персонажей
        for char_name in moment.get("characters", []):
            profile = book_context.get_character(char_name)
            if profile:
                char_desc = profile.to_prompt_fragment()
                parts.append(f"{char_name}: {char_desc}")
                profile.generation_count += 1
                profile.is_established = True
        
        # 3. Добавить элементы сцены
        if moment.get("scene_elements"):
            parts.extend(moment["scene_elements"])
        
        # 4. Добавить атмосферу из анализа
        if page_analysis.get("atmosphere"):
            parts.append(f"{page_analysis['atmosphere']} atmosphere")
        
        if page_analysis.get("time_of_day"):
            parts.append(f"{page_analysis['time_of_day']} lighting")
        
        # 5. Добавить стиль
        if request.style:
            parts.append(f"{request.style} style")
        elif book_context.preferred_style:
            parts.append(f"{book_context.preferred_style} style")
        
        # 6. Добавить подсказку автора
        if request.author_hint:
            parts.append(request.author_hint)
        
        # 7. Собрать промпт
        base_prompt = ", ".join(parts)
        
        # 8. Улучшить промпт для целевой модели
        model_config = self.model_defaults.get(request.target_model, {})
        
        # Обрезать до максимальной длины
        max_length = model_config.get("max_length", 2000)
        if len(base_prompt) > max_length:
            base_prompt = base_prompt[:max_length - 3] + "..."
        
        # Добавить суффикс модели
        style_suffix = model_config.get("style_suffix", "")
        final_prompt = base_prompt + style_suffix
        
        # 9. Негативный промпт
        negative_prompt = None
        if request.include_negative_prompt:
            negative_prompt = model_config.get(
                "negative_default", 
                "blurry, low quality, distorted"
            )
        
        # 10. Определить соотношение сторон
        composition = moment.get("suggested_composition", "square")
        aspect_ratio_map = {
            "portrait": "2:3",
            "landscape": "3:2",
            "square": "1:1",
            "wide": "16:9",
            "tall": "9:16"
        }
        aspect_ratio = aspect_ratio_map.get(composition, "1:1")
        
        return GeneratedPrompt(
            prompt=final_prompt,
            negative_prompt=negative_prompt,
            moment_description=moment["description"],
            moment_type=moment.get("type", "establishing"),
            importance=moment.get("importance", "medium"),
            characters=moment.get("characters", []),
            scene_elements=moment.get("scene_elements", []),
            suggested_aspect_ratio=aspect_ratio,
            suggested_parameters=self._get_model_parameters(
                request.target_model, 
                aspect_ratio
            )
        )
    
    def _get_model_parameters(
        self, 
        model: str, 
        aspect_ratio: str
    ) -> Dict[str, Any]:
        """Получить параметры для конкретной модели"""
        
        params = {"aspect_ratio": aspect_ratio}
        
        if model == "dalle3":
            params["quality"] = "hd"
            params["style"] = "vivid"
            # DALL-E 3 поддерживает только определенные размеры
            size_map = {
                "1:1": "1024x1024",
                "2:3": "1024x1792",
                "3:2": "1792x1024",
                "16:9": "1792x1024",
                "9:16": "1024x1792"
            }
            params["size"] = size_map.get(aspect_ratio, "1024x1024")
        
        elif model == "midjourney":
            ar_map = {"1:1": "--ar 1:1", "2:3": "--ar 2:3", "3:2": "--ar 3:2"}
            params["ar_suffix"] = ar_map.get(aspect_ratio, "--ar 1:1")
            params["quality"] = "--q 2"
            params["stylize"] = "--s 750"
        
        elif model == "stable-diffusion":
            params["steps"] = 30
            params["cfg_scale"] = 7.5
            size_map = {
                "1:1": (1024, 1024),
                "2:3": (832, 1216),
                "3:2": (1216, 832)
            }
            params["size"] = size_map.get(aspect_ratio, (1024, 1024))
        
        elif model == "flux":
            params["guidance_scale"] = 3.5
            params["num_inference_steps"] = 50
        
        return params