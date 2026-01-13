# src/Services/PromptGen.API/core/engines/prompt_enhancer.py
"""
Prompt Enhancer Engine - улучшение промптов для различных AI моделей
"""
import json
import logging
from typing import Dict, List, Optional, Any
from sqlalchemy.ext.asyncio import AsyncSession

from services.ai.openai_service import OpenAIService
from services.storage.cache_service import CacheService

logger = logging.getLogger(__name__)


class PromptEnhancer:
    """
    Engine для улучшения промптов под конкретные AI модели.
    Преобразует простой текст в детальный, оптимизированный промпт.
    """
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.ai_service = OpenAIService()
        
        # Шаблоны стилей
        self.style_templates = {
            "realistic": "photorealistic, highly detailed, natural lighting, professional photography",
            "anime": "anime style, vibrant colors, expressive characters, Studio Ghibli inspired",
            "manga": "manga art style, black and white, dynamic lines, dramatic shading",
            "fantasy": "fantasy art, magical atmosphere, ethereal lighting, detailed environment",
            "oil-painting": "oil painting style, classical art, rich colors, brushstroke texture",
            "watercolor": "watercolor painting, soft colors, flowing edges, artistic",
            "comic": "comic book style, bold outlines, dynamic poses, action panels",
            "cinematic": "cinematic composition, dramatic lighting, movie still, widescreen"
        }
        
        # Оптимизации под модели
        self.model_optimizations = {
            "dalle3": {
                "max_length": 4000,
                "prefer_natural_language": True,
                "avoid_terms": ["photo of", "picture of"],
                "quality_terms": ["highly detailed", "professional quality", "4K"],
                "suffix": ""
            },
            "midjourney": {
                "max_length": 6000,
                "prefer_natural_language": False,
                "use_parameters": True,
                "quality_terms": ["intricate details", "8k uhd", "unreal engine"],
                "suffix": " --q 2 --s 750 --v 6"
            },
            "stable-diffusion": {
                "max_length": 380,
                "prefer_natural_language": False,
                "use_weights": True,
                "quality_terms": ["masterpiece", "best quality", "highly detailed"],
                "negative_default": "lowres, bad anatomy, bad hands, text, error, missing fingers, extra digit, fewer digits, cropped, worst quality, low quality, normal quality, jpeg artifacts"
            },
            "flux": {
                "max_length": 2000,
                "prefer_natural_language": True,
                "quality_terms": ["ultra detailed", "sharp focus", "professional"],
                "suffix": ""
            }
        }
    
    async def enhance(
        self,
        text: str,
        model: str = "dalle3",
        style: Optional[str] = None,
        parameters: Optional[Dict[str, Any]] = None,
        character_context: Optional[Dict[str, str]] = None
    ) -> Dict[str, Any]:
        """
        Улучшить текст до оптимизированного промпта.
        
        Args:
            text: Исходный текст для улучшения
            model: Целевая AI модель
            style: Стиль визуализации
            parameters: Дополнительные параметры
            character_context: Контекст персонажей для consistency
            
        Returns:
            Dict с enhanced prompt и метаданными
        """
        
        # Проверить кэш
        cache_key = f"enhance:{model}:{style}:{self._hash(text)}"
        cached = await self.cache.get(cache_key)
        if cached:
            return json.loads(cached)
        
        model_config = self.model_optimizations.get(model, self.model_optimizations["dalle3"])
        
        # 1. Анализ исходного текста
        analysis = await self._analyze_input(text)
        
        # 2. Расширение описания
        expanded = await self._expand_description(text, analysis, model_config)
        
        # 3. Добавление стиля
        if style and style in self.style_templates:
            expanded = f"{expanded}, {self.style_templates[style]}"
        
        # 4. Добавление контекста персонажей
        if character_context:
            char_descriptions = []
            for name, description in character_context.items():
                char_descriptions.append(f"{name}: {description}")
            if char_descriptions:
                expanded = f"{expanded}. Characters: {', '.join(char_descriptions)}"
        
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
        
        result = {
            "original": text,
            "enhanced": optimized,
            "model": model,
            "style": style,
            "analysis": analysis,
            "improvements": await self._list_improvements(text, optimized)
        }
        
        # Кэшировать результат
        await self.cache.set(cache_key, json.dumps(result), expire=3600)
        
        return result
    
    async def _analyze_input(self, text: str) -> Dict[str, Any]:
        """Анализ исходного текста"""
        
        system_prompt = """Analyze this text for visual prompt generation.
        Identify:
        - subject: main subject of the scene
        - action: what's happening
        - setting: where it takes place
        - mood: emotional tone
        - lighting: lighting conditions if mentioned
        - composition: camera angle/framing if suggested
        Return as JSON."""
        
        try:
            response = await self.ai_service.generate(
                system_prompt=system_prompt,
                user_prompt=text[:1500],
                response_format="json"
            )
            return json.loads(response)
        except Exception as e:
            logger.warning(f"Analysis failed: {e}")
            return {
                "subject": text,
                "action": "unknown",
                "setting": "unspecified",
                "mood": "neutral"
            }
    
    async def _expand_description(
        self,
        text: str,
        analysis: Dict[str, Any],
        model_config: Dict
    ) -> str:
        """Расширить описание с деталями"""
        
        prefer_natural = model_config.get("prefer_natural_language", True)
        
        if prefer_natural:
            system_prompt = """Expand this text into a detailed visual description.
            Write in natural language, focusing on:
            - Visual details and appearance
            - Environment and atmosphere
            - Lighting and colors
            - Composition and perspective
            Keep it under 300 words."""
        else:
            system_prompt = """Convert this text into a detailed prompt for AI image generation.
            Use comma-separated descriptive tags:
            - Subject and action
            - Style and medium
            - Lighting and atmosphere
            - Quality modifiers
            Keep it concise and descriptive."""
        
        try:
            response = await self.ai_service.generate(
                system_prompt=system_prompt,
                user_prompt=f"Text: {text}\n\nAnalysis: {json.dumps(analysis)}",
                max_tokens=400,
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
        model_config: Dict
    ) -> str:
        """Оптимизация промпта под конкретную модель"""
        
        # Для Stable Diffusion - добавить веса
        if model == "stable-diffusion" and model_config.get("use_weights"):
            # Добавить веса к важным элементам
            prompt = self._add_sd_weights(prompt)
        
        # Для Midjourney - использовать специфический синтаксис
        elif model == "midjourney":
            # Убрать лишние слова
            avoid = ["a photo of", "an image of", "picture of"]
            for term in avoid:
                prompt = prompt.replace(term, "")
        
        # Удалить запрещенные термины
        avoid_terms = model_config.get("avoid_terms", [])
        for term in avoid_terms:
            prompt = prompt.replace(term, "")
        
        return prompt.strip()
    
    def _add_sd_weights(self, prompt: str) -> str:
        """Добавить веса для Stable Diffusion"""
        
        # Простая эвристика - усилить ключевые слова
        important_words = ["detailed", "quality", "masterpiece", "beautiful", "intricate"]
        
        for word in important_words:
            if word in prompt.lower():
                prompt = prompt.replace(word, f"({word}:1.2)")
        
        return prompt
    
    async def _list_improvements(self, original: str, enhanced: str) -> List[str]:
        """Список улучшений"""
        
        improvements = []
        
        if len(enhanced) > len(original) * 1.5:
            improvements.append("Added detailed descriptions")
        
        if "lighting" in enhanced.lower() and "lighting" not in original.lower():
            improvements.append("Added lighting details")
        
        if "style" in enhanced.lower():
            improvements.append("Applied artistic style")
        
        if any(q in enhanced.lower() for q in ["detailed", "quality", "professional"]):
            improvements.append("Added quality modifiers")
        
        if not improvements:
            improvements.append("Optimized for target model")
        
        return improvements
    
    def _hash(self, text: str) -> str:
        """Генерация хэша для кэширования"""
        import hashlib
        return hashlib.md5(text.encode()).hexdigest()[:12]