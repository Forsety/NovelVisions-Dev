# services/ai/openai_service.py
"""
Сервис для работы с OpenAI API.

Поддерживает:
- GPT-4 для генерации и анализа текста
- text-embedding-3-small для эмбеддингов
- Модерацию контента
- DALL-E 3 генерацию (опционально)
"""

import json
import asyncio
from typing import Optional, List, Dict, Any, Union
from dataclasses import dataclass

import openai
from openai import AsyncOpenAI

from app.config import settings


@dataclass
class GenerationResult:
    """Результат генерации"""
    text: str
    model: str
    usage: Dict[str, int]
    finish_reason: str


@dataclass
class EmbeddingResult:
    """Результат эмбеддинга"""
    vector: List[float]
    model: str
    usage: Dict[str, int]


class OpenAIService:
    """
    Сервис для работы с OpenAI API.
    
    Использование:
        service = OpenAIService()
        result = await service.generate("Describe this scene...")
        embedding = await service.get_embedding("Some text")
    """
    
    def __init__(self, api_key: Optional[str] = None):
        self.api_key = api_key or getattr(settings, 'OPENAI_API_KEY', None)
        
        if not self.api_key:
            raise ValueError("OpenAI API key is required")
        
        self.client = AsyncOpenAI(api_key=self.api_key)
        
        # Модели по умолчанию
        self.default_model = getattr(settings, 'OPENAI_MODEL', 'gpt-4-turbo-preview')
        self.embedding_model = getattr(settings, 'OPENAI_EMBEDDING_MODEL', 'text-embedding-3-small')
        
        # Лимиты
        self.max_retries = 3
        self.retry_delay = 1.0
    
    async def generate(
        self,
        user_prompt: str,
        system_prompt: Optional[str] = None,
        model: Optional[str] = None,
        max_tokens: int = 1000,
        temperature: float = 0.7,
        response_format: Optional[str] = None,
        stop: Optional[List[str]] = None
    ) -> str:
        """
        Генерирует текст через GPT.
        
        Args:
            user_prompt: Промпт пользователя
            system_prompt: Системный промпт
            model: Модель (по умолчанию gpt-4-turbo-preview)
            max_tokens: Максимум токенов
            temperature: Температура (0-2)
            response_format: Формат ответа ("json" для JSON mode)
            stop: Стоп-последовательности
            
        Returns:
            Сгенерированный текст
        """
        model = model or self.default_model
        
        messages = []
        if system_prompt:
            messages.append({"role": "system", "content": system_prompt})
        messages.append({"role": "user", "content": user_prompt})
        
        kwargs = {
            "model": model,
            "messages": messages,
            "max_tokens": max_tokens,
            "temperature": temperature
        }
        
        if stop:
            kwargs["stop"] = stop
        
        # JSON mode
        if response_format == "json":
            kwargs["response_format"] = {"type": "json_object"}
        
        # Retry логика
        last_error = None
        for attempt in range(self.max_retries):
            try:
                response = await self.client.chat.completions.create(**kwargs)
                return response.choices[0].message.content.strip()
                
            except openai.RateLimitError as e:
                last_error = e
                wait_time = self.retry_delay * (2 ** attempt)
                print(f"Rate limit hit, waiting {wait_time}s...")
                await asyncio.sleep(wait_time)
                
            except openai.APIError as e:
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
    ) -> GenerationResult:
        """Генерация с полной информацией о результате"""
        
        model = kwargs.get('model', self.default_model)
        
        messages = []
        if system_prompt:
            messages.append({"role": "system", "content": system_prompt})
        messages.append({"role": "user", "content": user_prompt})
        
        response = await self.client.chat.completions.create(
            model=model,
            messages=messages,
            max_tokens=kwargs.get('max_tokens', 1000),
            temperature=kwargs.get('temperature', 0.7)
        )
        
        choice = response.choices[0]
        
        return GenerationResult(
            text=choice.message.content.strip(),
            model=response.model,
            usage={
                "prompt_tokens": response.usage.prompt_tokens,
                "completion_tokens": response.usage.completion_tokens,
                "total_tokens": response.usage.total_tokens
            },
            finish_reason=choice.finish_reason
        )
    
    async def get_embedding(
        self,
        text: str,
        model: Optional[str] = None
    ) -> List[float]:
        """
        Получает эмбеддинг для текста.
        
        Args:
            text: Текст для эмбеддинга
            model: Модель эмбеддинга
            
        Returns:
            Вектор эмбеддинга
        """
        model = model or self.embedding_model
        
        # Обрезаем слишком длинный текст
        max_chars = 8000  # ~2000 токенов для safety
        if len(text) > max_chars:
            text = text[:max_chars]
        
        response = await self.client.embeddings.create(
            model=model,
            input=text
        )
        
        return response.data[0].embedding
    
    async def get_embeddings_batch(
        self,
        texts: List[str],
        model: Optional[str] = None
    ) -> List[List[float]]:
        """
        Получает эмбеддинги для списка текстов.
        
        Args:
            texts: Список текстов
            model: Модель эмбеддинга
            
        Returns:
            Список векторов
        """
        model = model or self.embedding_model
        
        # Ограничиваем длину каждого текста
        max_chars = 8000
        processed_texts = [t[:max_chars] for t in texts]
        
        response = await self.client.embeddings.create(
            model=model,
            input=processed_texts
        )
        
        # Сортируем по индексу (API может вернуть в другом порядке)
        embeddings = sorted(response.data, key=lambda x: x.index)
        return [e.embedding for e in embeddings]
    
    async def moderate(self, text: str) -> Dict[str, Any]:
        """
        Проверяет текст на соответствие правилам.
        
        Args:
            text: Текст для проверки
            
        Returns:
            Результат модерации
        """
        response = await self.client.moderations.create(input=text)
        
        result = response.results[0]
        
        return {
            "flagged": result.flagged,
            "categories": {
                cat: getattr(result.categories, cat)
                for cat in dir(result.categories)
                if not cat.startswith('_')
            },
            "scores": {
                cat: getattr(result.category_scores, cat)
                for cat in dir(result.category_scores)
                if not cat.startswith('_')
            }
        }
    
    async def analyze_image(
        self,
        image_url: str,
        prompt: str = "Describe this image in detail."
    ) -> str:
        """
        Анализирует изображение через GPT-4 Vision.
        
        Args:
            image_url: URL изображения
            prompt: Промпт для анализа
            
        Returns:
            Описание изображения
        """
        response = await self.client.chat.completions.create(
            model="gpt-4-vision-preview",
            messages=[
                {
                    "role": "user",
                    "content": [
                        {"type": "text", "text": prompt},
                        {
                            "type": "image_url",
                            "image_url": {"url": image_url}
                        }
                    ]
                }
            ],
            max_tokens=1000
        )
        
        return response.choices[0].message.content.strip()
    
    async def generate_dalle_image(
        self,
        prompt: str,
        size: str = "1024x1024",
        quality: str = "standard",
        style: str = "vivid"
    ) -> str:
        """
        Генерирует изображение через DALL-E 3.
        
        Args:
            prompt: Промпт для генерации
            size: Размер (1024x1024, 1792x1024, 1024x1792)
            quality: Качество (standard, hd)
            style: Стиль (vivid, natural)
            
        Returns:
            URL сгенерированного изображения
        """
        response = await self.client.images.generate(
            model="dall-e-3",
            prompt=prompt,
            size=size,
            quality=quality,
            style=style,
            n=1
        )
        
        return response.data[0].url
    
    async def health_check(self) -> bool:
        """Проверка доступности API"""
        try:
            await self.client.models.list()
            return True
        except Exception as e:
            print(f"OpenAI health check failed: {e}")
            return False