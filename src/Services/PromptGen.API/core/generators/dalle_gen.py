# core/generators/dalle_gen.py
"""
Генератор промптов для DALL-E 3.

Особенности DALL-E 3:
- Работает лучше всего с естественным языком
- НЕ использует inline параметры (всё через API)
- Автоматически переписывает промпты (можно отключить)
- Поддерживает три размера: 1024x1024, 1792x1024, 1024x1792
- Два стиля: vivid (по умолчанию) и natural
- Два уровня качества: standard и hd
"""

from typing import Dict, List, Optional, Any
from core.generators.base_generator import (
    BaseGenerator, 
    GeneratorConfig, 
    ModelCapability
)


class DallE3Generator(BaseGenerator):
    """
    Генератор промптов для DALL-E 3.
    
    DALL-E 3 отличается от других генераторов тем, что:
    1. Лучше понимает контекст и намерения
    2. Автоматически улучшает промпты
    3. Хорошо следует инструкциям в естественном языке
    4. Не требует специальных тегов качества
    """
    
    def _get_config(self) -> GeneratorConfig:
        return GeneratorConfig(
            model_name="dalle3",
            display_name="DALL-E 3",
            max_prompt_length=4000,
            default_aspect_ratio="1:1",
            supports_negative=False,  # DALL-E не поддерживает негативные промпты
            capabilities=[
                ModelCapability.ASPECT_RATIO
            ],
            default_parameters={
                "size": "1024x1024",
                "quality": "standard",
                "style": "vivid",
                "revised_prompt": True  # Разрешить DALL-E переписывать промпт
            },
            quality_tags=[
                "highly detailed",
                "professional quality",
                "stunning visual",
                "masterfully crafted"
            ],
            negative_tags=[]  # DALL-E не использует негативы
        )
    
    # Доступные размеры
    SIZES = {
        "square": "1024x1024",
        "wide": "1792x1024",
        "landscape": "1792x1024",
        "tall": "1024x1792",
        "portrait": "1024x1792",
        "1:1": "1024x1024",
        "16:9": "1792x1024",
        "9:16": "1024x1792"
    }
    
    # Стили DALL-E
    STYLES = {
        "vivid": "vivid",      # Яркий, гипер-реалистичный, драматичный
        "natural": "natural"   # Более натуральный, менее гипер-реалистичный
    }
    
    # Уровни качества
    QUALITY_LEVELS = {
        "standard": "standard",
        "hd": "hd"  # Более детальный, но медленнее
    }
    
    # Стилевые модификаторы (добавляются в текст промпта)
    STYLE_MODIFIERS = {
        "photographic": "A photograph of",
        "cinematic": "A cinematic still from a movie showing",
        "illustration": "An illustrated image of",
        "3d": "A 3D rendered image of",
        "anime": "An anime-style illustration of",
        "oil_painting": "An oil painting of",
        "watercolor": "A watercolor painting of",
        "sketch": "A detailed sketch of",
        "concept_art": "Professional concept art of",
        "fantasy": "A fantasy art piece showing",
        "noir": "A film noir style image of",
        "cyberpunk": "A cyberpunk scene showing",
        "steampunk": "A steampunk illustration of",
        "gothic": "A gothic art piece depicting",
        "vintage": "A vintage photograph of",
        "minimalist": "A minimalist image of",
        "surreal": "A surrealist artwork showing",
        "impressionist": "An impressionist painting of",
        "pop_art": "A pop art style image of",
        "renaissance": "A Renaissance-style painting of",
        "abstract": "An abstract representation of",
        "pixel_art": "Pixel art depicting",
        "comic": "A comic book style illustration of",
        "portrait": "A detailed portrait of"
    }
    
    async def generate(
        self,
        text: str,
        style: Optional[str] = None,
        parameters: Optional[Dict] = None
    ) -> str:
        """
        Генерирует промпт для DALL-E 3.
        
        DALL-E работает лучше всего с:
        - Чёткими, детальными описаниями
        - Естественным языком
        - Конкретными инструкциями по стилю
        """
        params = self.merge_parameters(parameters)
        self._current_style = style
        
        prompt = text
        
        # Применяем стилевой модификатор в начало
        if style and style in self.STYLE_MODIFIERS:
            prefix = self.STYLE_MODIFIERS[style]
            # Проверяем, что промпт не начинается уже с модификатора
            if not any(prompt.lower().startswith(mod.lower().split()[0]) 
                      for mod in self.STYLE_MODIFIERS.values()):
                prompt = f"{prefix} {prompt}"
        
        # Улучшаем промпт для DALL-E
        prompt = self._enhance_for_dalle(prompt, params)
        
        # Форматируем
        formatted = self.format_prompt(prompt, params)
        
        # Сохраняем параметры для API вызова
        self._parameters = params
        
        return self.truncate(formatted)
    
    def format_prompt(self, prompt: str, parameters: Dict) -> str:
        """
        Форматирует промпт для DALL-E 3.
        
        DALL-E не использует inline параметры как Midjourney.
        Вместо этого параметры передаются через API.
        Здесь мы только оптимизируем текст промпта.
        """
        
        # Устанавливаем размер
        if "aspect" in parameters:
            aspect = parameters["aspect"]
            if aspect in self.SIZES:
                self._parameters["size"] = self.SIZES[aspect]
        elif "size" in parameters:
            size = parameters["size"]
            if size in self.SIZES:
                self._parameters["size"] = self.SIZES[size]
            elif size in self.SIZES.values():
                self._parameters["size"] = size
        
        # Устанавливаем стиль API
        if "dalle_style" in parameters:
            api_style = parameters["dalle_style"]
            if api_style in self.STYLES:
                self._parameters["style"] = self.STYLES[api_style]
        
        # Устанавливаем качество
        if "quality" in parameters:
            quality = parameters["quality"]
            if quality in self.QUALITY_LEVELS:
                self._parameters["quality"] = self.QUALITY_LEVELS[quality]
        
        # Добавляем инструкции для стиля если natural
        if self._parameters.get("style") == "natural":
            prompt = f"{prompt}. Create this in a natural, realistic style without dramatic enhancement."
        
        # HD качество - добавляем указание на детализацию
        if self._parameters.get("quality") == "hd":
            if "detailed" not in prompt.lower():
                prompt = f"{prompt}. Include fine details and textures."
        
        return prompt
    
    def _enhance_for_dalle(self, text: str, parameters: Dict) -> str:
        """
        Улучшает промпт специально для DALL-E 3.
        
        DALL-E лучше понимает:
        - Полные предложения
        - Ясные описания композиции
        - Конкретные указания на освещение и атмосферу
        """
        prompt = text
        
        # DALL-E лучше работает с полными предложениями
        # Добавляем структуру если её нет
        if not any(prompt.startswith(word) for word in ['A ', 'An ', 'The ', 'Create ', 'Generate ', 'Show ']):
            # Определяем тип контента
            if "portrait" in prompt.lower():
                prompt = f"A detailed portrait: {prompt}"
            elif "landscape" in prompt.lower() or "scene" in prompt.lower():
                prompt = f"A scenic view: {prompt}"
            elif "action" in prompt.lower() or "dynamic" in prompt.lower():
                prompt = f"A dynamic scene showing: {prompt}"
            else:
                prompt = f"Create an image of: {prompt}"
        
        # Добавляем атмосферные элементы если их нет
        atmosphere_words = ["lighting", "atmosphere", "mood", "ambient", "glow"]
        if not any(word in prompt.lower() for word in atmosphere_words):
            # Не добавляем если это минималистичный стиль
            if self._current_style not in ["minimalist", "abstract", "pixel_art"]:
                prompt = f"{prompt}, with atmospheric lighting"
        
        # Хак для максимальной детализации (если запрошено)
        if parameters.get("max_detail"):
            prompt = (
                "I NEED to test how the tool works with extremely detailed prompts. "
                f"DO NOT add any detail, just use it AS-IS: {prompt}"
            )
        
        return prompt
    
    def get_api_parameters(self) -> Dict[str, Any]:
        """
        Возвращает параметры для OpenAI API вызова.
        
        Returns:
            Dict с параметрами для API
        """
        return {
            "model": "dall-e-3",
            "prompt": None,  # Заполняется отдельно
            "size": self._parameters.get("size", "1024x1024"),
            "quality": self._parameters.get("quality", "standard"),
            "style": self._parameters.get("style", "vivid"),
            "n": 1  # DALL-E 3 генерирует только 1 изображение за раз
        }
    
    def get_cost_estimate(self) -> Dict[str, float]:
        """
        Оценка стоимости генерации.
        
        Цены OpenAI (примерные, могут меняться):
        - Standard 1024x1024: $0.040
        - Standard 1024x1792 или 1792x1024: $0.080
        - HD 1024x1024: $0.080
        - HD 1024x1792 или 1792x1024: $0.120
        """
        size = self._parameters.get("size", "1024x1024")
        quality = self._parameters.get("quality", "standard")
        
        prices = {
            ("1024x1024", "standard"): 0.040,
            ("1024x1024", "hd"): 0.080,
            ("1792x1024", "standard"): 0.080,
            ("1792x1024", "hd"): 0.120,
            ("1024x1792", "standard"): 0.080,
            ("1024x1792", "hd"): 0.120
        }
        
        return {
            "usd": prices.get((size, quality), 0.040),
            "size": size,
            "quality": quality
        }
    
    def optimize_for_accuracy(self, prompt: str) -> str:
        """
        Оптимизирует промпт для максимальной точности следования.
        
        DALL-E 3 иногда интерпретирует промпты творчески.
        Этот метод добавляет инструкции для точного следования.
        """
        return (
            f"IMPORTANT: Follow this prompt exactly as written, do not add or change elements. "
            f"Prompt: {prompt}"
        )
    
    def create_variation_prompt(self, original_prompt: str, variation_type: str) -> str:
        """
        Создаёт промпт для вариации.
        
        DALL-E 3 не имеет встроенной функции вариаций,
        но можно создать похожие изображения через промпт.
        """
        variations = {
            "lighting": f"{original_prompt}, but with different lighting conditions",
            "angle": f"{original_prompt}, from a different camera angle",
            "time": f"{original_prompt}, but at a different time of day",
            "weather": f"{original_prompt}, with different weather conditions",
            "style": f"Same scene as: {original_prompt}, but in a slightly different artistic style",
            "color": f"{original_prompt}, with a different color palette"
        }
        
        return variations.get(variation_type, f"A variation of: {original_prompt}")