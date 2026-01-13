import numpy as np
from typing import List, Optional
from app.config import settings


class EmbeddingService:
    """Service for generating embeddings"""
    
    def __init__(self):
        self.provider = settings.DEFAULT_AI_PROVIDER
        self.openai_service = None
        self.local_service = None
        
        if settings.OPENAI_API_KEY:
            from services.ai.openai_service import OpenAIService
            self.openai_service = OpenAIService()
    
    async def generate(self, text: str) -> List[float]:
        """Generate embedding for text"""
        
        if self.provider == "openai" and self.openai_service:
            return await self.openai_service.generate_embedding(text)
        else:
            # Fallback to simple hash-based embedding
            return self._generate_simple_embedding(text)
    
    def _generate_simple_embedding(self, text: str, dim: int = 384) -> List[float]:
        """Generate simple deterministic embedding"""
        
        # Simple hash-based embedding for testing
        import hashlib
        
        # Create multiple hashes for different dimensions
        embeddings = []
        for i in range(dim):
            hash_obj = hashlib.sha256(f"{text}:{i}".encode())
            hash_hex = hash_obj.hexdigest()
            # Convert hex to float between -1 and 1
            value = (int(hash_hex[:8], 16) / 0xFFFFFFFF) * 2 - 1
            embeddings.append(value)
        
        # Normalize
        norm = np.linalg.norm(embeddings)
        if norm > 0:
            embeddings = (np.array(embeddings) / norm).tolist()
        
        return embeddings
    
    def cosine_similarity(self, vec1: List[float], vec2: List[float]) -> float:
        """Calculate cosine similarity between two vectors"""
        
        vec1 = np.array(vec1)
        vec2 = np.array(vec2)
        
        dot_product = np.dot(vec1, vec2)
        norm1 = np.linalg.norm(vec1)
        norm2 = np.linalg.norm(vec2)
        
        if norm1 == 0 or norm2 == 0:
            return 0
        
        return dot_product / (norm1 * norm2)
