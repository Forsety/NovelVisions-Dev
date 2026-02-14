# models/schemas/response/scene_response.py
"""
Response schemas для Scene endpoints.
"""

from typing import Optional, Dict, Any, List
from pydantic import BaseModel


class SceneResponse(BaseModel):
    """Базовый response для сцены."""
    
    id: str
    book_id: str
    name: str
    description: Optional[str] = None
    location_type: Optional[str] = None
    setting_type: Optional[str] = None
    atmosphere: Optional[str] = None
    is_established: bool = False
    generation_count: int = 0
    created_at: Optional[str] = None
    updated_at: Optional[str] = None
    
    class Config:
        from_attributes = True


class SceneDetailResponse(SceneResponse):
    """Детальный response для сцены."""
    
    architecture: Optional[str] = None
    materials: Optional[str] = None
    colors: List[str] = []
    textures: Optional[str] = None
    mood: Optional[str] = None
    default_lighting: Optional[str] = None
    light_sources: List[str] = []
    shadow_intensity: Optional[str] = None
    default_weather: Optional[str] = None
    time_period: Optional[str] = None
    typical_time_of_day: Optional[str] = None
    season: Optional[str] = None
    key_elements: List[str] = []
    decorations: Optional[str] = None
    furniture: Optional[str] = None
    vegetation: Optional[str] = None
    props: List[str] = []
    scale: Optional[str] = None
    camera_suggestions: List[str] = []
    aliases: List[str] = []
    first_appearance_page: Optional[int] = None
    first_appearance_chapter: Optional[int] = None
    appearance_count: int = 0
    importance: int = 5
    reference_image_url: Optional[str] = None
    base_prompt: Optional[str] = None
    attributes: Dict[str, Any] = {}


class ScenePromptResponse(BaseModel):
    """Response для сгенерированного промпта сцены."""
    
    scene_id: str
    scene_name: str
    book_id: str
    prompt: str
    negative_prompt: Optional[str] = None
    target_model: str
    style: Optional[str] = None
    elements: Dict[str, Optional[Any]] = {}
    consistency_applied: bool = False


class SceneListResponse(BaseModel):
    """Response для списка сцен (краткий)."""
    
    id: str
    book_id: str
    name: str
    location_type: Optional[str] = None
    setting_type: Optional[str] = None
    is_established: bool = False
    importance: int = 5
    appearance_count: int = 0