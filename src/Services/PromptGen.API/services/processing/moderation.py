from typing import Dict, List, Tuple,Any
import re


class ModerationService:
    """Service for content moderation"""
    
    def __init__(self):
        # Simple blocked terms for demonstration
        self.blocked_terms = [
            # Add actual blocked terms here
        ]
        
        self.sensitive_categories = [
            "violence",
            "adult",
            "illegal",
            "harmful"
        ]
    
    async def check_content(self, text: str) -> Dict[str, Any]:
        """Check content for policy violations"""
        
        result = {
            "safe": True,
            "categories": {},
            "warnings": []
        }
        
        # Check for blocked terms
        text_lower = text.lower()
        for term in self.blocked_terms:
            if term in text_lower:
                result["safe"] = False
                result["warnings"].append(f"Blocked term detected: {term}")
        
        # Check categories (placeholder for ML-based moderation)
        result["categories"] = {
            "violence": self._check_violence(text),
            "adult": self._check_adult_content(text),
            "illegal": self._check_illegal_content(text),
            "harmful": self._check_harmful_content(text)
        }
        
        # Mark as unsafe if any category is flagged
        if any(result["categories"].values()):
            result["safe"] = False
        
        return result
    
    def _check_violence(self, text: str) -> bool:
        """Check for violent content"""
        
        violence_keywords = ["kill", "murder", "torture", "assault"]
        text_lower = text.lower()
        
        for keyword in violence_keywords:
            if keyword in text_lower:
                # Context matters - check surrounding words
                # This is simplified; real implementation would be more sophisticated
                return True
        
        return False
    
    def _check_adult_content(self, text: str) -> bool:
        """Check for adult content"""
        
        # Placeholder - implement actual detection
        return False
    
    def _check_illegal_content(self, text: str) -> bool:
        """Check for illegal content"""
        
        # Placeholder - implement actual detection
        return False
    
    def _check_harmful_content(self, text: str) -> bool:
        """Check for harmful content"""
        
        # Placeholder - implement actual detection
        return False
    
    def sanitize_prompt(self, prompt: str) -> str:
        """Sanitize prompt by removing problematic content"""
        
        # Remove any detected blocked terms
        for term in self.blocked_terms:
            prompt = prompt.replace(term, "[removed]")
        
        return prompt
