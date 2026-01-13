# services/storage/cache_service.py
"""
Сервис кэширования с Redis.
"""

import json
import asyncio  # <-- ДОБАВЛЕНО
from typing import Optional, Any, List, Dict, TypeVar
import hashlib

try:
    import redis.asyncio as redis
    REDIS_AVAILABLE = True
except ImportError:
    redis = None  # type: ignore
    REDIS_AVAILABLE = False

from app.config import settings


T = TypeVar('T')


class CacheService:
    """
    Асинхронный сервис кэширования.
    """
    
    def __init__(
        self,
        redis_url: Optional[str] = None,
        prefix: str = "promptgen:"
    ):
        self.prefix = prefix
        self._client: Optional[Any] = None  # redis.Redis or None
        self._redis_url = redis_url or self._get_redis_url()
        
        # Fallback на in-memory если Redis недоступен
        self._memory_cache: Dict[str, Any] = {}
        self._use_memory = not REDIS_AVAILABLE
    
    def _get_redis_url(self) -> str:
        """Формирует URL Redis"""
        if hasattr(settings, 'REDIS_URL') and settings.REDIS_URL:
            return settings.REDIS_URL
        
        host = getattr(settings, 'REDIS_HOST', 'localhost')
        port = getattr(settings, 'REDIS_PORT', 6379)
        db = getattr(settings, 'REDIS_DB', 0)
        password = getattr(settings, 'REDIS_PASSWORD', None)
        
        if password:
            return f"redis://:{password}@{host}:{port}/{db}"
        return f"redis://{host}:{port}/{db}"
    
    async def _get_client(self) -> Optional[Any]:
        """Получает или создаёт клиент Redis"""
        if self._use_memory or redis is None:
            return None
        
        if self._client is None:
            try:
                self._client = redis.from_url(
                    self._redis_url,
                    encoding="utf-8",
                    decode_responses=True
                )
                # Проверяем подключение
                await self._client.ping()
            except Exception as e:
                print(f"Redis connection failed: {e}, using memory cache")
                self._use_memory = True
                return None
        
        return self._client
    
    def _make_key(self, key: str) -> str:
        """Создаёт полный ключ с префиксом"""
        return f"{self.prefix}{key}"
    
    async def get(self, key: str) -> Optional[str]:
        """Получает значение по ключу."""
        full_key = self._make_key(key)
        
        if self._use_memory:
            return self._memory_cache.get(full_key)
        
        client = await self._get_client()
        if client:
            return await client.get(full_key)
        
        return self._memory_cache.get(full_key)
    
    async def set(
        self,
        key: str,
        value: str,
        expire: Optional[int] = None
    ) -> bool:
        """Устанавливает значение."""
        full_key = self._make_key(key)
        
        if self._use_memory:
            self._memory_cache[full_key] = value
            return True
        
        client = await self._get_client()
        if client:
            if expire:
                await client.setex(full_key, expire, value)
            else:
                await client.set(full_key, value)
            return True
        
        self._memory_cache[full_key] = value
        return True
    
    async def get_json(self, key: str) -> Optional[Any]:
        """Получает JSON значение"""
        value = await self.get(key)
        if value:
            try:
                return json.loads(value)
            except json.JSONDecodeError:
                return None
        return None
    
    async def set_json(
        self,
        key: str,
        value: Any,
        expire: Optional[int] = None
    ) -> bool:
        """Устанавливает JSON значение"""
        try:
            json_value = json.dumps(value, default=str)
            return await self.set(key, json_value, expire)
        except Exception as e:
            print(f"JSON serialization error: {e}")
            return False
    
    async def delete(self, key: str) -> bool:
        """Удаляет ключ"""
        full_key = self._make_key(key)
        
        if self._use_memory:
            self._memory_cache.pop(full_key, None)
            return True
        
        client = await self._get_client()
        if client:
            await client.delete(full_key)
            return True
        
        self._memory_cache.pop(full_key, None)
        return True
    
    async def exists(self, key: str) -> bool:
        """Проверяет существование ключа"""
        full_key = self._make_key(key)
        
        if self._use_memory:
            return full_key in self._memory_cache
        
        client = await self._get_client()
        if client:
            result = await client.exists(full_key)
            return result > 0
        
        return full_key in self._memory_cache
    
    async def expire(self, key: str, seconds: int) -> bool:
        """Устанавливает TTL для ключа"""
        full_key = self._make_key(key)
        
        if self._use_memory:
            return True
        
        client = await self._get_client()
        if client:
            return await client.expire(full_key, seconds)
        
        return True
    
    async def ttl(self, key: str) -> int:
        """Получает TTL ключа"""
        full_key = self._make_key(key)
        
        if self._use_memory:
            return -1
        
        client = await self._get_client()
        if client:
            return await client.ttl(full_key)
        
        return -1
    
    async def keys(self, pattern: str = "*") -> List[str]:
        """Получает ключи по паттерну"""
        full_pattern = self._make_key(pattern)
        
        if self._use_memory:
            import fnmatch
            return [
                k[len(self.prefix):] for k in self._memory_cache.keys()
                if fnmatch.fnmatch(k, full_pattern)
            ]
        
        client = await self._get_client()
        if client:
            keys_list = await client.keys(full_pattern)
            return [k[len(self.prefix):] for k in keys_list]
        
        return []
    
    async def delete_pattern(self, pattern: str) -> int:
        """Удаляет ключи по паттерну"""
        keys_list = await self.keys(pattern)
        
        for key in keys_list:
            await self.delete(key)
        
        return len(keys_list)
    
    async def incr(self, key: str, amount: int = 1) -> int:
        """Инкрементирует значение"""
        full_key = self._make_key(key)
        
        if self._use_memory:
            current = int(self._memory_cache.get(full_key, 0))
            new_value = current + amount
            self._memory_cache[full_key] = str(new_value)
            return new_value
        
        client = await self._get_client()
        if client:
            return await client.incrby(full_key, amount)
        
        return 0
    
    async def decr(self, key: str, amount: int = 1) -> int:
        """Декрементирует значение"""
        return await self.incr(key, -amount)
    
    # === Hash операции ===
    
    async def hget(self, name: str, key: str) -> Optional[str]:
        """Получает поле хэша"""
        full_name = self._make_key(name)
        
        if self._use_memory:
            hash_data = self._memory_cache.get(full_name, {})
            return hash_data.get(key) if isinstance(hash_data, dict) else None
        
        client = await self._get_client()
        if client:
            return await client.hget(full_name, key)
        
        return None
    
    async def hset(self, name: str, key: str, value: str) -> bool:
        """Устанавливает поле хэша"""
        full_name = self._make_key(name)
        
        if self._use_memory:
            if full_name not in self._memory_cache:
                self._memory_cache[full_name] = {}
            self._memory_cache[full_name][key] = value
            return True
        
        client = await self._get_client()
        if client:
            await client.hset(full_name, key, value)
            return True
        
        return False
    
    async def hgetall(self, name: str) -> Dict[str, str]:
        """Получает весь хэш"""
        full_name = self._make_key(name)
        
        if self._use_memory:
            data = self._memory_cache.get(full_name, {})
            return data if isinstance(data, dict) else {}
        
        client = await self._get_client()
        if client:
            return await client.hgetall(full_name)
        
        return {}
    
    async def hdel(self, name: str, *keys: str) -> int:
        """Удаляет поля хэша"""
        full_name = self._make_key(name)
        
        if self._use_memory:
            hash_data = self._memory_cache.get(full_name, {})
            if isinstance(hash_data, dict):
                deleted = 0
                for key in keys:
                    if key in hash_data:
                        del hash_data[key]
                        deleted += 1
                return deleted
            return 0
        
        client = await self._get_client()
        if client:
            return await client.hdel(full_name, *keys)
        
        return 0
    
    # === Utility методы ===
    
    def cache_key(self, *parts: str) -> str:
        """Создаёт ключ кэша из частей"""
        return ":".join(str(p) for p in parts)
    
    def hash_key(self, data: Any) -> str:
        """Создаёт хэш ключ из данных"""
        if isinstance(data, str):
            content = data
        else:
            content = json.dumps(data, sort_keys=True, default=str)
        
        return hashlib.md5(content.encode()).hexdigest()
    
    async def get_or_set(
        self,
        key: str,
        factory: Any,
        expire: Optional[int] = None
    ) -> Any:
        """Получает значение или создаёт через factory."""
        value = await self.get_json(key)
        
        if value is not None:
            return value
        
        # Создаём новое значение
        if asyncio.iscoroutinefunction(factory):
            value = await factory()
        else:
            value = factory()
        
        await self.set_json(key, value, expire)
        return value
    
    async def health_check(self) -> bool:
        """Проверка работоспособности"""
        if self._use_memory:
            return True
        
        try:
            client = await self._get_client()
            if client:
                await client.ping()
                return True
        except Exception:
            pass
        
        return False
    
    async def close(self) -> None:
        """Закрывает соединение"""
        if self._client:
            await self._client.close()
            self._client = None


# Глобальный экземпляр
cache_service = CacheService()


# Dependency injection
async def get_cache() -> CacheService:
    """Dependency для получения кэша"""
    return cache_service