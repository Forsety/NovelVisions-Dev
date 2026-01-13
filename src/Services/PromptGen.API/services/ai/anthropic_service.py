# services/ai/anthropic_service.py
"""
Сервис для работы с Anthropic Claude API.

Используется как альтернативный провайдер для:
- Анализа текста
- Генерации промптов
- Сложных рассуждений
"""

import asyncio
from typing import Optional, List, Dict, Any
from dataclasses import dataclass

try:
    import anthropic
    from anthropic import AsyncAnthropic
    ANTHROPIC_AVAILABLE = True
except ImportError:
    ANTHROPIC_AVAILABLE = False

from app.config import settings


@dataclass
class ClaudeResponse:
    """Результат генерации Claude"""
    text: str
    model: str
    usage: Dict[str, int]
    stop_reason: str


class AnthropicService:
    """
    Сервис для работы с Claude API.
    
    Использование:
        service = AnthropicService()
        result = await service.generate("Analyze this text...")
    """
    
    def __init__(self, api_key: Optional[str] = None):
        if not ANTHROPIC_AVAILABLE:
            raise ImportError("anthropic package is not installed. Run: pip install anthropic")
        
        self.api_key = api_key or getattr(settings, 'ANTHROPIC_API_KEY', None)
        
        if not self.api_key:
            raise ValueError("Anthropic API key is required")
        
        self.client = AsyncAnthropic(api_key=self.api_key)
        
        # Модель по умолчанию
        self.default_model = getattr(settings, 'ANTHROPIC_MODEL', 'claude-3-sonnet-20240229')
        
        # Доступные модели
        self.models = {
            "opus": "claude-3-opus-20240229",
            "sonnet": "claude-3-sonnet-20240229",
            "haiku": "claude-3-haiku-20240307"
        }
        
        # Retry параметры
        self.max_retries = 3
        self.retry_delay = 1.0
    
    async def generate(
        self,
        user_prompt: str,
        system_prompt: Optional[str] = None,
        model: Optional[str] = None,
        max_tokens: int = 1000,
        temperature: float = 0.7,
        stop_sequences: Optional[List[str]] = None
    ) -> str:
        """
        Генерирует текст через Claude.
        
        Args:
            user_prompt: Промпт пользователя
            system_prompt: Системный промпт
            model: Модель (opus, sonnet, haiku или полное имя)
            max_tokens: Максимум токенов
            temperature: Температура (0-1)
            stop_sequences: Стоп-последовательности
            
        Returns:
            Сгенерированный текст
        """
        # Определяем модель
        if model in self.models:
            model = self.models[model]
        elif model is None:
            model = self.default_model
        
        kwargs = {
            "model": model,
            "max_tokens": max_tokens,
            "temperature": temperature,
            "messages": [{"role": "user", "content": user_prompt}]
        }
        
        if system_prompt:
            kwargs["system"] = system_prompt
        
        if stop_sequences:
            kwargs["stop_sequences"] = stop_sequences
        
        # Retry логика
        last_error = None
        for attempt in range(self.max_retries):
            try:
                response = await self.client.messages.create(**kwargs)
                
                # Извлекаем текст из ответа
                text_content = ""
                for block in response.content:
                    if hasattr(block, 'text'):
                        text_content += block.text
                
                return text_content.strip()
                
            except anthropic.RateLimitError as e:
                last_error = e
                wait_time = self.retry_delay * (2 ** attempt)
                print(f"Claude rate limit, waiting {wait_time}s...")
                await asyncio.sleep(wait_time)
                
            except anthropic.APIError as e:
                last_error = e
                if attempt < self.max_retries - 1:
                    await asyncio.sleep(self.retry_delay)
                    continue
                raise
        
        raise last_error or Exception("Max retries exceeded")
    
    async def generate_with_details(
        self,
        user_prompt: str,
        system_prompt: Optional[str] = None,
        **kwargs
    ) -> ClaudeResponse:
        """Генерация с полной информацией"""
        
        model = kwargs.get('model', self.default_model)
        if model in self.models:
            model = self.models[model]
        
        request_kwargs = {
            "model": model,
            "max_tokens": kwargs.get('max_tokens', 1000),
            "temperature": kwargs.get('temperature', 0.7),
            "messages": [{"role": "user", "content": user_prompt}]
        }
        
        if system_prompt:
            request_kwargs["system"] = system_prompt
        
        response = await self.client.messages.create(**request_kwargs)
        
        text_content = ""
        for block in response.content:
            if hasattr(block, 'text'):
                text_content += block.text
        
        return ClaudeResponse(
            text=text_content.strip(),
            model=response.model,
            usage={
                "input_tokens": response.usage.input_tokens,
                "output_tokens": response.usage.output_tokens
            },
            stop_reason=response.stop_reason
        )
    
    async def analyze_text(
        self,
        text: str,
        analysis_type: str = "general"
    ) -> Dict[str, Any]:
        """
        Анализирует текст.
        
        Args:
            text: Текст для анализа
            analysis_type: Тип анализа (general, scene, characters, mood)
            
        Returns:
            Результат анализа
        """
        prompts = {
            "general": """Analyze this text and provide:
1. Main themes
2. Key elements
3. Mood/atmosphere
4. Visual elements that could be illustrated

Return as JSON.""",
            
            "scene": """Analyze this text as a scene for illustration:
1. Scene type (action, dialogue, atmospheric, etc.)
2. Location/setting
3. Characters present
4. Key visual elements
5. Lighting and atmosphere
6. Suggested camera angle

Return as JSON.""",
            
            "characters": """Extract character information:
1. Names mentioned
2. Physical descriptions
3. Actions/poses
4. Emotional states
5. Relationships

Return as JSON.""",
            
            "mood": """Analyze the mood and atmosphere:
1. Emotional tone
2. Color palette suggestion
3. Lighting type
4. Weather/time of day
5. Overall atmosphere

Return as JSON."""
        }
        
        prompt = prompts.get(analysis_type, prompts["general"])
        
        system = "You are an expert at analyzing text for visual illustration. Always respond with valid JSON."
        
        response = await self.generate(
            user_prompt=f"{prompt}\n\nText to analyze:\n{text}",
            system_prompt=system,
            temperature=0.3
        )
        
        # Пробуем распарсить JSON
        try:
            import json
            return json.loads(response)
        except:
            return {"raw_analysis": response}
    
    async def enhance_prompt(
        self,
        original_prompt: str,
        style: Optional[str] = None,
        target_model: str = "midjourney"
    ) -> str:
        """
        Улучшает промпт для генерации изображений.
        
        Args:
            original_prompt: Исходный промпт
            style: Желаемый стиль
            target_model: Целевая модель (midjourney, dalle, sd)
            
        Returns:
            Улучшенный промпт
        """
        system = f"""You are an expert at creating prompts for {target_model} image generation.
Your task is to enhance prompts to be more detailed and visually descriptive.

Guidelines:
- Add specific visual details (lighting, colors, textures)
- Include composition elements (camera angle, framing)
- Add atmospheric elements (mood, weather, time of day)
- Be concise but descriptive
- Focus on what can be SEEN, not abstract concepts

{f"Apply {style} style." if style else ""}

Return ONLY the enhanced prompt, no explanations."""

        return await self.generate(
            user_prompt=f"Enhance this prompt:\n{original_prompt}",
            system_prompt=system,
            temperature=0.7,
            max_tokens=500
        )
    
    async def health_check(self) -> bool:
        """Проверка доступности API"""
        try:
            # Простой запрос для проверки
            await self.generate(
                user_prompt="Say 'ok'",
                max_tokens=10
            )
            return True
        except Exception as e:
            print(f"Anthropic health check failed: {e}")
            return False


class AIServiceFactory:
    """Фабрика для AI сервисов"""
    
    @staticmethod
    def get_service(provider: str = "openai"):
        """
        Получает AI сервис по имени провайдера.
        
        Args:
            provider: openai или anthropic
        """
        if provider == "openai":
            from services.ai.openai_service import OpenAIService
            return OpenAIService()
        elif provider == "anthropic":
            return AnthropicService()
        else:
            raise ValueError(f"Unknown AI provider: {provider}")
    
    @staticmethod
    def get_available_providers() -> List[str]:
        """Возвращает список доступных провайдеров"""
        providers = []
        
        if getattr(settings, 'OPENAI_API_KEY', None):
            providers.append("openai")
        
        if getattr(settings, 'ANTHROPIC_API_KEY', None) and ANTHROPIC_AVAILABLE:
            providers.append("anthropic")
        
        return providers