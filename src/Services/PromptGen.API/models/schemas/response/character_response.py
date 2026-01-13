from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field
from datetime import datetime


class CharacterResponse(BaseModel):
    """Character response"""
    
    id: str
    name: str
    description: str
    appearance: Optional[str]
    personality: Optional[str]
    clothing: Optional[str]
    created_at: str
    updated_at: Optional[str]


class CharacterDetailResponse(CharacterResponse):
    """Detailed character response"""
    
    attributes: Optional[Dict[str, Any]]
    visual_references: Optional[List[str]]
    consistency_data: Optional[Dict]
    user_id: str
    story_id: Optional[str]


class CharacterPromptResponse(BaseModel):
    """Character prompt response"""
    
    character_id: str
    character_name: str
    prompt: str
    target_model: str
    elements: Dict[str, Optional[str]]


# models/schemas/response/scene_response.py
"""Scene response schemas"""
from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field
from datetime import datetime


class SceneResponse(BaseModel):
    """Scene response"""
    
    id: str
    name: str
    description: str
    location: Optional[str]
    time_of_day: Optional[str]
    weather: Optional[str]
    lighting: Optional[str]
    atmosphere: Optional[str]
    created_at: str
    updated_at: Optional[str]


class SceneDetailResponse(SceneResponse):
    """Detailed scene response"""
    
    objects: Optional[List[str]]
    visual_style: Optional[str]
    camera_angles: Optional[List[str]]
    user_id: str
    story_id: Optional[str]


class ScenePromptResponse(BaseModel):
    """Scene prompt response"""
    
    scene_id: str
    scene_name: str
    prompt: str
    target_model: str
    elements: Dict[str, Optional[Any]]


# models/schemas/response/story_response.py
"""Story response schemas"""
from typing import Optional, Dict, Any, List
from pydantic import BaseModel, Field
from datetime import datetime


class StoryResponse(BaseModel):
    """Story response"""
    
    id: str
    title: str
    description: str
    genre: Optional[str]
    style: Optional[str]
    created_at: str
    updated_at: Optional[str]


class StoryDetailResponse(StoryResponse):
    """Detailed story response"""
    
    characters: List[str]
    scenes: List[str]
    settings: Optional[Dict[str, Any]]
    total_pages: Optional[int]
    current_page: Optional[int]
    user_id: str


class PagePromptResponse(BaseModel):
    """Page prompt response"""
    
    story_id: str
    page_number: int
    prompts: List[Dict]
    context: Dict
    analysis: Dict


class StoryAnalysisResponse(BaseModel):
    """Story analysis response"""
    
    characters: List[Dict]
    scenes: List[Dict]
    objects: List[Dict]
    plot_points: List[str]
    narrative_style: Dict
    suggested_visuals: List[str]
    mood: Dict
    themes: List[str]
