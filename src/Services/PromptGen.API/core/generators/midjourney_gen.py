# core/generators/midjourney_gen.py
"""
Генератор промптов для Midjourney v5/v6.

Особенности Midjourney:
- Поддержка параметров: --ar, --v, --q, --s, --c, --weird, --seed
- Style reference (--sref) для копирования стиля
- Character reference (--cref) для консистентности персонажей
- Niji mode для аниме стиля
- Raw mode для максимального реализма
- Tile mode для создания паттернов
"""

from typing import Dict, List, Optional, Any
from core.generators.base_generator import (
    BaseGenerator, 
    GeneratorConfig, 
    ModelCapability
)


class MidjourneyGenerator(BaseGenerator):
    """
    Генератор промптов для Midjourney.
    
    Midjourney - один из лучших генераторов для художественных изображений.
    Поддерживает длинные промпты до 6000 символов.
    """
    
    def _get_config(self) -> GeneratorConfig:
        return GeneratorConfig(
            model_name="midjourney",
            display_name="Midjourney",
            max_prompt_length=6000,
            default_aspect_ratio="16:9",
            supports_negative=True,  # Через параметр --no
            capabilities=[
                ModelCapability.ASPECT_RATIO,
                ModelCapability.SEED,
                ModelCapability.STYLE_REFERENCE,
                ModelCapability.CHARACTER_REFERENCE,
                ModelCapability.UPSCALE,
                ModelCapability.VARIATIONS,
                ModelCapability.TILE,
                ModelCapability.IMAGE_TO_IMAGE
            ],
            default_parameters={
                "version": "6.1",
                "quality": "1",
                "stylize": "100",
                "chaos": "0"
            },
            quality_tags=[
                "highly detailed",
                "professional quality",
                "stunning",
                "masterpiece",
                "award winning",
                "breathtaking"
            ],
            negative_tags=[
                "ugly",
                "blurry", 
                "low quality",
                "distorted",
                "deformed",
                "amateur"
            ]
        )
    
    # Соотношения сторон
    ASPECT_RATIOS = {
        "square": "1:1",
        "portrait": "2:3",
        "landscape": "3:2",
        "wide": "16:9",
        "ultrawide": "21:9",
        "tall": "9:16",
        "poster": "2:3",
        "cinema": "2.39:1",
        "cinemascope": "2.35:1",
        "instagram": "4:5",
        "instagram_story": "9:16",
        "twitter": "16:9",
        "facebook": "1.91:1",
        "pinterest": "2:3",
        "youtube": "16:9",
        "tiktok": "9:16",
        "phone": "9:19.5",
        "desktop": "16:10",
        "ultrawide_desktop": "21:9"
    }
    
    # Пресеты стилей Midjourney
    STYLE_PRESETS = {
        "raw": "--style raw",
        "expressive": "--s 500",
        "balanced": "--s 100",
        "subtle": "--s 50",
        "artistic": "--s 750",
        "maximum": "--s 1000",
        
        # Niji режимы (аниме)
        "niji": "--niji 6",
        "niji_cute": "--niji 6 --style cute",
        "niji_scenic": "--niji 6 --style scenic",
        "niji_expressive": "--niji 6 --style expressive",
        "niji_original": "--niji 6 --style original"
    }
    
    # Стилевые модификаторы для добавления в промпт
    STYLE_MODIFIERS = {
        "photographic": "photograph, photorealistic, camera shot, DSLR, 85mm lens, natural lighting",
        "cinematic": "cinematic shot, movie still, anamorphic, film grain, dramatic lighting, color grading",
        "illustration": "digital illustration, artwork, artistic rendering, stylized",
        "3d": "3d render, octane render, unreal engine 5, ray tracing, CGI",
        "anime": "anime style, cel shading, japanese animation, vibrant colors",
        "oil_painting": "oil painting, classical art, visible brushstrokes, canvas texture, rich colors",
        "watercolor": "watercolor painting, soft edges, flowing colors, wet on wet, paper texture",
        "sketch": "pencil sketch, graphite drawing, cross-hatching, hand drawn",
        "concept_art": "concept art, matte painting, artstation, production design",
        "fantasy": "fantasy art, magical, ethereal, enchanted, mythical",
        "noir": "film noir, black and white, high contrast, dramatic shadows, moody",
        "cyberpunk": "cyberpunk, neon lights, rain, futuristic city, dystopian, holographic",
        "steampunk": "steampunk, brass and copper, gears, victorian, steam-powered",
        "gothic": "gothic art, dark, ornate, medieval, cathedral, dramatic",
        "vintage": "vintage photography, retro, nostalgic, faded colors, film grain",
        "minimalist": "minimalist, clean, simple, negative space, elegant",
        "surreal": "surrealist, dreamlike, impossible, Salvador Dali inspired",
        "impressionist": "impressionist painting, soft brushstrokes, light and color, Monet inspired",
        "pop_art": "pop art, bold colors, comic style, Andy Warhol inspired",
        "art_deco": "art deco, geometric, gold accents, 1920s glamour",
        "ukiyo_e": "ukiyo-e, japanese woodblock print, traditional, Hokusai inspired",
        "baroque": "baroque painting, dramatic, ornate, Caravaggio inspired, chiaroscuro",
        "renaissance": "renaissance painting, classical, detailed, Leonardo da Vinci inspired",
        "abstract": "abstract art, non-representational, shapes and colors, modern art",
        "pixel_art": "pixel art, 8-bit, retro game, nostalgic",
        "vaporwave": "vaporwave aesthetic, pink and cyan, retro, glitch, 80s"
    }
    
    async def generate(
        self,
        text: str,
        style: Optional[str] = None,
        parameters: Optional[Dict] = None
    ) -> str:
        """
        Генерирует Midjourney промпт.
        
        Args:
            text: Базовый текст промпта
            style: Художественный стиль
            parameters: Параметры генерации
            
        Returns:
            Отформатированный промпт для Midjourney
        """
        params = self.merge_parameters(parameters)
        self._current_style = style
        
        prompt = text
        
        # Применяем стилевой модификатор
        if style and style in self.STYLE_MODIFIERS:
            prompt = f"{prompt}, {self.STYLE_MODIFIERS[style]}"
        
        # Добавляем теги качества если не raw mode
        if params.get("style_preset") != "raw" and not params.get("raw"):
            quality = params.get("quality_level", "high")
            if quality != "none":
                prompt = self.add_quality_tags(prompt, quality)
        
        # Форматируем с параметрами
        formatted = self.format_prompt(prompt, params)
        
        # Сохраняем параметры
        self._parameters = params
        
        # Обрезаем если нужно
        return self.truncate(formatted)
    
    def format_prompt(self, prompt: str, parameters: Dict) -> str:
        """
        Форматирует промпт с параметрами Midjourney.
        
        Порядок параметров:
        1. Основной промпт
        2. Image prompts (URLs)
        3. --ar (aspect ratio)
        4. --v (version)
        5. --q (quality)
        6. --s (stylize)
        7. --c (chaos)
        8. --weird
        9. --seed
        10. --sref (style reference)
        11. --cref (character reference)
        12. --no (negative)
        13. Стиль пресет
        """
        parts = [prompt]
        
        # Image prompt (URL) в начале
        if "image_url" in parameters:
            parts.insert(0, parameters["image_url"])
        
        # Aspect ratio
        if "aspect" in parameters:
            ar = self.ASPECT_RATIOS.get(parameters["aspect"], parameters["aspect"])
            parts.append(f"--ar {ar}")
        elif "ar" in parameters:
            parts.append(f"--ar {parameters['ar']}")
        
        # Version
        if "version" in parameters:
            v = str(parameters["version"])
            if v.lower() not in ["niji", "niji6"]:
                parts.append(f"--v {v}")
        
        # Quality (1 = default, 0.5 = half, 0.25 = quarter)
        if "quality" in parameters:
            q = parameters["quality"]
            if str(q) != "1":
                parts.append(f"--q {q}")
        
        # Stylize (0-1000)
        if "stylize" in parameters:
            s = parameters["stylize"]
            if str(s) != "100":  # 100 is default
                parts.append(f"--s {s}")
        
        # Chaos (0-100)
        if "chaos" in parameters:
            c = int(parameters["chaos"])
            if c > 0:
                parts.append(f"--c {c}")
        
        # Weird (0-3000)
        if "weird" in parameters:
            w = int(parameters["weird"])
            if w > 0:
                parts.append(f"--weird {w}")
        
        # Seed для воспроизводимости
        if "seed" in parameters:
            parts.append(f"--seed {parameters['seed']}")
        
        # Style reference
        if "sref" in parameters:
            sref = parameters["sref"]
            parts.append(f"--sref {sref}")
            # Style weight
            if "sw" in parameters:
                parts.append(f"--sw {parameters['sw']}")
        
        # Character reference
        if "cref" in parameters:
            cref = parameters["cref"]
            parts.append(f"--cref {cref}")
            # Character weight
            if "cw" in parameters:
                parts.append(f"--cw {parameters['cw']}")
        
        # Negative через --no
        if "negative" in parameters and parameters["negative"]:
            parts.append(f"--no {parameters['negative']}")
        elif "no" in parameters and parameters["no"]:
            parts.append(f"--no {parameters['no']}")
        
        # Tile для паттернов
        if parameters.get("tile"):
            parts.append("--tile")
        
        # Repeat для нескольких генераций
        if "repeat" in parameters:
            r = int(parameters["repeat"])
            if r > 1:
                parts.append(f"--repeat {r}")
        
        # Стиль пресет
        if "style_preset" in parameters:
            preset = parameters["style_preset"]
            if preset in self.STYLE_PRESETS:
                parts.append(self.STYLE_PRESETS[preset])
        
        # Raw mode
        if parameters.get("raw"):
            if "--style raw" not in " ".join(parts):
                parts.append("--style raw")
        
        return " ".join(parts).strip()
    
    def get_negative_prompt(
        self,
        scene_type: Optional[str] = None,
        style: Optional[str] = None,
        custom: Optional[List[str]] = None
    ) -> str:
        """
        Возвращает негативные элементы для --no параметра.
        
        В Midjourney негативы добавляются через --no param1, param2
        """
        negatives = list(self.config.negative_tags)
        
        # Добавляем стиль-специфичные негативы
        style_negatives = {
            "photographic": ["cartoon", "anime", "drawing", "painting", "illustration", "cgi"],
            "anime": ["realistic", "photograph", "3d render", "western"],
            "3d": ["2d", "flat", "drawing", "sketch", "painting"],
            "oil_painting": ["digital", "photograph", "3d", "anime", "smooth"],
            "sketch": ["color", "painted", "rendered", "photographic"],
            "noir": ["color", "colorful", "vibrant", "bright", "cheerful"],
            "minimalist": ["cluttered", "busy", "detailed", "ornate"]
        }
        
        if style and style in style_negatives:
            negatives.extend(style_negatives[style])
        
        # Добавляем кастомные
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
    
    def get_upscale_command(self, index: int) -> str:
        """Возвращает команду для апскейла конкретного изображения"""
        if index < 1 or index > 4:
            raise ValueError("Index must be between 1 and 4")
        return f"U{index}"
    
    def get_variation_command(self, index: int, strength: str = "subtle") -> str:
        """
        Возвращает команду для создания вариации.
        
        Args:
            index: Номер изображения (1-4)
            strength: Сила вариации (subtle, strong)
        """
        if index < 1 or index > 4:
            raise ValueError("Index must be between 1 and 4")
        
        if strength == "strong":
            return f"V{index}"  # Strong variation
        return f"V{index}"  # По умолчанию subtle в v6
    
    def get_zoom_out_params(self, factor: float = 1.5) -> str:
        """Параметры для zoom out"""
        if factor not in [1.5, 2.0]:
            factor = 1.5
        return f"--zoom {factor}"
    
    def get_pan_command(self, direction: str) -> str:
        """Команда для панорамирования"""
        directions = {
            "left": "⬅️",
            "right": "➡️",
            "up": "⬆️",
            "down": "⬇️"
        }
        return directions.get(direction.lower(), "➡️")
    
    def create_blend_prompt(self, image_urls: List[str], dimensions: str = "square") -> str:
        """
        Создаёт промпт для blend (смешивание изображений).
        
        Args:
            image_urls: Список URL изображений (2-5)
            dimensions: Формат результата (square, portrait, landscape)
        """
        if len(image_urls) < 2 or len(image_urls) > 5:
            raise ValueError("Blend requires 2-5 images")
        
        dim_map = {
            "square": "--ar 1:1",
            "portrait": "--ar 2:3",
            "landscape": "--ar 3:2"
        }
        
        urls = " ".join(image_urls)
        dim = dim_map.get(dimensions, "--ar 1:1")
        
        return f"/blend {urls} {dim}"
    
    def get_describe_command(self, image_url: str) -> str:
        """Команда для получения описания изображения"""
        return f"/describe {image_url}"
    
    def optimize_for_version(self, prompt: str, version: str) -> str:
        """Оптимизирует промпт для конкретной версии Midjourney"""
        
        v = str(version).lower()
        
        if v.startswith("6"):
            # V6 лучше понимает естественный язык
            # Можно использовать более длинные описания
            return prompt
        
        elif v.startswith("5"):
            # V5 предпочитает ключевые слова
            # Убираем лишние артикли и предлоги
            import re
            # Упрощаем структуру
            cleaned = re.sub(r'\b(the|a|an|of|in|on|at|to|for|with|by)\b', '', prompt, flags=re.IGNORECASE)
            cleaned = re.sub(r'\s+', ' ', cleaned)
            return cleaned.strip()
        
        elif "niji" in v:
            # Niji оптимизирован для аниме
            if "anime" not in prompt.lower():
                return f"anime style, {prompt}"
            return prompt
        
        return prompt