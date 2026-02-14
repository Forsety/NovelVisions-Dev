import json
from typing import Dict, List, Optional
from sqlalchemy.ext.asyncio import AsyncSession

from services.ai.openai_service import OpenAIService
from services.storage.cache_service import CacheService


class ContextAnalyzer:
    """Engine for analyzing story context"""
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.ai_service = OpenAIService()
    
    async def analyze(
        self,
        text: str,
        extract_characters: bool = True,
        extract_scenes: bool = True,
        extract_objects: bool = True
    ) -> Dict:
        """Analyze text and extract context"""
        
        # Check cache
        cache_key = f"context:analyze:{self._generate_hash(text)}"
        cached = await self.cache.get(cache_key)
        if cached:
            return json.loads(cached)
        
        result = {
            "text_length": len(text),
            "word_count": len(text.split())
        }
        
        # Extract requested elements
        if extract_characters:
            result["characters"] = await self._extract_characters(text)
        
        if extract_scenes:
            result["scenes"] = await self._extract_scenes(text)
        
        if extract_objects:
            result["objects"] = await self._extract_objects(text)
        
        # Analyze mood and tone
        result["mood"] = await self._analyze_mood(text)
        result["themes"] = await self._extract_themes(text)
        
        # Cache result
        await self.cache.set(cache_key, json.dumps(result), expire=3600)
        
        return result
    
    async def _extract_characters(self, text: str) -> List[Dict]:
        """Extract characters from text"""
        
        prompt = """Extract all characters from this text.
        For each character, identify:
        - name
        - description (physical appearance if mentioned)
        - role (protagonist, antagonist, supporting, etc.)
        - key traits
        Return as JSON array."""
        
        response = await self.ai_service.generate(
            system_prompt=prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return []
    
    async def _extract_scenes(self, text: str) -> List[Dict]:
        """Extract scenes/locations from text"""
        
        prompt = """Extract all scenes and locations from this text.
        For each scene, identify:
        - location name
        - description
        - time of day (if mentioned)
        - atmosphere/mood
        Return as JSON array."""
        
        response = await self.ai_service.generate(
            system_prompt=prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return []
    
    async def _extract_objects(self, text: str) -> List[Dict]:
        """Extract important objects from text"""
        
        prompt = """Extract significant objects and items from this text.
        Focus on objects that are important to the story.
        For each object, identify:
        - name
        - description
        - significance
        Return as JSON array."""
        
        response = await self.ai_service.generate(
            system_prompt=prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return []
    
    async def _analyze_mood(self, text: str) -> Dict:
        """Analyze mood and atmosphere"""
        
        prompt = """Analyze the mood and atmosphere of this text.
        Identify:
        - primary mood (e.g., tense, cheerful, mysterious)
        - emotional tone
        - atmosphere descriptors
        Return as JSON."""
        
        response = await self.ai_service.generate(
            system_prompt=prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return {"mood": "neutral", "tone": "neutral"}
    
    async def _extract_themes(self, text: str) -> List[str]:
        """Extract themes from text"""
        
        prompt = """Identify the main themes in this text.
        Return as JSON array of theme strings."""
        
        response = await self.ai_service.generate(
            system_prompt=prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return []
    
    def _generate_hash(self, text: str) -> str:
        """Generate cache hash"""
        import hashlib
        return hashlib.md5(text.encode()).hexdigest()[:16]
