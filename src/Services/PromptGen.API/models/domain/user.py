from datetime import datetime
from typing import Optional, Dict, Any
from sqlalchemy import Column, String, DateTime, JSON, Boolean
from sqlalchemy.orm import relationship
from models.database.base import Base


class User(Base):
    """User model"""
    
    __tablename__ = "users"
    
    id = Column(String(36), primary_key=True)
    email = Column(String(255), unique=True, nullable=False)
    username = Column(String(100), unique=True, nullable=False)
    
    preferences = Column(JSON, nullable=True)
    api_keys = Column(JSON, nullable=True)  # Encrypted
    
    is_active = Column(Boolean, default=True)
    is_premium = Column(Boolean, default=False)
    
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, onupdate=datetime.utcnow)
    last_login = Column(DateTime, nullable=True)
    
    # Relationships
    prompts = relationship("Prompt", back_populates="user")
    characters = relationship("Character", back_populates="user")
    scenes = relationship("Scene", back_populates="user")
    objects = relationship("Object", back_populates="user")
    stories = relationship("Story", back_populates="user")
    styles = relationship("Style", back_populates="user")
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary"""
        return {
            "id": self.id,
            "email": self.email,
            "username": self.username,
            "preferences": self.preferences,
            "is_active": self.is_active,
            "is_premium": self.is_premium,
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None,
            "last_login": self.last_login.isoformat() if self.last_login else None
        }
