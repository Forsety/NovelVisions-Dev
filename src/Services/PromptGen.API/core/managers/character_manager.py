# core/managers/character_manager.py
"""
Character Manager - управление персонажами для консистентности.
РЕФАКТОРИНГ: story_id → book_id
"""

import json
import uuid
import logging
from typing import Dict, List, Optional, Any
from datetime import datetime
from sqlalchemy import select, delete, func
from sqlalchemy.ext.asyncio import AsyncSession

from models.domain.character import Character
from services.storage.cache_service import CacheService
from services.ai.openai_service import OpenAIService

logger = logging.getLogger(__name__)


class CharacterManager:
    """
    Менеджер персонажей.
    
    Отвечает за:
    - CRUD операции с персонажами
    - Генерацию промптов для персонажей
    - Поддержание консистентности внешности
    """
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.ai_service = OpenAIService()
    
    # ===========================================
    # CREATE
    # ===========================================
    
    async def create(
        self,
        book_id: str,
        name: str,
        description: Optional[str] = None,
        role: Optional[str] = None,
        gender: Optional[str] = None,
        age: Optional[str] = None,
        height: Optional[str] = None,
        build: Optional[str] = None,
        appearance: Optional[str] = None,
        hair: Optional[str] = None,
        eyes: Optional[str] = None,
        skin: Optional[str] = None,
        facial_features: Optional[str] = None,
        distinguishing_features: Optional[str] = None,
        default_clothing: Optional[str] = None,
        accessories: Optional[str] = None,
        aliases: Optional[List[str]] = None,
        attributes: Optional[Dict[str, Any]] = None,
        reference_image_url: Optional[str] = None,
        **kwargs
    ) -> Dict[str, Any]:
        """Создать нового персонажа."""
        
        character_id = str(uuid.uuid4())
        
        # Создать base_prompt если достаточно данных
        base_prompt = await self._generate_base_prompt({
            "name": name,
            "gender": gender,
            "age": age,
            "height": height,
            "build": build,
            "appearance": appearance,
            "hair": hair,
            "eyes": eyes,
            "skin": skin,
            "facial_features": facial_features,
            "distinguishing_features": distinguishing_features,
            "default_clothing": default_clothing,
            "accessories": accessories
        })
        
        character = Character(
            id=character_id,
            book_id=book_id,
            name=name,
            description=description,
            role=role,
            gender=gender,
            age=age,
            height=height,
            build=build,
            appearance=appearance,
            hair=hair,
            eyes=eyes,
            skin=skin,
            facial_features=facial_features,
            distinguishing_features=distinguishing_features,
            default_clothing=default_clothing,
            accessories=accessories,
            aliases=aliases or [],
            attributes=attributes or {},
            reference_image_url=reference_image_url,
            base_prompt=base_prompt,
            is_established=False,
            generation_count=0,
            created_at=datetime.utcnow()
        )
        
        self.db.add(character)
        await self.db.commit()
        await self.db.refresh(character)
        
        # Кэшировать
        await self._cache_character(character)
        
        logger.info(f"Created character '{name}' (ID: {character_id}) for book {book_id}")
        
        return character.to_dict()
    
    # ===========================================
    # READ
    # ===========================================
    
    async def get_by_id(self, character_id: str) -> Optional[Dict[str, Any]]:
        """Получить персонажа по ID."""
        
        # Проверить кэш
        cache_key = f"character:{character_id}"
        cached = await self.cache.get(cache_key)
        if cached:
            return json.loads(cached)
        
        # Запрос в БД
        result = await self.db.execute(
            select(Character).where(Character.id == character_id)
        )
        character = result.scalar_one_or_none()
        
        if not character:
            return None
        
        data = character.to_dict()
        await self.cache.set(cache_key, json.dumps(data), expire=3600)
        
        return data
    
    async def get_by_name(self, book_id: str, name: str) -> Optional[Dict[str, Any]]:
        """Найти персонажа по имени в книге."""
        
        result = await self.db.execute(
            select(Character).where(
                Character.book_id == book_id,
                Character.name.ilike(f"%{name}%")
            )
        )
        character = result.scalar_one_or_none()
        
        if not character:
            # Поиск по алиасам
            result = await self.db.execute(
                select(Character).where(
                    Character.book_id == book_id,
                    Character.aliases.contains([name])
                )
            )
            character = result.scalar_one_or_none()
        
        return character.to_dict() if character else None
    
    async def get_by_book(
        self, 
        book_id: str,
        skip: int = 0,
        limit: int = 50
    ) -> List[Dict[str, Any]]:
        """Получить всех персонажей книги."""
        
        # Проверить кэш
        cache_key = f"characters:book:{book_id}"
        cached = await self.cache.get(cache_key)
        if cached and skip == 0:
            return json.loads(cached)
        
        result = await self.db.execute(
            select(Character)
            .where(Character.book_id == book_id)
            .order_by(Character.importance.desc(), Character.name)
            .offset(skip)
            .limit(limit)
        )
        characters = result.scalars().all()
        
        data = [c.to_dict() for c in characters]
        
        # Кэшировать первую страницу
        if skip == 0:
            await self.cache.set(cache_key, json.dumps(data), expire=1800)
        
        return data
    
    async def search(
        self,
        book_id: str,
        query: str,
        limit: int = 10
    ) -> List[Dict[str, Any]]:
        """Поиск персонажей по запросу."""
        
        result = await self.db.execute(
            select(Character).where(
                Character.book_id == book_id,
                (Character.name.ilike(f"%{query}%")) |
                (Character.description.ilike(f"%{query}%"))
            ).limit(limit)
        )
        characters = result.scalars().all()
        
        return [c.to_dict() for c in characters]
    
    # ===========================================
    # UPDATE
    # ===========================================
    
    async def update(
        self,
        character_id: str,
        **kwargs
    ) -> Optional[Dict[str, Any]]:
        """Обновить персонажа."""
        
        result = await self.db.execute(
            select(Character).where(Character.id == character_id)
        )
        character = result.scalar_one_or_none()
        
        if not character:
            return None
        
        # Обновить поля
        for key, value in kwargs.items():
            if hasattr(character, key) and value is not None:
                setattr(character, key, value)
        
        character.updated_at = datetime.utcnow()
        
        # Перегенерировать base_prompt если изменились визуальные характеристики
        visual_fields = ['appearance', 'hair', 'eyes', 'skin', 'build', 'clothing']
        if any(f in kwargs for f in visual_fields):
            character.base_prompt = await self._generate_base_prompt(character.to_dict())
        
        await self.db.commit()
        await self.db.refresh(character)
        
        # Обновить кэш
        await self._cache_character(character)
        
        return character.to_dict()
    
    async def mark_established(self, character_id: str) -> Optional[Dict[str, Any]]:
        """Пометить персонажа как 'установленного'."""
        
        result = await self.db.execute(
            select(Character).where(Character.id == character_id)
        )
        character = result.scalar_one_or_none()
        
        if not character:
            return None
        
        character.is_established = True
        character.updated_at = datetime.utcnow()
        
        await self.db.commit()
        await self._cache_character(character)
        
        return character.to_dict()
    
    async def increment_generation_count(self, character_id: str):
        """Увеличить счётчик генераций."""
        
        result = await self.db.execute(
            select(Character).where(Character.id == character_id)
        )
        character = result.scalar_one_or_none()
        
        if character:
            character.generation_count += 1
            character.appearance_count += 1
            await self.db.commit()
    
    # ===========================================
    # DELETE
    # ===========================================
    
    async def delete(self, character_id: str) -> bool:
        """Удалить персонажа."""
        
        result = await self.db.execute(
            select(Character).where(Character.id == character_id)
        )
        character = result.scalar_one_or_none()
        
        if not character:
            return False
        
        book_id = character.book_id
        
        await self.db.delete(character)
        await self.db.commit()
        
        # Очистить кэш
        await self.cache.delete(f"character:{character_id}")
        await self.cache.delete(f"characters:book:{book_id}")
        
        return True
    
    async def delete_by_book(self, book_id: str) -> int:
        """Удалить всех персонажей книги."""
        
        result = await self.db.execute(
            select(func.count()).select_from(Character).where(
                Character.book_id == book_id
            )
        )
        count = result.scalar()
        
        await self.db.execute(
            delete(Character).where(Character.book_id == book_id)
        )
        await self.db.commit()
        
        # Очистить кэш
        await self.cache.delete(f"characters:book:{book_id}")
        
        return count
    
    # ===========================================
    # PROMPT GENERATION
    # ===========================================
    
    async def generate_prompt(
        self,
        character_id: str,
        action: Optional[str] = None,
        emotion: Optional[str] = None,
        pose: Optional[str] = None,
        scene_context: Optional[str] = None,
        target_model: str = "dalle3",
        style: Optional[str] = None
    ) -> Optional[Dict[str, Any]]:
        """Сгенерировать промпт для персонажа."""
        
        character = await self.get_by_id(character_id)
        if not character:
            return None
        
        # Получить base prompt
        base_prompt = character.get("base_prompt") or self._build_base_prompt(character)
        
        # Собрать промпт
        parts = [base_prompt]
        
        if emotion:
            parts.append(f"{emotion} expression")
        
        if pose:
            parts.append(pose)
        
        if action:
            parts.append(action)
        
        if scene_context:
            parts.append(f"in {scene_context}")
        
        if style:
            parts.append(f"{style} style")
        
        prompt = ", ".join(parts)
        
        # Увеличить счётчик
        await self.increment_generation_count(character_id)
        
        return {
            "character_id": character_id,
            "character_name": character["name"],
            "prompt": prompt,
            "target_model": target_model,
            "elements": {
                "base": base_prompt,
                "emotion": emotion,
                "pose": pose,
                "action": action,
                "scene": scene_context,
                "style": style
            }
        }
    
    # ===========================================
    # PRIVATE METHODS
    # ===========================================
    
    def _build_base_prompt(self, character: Dict[str, Any]) -> str:
        """Собрать базовый промпт из характеристик."""
        
        parts = []
        
        # Имя
        parts.append(character.get("name", "character"))
        
        # Возраст и пол
        if character.get("age"):
            parts.append(character["age"])
        if character.get("gender"):
            parts.append(character["gender"])
        
        # Телосложение
        if character.get("build"):
            parts.append(f"{character['build']} build")
        
        # Волосы
        if character.get("hair"):
            parts.append(f"{character['hair']} hair")
        
        # Глаза
        if character.get("eyes"):
            parts.append(f"{character['eyes']} eyes")
        
        # Кожа
        if character.get("skin"):
            parts.append(f"{character['skin']} skin")
        
        # Особенности
        if character.get("distinguishing_features"):
            parts.append(character["distinguishing_features"])
        
        # Одежда
        if character.get("default_clothing"):
            parts.append(f"wearing {character['default_clothing']}")
        
        return ", ".join(parts)
    
    async def _generate_base_prompt(self, data: Dict[str, Any]) -> str:
        """Сгенерировать улучшенный base_prompt через AI."""
        
        # Если мало данных - просто собрать из полей
        meaningful_fields = [
            data.get("appearance"),
            data.get("hair"),
            data.get("eyes"),
            data.get("build")
        ]
        
        if not any(meaningful_fields):
            return self._build_base_prompt(data)
        
        try:
            system_prompt = """Create a concise visual description for image generation.
            Include: physical appearance, hair, eyes, build, clothing.
            Format: comma-separated phrases, no sentences.
            Max 100 words."""
            
            user_prompt = f"""Character: {data.get('name', 'character')}
            Gender: {data.get('gender', 'unknown')}
            Age: {data.get('age', 'unknown')}
            Build: {data.get('build', '')}
            Hair: {data.get('hair', '')}
            Eyes: {data.get('eyes', '')}
            Skin: {data.get('skin', '')}
            Features: {data.get('distinguishing_features', '')}
            Clothing: {data.get('default_clothing', '')}
            Full description: {data.get('appearance', '')}"""
            
            response = await self.ai_service.generate(
                system_prompt=system_prompt,
                user_prompt=user_prompt,
                max_tokens=150,
                temperature=0.3
            )
            
            return response.strip()
            
        except Exception as e:
            logger.warning(f"AI prompt generation failed: {e}")
            return self._build_base_prompt(data)
    
    async def _cache_character(self, character: Character):
        """Кэшировать персонажа."""
        
        data = character.to_dict()
        await self.cache.set(
            f"character:{character.id}",
            json.dumps(data),
            expire=3600
        )
        
        # Инвалидировать список персонажей книги
        await self.cache.delete(f"characters:book:{character.book_id}")