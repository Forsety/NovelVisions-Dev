from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field, validator


class PromptEnhanceRequest(BaseModel):
    """Request for prompt enhancement"""
    
    text: str = Field(..., min_length=1, max_length=2000, description="Text to enhance")
    target_model: str = Field(default="midjourney", description="Target AI model")
    style: Optional[str] = Field(None, description="Art style to apply")
    parameters: Optional[Dict[str, Any]] = Field(None, description="Model-specific parameters")
    
    @validator("target_model")
    def validate_model(cls, v):
        allowed = ["midjourney", "dalle3", "stable-diffusion", "flux"]
        if v not in allowed:
            raise ValueError(f"Model must be one of {allowed}")
        return v


class PromptAnalyzeRequest(BaseModel):
    """Request for prompt analysis"""
    
    text: str = Field(..., min_length=1, max_length=5000)
    extract_characters: bool = Field(default=True)
    extract_scenes: bool = Field(default=True)
    extract_objects: bool = Field(default=True)


class PromptBatchRequest(BaseModel):
    """Request for batch prompt enhancement"""
    
    prompts: List[PromptEnhanceRequest] = Field(..., min_items=1, max_items=10)


# models/schemas/request/character_request.py
"""Character request schemas"""
from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field


class CharacterCreateRequest(BaseModel):
    """Request to create a character"""
    
    name: str = Field(..., min_length=1, max_length=100)
    description: str = Field(..., min_length=1, max_length=2000)
    appearance: Optional[str] = Field(None, max_length=1000)
    personality: Optional[str] = Field(None, max_length=1000)
    clothing: Optional[str] = Field(None, max_length=500)
    attributes: Optional[Dict[str, Any]] = Field(None)
    story_id: Optional[str] = Field(None)


class CharacterUpdateRequest(BaseModel):
    """Request to update a character"""
    
    name: Optional[str] = Field(None, min_length=1, max_length=100)
    description: Optional[str] = Field(None, min_length=1, max_length=2000)
    appearance: Optional[str] = Field(None, max_length=1000)
    personality: Optional[str] = Field(None, max_length=1000)
    clothing: Optional[str] = Field(None, max_length=500)
    attributes: Optional[Dict[str, Any]] = Field(None)


class CharacterPromptRequest(BaseModel):
    """Request to generate character prompt"""
    
    action: Optional[str] = Field(None, description="Action the character is performing")
    emotion: Optional[str] = Field(None, description="Emotion to express")
    pose: Optional[str] = Field(None, description="Pose or position")
    scene_context: Optional[str] = Field(None, description="Scene context")
    target_model: str = Field(default="midjourney")
