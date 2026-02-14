import json
from typing import Dict, List, Optional, Set
from sqlalchemy.ext.asyncio import AsyncSession
import numpy as np

from services.ai.embedding_service import EmbeddingService
from services.storage.cache_service import CacheService
from services.storage.vector_store import VectorStore


class ConsistencyEngine:
    """Engine for maintaining consistency across generations"""
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.embedding_service = EmbeddingService()
        self.vector_store = VectorStore()
        
        # Consistency rules
        self.rules = {
            "character": {
                "required": ["appearance", "clothing", "hair", "eyes"],
                "optional": ["age", "height", "build", "distinguishing_features"]
            },
            "scene": {
                "required": ["location", "lighting", "atmosphere"],
                "optional": ["time_of_day", "weather", "season"]
            },
            "object": {
                "required": ["appearance", "size", "material"],
                "optional": ["color", "condition", "position"]
            }
        }
    
    async def ensure_consistency(
        self,
        prompt: str,
        story_id: str,
        element_type: str,
        element_id: str
    ) -> str:
        """Ensure prompt maintains consistency with established elements"""
        
        # Get element history
        history = await self._get_element_history(story_id, element_type, element_id)
        
        if not history:
            # First occurrence - establish baseline
            await self._establish_baseline(prompt, story_id, element_type, element_id)
            return prompt
        
        # Check consistency
        consistency_check = await self._check_consistency(prompt, history)
        
        if consistency_check["is_consistent"]:
            # Update history
            await self._update_history(prompt, story_id, element_type, element_id)
            return prompt
        
        # Fix inconsistencies
        fixed_prompt = await self._fix_inconsistencies(
            prompt,
            history,
            consistency_check["issues"]
        )
        
        # Update history with fixed version
        await self._update_history(fixed_prompt, story_id, element_type, element_id)
        
        return fixed_prompt
    
    async def _get_element_history(
        self,
        story_id: str,
        element_type: str,
        element_id: str
    ) -> List[Dict]:
        """Get historical descriptions of an element"""
        
        cache_key = f"consistency:{story_id}:{element_type}:{element_id}"
        cached = await self.cache.get(cache_key)
        
        if cached:
            return json.loads(cached)
        
        # Query from vector store
        history = await self.vector_store.search(
            collection=f"{story_id}_{element_type}",
            query=element_id,
            limit=10
        )
        
        return history
    
    async def _establish_baseline(
        self,
        prompt: str,
        story_id: str,
        element_type: str,
        element_id: str
    ):
        """Establish baseline description for new element"""
        
        # Extract features
        features = await self._extract_features(prompt, element_type)
        
        # Generate embedding
        embedding = await self.embedding_service.generate(prompt)
        
        # Store in vector database
        await self.vector_store.insert(
            collection=f"{story_id}_{element_type}",
            id=element_id,
            vector=embedding,
            metadata={
                "prompt": prompt,
                "features": features,
                "timestamp": "now()"
            }
        )
        
        # Cache
        cache_key = f"consistency:{story_id}:{element_type}:{element_id}"
        await self.cache.set(
            cache_key,
            json.dumps([{"prompt": prompt, "features": features}]),
            expire=7200
        )
    
    async def _check_consistency(
        self,
        prompt: str,
        history: List[Dict]
    ) -> Dict:
        """Check if prompt is consistent with history"""
        
        # Extract current features
        current_features = await self._extract_features_from_prompt(prompt)
        
        # Compare with historical features
        issues = []
        
        for historical in history:
            hist_features = historical.get("features", {})
            
            for key in hist_features:
                if key in current_features:
                    if not self._are_compatible(
                        current_features[key],
                        hist_features[key]
                    ):
                        issues.append({
                            "feature": key,
                            "current": current_features[key],
                            "expected": hist_features[key]
                        })
        
        return {
            "is_consistent": len(issues) == 0,
            "issues": issues
        }
    
    async def _fix_inconsistencies(
        self,
        prompt: str,
        history: List[Dict],
        issues: List[Dict]
    ) -> str:
        """Fix consistency issues in prompt"""
        
        # Build correction prompt
        corrections = []
        for issue in issues:
            corrections.append(
                f"- {issue['feature']}: should be '{issue['expected']}' not '{issue['current']}'"
            )
        
        system_prompt = """Fix the following consistency issues in the prompt.
        Maintain all other aspects while correcting only the specified issues."""
        
        user_prompt = f"""Original prompt: {prompt}
        
        Required corrections:
        {chr(10).join(corrections)}
        
        Historical context:
        {json.dumps(history[:3], indent=2)}
        
        Return the corrected prompt."""
        
        from services.ai.openai_service import OpenAIService
        ai_service = OpenAIService()
        
        fixed = await ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=user_prompt
        )
        
        return fixed
    
    async def _extract_features(
        self,
        prompt: str,
        element_type: str
    ) -> Dict:
        """Extract features from prompt based on element type"""
        
        rules = self.rules.get(element_type, {})
        required = rules.get("required", [])
        optional = rules.get("optional", [])
        
        system_prompt = f"""Extract the following features from the prompt:
        Required: {', '.join(required)}
        Optional: {', '.join(optional)}
        Return as JSON with feature names as keys."""
        
        from services.ai.openai_service import OpenAIService
        ai_service = OpenAIService()
        
        response = await ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=prompt,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return {}
    
    async def _extract_features_from_prompt(self, prompt: str) -> Dict:
        """Extract generic features from prompt"""
        
        system_prompt = """Extract visual features from this prompt.
        Include: colors, sizes, materials, styles, positions, etc.
        Return as JSON."""
        
        from services.ai.openai_service import OpenAIService
        ai_service = OpenAIService()
        
        response = await ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=prompt,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return {}
    
    def _are_compatible(self, value1: str, value2: str) -> bool:
        """Check if two feature values are compatible"""
        
        # Exact match
        if value1.lower() == value2.lower():
            return True
        
        # Semantic similarity check
        # Could use embeddings for more sophisticated comparison
        
        # For now, simple keyword overlap
        words1 = set(value1.lower().split())
        words2 = set(value2.lower().split())
        
        overlap = len(words1.intersection(words2))
        total = len(words1.union(words2))
        
        if total == 0:
            return True
        
        similarity = overlap / total
        return similarity > 0.5
    
    async def _update_history(
        self,
        prompt: str,
        story_id: str,
        element_type: str,
        element_id: str
    ):
        """Update element history with new prompt"""
        
        # Extract features
        features = await self._extract_features(prompt, element_type)
        
        # Generate embedding
        embedding = await self.embedding_service.generate(prompt)
        
        # Add to vector store
        await self.vector_store.insert(
            collection=f"{story_id}_{element_type}",
            id=f"{element_id}_{hash(prompt)}",
            vector=embedding,
            metadata={
                "prompt": prompt,
                "features": features,
                "element_id": element_id,
                "timestamp": "now()"
            }
        )
        
        # Update cache
        cache_key = f"consistency:{story_id}:{element_type}:{element_id}"
        history = await self._get_element_history(story_id, element_type, element_id)
        history.append({"prompt": prompt, "features": features})
        
        # Keep only last 10 entries
        if len(history) > 10:
            history = history[-10:]
        
        await self.cache.set(cache_key, json.dumps(history), expire=7200)
