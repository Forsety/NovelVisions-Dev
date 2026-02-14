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
    EnhancePromptRequest
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
            target_model=request.target_model,
            style=request.style,
            consistency_data={
                "characters_used": list(extracted_characters),
                "new_characters": []
            },
            processing_time_ms=processing_time,
            tokens_used=0,
            generated_at=datetime.utcnow()
        )
    
    async def enhance_prompt(
        self,
        request: EnhancePromptRequest
    ) -> EnhancePromptResponse:
        """Улучшить существующий промпт"""
        
        # Получить контекст книги если указан
        character_context = {}
        if request.book_id:
            book_context = await self._get_book_context(request.book_id)
            if book_context:
                for name, profile in book_context.characters.items():
                    character_context[name] = profile.to_prompt_fragment()
        
        # Улучшить промпт через AI
        enhanced_text = await self._enhance_with_ai(
            request.prompt,
            request.target_model,
            request.style,
            character_context
        )
        
        # Добавить негативный промпт
        model_config = self.model_defaults.get(request.target_model, {})
        negative_prompt = model_config.get("negative_default", "")
        
        return EnhancePromptResponse(
            original_prompt=request.prompt,
            enhanced_prompt=enhanced_text,
            negative_prompt=negative_prompt,
            improvements=["Added visual details", "Optimized for target model"],
            target_model=request.target_model,
            style=request.style,
            length_increase_percent=((len(enhanced_text) - len(request.prompt)) / len(request.prompt)) * 100 if request.prompt else 0
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
            appearance=profile.appearance,
            clothing=profile.clothing,
            distinguishing_features=[profile.distinguishing_features] if profile.distinguishing_features else [],
            prompt_fragment=profile.to_prompt_fragment(),
            is_established=profile.is_established,
            generation_count=profile.generation_count,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
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
            # Создать новый контекст с правильными аргументами
            context = BookContext(
                book_id=request.book_id,
                default_style=request.style,
                default_model=request.target_model or "dalle3"
            )
        
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
                default_style=data.get("default_style"),
                default_model=data.get("default_model", "dalle3")
            )
            
            # Восстановить персонажей
            for name, char_data in data.get("characters", {}).items():
                profile = CharacterProfile(
                    name=char_data["name"],
                    book_id=char_data["book_id"],
                    appearance=char_data.get("appearance", ""),
                    hair=char_data.get("hair"),
                    eyes=char_data.get("eyes"),
                    clothing=char_data.get("clothing"),
                    distinguishing_features=char_data.get("distinguishing_features"),
                    is_established=char_data.get("is_established", False),
                    generation_count=char_data.get("generation_count", 0)
                )
                context.characters[name] = profile
            
            # Восстановить сцены
            for name, scene_data in data.get("scenes", {}).items():
                scene = SceneContext(
                    name=scene_data["name"],
                    book_id=scene_data["book_id"],
                    description=scene_data.get("description", ""),
                    atmosphere=scene_data.get("atmosphere"),
                    lighting=scene_data.get("lighting"),
                    is_established=scene_data.get("is_established", False)
                )
                context.scenes[name] = scene
            
            return context
            
        except Exception as e:
            logger.error(f"Error loading book context from cache: {e}")
            return None
    
    async def _save_book_context(self, context: BookContext) -> None:
        """Сохранить контекст книги в кэш"""
        
        cache_key = f"book_context:{context.book_id}"
        
        data = {
            "book_id": context.book_id,
            "default_style": context.default_style,
            "default_model": context.default_model,
            "characters": {
                name: profile.to_dict()
                for name, profile in context.characters.items()
            },
            "scenes": {
                name: scene.to_dict()
                for name, scene in context.scenes.items()
            },
            "objects": {
                name: obj.to_dict()
                for name, obj in context.objects.items()
            }
        }
        
        await self.cache.set(cache_key, json.dumps(data), expire=86400)  # 24 часа
    
    async def _analyze_page(self, text: str) -> Dict[str, Any]:
        """Анализ текста страницы через AI"""
        
        system_prompt = """Analyze this text excerpt from a book and extract:
1. Main action or scene happening
2. Characters present (names only)
3. Setting/location
4. Mood/atmosphere
5. Key visual elements

Return as JSON with keys: action, characters, location, mood, visual_elements"""
        
        try:
            response = await self.ai_service.generate(
                user_prompt=text[:2000],  # Ограничиваем длину
                system_prompt=system_prompt,
                response_format="json",
                max_tokens=500
            )
            return json.loads(response)
        except Exception as e:
            logger.error(f"Error analyzing page: {e}")
            return {
                "action": "",
                "characters": [],
                "location": "",
                "mood": "",
                "visual_elements": []
            }
    
    async def _extract_characters(
        self,
        text: str,
        analysis: Dict[str, Any]
    ) -> List[str]:
        """Извлечь имена персонажей из текста"""
        
        # Из анализа
        characters = analysis.get("characters", [])
        
        if not characters:
            # Попробовать извлечь через AI
            try:
                response = await self.ai_service.generate(
                    user_prompt=f"Extract all character names from this text. Return only a JSON array of names:\n\n{text[:1500]}",
                    system_prompt="You extract character names from text. Return only valid JSON array.",
                    response_format="json",
                    max_tokens=200
                )
                characters = json.loads(response)
            except Exception as e:
                logger.error(f"Error extracting characters: {e}")
                characters = []
        
        return characters if isinstance(characters, list) else []
    
    async def _create_character_profile(
        self,
        name: str,
        text: str,
        book_id: str
    ) -> CharacterProfile:
        """Создать профиль персонажа на основе текста"""
        
        system_prompt = """Extract physical description of the character from the text.
Return JSON with: gender, age, hair, eyes, build, clothing, distinguishing_features
If not mentioned, use null."""
        
        try:
            response = await self.ai_service.generate(
                user_prompt=f"Character: {name}\n\nText:\n{text[:1500]}",
                system_prompt=system_prompt,
                response_format="json",
                max_tokens=300
            )
            data = json.loads(response)
            
            return CharacterProfile(
                name=name,
                book_id=book_id,
                gender=data.get("gender"),
                age=data.get("age"),
                hair=data.get("hair"),
                eyes=data.get("eyes"),
                build=data.get("build"),
                clothing=data.get("clothing"),
                distinguishing_features=data.get("distinguishing_features"),
                is_established=False,
                generation_count=0
            )
        except Exception as e:
            logger.error(f"Error creating character profile: {e}")
            return CharacterProfile(
                name=name,
                book_id=book_id,
                is_established=False
            )
    
    async def _identify_visual_moments(
        self,
        text: str,
        analysis: Dict[str, Any],
        max_moments: int = 1
    ) -> List[Dict[str, Any]]:
        """Определить визуальные моменты для иллюстрации"""
        
        system_prompt = f"""Identify the {max_moments} most visually interesting moment(s) in this text.
For each moment, provide:
- description: brief description of what's happening
- characters: list of characters present
- action: what they're doing
- location: where it's happening
- mood: emotional tone

Return as JSON array."""
        
        try:
            response = await self.ai_service.generate(
                user_prompt=text[:2000],
                system_prompt=system_prompt,
                response_format="json",
                max_tokens=500
            )
            moments = json.loads(response)
            return moments if isinstance(moments, list) else [moments]
        except Exception as e:
            logger.error(f"Error identifying visual moments: {e}")
            # Fallback: использовать анализ
            return [{
                "description": analysis.get("action", text[:200]),
                "characters": analysis.get("characters", []),
                "action": analysis.get("action", ""),
                "location": analysis.get("location", ""),
                "mood": analysis.get("mood", "")
            }]
    
    async def _generate_moment_prompt(
        self,
        moment: Dict[str, Any],
        book_context: BookContext,
        request: GeneratePromptsRequest,
        page_analysis: Dict[str, Any]
    ) -> GeneratedPrompt:
        """Сгенерировать промпт для конкретного момента"""
        
        # Собрать описания персонажей
        character_descriptions = []
        for char_name in moment.get("characters", []):
            profile = book_context.get_character(char_name)
            if profile:
                character_descriptions.append(profile.to_prompt_fragment())
            else:
                character_descriptions.append(char_name)
        
        # Построить промпт
        system_prompt = f"""Create an image generation prompt for {request.target_model}.

The prompt should describe:
- Scene: {moment.get('description', '')}
- Characters: {', '.join(character_descriptions) if character_descriptions else 'unspecified'}
- Action: {moment.get('action', '')}
- Location: {moment.get('location', '')}
- Mood: {moment.get('mood', '')}
- Style: {request.style or 'detailed illustration'}

Return ONLY the prompt text, optimized for {request.target_model}. 
Include visual details: lighting, composition, atmosphere.
Do not include any explanations."""
        
        try:
            enhanced_prompt = await self.ai_service.generate(
                user_prompt=f"Create prompt for: {moment.get('description', '')}",
                system_prompt=system_prompt,
                max_tokens=500
            )
        except Exception as e:
            logger.error(f"Error generating prompt: {e}")
            # Fallback
            enhanced_prompt = f"{moment.get('description', '')}, {request.style or 'detailed illustration'}"
        
        # Добавить суффикс модели
        model_config = self.model_defaults.get(request.target_model, self.model_defaults["dalle3"])
        enhanced_prompt += model_config.get("style_suffix", "")
        
        # Обрезать если нужно
        max_length = model_config.get("max_length", 4000)
        if len(enhanced_prompt) > max_length:
            enhanced_prompt = enhanced_prompt[:max_length - 3] + "..."
        
        return GeneratedPrompt(
            prompt=enhanced_prompt,
            negative_prompt=model_config.get("negative_default", ""),
            scene_description=moment.get("description", ""),
            scene_type="action" if moment.get("action") else "establishing",
            importance="high",
            characters=moment.get("characters", []),
            objects=[],
            location=moment.get("location"),
            parameters={}
        )
    
    async def _enhance_with_ai(
        self,
        prompt: str,
        target_model: str,
        style: Optional[str],
        character_context: Dict[str, str]
    ) -> str:
        """Улучшить промпт через AI"""
        
        system_prompt = f"""Enhance this image generation prompt for {target_model}.

Guidelines:
- Add visual details (lighting, colors, textures)
- Include composition elements (camera angle, framing)
- Add atmospheric elements (mood, weather, time of day)
- Be concise but descriptive
- Style: {style or 'detailed illustration'}

Character references to include:
{json.dumps(character_context, indent=2) if character_context else 'None'}

Return ONLY the enhanced prompt."""
        
        try:
            enhanced = await self.ai_service.generate(
                user_prompt=f"Enhance this prompt:\n{prompt}",
                system_prompt=system_prompt,
                max_tokens=500
            )
            return enhanced
        except Exception as e:
            logger.error(f"Error enhancing prompt: {e}")
            return prompt