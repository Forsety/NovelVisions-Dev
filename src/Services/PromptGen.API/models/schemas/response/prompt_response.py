from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field
from datetime import datetime


class PromptEnhanceResponse(BaseModel):
    """Response for prompt enhancement"""
    
    original: str
    enhanced: str
    model: str
    style: Optional[str]
    entities: Optional[Dict[str, List]]
    parameters: Optional[Dict[str, Any]]


class PromptAnalysisResponse(BaseModel):
    """Response for prompt analysis"""
    
    text_length: int
    word_count: int
    characters: Optional[List[Dict]]
    scenes: Optional[List[Dict]]
    objects: Optional[List[Dict]]
    mood: Optional[Dict]
    themes: Optional[List[str]]


class PromptSuggestion(BaseModel):
    """Prompt improvement suggestion"""
    
    improvement: str
    reason: str
    example: str
