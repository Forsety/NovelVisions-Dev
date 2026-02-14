# core/managers/scene_manager.py
"""
Scene Manager - управление сценами/локациями для консистентности.
РЕФАКТОРИНГ: story_id → book_id
"""

import json
import uuid
import logging
from typing import Dict, List, Optional, Any
from datetime import datetime
from sqlalchemy import select, delete, func
from sqlalchemy.ext.asyncio import AsyncSession

from models.domain.scene import Scene
from services.storage.cache_service import CacheService
from services.ai.openai_service import OpenAIService

logger = logging.getLogger(__name__)


class SceneManager:
    """
    Менеджер сцен/локаций.
    
    Отвечает за:
    - CRUD операции со сценами
    - Генерацию промптов для локаций
    - Поддержание консистентности окружения
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
        location_type: Optional[str] = None,
        setting: Optional[str] = None,
        time_period: Optional[str] = None,
        architecture_style: Optional[str] = None,
        size_scale: Optional[str] = None,
        key_elements: Optional[List[str]] = None,
        props: Optional[List[str]] = None,
        colors: Optional[List[str]] = None,
        materials: Optional[List[str]] = None,
        default_lighting: Optional[str] = None,
        default_weather: Optional[str] = None,
        default_time_of_day: Optional[str] = None,
        atmosphere: Optional[str] = None,
        mood: Optional[str] = None,
        sounds: Optional[str] = None,
        smells: Optional[str] = None,
        reference_image_url: Optional[str] = None,
        attributes: Optional[Dict[str, Any]] = None,
        **kwargs
    ) -> Dict[str, Any]:
        """Создать новую сцену/локацию."""
        
        scene_id = str(uuid.uuid4())
        
        # Генерировать base_prompt
        base_prompt = await self._generate_base_prompt({
            "name": name,
            "description": description,
            "location_type": location_type,
            "setting": setting,
            "architecture_style": architecture_style,
            "key_elements": key_elements,
            "default_lighting": default_lighting,
            "atmosphere": atmosphere
        })
        
        scene = Scene(
            id=scene_id,
            book_id=book_id,
            name=name,
            description=description,
            location_type=location_type,
            setting=setting,
            time_period=time_period,
            architecture_style=architecture_style,
            size_scale=size_scale,
            key_elements=key_elements or [],
            props=props or [],
            colors=colors or [],
            materials=materials or [],
            default_lighting=default_lighting,
            default_weather=default_weather,
            default_time_of_day=default_time_of_day,
            atmosphere=atmosphere,
            mood=mood,
            sounds=sounds,
            smells=smells,
            reference_image_url=reference_image_url,
            attributes=attributes or {},
            base_prompt=base_prompt,
            is_established=False,
            generation_count=0,
            created_at=datetime.utcnow()
        )
        
        self.db.add(scene)
        await self.db.commit()
        await self.db.refresh(scene)
        
        # Кэшировать
        await self._cache_scene(scene)
        
        logger.info(f"Created scene '{name}' (ID: {scene_id}) for book {book_id}")
        
        return scene.to_dict()
    
    # ===========================================
    # READ
    # ===========================================
    
    async def get_by_id(self, scene_id: str) -> Optional[Dict[str, Any]]:
        """Получить сцену по ID."""
        
        # Проверить кэш
        cache_key = f"scene:{scene_id}"
        cached = await self.cache.get(cache_key)
        if cached:
            return json.loads(cached)
        
        result = await self.db.execute(
            select(Scene).where(Scene.id == scene_id)
        )
        scene = result.scalar_one_or_none()
        
        if not scene:
            return None
        
        data = scene.to_dict()
        await self.cache.set(cache_key, json.dumps(data), expire=3600)
        
        return data
    
    async def get_by_name(self, book_id: str, name: str) -> Optional[Dict[str, Any]]:
        """Найти сцену по имени в книге."""
        
        result = await self.db.execute(
            select(Scene).where(
                Scene.book_id == book_id,
                Scene.name.ilike(f"%{name}%")
            )
        )
        scene = result.scalar_one_or_none()
        
        return scene.to_dict() if scene else None
    
    async def get_by_book(
        self, 
        book_id: str,
        skip: int = 0,
        limit: int = 50
    ) -> List[Dict[str, Any]]:
        """Получить все сцены книги."""
        
        # Проверить кэш
        cache_key = f"scenes:book:{book_id}"
        cached = await self.cache.get(cache_key)
        if cached and skip == 0:
            return json.loads(cached)
        
        result = await self.db.execute(
            select(Scene)
            .where(Scene.book_id == book_id)
            .order_by(Scene.importance.desc(), Scene.name)
            .offset(skip)
            .limit(limit)
        )
        scenes = result.scalars().all()
        
        data = [s.to_dict() for s in scenes]
        
        if skip == 0:
            await self.cache.set(cache_key, json.dumps(data), expire=1800)
        
        return data
    
    async def search(
        self,
        book_id: str,
        query: str,
        limit: int = 10
    ) -> List[Dict[str, Any]]:
        """Поиск сцен по запросу."""
        
        result = await self.db.execute(
            select(Scene).where(
                Scene.book_id == book_id,
                (Scene.name.ilike(f"%{query}%")) |
                (Scene.description.ilike(f"%{query}%")) |
                (Scene.location_type.ilike(f"%{query}%"))
            ).limit(limit)
        )
        scenes = result.scalars().all()
        
        return [s.to_dict() for s in scenes]
    
    # ===========================================
    # UPDATE
    # ===========================================
    
    async def update(
        self,
        scene_id: str,
        **kwargs
    ) -> Optional[Dict[str, Any]]:
        """Обновить сцену."""
        
        result = await self.db.execute(
            select(Scene).where(Scene.id == scene_id)
        )
        scene = result.scalar_one_or_none()
        
        if not scene:
            return None
        
        for key, value in kwargs.items():
            if hasattr(scene, key) and value is not None:
                setattr(scene, key, value)
        
        scene.updated_at = datetime.utcnow()
        
        # Перегенерировать base_prompt если изменились визуальные характеристики
        visual_fields = ['description', 'key_elements', 'atmosphere', 'default_lighting']
        if any(f in kwargs for f in visual_fields):
            scene.base_prompt = await self._generate_base_prompt(scene.to_dict())
        
        await self.db.commit()
        await self.db.refresh(scene)
        
        await self._cache_scene(scene)
        
        return scene.to_dict()
    
    async def mark_established(self, scene_id: str) -> Optional[Dict[str, Any]]:
        """Пометить сцену как 'установленную'."""
        
        result = await self.db.execute(
            select(Scene).where(Scene.id == scene_id)
        )
        scene = result.scalar_one_or_none()
        
        if not scene:
            return None
        
        scene.is_established = True
        scene.updated_at = datetime.utcnow()
        
        await self.db.commit()
        await self._cache_scene(scene)
        
        return scene.to_dict()
    
    async def increment_generation_count(self, scene_id: str):
        """Увеличить счётчик генераций."""
        
        result = await self.db.execute(
            select(Scene).where(Scene.id == scene_id)
        )
        scene = result.scalar_one_or_none()
        
        if scene:
            scene.generation_count += 1
            scene.appearance_count += 1
            await self.db.commit()
    
    # ===========================================
    # DELETE
    # ===========================================
    
    async def delete(self, scene_id: str) -> bool:
        """Удалить сцену."""
        
        result = await self.db.execute(
            select(Scene).where(Scene.id == scene_id)
        )
        scene = result.scalar_one_or_none()
        
        if not scene:
            return False
        
        book_id = scene.book_id
        
        await self.db.delete(scene)
        await self.db.commit()
        
        await self.cache.delete(f"scene:{scene_id}")
        await self.cache.delete(f"scenes:book:{book_id}")
        
        return True
    
    async def delete_by_book(self, book_id: str) -> int:
        """Удалить все сцены книги."""
        
        result = await self.db.execute(
            select(func.count()).select_from(Scene).where(
                Scene.book_id == book_id
            )
        )
        count = result.scalar()
        
        await self.db.execute(
            delete(Scene).where(Scene.book_id == book_id)
        )
        await self.db.commit()
        
        await self.cache.delete(f"scenes:book:{book_id}")
        
        return count
    
    # ===========================================
    # PROMPT GENERATION
    # ===========================================
    
    async def generate_prompt(
        self,
        scene_id: str,
        time_of_day: Optional[str] = None,
        weather: Optional[str] = None,
        lighting: Optional[str] = None,
        characters: Optional[List[str]] = None,
        action: Optional[str] = None,
        target_model: str = "dalle3",
        style: Optional[str] = None,
        camera_angle: Optional[str] = None,
        shot_type: Optional[str] = None
    ) -> Optional[Dict[str, Any]]:
        """Сгенерировать промпт для сцены."""
        
        scene = await self.get_by_id(scene_id)
        if not scene:
            return None
        
        # Получить base prompt
        base_prompt = scene.get("base_prompt") or self._build_base_prompt(scene)
        
        # Собрать промпт
        parts = [base_prompt]
        
        # Время суток
        effective_time = time_of_day or scene.get("default_time_of_day")
        if effective_time:
            parts.append(f"{effective_time} lighting")
        
        # Погода
        effective_weather = weather or scene.get("default_weather")
        if effective_weather:
            parts.append(effective_weather)
        
        # Освещение
        effective_lighting = lighting or scene.get("default_lighting")
        if effective_lighting:
            parts.append(effective_lighting)
        
        # Атмосфера
        if scene.get("atmosphere"):
            parts.append(f"{scene['atmosphere']} atmosphere")
        
        # Персонажи
        if characters:
            parts.append(f"with {', '.join(characters)}")
        
        # Действие
        if action:
            parts.append(action)
        
        # Ракурс камеры
        if shot_type:
            parts.append(f"{shot_type} shot")
        if camera_angle:
            parts.append(f"{camera_angle} angle")
        
        # Стиль
        if style:
            parts.append(f"{style} style")
        
        prompt = ", ".join(parts)
        
        # Увеличить счётчик
        await self.increment_generation_count(scene_id)
        
        return {
            "scene_id": scene_id,
            "scene_name": scene["name"],
            "prompt": prompt,
            "target_model": target_model,
            "elements": {
                "base": base_prompt,
                "time_of_day": effective_time,
                "weather": effective_weather,
                "lighting": effective_lighting,
                "characters": characters,
                "action": action,
                "camera_angle": camera_angle,
                "shot_type": shot_type,
                "style": style
            }
        }
    
    # ===========================================
    # PRIVATE METHODS
    # ===========================================
    
    def _build_base_prompt(self, scene: Dict[str, Any]) -> str:
        """Собрать базовый промпт из характеристик."""
        
        parts = []
        
        # Тип локации
        if scene.get("location_type"):
            parts.append(scene["location_type"])
        
        # Название
        parts.append(scene.get("name", "scene"))
        
        # Сеттинг
        if scene.get("setting"):
            parts.append(scene["setting"])
        
        # Архитектурный стиль
        if scene.get("architecture_style"):
            parts.append(f"{scene['architecture_style']} architecture")
        
        # Ключевые элементы
        if scene.get("key_elements"):
            elements = scene["key_elements"][:5]  # Макс 5 элементов
            parts.append(", ".join(elements))
        
        # Атмосфера
        if scene.get("atmosphere"):
            parts.append(f"{scene['atmosphere']} atmosphere")
        
        return ", ".join(parts)
    
    async def _generate_base_prompt(self, data: Dict[str, Any]) -> str:
        """Сгенерировать улучшенный base_prompt через AI."""
        
        meaningful_fields = [
            data.get("description"),
            data.get("key_elements"),
            data.get("atmosphere")
        ]
        
        if not any(meaningful_fields):
            return self._build_base_prompt(data)
        
        try:
            system_prompt = """Create a concise visual description for scene/location image generation.
            Include: setting, architecture, key elements, atmosphere, lighting.
            Format: comma-separated phrases, no sentences.
            Max 100 words."""
            
            user_prompt = f"""Location: {data.get('name', 'scene')}
            Type: {data.get('location_type', '')}
            Setting: {data.get('setting', '')}
            Architecture: {data.get('architecture_style', '')}
            Key elements: {data.get('key_elements', [])}
            Atmosphere: {data.get('atmosphere', '')}
            Description: {data.get('description', '')}"""
            
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
    
    async def _cache_scene(self, scene: Scene):
        """Кэшировать сцену."""
        
        data = scene.to_dict()
        await self.cache.set(
            f"scene:{scene.id}",
            json.dumps(data),
            expire=3600
        )
        
        await self.cache.delete(f"scenes:book:{scene.book_id}")