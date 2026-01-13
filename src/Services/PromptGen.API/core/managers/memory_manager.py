import json
from typing import Dict, List, Optional
from sqlalchemy.ext.asyncio import AsyncSession

from services.storage.cache_service import CacheService
from services.storage.vector_store import VectorStore


class MemoryManager:
    """Manager for story memory and context"""
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.vector_store = VectorStore()
    
    async def initialize_story(self, story_id: str):
        """Initialize memory context for a new story"""
        
        context = {
            "story_id": story_id,
            "characters": {},
            "scenes": {},
            "objects": {},
            "plot_points": [],
            "style_elements": [],
            "page_history": {}
        }
        
        cache_key = f"memory:{story_id}"
        await self.cache.set(cache_key, json.dumps(context), expire=86400)
    
    async def get_context(
        self,
        story_id: str,
        page_number: Optional[int] = None
    ) -> Dict:
        """Get memory context for a story"""
        
        cache_key = f"memory:{story_id}"
        cached = await self.cache.get(cache_key)
        
        if cached:
            context = json.loads(cached)
        else:
            context = await self.initialize_story(story_id)
        
        # If page number specified, get page-specific context
        if page_number:
            context["current_page"] = page_number
            context["recent_pages"] = await self._get_recent_pages(
                story_id,
                page_number
            )
        
        return context
    
    async def update_from_page(
        self,
        story_id: str,
        page_number: int,
        analysis: Dict
    ):
        """Update memory context from page analysis"""
        
        context = await self.get_context(story_id)
        
        # Update characters
        for char in analysis.get("characters", []):
            char_name = char.get("name")
            if char_name and char_name not in context["characters"]:
                context["characters"][char_name] = {
                    "first_appearance": page_number,
                    "description": char.get("description"),
                    "appearance": char.get("appearance"),
                    "traits": char.get("traits", [])
                }
        
        # Update scenes
        for scene in analysis.get("scenes", []):
            scene_name = scene.get("location")
            if scene_name and scene_name not in context["scenes"]:
                context["scenes"][scene_name] = {
                    "first_appearance": page_number,
                    "description": scene.get("description"),
                    "atmosphere": scene.get("atmosphere")
                }
        
        # Update objects
        for obj in analysis.get("objects", []):
            obj_name = obj.get("name")
            if obj_name and obj_name not in context["objects"]:
                context["objects"][obj_name] = {
                    "first_appearance": page_number,
                    "description": obj.get("description"),
                    "significance": obj.get("significance")
                }
        
        # Add page to history
        context["page_history"][str(page_number)] = {
            "characters": [c.get("name") for c in analysis.get("characters", [])],
            "scenes": [s.get("location") for s in analysis.get("scenes", [])],
            "mood": analysis.get("mood"),
            "themes": analysis.get("themes", [])
        }
        
        # Save updated context
        cache_key = f"memory:{story_id}"
        await self.cache.set(cache_key, json.dumps(context), expire=86400)
    
    async def _get_recent_pages(
        self,
        story_id: str,
        current_page: int,
        window: int = 3
    ) -> List[Dict]:
        """Get recent page contexts"""
        
        context = await self.get_context(story_id)
        page_history = context.get("page_history", {})
        
        recent = []
        for i in range(max(1, current_page - window), current_page):
            if str(i) in page_history:
                recent.append({
                    "page": i,
                    **page_history[str(i)]
                })
        
        return recent
