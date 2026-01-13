import json
import uuid
from typing import Dict, List, Optional
from datetime import datetime
from sqlalchemy.ext.asyncio import AsyncSession

from models.domain.object import Object
from services.storage.cache_service import CacheService
from services.ai.openai_service import OpenAIService


class ObjectManager:
    """Manager for object operations"""
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.ai_service = OpenAIService()
    
    async def create(
        self,
        name: str,
        description: str,
        user_id: str,
        story_id: Optional[str] = None,
        **kwargs
    ) -> Dict:
        """Create a new object"""
        
        object_id = str(uuid.uuid4())
        
        # Extract object properties
        properties = await self._extract_object_properties(description)
        
        obj = Object(
            id=object_id,
            user_id=user_id,
            story_id=story_id,
            name=name,
            description=description,
            appearance=kwargs.get("appearance") or properties.get("appearance"),
            size=kwargs.get("size") or properties.get("size"),
            material=kwargs.get("material") or properties.get("material"),
            color=kwargs.get("color") or properties.get("color"),
            properties=kwargs.get("properties") or properties,
            created_at=datetime.utcnow()
        )
        
        self.db.add(obj)
        await self.db.commit()
        
        # Cache object
        await self._cache_object(obj)
        
        return self._serialize_object(obj)
    
    async def get_story_objects(self, story_id: str) -> List[Dict]:
        """Get all objects for a story"""
        
        cache_key = f"objects:story:{story_id}"
        cached = await self.cache.get(cache_key)
        if cached:
            return json.loads(cached)
        
        from sqlalchemy import select
        result = await self.db.execute(
            select(Object).where(Object.story_id == story_id)
        )
        objects = result.scalars().all()
        
        serialized = [self._serialize_object(obj) for obj in objects]
        
        # Cache result
        await self.cache.set(cache_key, json.dumps(serialized), expire=3600)
        
        return serialized
    
    async def track_object_usage(
        self,
        object_id: str,
        page_number: int,
        context: str
    ):
        """Track where an object is used in the story"""
        
        # Get object
        from sqlalchemy import select
        result = await self.db.execute(
            select(Object).where(Object.id == object_id)
        )
        obj = result.scalar_one_or_none()
        
        if not obj:
            return
        
        # Update usage tracking
        if not obj.usage_tracking:
            obj.usage_tracking = {}
        
        obj.usage_tracking[str(page_number)] = {
            "context": context,
            "timestamp": datetime.utcnow().isoformat()
        }
        
        await self.db.commit()
        
        # Update cache
        await self._cache_object(obj)
    
    async def _extract_object_properties(self, description: str) -> Dict:
        """Extract object properties from description"""
        
        system_prompt = """Extract object properties from the description.
        Return JSON with keys: appearance, size, material, color, special_properties.
        Be specific and visual."""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=description,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return {}
    
    async def _cache_object(self, obj: Object):
        """Cache object data"""
        
        cache_key = f"object:{obj.id}"
        data = self._serialize_object(obj)
        await self.cache.set(cache_key, json.dumps(data), expire=3600)
    
    def _serialize_object(self, obj: Object) -> Dict:
        """Serialize object to dict"""
        
        return {
            "id": obj.id,
            "user_id": obj.user_id,
            "story_id": obj.story_id,
            "name": obj.name,
            "description": obj.description,
            "appearance": obj.appearance,
            "size": obj.size,
            "material": obj.material,
            "color": obj.color,
            "properties": obj.properties,
            "usage_tracking": obj.usage_tracking,
            "created_at": obj.created_at.isoformat()
        }
