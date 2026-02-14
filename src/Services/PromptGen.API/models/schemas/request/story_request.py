from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field


class StoryCreateRequest(BaseModel):
    """Request to create a story"""
    
    title: str = Field(..., min_length=1, max_length=200)
    description: str = Field(..., min_length=1, max_length=2000)
    genre: Optional[str] = Field(None, max_length=50)
    style: Optional[str] = Field(None, max_length=50)
    characters: Optional[List[str]] = Field(None)
    scenes: Optional[List[str]] = Field(None)
    settings: Optional[Dict[str, Any]] = Field(None)


class StoryUpdateRequest(BaseModel):
    """Request to update a story"""
    
    title: Optional[str] = Field(None, min_length=1, max_length=200)
    description: Optional[str] = Field(None, min_length=1, max_length=2000)
    genre: Optional[str] = Field(None, max_length=50)
    style: Optional[str] = Field(None, max_length=50)
    settings: Optional[Dict[str, Any]] = Field(None)


class PagePromptRequest(BaseModel):
    """Request to generate page prompts"""
    
    page_text: str = Field(..., min_length=1, max_length=5000)
    context: Optional[str] = Field(None, description="Additional context")
    maintain_consistency: bool = Field(default=True)
    target_model: str = Field(default="midjourney")


class StoryAnalyzeRequest(BaseModel):
    """Request to analyze story text"""
    
    text: str = Field(..., min_length=1, max_length=10000)
    extract_all: bool = Field(default=True)
