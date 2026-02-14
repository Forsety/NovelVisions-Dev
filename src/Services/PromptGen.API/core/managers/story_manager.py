import json
import uuid
from typing import Dict, List, Optional
from datetime import datetime
from sqlalchemy.ext.asyncio import AsyncSession

from models.domain.story import Story
from services.storage.cache_service import CacheService
from services.ai.openai_service import OpenAIService
from core.engines.context_analyzer import ContextAnalyzer
from core.engines.prompt_enhancer import PromptEnhancer
from core.engines.consistency_engine import ConsistencyEngine
from core.managers.character_manager import CharacterManager
from core.managers.scene_manager import SceneManager
from core.managers.object_manager import ObjectManager
from core.managers.memory_manager import MemoryManager


class StoryManager:
    """Manager for story operations"""
    
    def __init__(self, db: AsyncSession, cache: CacheService):
        self.db = db
        self.cache = cache
        self.ai_service = OpenAIService()
        self.context_analyzer = ContextAnalyzer(db, cache)
        self.prompt_enhancer = PromptEnhancer(db, cache)
        self.consistency_engine = ConsistencyEngine(db, cache)
        self.character_manager = CharacterManager(db, cache)
        self.scene_manager = SceneManager(db, cache)
        self.object_manager = ObjectManager(db, cache)
        self.memory_manager = MemoryManager(db, cache)
    
    async def create(
        self,
        title: str,
        description: str,
        user_id: str,
        **kwargs
    ) -> Dict:
        """Create a new story"""
        
        story_id = str(uuid.uuid4())
        
        story = Story(
            id=story_id,
            user_id=user_id,
            title=title,
            description=description,
            genre=kwargs.get("genre"),
            style=kwargs.get("style"),
            characters=kwargs.get("characters", []),
            scenes=kwargs.get("scenes", []),
            settings=kwargs.get("settings", {}),
            created_at=datetime.utcnow()
        )
        
        self.db.add(story)
        await self.db.commit()
        
        # Initialize memory context for the story
        await self.memory_manager.initialize_story(story_id)
        
        # Cache story
        await self._cache_story(story)
        
        return self._serialize_story(story)
    
    async def analyze_text(
        self,
        text: str,
        extract_all: bool = True
    ) -> Dict:
        """Analyze story text and extract elements"""
        
        # Use context analyzer
        analysis = await self.context_analyzer.analyze(
            text=text,
            extract_characters=extract_all,
            extract_scenes=extract_all,
            extract_objects=extract_all
        )
        
        # Additional story-specific analysis
        analysis["plot_points"] = await self._extract_plot_points(text)
        analysis["narrative_style"] = await self._detect_narrative_style(text)
        analysis["suggested_visuals"] = await self._suggest_visuals(text)
        
        return analysis
    
    async def generate_page_prompt(
        self,
        story_id: str,
        page_number: int,
        page_text: str,
        context: Optional[str] = None,
        maintain_consistency: bool = True,
        target_model: str = "midjourney"
    ) -> Dict:
        """Generate prompts for a story page"""
        
        # Get story
        from sqlalchemy import select
        result = await self.db.execute(
            select(Story).where(Story.id == story_id)
        )
        story = result.scalar_one_or_none()
        
        if not story:
            raise ValueError("Story not found")
        
        # Get memory context
        memory_context = await self.memory_manager.get_context(
            story_id,
            page_number
        )
        
        # Analyze page text
        page_analysis = await self.context_analyzer.analyze(page_text)
        
        # Identify key visual moments
        visual_moments = await self._identify_visual_moments(
            page_text,
            page_analysis
        )
        
        # Generate prompts for each moment
        prompts = []
        
        for moment in visual_moments:
            # Build base prompt
            base_prompt = await self._build_moment_prompt(
                moment,
                story,
                memory_context
            )
            
            # Ensure consistency if requested
            if maintain_consistency:
                # Check for recurring elements
                for char in moment.get("characters", []):
                    if char in memory_context.get("characters", {}):
                        base_prompt = await self.consistency_engine.ensure_consistency(
                            base_prompt,
                            story_id,
                            "character",
                            char
                        )
                
                for scene in moment.get("scenes", []):
                    if scene in memory_context.get("scenes", {}):
                        base_prompt = await self.consistency_engine.ensure_consistency(
                            base_prompt,
                            story_id,
                            "scene",
                            scene
                        )
            
            # Enhance prompt
            enhanced = await self.prompt_enhancer.enhance(
                text=base_prompt,
                model=target_model,
                style=story.style,
                parameters={"page": page_number}
            )
            
            prompts.append({
                "moment": moment["description"],
                "prompt": enhanced["enhanced"],
                "type": moment["type"],
                "importance": moment["importance"]
            })
        
        # Update memory with new elements
        await self.memory_manager.update_from_page(
            story_id,
            page_number,
            page_analysis
        )
        
        return {
            "story_id": story_id,
            "page_number": page_number,
            "prompts": prompts,
            "context": memory_context,
            "analysis": page_analysis
        }
    
    async def _identify_visual_moments(
        self,
        text: str,
        analysis: Dict
    ) -> List[Dict]:
        """Identify key visual moments in text"""
        
        system_prompt = """Identify the most visually impactful moments in this text.
        For each moment, provide:
        - description: what's happening
        - type: action, emotion, establishing, reveal, etc.
        - importance: high, medium, low
        - characters: list of involved characters
        - scenes: list of involved locations
        Return as JSON array, maximum 3 moments."""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=f"Text: {text}\n\nAnalysis: {json.dumps(analysis)}",
            response_format="json"
        )
        
        try:
            moments = json.loads(response)
            return moments[:3]  # Limit to 3 moments per page
        except:
            return [{
                "description": "General scene from the text",
                "type": "establishing",
                "importance": "medium",
                "characters": analysis.get("characters", []),
                "scenes": analysis.get("scenes", [])
            }]
    
    async def _build_moment_prompt(
        self,
        moment: Dict,
        story: Story,
        memory_context: Dict
    ) -> str:
        """Build prompt for a visual moment"""
        
        parts = []
        
        # Add scene description
        parts.append(moment["description"])
        
        # Add character descriptions from memory
        for char_name in moment.get("characters", []):
            if char_name in memory_context.get("characters", {}):
                char_desc = memory_context["characters"][char_name]
                parts.append(char_desc["appearance"])
        
        # Add scene details from memory
        for scene_name in moment.get("scenes", []):
            if scene_name in memory_context.get("scenes", {}):
                scene_desc = memory_context["scenes"][scene_name]
                parts.append(scene_desc["description"])
        
        # Add story style
        if story.style:
            parts.append(f"{story.style} style")
        
        # Add moment type specific elements
        if moment["type"] == "action":
            parts.append("dynamic motion, action shot")
        elif moment["type"] == "emotion":
            parts.append("emotional focus, character expression")
        elif moment["type"] == "establishing":
            parts.append("wide shot, environmental detail")
        elif moment["type"] == "reveal":
            parts.append("dramatic reveal, focal point")
        
        return ", ".join(parts)
    
    async def _extract_plot_points(self, text: str) -> List[str]:
        """Extract main plot points from text"""
        
        system_prompt = """Extract the main plot points from this text.
        Return as JSON array of strings, each describing a key event."""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return []
    
    async def _detect_narrative_style(self, text: str) -> Dict:
        """Detect narrative style of text"""
        
        system_prompt = """Analyze the narrative style of this text.
        Return JSON with: perspective (first/third), tense (past/present), 
        tone (dark/light/neutral), pace (fast/slow/moderate)."""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return {
                "perspective": "third",
                "tense": "past",
                "tone": "neutral",
                "pace": "moderate"
            }
    
    async def _suggest_visuals(self, text: str) -> List[str]:
        """Suggest visual styles for text"""
        
        system_prompt = """Based on this text, suggest 3 visual art styles that would complement it.
        Return as JSON array of style names."""
        
        response = await self.ai_service.generate(
            system_prompt=system_prompt,
            user_prompt=text,
            response_format="json"
        )
        
        try:
            return json.loads(response)
        except:
            return ["realistic", "illustrated", "painterly"]
    
    async def _cache_story(self, story: Story):
        """Cache story data"""
        
        cache_key = f"story:{story.id}"
        data = self._serialize_story(story)
        await self.cache.set(cache_key, json.dumps(data), expire=3600)
    
    def _serialize_story(self, story: Story) -> Dict:
        """Serialize story to dict"""
        
        return {
            "id": story.id,
            "user_id": story.user_id,
            "title": story.title,
            "description": story.description,
            "genre": story.genre,
            "style": story.style,
            "characters": story.characters,
            "scenes": story.scenes,
            "settings": story.settings,
            "created_at": story.created_at.isoformat(),
            "updated_at": story.updated_at.isoformat() if story.updated_at else None
        }
