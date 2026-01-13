from datetime import datetime
from typing import Optional, Dict, Any, List
from sqlalchemy import Column, String, Text, JSON, DateTime, ForeignKey, Boolean
from sqlalchemy.orm import relationship
from models.database.base import Base


class Style(Base):
    """Style model"""
    
    __tablename__ = "styles"
    
    id = Column(String(36), primary_key=True)
    user_id = Column(String(36), ForeignKey("users.id"), nullable=True)
    
    name = Column(String(100), nullable=False)
    description = Column(Text, nullable=False)
    category = Column(String(50), nullable=True)
    
    keywords = Column(JSON, nullable=False)
    lighting = Column(Text, nullable=True)
    camera = Column(Text, nullable=True)
    post_processing = Column(Text, nullable=True)
    
    examples = Column(JSON, nullable=True)
    tips = Column(JSON, nullable=True)
    
    is_preset = Column(Boolean, default=False)
    is_public = Column(Boolean, default=False)
    
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, onupdate=datetime.utcnow)
    
    # Relationships
    user = relationship("User", back_populates="styles")
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary"""
        return {
            "id": self.id,
            "user_id": self.user_id,
            "name": self.name,
            "description": self.description,
            "category": self.category,
            "keywords": self.keywords,
            "lighting": self.lighting,
            "camera": self.camera,
            "post_processing": self.post_processing,
            "examples": self.examples,
            "tips": self.tips,
            "is_preset": self.is_preset,
            "is_public": self.is_public,
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None
        }
