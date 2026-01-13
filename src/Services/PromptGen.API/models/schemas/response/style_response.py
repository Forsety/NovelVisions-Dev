"Style response schemas"""
from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field


class StyleResponse(BaseModel):
    """Style response"""
    
    id: str
    name: str
    description: str
    keywords: List[str]
    lighting: Optional[str]
    camera: Optional[str]
    post_processing: Optional[str]
    examples: Optional[List[str]]
    tips: Optional[List[str]]


class StylePresetResponse(BaseModel):
    """Style preset response"""
    
    id: str
    name: str
    description: str
    category: str
    preview: Optional[str]
    keywords: List[str]
