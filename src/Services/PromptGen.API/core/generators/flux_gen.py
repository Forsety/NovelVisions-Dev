# core/generators/flux_gen.py
"""
Генератор промптов для Flux (Pro/Dev/Schnell).

Особенности Flux:
- Отлично понимает естественный язык
- Поддерживает длинные детальные описания
- Guidance scale влияет на следование промпту
- Три варианта: Pro (лучшее качество), Dev (баланс), Schnell (быстрый)
- НЕ использует негативные промпты
- Хорошо работает с текстом в изображениях
"""

from typing import Dict, List, Optional, Any
from core.generators.base_generator import (
    BaseGenerator, 
    GeneratorConfig, 
    ModelCapability
)


class FluxGenerator(BaseGenerator):
    """
    Генератор промптов для Flux.
    
    Flux от Black Forest Labs - одна из лучших открытых моделей.
    Отличается отличным пониманием естественного языка и
    способностью генерировать текст в изображениях.
    """
    
    def _get_config(self) -> GeneratorConfig:
        return GeneratorConfig(
            model_name="flux",
            display_name="Flux",
            max_prompt_length=2000,
            default_aspect_ratio="1:1",
            supports_negative=False,  # Flux не использует негативные промпты
            capabilities=[
                ModelCapability.ASPECT_RATIO,
                ModelCapability.SEED
            ],
            default_parameters={
                "variant": "dev",  # pro, dev, schnell
                "guidance_scale": 3.5,
                "num_inference_steps": 28,
                "width": 1024,
                "height": 1024
            },
            quality_tags=[
                "high quality",
                "detailed",
                "professional",
                "sharp focus",
                "vivid colors",
                "masterfully composed"
            ],
            negative_tags=[]  # Flux не использует негативы
        )
    
    # Размеры для разных аспектов (должны быть кратны 8)
    ASPECT_SIZES = {
        "1:1": (1024, 1024),
        "square": (1024, 1024),
        "16:9": (1344, 768),
        "landscape": (1344, 768),
        "wide": (1344, 768),
        "9:16": (768, 1344),
        "portrait": (768, 1344),
        "tall": (768, 1344),
        "4:3": (1152, 896),
        "3:4": (896, 1152),
        "3:2": (1216, 832),
        "2:3": (832, 1216),
        "21:9": (1536, 640),
        "ultrawide": (1536, 640),
        "9:21": (640, 1536)
    }
    
    # Конфигурации вариантов Flux
    VARIANTS = {
        "pro": {
            "name": "Flux Pro",
            "steps": 50,
            "guidance": 3.5,
            "quality": "highest",
            "description": "Лучшее качество, самая медленная генерация"
        },
        "dev": {
            "name": "Flux Dev",
            "steps": 28,
            "guidance": 3.0,
            "quality": "high",
            "description": "Баланс качества и скорости"
        },
        "schnell": {
            "name": "Flux Schnell",
            "steps": 4,
            "guidance": 0,  # Schnell не использует guidance
            "quality": "fast",
            "description": "Максимальная скорость, хорошее качество"
        }
    }
    
    # Стилевые модификаторы для Flux
    STYLE_MODIFIERS = {
        "photographic": "professional photograph, camera shot, realistic lighting, DSLR quality",
        "cinematic": "cinematic film still, movie scene, dramatic lighting, anamorphic lens",
        "illustration": "digital illustration, artistic rendering, stylized artwork",
        "3d": "3D rendered image, CGI, computer graphics, realistic materials and lighting",
        "anime": "anime art style, Japanese animation, cel shaded, vibrant colors",
        "oil_painting": "oil painting on canvas, visible brushstrokes, classical art style",
        "watercolor": "watercolor painting, soft blending, paper texture visible",
        "sketch": "pencil sketch, graphite drawing, hand-drawn artwork",
        "concept_art": "professional concept art, production design, game art",
        "fantasy": "fantasy art, magical atmosphere, ethereal lighting",
        "portrait": "portrait photography, studio lighting, shallow depth of field",
        "landscape": "landscape photography, natural lighting, scenic vista",
        "noir": "film noir style, black and white, high contrast, dramatic shadows",
        "cyberpunk": "cyberpunk aesthetic, neon lights, futuristic, rain-slicked streets",
        "steampunk": "steampunk design, brass and copper, Victorian era, clockwork",
        "gothic": "gothic art style, dark atmosphere, ornate details",
        "minimalist": "minimalist design, clean, simple, elegant negative space",
        "vintage": "vintage style, retro aesthetic, nostalgic, faded colors",
        "pop_art": "pop art style, bold colors, graphic design",
        "impressionist": "impressionist style, soft brushstrokes, light and color play"
    }
    
    async def generate(
        self,
        text: str,
        style: Optional[str] = None,
        parameters: Optional[Dict] = None
    ) -> str:
        """
        Генерирует промпт для Flux.
        
        Flux лучше всего работает с:
        - Чёткими, структурированными описаниями
        - Естественным языком
        - Конкретными деталями о композиции и освещении
        """
        params = self.merge_parameters(parameters)
        self._current_style = style
        
        prompt_parts = []
        
        # Flux любит когда стиль идёт в начале
        if style and style in self.STYLE_MODIFIERS:
            prompt_parts.append(self.STYLE_MODIFIERS[style])
        
        # Основной текст
        prompt_parts.append(text)
        
        # Добавляем качественные теги для не-Schnell вариантов
        variant = params.get("variant", "dev")
        if variant != "schnell":
            # Flux хорошо работает с описательными тегами
            quality_suffix = ", ".join(self.config.quality_tags[:3])
            prompt_parts.append(quality_suffix)
        
        # Объединяем через точку для лучшего понимания структуры
        prompt = ". ".join(filter(None, prompt_parts))
        
        # Форматируем
        formatted = self.format_prompt(prompt, params)
        
        # Сохраняем параметры
        self._parameters = params
        
        return self.truncate(formatted)
    
    def format_prompt(self, prompt: str, parameters: Dict) -> str:
        """
        Flux использует чистый текст без inline параметров.
        Все параметры передаются через API отдельно.
        """
        
        # Устанавливаем размеры на основе аспекта
        if "aspect" in parameters:
            aspect = parameters["aspect"]
            if aspect in self.ASPECT_SIZES:
                w, h = self.ASPECT_SIZES[aspect]
                self._parameters["width"] = w
                self._parameters["height"] = h
        
        # Настраиваем параметры на основе варианта
        variant = parameters.get("variant", "dev")
        if variant in self.VARIANTS:
            variant_config = self.VARIANTS[variant]
            
            if "num_inference_steps" not in parameters:
                self._parameters["num_inference_steps"] = variant_config["steps"]
            
            if "guidance_scale" not in parameters:
                self._parameters["guidance_scale"] = variant_config["guidance"]
        
        # Для Schnell обрабатываем особо
        if variant == "schnell":
            prompt = self._optimize_for_schnell(prompt)
        
        return prompt
    
    def _optimize_for_schnell(self, prompt: str) -> str:
        """
        Оптимизирует промпт для Flux Schnell.
        
        Schnell лучше работает с:
        - Более короткими промптами
        - Чёткими, конкретными описаниями
        - Без избыточных качественных тегов
        """
        # Убираем качественные теги - Schnell их не нуждается
        cleaned = prompt
        for tag in self.config.quality_tags:
            cleaned = cleaned.replace(f", {tag}", "")
            cleaned = cleaned.replace(f"{tag}, ", "")
            cleaned = cleaned.replace(f". {tag}", "")
        
        # Ограничиваем длину
        if len(cleaned) > 500:
            # Ищем логичное место для обрезки
            cutoff = cleaned[:500].rfind(".")
            if cutoff > 300:
                cleaned = cleaned[:cutoff + 1]
            else:
                cutoff = cleaned[:500].rfind(",")
                if cutoff > 350:
                    cleaned = cleaned[:cutoff]
                else:
                    cleaned = cleaned[:500]
        
        return cleaned.strip()
    
    def get_api_parameters(self) -> Dict[str, Any]:
        """Возвращает параметры для API вызова Flux"""
        
        return {
            "prompt": None,  # Заполняется отдельно
            "width": self._parameters.get("width", 1024),
            "height": self._parameters.get("height", 1024),
            "guidance_scale": self._parameters.get("guidance_scale", 3.5),
            "num_inference_steps": self._parameters.get("num_inference_steps", 28),
            "seed": self._parameters.get("seed"),
            "output_format": self._parameters.get("output_format", "png")
        }
    
    def get_variant_info(self, variant: str) -> Dict[str, Any]:
        """Возвращает информацию о варианте Flux"""
        return self.VARIANTS.get(variant, self.VARIANTS["dev"])
    
    def optimize_for_text_generation(self, prompt: str, text_content: str) -> str:
        """
        Оптимизирует промпт для генерации изображений с текстом.
        
        Flux отлично справляется с текстом в изображениях!
        
        Args:
            prompt: Базовый промпт
            text_content: Текст, который должен появиться в изображении
        """
        # Flux понимает прямые инструкции о тексте
        text_instruction = f'The image contains the text "{text_content}" clearly visible and readable'
        
        return f"{prompt}. {text_instruction}"
    
    def create_image_to_image_params(
        self,
        source_image: str,
        strength: float = 0.75
    ) -> Dict[str, Any]:
        """
        Параметры для img2img с Flux.
        
        Args:
            source_image: URL или base64 исходного изображения
            strength: Сила изменения (0.0 - 1.0)
        """
        return {
            "image": source_image,
            "strength": strength,
            "num_inference_steps": max(4, int(self._parameters.get("num_inference_steps", 28) * strength))
        }
    
    def estimate_generation_time(self) -> Dict[str, float]:
        """
        Оценка времени генерации.
        
        Примерные значения для A100 GPU.
        """
        variant = self._parameters.get("variant", "dev")
        steps = self._parameters.get("num_inference_steps", 28)
        
        # Базовое время на шаг (секунды)
        time_per_step = {
            "pro": 0.5,
            "dev": 0.3,
            "schnell": 0.2
        }
        
        base_time = time_per_step.get(variant, 0.3)
        estimated_seconds = steps * base_time
        
        return {
            "estimated_seconds": estimated_seconds,
            "variant": variant,
            "steps": steps
        }