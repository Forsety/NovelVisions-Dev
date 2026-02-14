# core/generators/base_generator.py
"""
Базовый класс для всех генераторов промптов.
Каждая AI модель (Midjourney, DALL-E, SD, Flux) наследуется от этого класса.
"""

from abc import ABC, abstractmethod
from typing import Dict, List, Optional, Any, Type
from dataclasses import dataclass, field
from enum import Enum


class ModelCapability(Enum):
    """Возможности AI модели"""
    NEGATIVE_PROMPT = "negative_prompt"
    ASPECT_RATIO = "aspect_ratio"
    SEED = "seed"
    STYLE_REFERENCE = "style_reference"
    CHARACTER_REFERENCE = "character_reference"
    INPAINTING = "inpainting"
    OUTPAINTING = "outpainting"
    CONTROLNET = "controlnet"
    LORA = "lora"
    UPSCALE = "upscale"
    VARIATIONS = "variations"
    IMAGE_TO_IMAGE = "img2img"
    TILE = "tile"


@dataclass
class GeneratorConfig:
    """Конфигурация генератора"""
    model_name: str
    display_name: str
    max_prompt_length: int
    default_aspect_ratio: str = "1:1"
    supports_negative: bool = False
    capabilities: List[ModelCapability] = field(default_factory=list)
    default_parameters: Dict[str, Any] = field(default_factory=dict)
    quality_tags: List[str] = field(default_factory=list)
    negative_tags: List[str] = field(default_factory=list)


class BaseGenerator(ABC):
    """
    Абстрактный базовый класс для всех генераторов промптов.
    
    Каждая AI модель имеет свои особенности:
    - Midjourney: параметры через --ar, --v, --s
    - DALL-E 3: естественный язык, без inline параметров
    - Stable Diffusion: веса (element:1.2), негативные промпты
    - Flux: структурированные описания, guidance scale
    """
    
    def __init__(self, config: Optional[Dict] = None):
        self.config = self._get_config()
        self.user_config = config or {}
        self._parameters: Dict[str, Any] = {}
        self._current_style: Optional[str] = None
    
    @abstractmethod
    def _get_config(self) -> GeneratorConfig:
        """Возвращает конфигурацию генератора. Должен быть реализован в подклассах."""
        pass
    
    @property
    def model_name(self) -> str:
        """Имя модели"""
        return self.config.model_name
    
    @property
    def display_name(self) -> str:
        """Отображаемое имя"""
        return self.config.display_name
    
    @property
    def max_length(self) -> int:
        """Максимальная длина промпта"""
        return self.config.max_prompt_length
    
    @property
    def supports_negative(self) -> bool:
        """Поддерживает ли негативные промпты"""
        return self.config.supports_negative
    
    @abstractmethod
    async def generate(
        self,
        text: str,
        style: Optional[str] = None,
        parameters: Optional[Dict] = None
    ) -> str:
        """
        Генерирует промпт для конкретной модели.
        
        Args:
            text: Улучшенный текст промпта
            style: Художественный стиль
            parameters: Дополнительные параметры
            
        Returns:
            Отформатированный промпт для модели
        """
        pass
    
    @abstractmethod
    def format_prompt(self, prompt: str, parameters: Dict) -> str:
        """Форматирует промпт с параметрами модели"""
        pass
    
    def add_quality_tags(self, prompt: str, quality: str = "high") -> str:
        """
        Добавляет теги качества в начало промпта.
        
        Args:
            prompt: Исходный промпт
            quality: Уровень качества (low, medium, high, ultra)
            
        Returns:
            Промпт с тегами качества
        """
        if not self.config.quality_tags:
            return prompt
        
        quality_map = {
            "low": self.config.quality_tags[:1],
            "medium": self.config.quality_tags[:3],
            "high": self.config.quality_tags[:5],
            "ultra": self.config.quality_tags,
            "none": []
        }
        
        tags = quality_map.get(quality, self.config.quality_tags[:3])
        
        if not tags:
            return prompt
        
        return f"{', '.join(tags)}, {prompt}"
    
    def get_negative_prompt(
        self,
        scene_type: Optional[str] = None,
        style: Optional[str] = None,
        custom: Optional[List[str]] = None
    ) -> Optional[str]:
        """
        Генерирует негативный промпт.
        
        Args:
            scene_type: Тип сцены для специфичных негативов
            style: Стиль для специфичных негативов
            custom: Кастомные негативные теги
            
        Returns:
            Негативный промпт или None
        """
        if not self.supports_negative:
            return None
        
        negatives = list(self.config.negative_tags)
        
        # Добавляем кастомные
        if custom:
            negatives.extend(custom)
        
        # Убираем дубликаты, сохраняя порядок
        seen = set()
        unique_negatives = []
        for neg in negatives:
            if neg.lower() not in seen:
                seen.add(neg.lower())
                unique_negatives.append(neg)
        
        return ", ".join(unique_negatives) if unique_negatives else None
    
    def truncate(self, prompt: str) -> str:
        """
        Умно обрезает промпт до максимальной длины.
        
        Args:
            prompt: Промпт для обрезки
            
        Returns:
            Обрезанный промпт
        """
        if len(prompt) <= self.max_length:
            return prompt
        
        # Ищем последнюю запятую в допустимом диапазоне
        truncated = prompt[:self.max_length]
        last_comma = truncated.rfind(",")
        
        # Если запятая найдена и она не слишком далеко
        if last_comma > self.max_length * 0.7:
            return truncated[:last_comma].strip()
        
        # Ищем последний пробел
        last_space = truncated.rfind(" ")
        if last_space > self.max_length * 0.8:
            return truncated[:last_space].strip()
        
        return truncated.strip()
    
    def validate(self, prompt: str) -> tuple[bool, List[str]]:
        """
        Валидирует промпт.
        
        Args:
            prompt: Промпт для валидации
            
        Returns:
            Кортеж (is_valid, list_of_issues)
        """
        issues = []
        
        if not prompt or not prompt.strip():
            issues.append("Prompt is empty")
            return False, issues
        
        if len(prompt) > self.max_length:
            issues.append(f"Prompt exceeds max length ({len(prompt)} > {self.max_length})")
        
        if len(prompt) < 10:
            issues.append("Prompt is too short (less than 10 characters)")
        
        # Проверка на проблемные символы
        problematic_chars = ['<', '>', '{', '}', '|', '\\']
        for char in problematic_chars:
            if char in prompt and self.model_name not in ['stable-diffusion']:
                issues.append(f"Prompt contains potentially problematic character: {char}")
        
        return len(issues) == 0, issues
    
    def get_parameters(self) -> Dict[str, Any]:
        """Возвращает текущие параметры генерации"""
        return self._parameters.copy()
    
    def has_capability(self, capability: ModelCapability) -> bool:
        """Проверяет наличие возможности у модели"""
        return capability in self.config.capabilities
    
    def get_default_parameters(self) -> Dict[str, Any]:
        """Возвращает параметры по умолчанию"""
        return self.config.default_parameters.copy()
    
    def merge_parameters(self, custom: Optional[Dict] = None) -> Dict[str, Any]:
        """Объединяет дефолтные параметры с кастомными"""
        params = self.get_default_parameters()
        if custom:
            params.update(custom)
        return params


class GeneratorFactory:
    """
    Фабрика для создания генераторов.
    Паттерн Factory для lazy loading и кэширования.
    """
    
    _generators: Dict[str, Type[BaseGenerator]] = {}
    _instances: Dict[str, BaseGenerator] = {}
    _loaded: bool = False
    
    @classmethod
    def register(cls, name: str, generator_class: Type[BaseGenerator]):
        """Регистрирует класс генератора"""
        cls._generators[name] = generator_class
    
    @classmethod
    def get_generator(
        cls, 
        name: str, 
        config: Optional[Dict] = None,
        fresh: bool = False
    ) -> BaseGenerator:
        """
        Получает экземпляр генератора.
        
        Args:
            name: Имя генератора (midjourney, dalle3, stable-diffusion, flux)
            config: Опциональная конфигурация
            fresh: Создать новый экземпляр вместо кэшированного
            
        Returns:
            Экземпляр генератора
        """
        if not cls._loaded:
            cls._load_generators()
        
        name = name.lower()
        
        if name not in cls._generators:
            raise ValueError(f"Unknown generator: {name}. Available: {list(cls._generators.keys())}")
        
        # Возвращаем кэшированный или создаём новый
        cache_key = f"{name}_{hash(str(config))}"
        
        if not fresh and cache_key in cls._instances:
            return cls._instances[cache_key]
        
        instance = cls._generators[name](config)
        cls._instances[cache_key] = instance
        
        return instance
    
    @classmethod
    def _load_generators(cls):
        """Ленивая загрузка всех генераторов"""
        if cls._loaded:
            return
        
        try:
            from core.generators.midjourney_gen import MidjourneyGenerator
            from core.generators.dalle_gen import DallE3Generator
            from core.generators.sd_gen import StableDiffusionGenerator
            from core.generators.flux_gen import FluxGenerator
            
            cls._generators = {
                "midjourney": MidjourneyGenerator,
                "mj": MidjourneyGenerator,
                "dalle3": DallE3Generator,
                "dalle": DallE3Generator,
                "dall-e": DallE3Generator,
                "stable-diffusion": StableDiffusionGenerator,
                "sd": StableDiffusionGenerator,
                "sdxl": StableDiffusionGenerator,
                "flux": FluxGenerator,
                "flux-pro": FluxGenerator,
                "flux-dev": FluxGenerator
            }
            
            cls._loaded = True
        except ImportError as e:
            print(f"Warning: Could not load some generators: {e}")
            cls._loaded = True
    
    @classmethod
    def list_generators(cls) -> List[str]:
        """Возвращает список доступных генераторов"""
        if not cls._loaded:
            cls._load_generators()
        
        # Убираем алиасы
        unique = set()
        for name, gen_class in cls._generators.items():
            unique.add(gen_class._get_config.__name__ if hasattr(gen_class, '_get_config') else name)
        
        return ["midjourney", "dalle3", "stable-diffusion", "flux"]
    
    @classmethod
    def get_generator_info(cls, name: str) -> Dict[str, Any]:
        """Возвращает информацию о генераторе"""
        generator = cls.get_generator(name)
        config = generator.config
        
        return {
            "name": config.model_name,
            "display_name": config.display_name,
            "max_prompt_length": config.max_prompt_length,
            "supports_negative": config.supports_negative,
            "capabilities": [c.value for c in config.capabilities],
            "default_aspect_ratio": config.default_aspect_ratio
        }