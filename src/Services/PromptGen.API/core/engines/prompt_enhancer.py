# core/engines/prompt_enhancer.py
"""
Prompt Enhancer Engine - улучшение промптов для различных AI моделей
"""
import json
import logging
import hashlib
from typing import Dict, List, Optional, Any
from dataclasses import dataclass, field
from enum import Enum
from sqlalchemy.ext.asyncio import AsyncSession

from services.ai.openai_service import OpenAIService
from services.storage.cache_service import CacheService

logger = logging.getLogger(__name__)


# ===========================================
# ENUMS & DATA CLASSES
# ===========================================

class SceneType(Enum):
    """Тип сцены для визуализации"""
    ACTION = "action"
    DIALOGUE = "dialogue"
    LANDSCAPE = "landscape"
    PORTRAIT = "portrait"
    INTERIOR = "interior"
    EMOTIONAL = "emotional"
    DRAMATIC = "dramatic"
    PEACEFUL = "peaceful"
    UNKNOWN = "unknown"


@dataclass
class EnhancementContext:
    """Контекст для улучшения промпта"""
    story_id: Optional[str] = None
    book_id: Optional[str] = None
    page_number: Optional[int] = None
    chapter_number: Optional[int] = None
    characters: Optional[Dict[str, Dict]] = None
    scenes: Optional[Dict[str, Dict]] = None
    style: Optional[str] = None
    target_model: str = "midjourney"
    maintain_consistency: bool = True
    custom_parameters: Optional[Dict[str, Any]] = None
    
    def __post_init__(self):
        if self.characters is None:
            self.characters = {}
        if self.scenes is None:
            self.scenes = {}
        if self.custom_parameters is None:
            self.custom_parameters = {}


@dataclass
class EnhancedPrompt:
    """Результат улучшения промпта"""
    original: str
    enhanced: str
    negative_prompt: Optional[str] = None
    model: str = "midjourney"
    style: Optional[str] = None
    scene_type: SceneType = SceneType.UNKNOWN
    quality_score: int = 0
    parameters: Dict[str, Any] = field(default_factory=dict)
    entities: Dict[str, List] = field(default_factory=dict)
    composition: Dict[str, str] = field(default_factory=dict)
    warnings: List[str] = field(default_factory=list)


class PromptEnhancer:
    """
    Engine для улучшения промптов под конкретные AI модели.
    Преобразует простой текст в детальный, оптимизированный промпт.
    """
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.ai_service = OpenAIService()
        
        # Оптимизации для разных моделей
        self.model_optimizations = {
            "midjourney": {
                "max_length": 4000,
                "quality_terms": ["highly detailed", "8k", "professional", "masterpiece"],
                "suffix": "--v 6 --ar 16:9 --q 2",
                "style": "photorealistic"
            },
            "dalle3": {
                "max_length": 4000,
                "quality_terms": ["highly detailed", "professional photography", "sharp focus"],
                "suffix": "",
                "style": "natural"
            },
            "stable_diffusion": {
                "max_length": 2000,
                "quality_terms": ["highly detailed", "8k uhd", "sharp focus", "masterpiece"],
                "suffix": "",
                "style": "detailed"
            },
            "flux": {
                "max_length": 2000,
                "quality_terms": ["high quality", "detailed", "professional"],
                "suffix": "",
                "style": "cinematic"
            }
        }
        
        # Шаблоны стилей
        self.style_templates = {
            "fantasy": "epic fantasy art style, magical atmosphere, dramatic lighting",
            "anime": "anime style, vibrant colors, clean lines, manga aesthetic",
            "realistic": "photorealistic, hyperrealistic, natural lighting, detailed",
            "cinematic": "cinematic composition, movie still, dramatic lighting, film grain",
            "watercolor": "watercolor painting, soft edges, artistic, flowing colors",
            "oil_painting": "oil painting style, rich textures, classical art",
            "comic": "comic book style, bold lines, dynamic poses, vibrant",
            "noir": "film noir style, high contrast, black and white, shadows",
            "steampunk": "steampunk aesthetic, brass and copper, Victorian era, mechanical",
            "cyberpunk": "cyberpunk style, neon lights, futuristic, dystopian"
        }
        
        # Негативные промпты по умолчанию
        self.default_negative_prompts = {
            "midjourney": "blurry, low quality, distorted, deformed, ugly, bad anatomy",
            "dalle3": "",
            "stable_diffusion": "lowres, bad anatomy, bad hands, text, error, missing fingers, extra digit, fewer digits, cropped, worst quality, low quality, normal quality, jpeg artifacts, signature, watermark, username, blurry",
            "flux": "low quality, blurry, distorted"
        }
    
    async def enhance(
        self,
        text: str,
        context: EnhancementContext
    ) -> EnhancedPrompt:
        """
        Улучшает промпт с полным анализом.
        
        Args:
            text: Исходный текст
            context: Контекст улучшения
            
        Returns:
            EnhancedPrompt с результатами
        """
        
        model = context.target_model
        style = context.style
        
        # Проверить кэш
        cache_key = f"enhance:{model}:{style}:{self._hash(text)}"
        cached = await self.cache.get(cache_key)
        if cached:
            data = json.loads(cached)
            return EnhancedPrompt(**data)
        
        model_config = self.model_optimizations.get(model, self.model_optimizations["dalle3"])
        
        # 1. Анализ исходного текста
        analysis = await self._analyze_input(text)
        scene_type = self._determine_scene_type(analysis)
        
        # 2. Расширение описания
        expanded = await self._expand_description(text, analysis, model_config)
        
        # 3. Добавление контекста персонажей
        if context.characters and context.maintain_consistency:
            char_descriptions = []
            for name, char_data in context.characters.items():
                if isinstance(char_data, dict):
                    desc = char_data.get("appearance", char_data.get("description", name))
                else:
                    desc = str(char_data)
                char_descriptions.append(f"{name}: {desc}")
            if char_descriptions:
                expanded = f"{expanded}. Characters: {', '.join(char_descriptions)}"
        
        # 4. Добавление стиля
        if style and style in self.style_templates:
            expanded = f"{expanded}, {self.style_templates[style]}"
        
        # 5. Добавление качественных терминов
        quality_terms = ", ".join(model_config.get("quality_terms", []))
        if quality_terms:
            expanded = f"{expanded}, {quality_terms}"
        
        # 6. Оптимизация под модель
        optimized = await self._optimize_for_model(expanded, model, model_config)
        
        # 7. Обрезка до максимальной длины
        max_length = model_config.get("max_length", 2000)
        if len(optimized) > max_length:
            optimized = optimized[:max_length - 3] + "..."
        
        # 8. Добавление суффикса модели
        suffix = model_config.get("suffix", "")
        if suffix:
            optimized = f"{optimized} {suffix}"
        
        # 9. Получить негативный промпт
        negative_prompt = self.default_negative_prompts.get(model, "")
        
        # 10. Оценка качества
        quality_score = self._calculate_quality_score(text, optimized)
        
        # Формируем результат
        result = EnhancedPrompt(
            original=text,
            enhanced=optimized,
            negative_prompt=negative_prompt if negative_prompt else None,
            model=model,
            style=style,
            scene_type=scene_type,
            quality_score=quality_score,
            parameters=context.custom_parameters or {},
            entities={
                "characters": analysis.get("characters", []),
                "objects": analysis.get("objects", []),
                "locations": analysis.get("locations", [])
            },
            composition={
                "mood": analysis.get("mood", "neutral"),
                "lighting": analysis.get("lighting", "natural"),
                "perspective": analysis.get("perspective", "medium shot")
            },
            warnings=[]
        )
        
        # Кэшировать
        await self.cache.set(cache_key, json.dumps({
            "original": result.original,
            "enhanced": result.enhanced,
            "negative_prompt": result.negative_prompt,
            "model": result.model,
            "style": result.style,
            "scene_type": result.scene_type.value,
            "quality_score": result.quality_score,
            "parameters": result.parameters,
            "entities": result.entities,
            "composition": result.composition,
            "warnings": result.warnings
        }), expire=3600)
        
        return result
    
    async def enhance_batch(
        self,
        texts: List[str],
        context: EnhancementContext
    ) -> List[EnhancedPrompt]:
        """Пакетное улучшение промптов"""
        results = []
        for text in texts:
            result = await self.enhance(text, context)
            results.append(result)
        return results
    
    async def quick_enhance(
        self,
        text: str,
        model: str = "midjourney",
        style: Optional[str] = None
    ) -> str:
        """Быстрое улучшение без полного анализа"""
        
        model_config = self.model_optimizations.get(model, self.model_optimizations["dalle3"])
        
        # Простое улучшение
        enhanced = text
        
        # Добавить стиль
        if style and style in self.style_templates:
            enhanced = f"{enhanced}, {self.style_templates[style]}"
        
        # Добавить качество
        quality_terms = ", ".join(model_config.get("quality_terms", [])[:2])
        if quality_terms:
            enhanced = f"{enhanced}, {quality_terms}"
        
        # Суффикс
        suffix = model_config.get("suffix", "")
        if suffix:
            enhanced = f"{enhanced} {suffix}"
        
        return enhanced
    
    def _hash(self, text: str) -> str:
        """Генерирует хэш текста для кэширования"""
        return hashlib.md5(text.encode()).hexdigest()[:12]
    
    def _determine_scene_type(self, analysis: Dict[str, Any]) -> SceneType:
        """Определяет тип сцены на основе анализа"""
        
        scene_hints = analysis.get("scene_type_hints", [])
        mood = analysis.get("mood", "").lower()
        
        if "action" in scene_hints or "fight" in mood or "battle" in mood:
            return SceneType.ACTION
        elif "dialogue" in scene_hints or "conversation" in mood:
            return SceneType.DIALOGUE
        elif "landscape" in scene_hints or "nature" in mood:
            return SceneType.LANDSCAPE
        elif "portrait" in scene_hints or "character" in mood:
            return SceneType.PORTRAIT
        elif "interior" in scene_hints or "room" in mood:
            return SceneType.INTERIOR
        elif "emotional" in mood or "sad" in mood or "happy" in mood:
            return SceneType.EMOTIONAL
        elif "dramatic" in mood or "tense" in mood:
            return SceneType.DRAMATIC
        elif "peaceful" in mood or "calm" in mood:
            return SceneType.PEACEFUL
        
        return SceneType.UNKNOWN
    
    def _calculate_quality_score(self, original: str, enhanced: str) -> int:
        """Рассчитывает оценку качества улучшения (0-100)"""
        
        score = 50  # Базовая оценка
        
        # Длина улучшения
        if len(enhanced) > len(original) * 1.5:
            score += 10
        if len(enhanced) > len(original) * 2:
            score += 10
        
        # Наличие качественных терминов
        quality_keywords = ["detailed", "professional", "high quality", "sharp", "masterpiece"]
        for keyword in quality_keywords:
            if keyword in enhanced.lower():
                score += 5
        
        # Ограничиваем 0-100
        return min(100, max(0, score))
    
    async def _analyze_input(self, text: str) -> Dict[str, Any]:
        """Анализ исходного текста"""
        
        try:
            system_prompt = """Analyze this text for visual prompt generation.
            Return JSON with:
            - characters: list of character names
            - objects: list of important objects
            - locations: list of locations/settings
            - mood: overall mood (e.g., "tense", "peaceful", "dramatic")
            - lighting: suggested lighting (e.g., "natural", "dramatic", "soft")
            - perspective: suggested camera angle (e.g., "close-up", "wide shot")
            - scene_type_hints: hints about scene type (e.g., "action", "dialogue")
            """
            
            response = await self.ai_service.generate(
                system_prompt=system_prompt,
                user_prompt=text[:2000],
                response_format="json"
            )
            
            return json.loads(response)
        except Exception as e:
            logger.warning(f"Analysis failed: {e}")
            return {
                "characters": [],
                "objects": [],
                "locations": [],
                "mood": "neutral",
                "lighting": "natural",
                "perspective": "medium shot",
                "scene_type_hints": []
            }
    
    async def _expand_description(
        self,
        text: str,
        analysis: Dict[str, Any],
        model_config: Dict[str, Any]
    ) -> str:
        """Расширяет описание для визуализации"""
        
        try:
            system_prompt = f"""Transform this text into a detailed visual prompt for AI image generation.
            
            Guidelines:
            - Focus on visual details: colors, textures, lighting, composition
            - Use comma-separated descriptive phrases
            - Keep under {model_config.get('max_length', 2000)} characters
            - Style: {model_config.get('style', 'detailed')}
            
            Analysis context:
            - Mood: {analysis.get('mood', 'neutral')}
            - Lighting: {analysis.get('lighting', 'natural')}
            - Perspective: {analysis.get('perspective', 'medium shot')}
            """
            
            response = await self.ai_service.generate(
                system_prompt=system_prompt,
                user_prompt=text,
                max_tokens=500,
                temperature=0.7
            )
            
            return response.strip()
        except Exception as e:
            logger.warning(f"Expansion failed: {e}")
            return text
    
    async def _optimize_for_model(
        self,
        prompt: str,
        model: str,
        model_config: Dict[str, Any]
    ) -> str:
        """Оптимизирует промпт для конкретной модели"""
        
        # Для Midjourney - добавляем специфические теги
        if model == "midjourney":
            if "--" not in prompt:
                prompt = prompt.rstrip(",. ")
        
        # Для Stable Diffusion - более структурированный формат
        elif model == "stable_diffusion":
            # Убираем лишние запятые
            prompt = ", ".join([p.strip() for p in prompt.split(",") if p.strip()])
        
        return prompt