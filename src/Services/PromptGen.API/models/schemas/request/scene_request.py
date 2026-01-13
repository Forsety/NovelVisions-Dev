from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field


class SceneCreateRequest(BaseModel):
    """Request to create a scene"""
    
    name: str = Field(..., min_length=1, max_length=100)
    description: str = Field(..., min_length=1, max_length=2000)
    location: Optional[str] = Field(None, max_length=200)
    time_of_day: Optional[str] = Field(None, max_length=50)
    weather: Optional[str] = Field(None, max_length=100)
    lighting: Optional[str] = Field(None, max_length=200)
    atmosphere: Optional[str] = Field(None, max_length=500)
    objects: Optional[List[str]] = Field(None)
    story_id: Optional[str] = Field(None)


class SceneUpdateRequest(BaseModel):
    """Request to update a scene"""
    
    name: Optional[str] = Field(None, min_length=1, max_length=100)
    description: Optional[str] = Field(None, min_length=1, max_length=2000)
    location: Optional[str] = Field(None, max_length=200)
    time_of_day: Optional[str] = Field(None, max_length=50)
    weather: Optional[str] = Field(None, max_length=100)
    lighting: Optional[str] = Field(None, max_length=200)
    atmosphere: Optional[str] = Field(None, max_length=500)
    objects: Optional[List[str]] = Field(None)


class ScenePromptRequest(BaseModel):
    """Request to generate scene prompt"""
    
    camera_angle: Optional[str] = Field(None, description="Camera angle")
    focus: Optional[str] = Field(None, description="Focus point")
    mood: Optional[str] = Field(None, description="Mood to convey")
    characters: Optional[List[str]] = Field(None, description="Characters in scene")
    target_model: str = Field(default="midjourney")
