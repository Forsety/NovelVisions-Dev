# core/generators/sd_gen.py
"""
Генератор промптов для Stable Diffusion (SDXL, SD 1.5).

Особенности Stable Diffusion:
- Обязательные негативные промпты для качества
- Веса через синтаксис (element:1.2)
- Качественные теги в начале промпта
- Поддержка LoRA моделей
- Поддержка ControlNet
- Различные сэмплеры (DPM++, Euler, DDIM)
- CFG Scale для контроля следования промпту
"""

from typing import Dict, List, Optional, Any
from core.generators.base_generator import (
    BaseGenerator, 
    GeneratorConfig, 
    ModelCapability
)


class StableDiffusionGenerator(BaseGenerator):
    """
    Генератор промптов для Stable Diffusion.
    
    SD - самая гибкая модель с огромным количеством настроек.
    Работает локально или через API (Automatic1111, ComfyUI, и др.)
    """
    
    def _get_config(self) -> GeneratorConfig:
        return GeneratorConfig(
            model_name="stable-diffusion",
            display_name="Stable Diffusion",
            max_prompt_length=380,  # ~77 токенов, больше через BREAK
            default_aspect_ratio="1:1",
            supports_negative=True,
            capabilities=[
                ModelCapability.NEGATIVE_PROMPT,
                ModelCapability.ASPECT_RATIO,
                ModelCapability.SEED,
                ModelCapability.CONTROLNET,
                ModelCapability.LORA,
                ModelCapability.INPAINTING,
                ModelCapability.OUTPAINTING,
                ModelCapability.IMAGE_TO_IMAGE
            ],
            default_parameters={
                "steps": 30,
                "cfg_scale": 7.0,
                "sampler": "DPM++ 2M Karras",
                "width": 1024,
                "height": 1024,
                "clip_skip": 2,
                "variant": "sdxl"  # sdxl, sd15, turbo
            },
            quality_tags=[
                "masterpiece",
                "best quality",
                "highly detailed",
                "ultra-detailed",
                "sharp focus",
                "intricate details",
                "professional",
                "8k uhd",
                "high resolution"
            ],
            negative_tags=[
                "lowres",
                "bad anatomy",
                "bad hands",
                "text",
                "error",
                "missing fingers",
                "extra digit",
                "fewer digits",
                "cropped",
                "worst quality",
                "low quality",
                "normal quality",
                "jpeg artifacts",
                "signature",
                "watermark",
                "username",
                "blurry",
                "artist name",
                "bad proportions",
                "deformed",
                "disfigured",
                "mutation",
                "mutated",
                "ugly"
            ]
        )
    
    # Размеры для SDXL (должны быть кратны 64)
    SDXL_SIZES = {
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
        "ultrawide": (1536, 640)
    }
    
    # Размеры для SD 1.5
    SD15_SIZES = {
        "1:1": (512, 512),
        "square": (512, 512),
        "16:9": (768, 432),
        "landscape": (768, 512),
        "9:16": (432, 768),
        "portrait": (512, 768),
        "4:3": (640, 480),
        "3:4": (480, 640)
    }
    
    # Сэмплеры
    SAMPLERS = [
        "DPM++ 2M Karras",
        "DPM++ SDE Karras",
        "DPM++ 2M SDE Karras",
        "Euler a",
        "Euler",
        "DDIM",
        "UniPC",
        "LMS",
        "Heun",
        "DPM2 a Karras"
    ]
    
    # Стилевые модификаторы с весами
    STYLE_MODIFIERS = {
        "photographic": "(photorealistic:1.3), photograph, DSLR, 85mm lens, professional photography",
        "cinematic": "(cinematic:1.2), movie still, film grain, dramatic lighting, anamorphic",
        "illustration": "(digital illustration:1.2), artwork, artstation, trending",
        "3d": "(3d render:1.2), octane render, unreal engine 5, ray tracing, CGI",
        "anime": "(anime:1.3), cel shading, anime style, japanese animation",
        "oil_painting": "(oil painting:1.2), canvas texture, brushstrokes, classical art",
        "watercolor": "(watercolor:1.2), wet on wet, soft edges, paper texture",
        "sketch": "(pencil sketch:1.2), graphite, drawing, cross-hatching",
        "concept_art": "(concept art:1.2), matte painting, artstation trending",
        "fantasy": "(fantasy art:1.2), magical, ethereal, dreamy lighting",
        "gothic": "(gothic art:1.2), dark, ornate, dramatic shadows, medieval",
        "cyberpunk": "(cyberpunk:1.2), neon, rain, futuristic, dystopian",
        "steampunk": "(steampunk:1.2), brass, gears, victorian, steam-powered",
        "noir": "(film noir:1.3), black and white, high contrast, dramatic shadows",
        "vintage": "(vintage:1.2), retro, nostalgic, faded colors, film grain",
        "minimalist": "(minimalist:1.2), clean, simple, negative space",
        "surreal": "(surrealist:1.2), dreamlike, impossible, abstract",
        "impressionist": "(impressionist:1.2), soft brushstrokes, light and color",
        "ukiyo_e": "(ukiyo-e:1.2), japanese woodblock, traditional",
        "pop_art": "(pop art:1.2), bold colors, comic style, halftone",
        "baroque": "(baroque:1.2), dramatic, ornate, chiaroscuro",
        "renaissance": "(renaissance:1.2), classical, sfumato, detailed"
    }
    
    # Дополнительные негативы по стилям
    STYLE_NEGATIVES = {
        "photographic": "cartoon, anime, drawing, painting, illustration, cgi, 3d render, digital art",
        "anime": "realistic, photograph, 3d, western, photorealistic",
        "3d": "2d, flat, drawing, sketch, painting, hand drawn",
        "oil_painting": "digital, photograph, 3d, anime, smooth, photorealistic",
        "sketch": "color, painted, rendered, photographic, digital",
        "noir": "color, colorful, vibrant, bright, cheerful, saturated",
        "minimalist": "cluttered, busy, detailed, ornate, complex",
        "vintage": "modern, digital, clean, sharp, high definition"
    }
    
    async def generate(
        self,
        text: str,
        style: Optional[str] = None,
        parameters: Optional[Dict] = None
    ) -> str:
        """
        Генерирует промпт для Stable Diffusion.
        
        SD промпты обычно следуют структуре:
        1. Качественные теги
        2. Стилевые теги
        3. Основное описание
        4. Дополнительные детали
        """
        params = self.merge_parameters(parameters)
        self._current_style = style
        
        prompt_parts = []
        
        # Качественные теги в начале
        quality = params.get("quality_level", "high")
        if quality and quality != "none":
            quality_tags = self._get_quality_tags(quality)
            prompt_parts.append(quality_tags)
        
        # Стилевой модификатор
        if style and style in self.STYLE_MODIFIERS:
            prompt_parts.append(self.STYLE_MODIFIERS[style])
        
        # Основной текст с возможными весами
        weighted_text = self._apply_emphasis(text, params.get("emphasis", {}))
        prompt_parts.append(weighted_text)
        
        prompt = ", ".join(prompt_parts)
        
        # Форматируем
        formatted = self.format_prompt(prompt, params)
        
        # Сохраняем параметры
        self._parameters = params
        
        return self.truncate(formatted)
    
    def format_prompt(self, prompt: str, parameters: Dict) -> str:
        """
        Форматирует промпт для SD.
        
        Добавляет LoRA теги и устанавливает размеры.
        """
        
        # Определяем вариант SD
        variant = parameters.get("variant", "sdxl")
        
        # Устанавливаем размеры
        aspect = parameters.get("aspect", "1:1")
        sizes = self.SDXL_SIZES if variant == "sdxl" else self.SD15_SIZES
        
        if aspect in sizes:
            w, h = sizes[aspect]
            self._parameters["width"] = w
            self._parameters["height"] = h
        
        # Добавляем LoRA если указаны
        if "loras" in parameters:
            lora_tags = []
            for lora in parameters["loras"]:
                name = lora.get("name", "")
                weight = lora.get("weight", 0.8)
                if name:
                    lora_tags.append(f"<lora:{name}:{weight}>")
            if lora_tags:
                prompt = " ".join(lora_tags) + " " + prompt
        
        # Добавляем textual inversion если указаны
        if "embeddings" in parameters:
            for emb in parameters["embeddings"]:
                prompt = f"({emb}), " + prompt
        
        # Обрабатываем BREAK для длинных промптов
        if len(prompt) > 300 and "BREAK" not in prompt:
            # Можно разбить на части через BREAK
            # prompt = self._add_breaks(prompt)
            pass
        
        return prompt
    
    def _get_quality_tags(self, quality: str) -> str:
        """Возвращает теги качества по уровню"""
        
        quality_map = {
            "low": "detailed",
            "medium": "best quality, highly detailed",
            "high": "masterpiece, best quality, highly detailed, ultra-detailed",
            "ultra": ", ".join(self.config.quality_tags),
            "anime": "(masterpiece:1.2), best quality, high resolution, detailed",
            "photo": "RAW photo, 8k uhd, DSLR, high quality, realistic, detailed"
        }
        
        return quality_map.get(quality, quality_map["high"])
    
    def _apply_emphasis(self, text: str, emphasis: Dict[str, float]) -> str:
        """
        Применяет веса к указанным элементам.
        
        Args:
            text: Исходный текст
            emphasis: Dict с элементами и их весами {"element": 1.2}
        """
        if not emphasis:
            return text
        
        result = text
        for element, weight in emphasis.items():
            if element.lower() in result.lower():
                # Заменяем на weighted версию
                import re
                pattern = re.compile(re.escape(element), re.IGNORECASE)
                result = pattern.sub(f"({element}:{weight})", result, count=1)
        
        return result
    
    def add_emphasis(self, text: str, weight: float = 1.2) -> str:
        """Добавляет вес к элементу"""
        return f"({text}:{weight})"
    
    def add_strong_emphasis(self, text: str, level: int = 2) -> str:
        """
        Добавляет сильный акцент через множественные скобки.
        
        level=2: ((element))
        level=3: (((element)))
        """
        level = min(max(level, 1), 5)  # Ограничиваем 1-5
        return "(" * level + text + ")" * level
    
    def reduce_emphasis(self, text: str, weight: float = 0.8) -> str:
        """Уменьшает вес элемента"""
        return f"({text}:{weight})" if weight < 1 else text
    
    def get_negative_prompt(
        self,
        scene_type: Optional[str] = None,
        style: Optional[str] = None,
        custom: Optional[List[str]] = None
    ) -> str:
        """
        Генерирует негативный промпт.
        
        Для SD негативный промпт критически важен для качества!
        """
        negatives = list(self.config.negative_tags)
        
        # Добавляем стиль-специфичные негативы
        if style and style in self.STYLE_NEGATIVES:
            style_negs = self.STYLE_NEGATIVES[style].split(", ")
            negatives.extend(style_negs)
        
        # Специфичные для сцен
        scene_negatives = {
            "portrait": ["bad face", "ugly face", "asymmetric face", "bad eyes"],
            "landscape": ["people", "text", "signs", "modern objects"],
            "action": ["static", "stiff", "frozen"],
            "horror": ["cute", "happy", "bright", "cheerful"]
        }
        
        if scene_type and scene_type in scene_negatives:
            negatives.extend(scene_negatives[scene_type])
        
        # Кастомные
        if custom:
            negatives.extend(custom)
        
        # Убираем дубликаты
        seen = set()
        unique = []
        for neg in negatives:
            if neg.lower() not in seen:
                seen.add(neg.lower())
                unique.append(neg)
        
        return ", ".join(unique)
    
    def get_api_parameters(self) -> Dict[str, Any]:
        """Возвращает параметры для API (Automatic1111 / ComfyUI)"""
        
        return {
            "prompt": None,
            "negative_prompt": None,
            "width": self._parameters.get("width", 1024),
            "height": self._parameters.get("height", 1024),
            "steps": self._parameters.get("steps", 30),
            "cfg_scale": self._parameters.get("cfg_scale", 7.0),
            "sampler_name": self._parameters.get("sampler", "DPM++ 2M Karras"),
            "seed": self._parameters.get("seed", -1),
            "clip_skip": self._parameters.get("clip_skip", 2),
            "batch_size": self._parameters.get("batch_size", 1),
            "n_iter": self._parameters.get("n_iter", 1)
        }
    
    def create_controlnet_params(
        self,
        control_type: str,
        control_image: str,
        weight: float = 1.0
    ) -> Dict[str, Any]:
        """
        Создаёт параметры для ControlNet.
        
        Args:
            control_type: Тип контроля (canny, depth, pose, etc.)
            control_image: URL или base64 контрольного изображения
            weight: Вес применения (0.0 - 2.0)
        """
        control_types = {
            "canny": "control_canny",
            "depth": "control_depth",
            "pose": "control_openpose",
            "scribble": "control_scribble",
            "lineart": "control_lineart",
            "softedge": "control_softedge",
            "normal": "control_normal"
        }
        
        return {
            "controlnet_model": control_types.get(control_type, control_type),
            "controlnet_image": control_image,
            "controlnet_weight": weight,
            "controlnet_guidance_start": 0.0,
            "controlnet_guidance_end": 1.0
        }
    
    def optimize_for_turbo(self, prompt: str) -> tuple[str, Dict]:
        """
        Оптимизирует промпт и параметры для SDXL Turbo.
        
        Turbo использует меньше шагов и другой CFG.
        """
        # Turbo не нуждается в негативных промптах
        # И работает с 1-4 шагами
        
        params = {
            "steps": 4,
            "cfg_scale": 1.0,  # Turbo использует низкий CFG
            "sampler": "Euler a"
        }
        
        # Упрощаем промпт - turbo не нуждается в качественных тегах
        simplified = prompt
        for tag in self.config.quality_tags[:4]:
            simplified = simplified.replace(f"{tag}, ", "")
            simplified = simplified.replace(f"{tag},", "")
        
        return simplified.strip(), params