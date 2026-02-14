from datetime import datetime
from typing import Optional, Dict, Any
from sqlalchemy import Column, String, Text, JSON, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from models.database.base import Base


class Prompt(Base):
    """Prompt model"""
    
    __tablename__ = "prompts"
    
    id = Column(String(36), primary_key=True)
    user_id = Column(String(36), ForeignKey("users.id"), nullable=False)
    story_id = Column(String(36), ForeignKey("stories.id"), nullable=True)
    
    original = Column(Text, nullable=False)
    enhanced = Column(Text, nullable=False)
    model = Column(String(50), nullable=False)
    style = Column(String(50), nullable=True)
    parameters = Column(JSON, nullable=True)
    
    entities = Column(JSON, nullable=True)
    metadata = Column(JSON, nullable=True)
    
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, onupdate=datetime.utcnow)
    
    # Relationships
    user = relationship("User", back_populates="prompts")
    story = relationship("Story", back_populates="prompts")
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary"""
        return {
            "id": self.id,
            "user_id": self.user_id,
            "story_id": self.story_id,
            "original": self.original,
            "enhanced": self.enhanced,
            "model": self.model,
            "style": self.style,
            "parameters": self.parameters,
            "entities": self.entities,
            "metadata": self.metadata,
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None
        }
