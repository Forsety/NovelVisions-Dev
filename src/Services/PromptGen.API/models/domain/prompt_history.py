# models/domain/prompt_history.py
"""
Domain model для PromptHistory (История промптов).
"""

from datetime import datetime
from typing import Optional, Dict, Any
from sqlalchemy import Column, String, Text, DateTime, JSON, ForeignKey, Integer, Float
from sqlalchemy.orm import relationship
import uuid

from models.database.base import Base


class PromptHistory(Base):
    """
    Модель истории промптов.
    
    PromptHistory сохраняет все сгенерированные промпты для:
    - Аналитики и улучшения качества
    - Возможности повторить генерацию
    - Отслеживания эволюции стиля
    """
    
    __tablename__ = "prompt_history"
    
    # Первичные ключи
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    story_id = Column(String(36), ForeignKey("stories.id"), nullable=True, index=True)
    user_id = Column(String(36), nullable=False, index=True)
    
    # Привязка к странице/главе
    page_number = Column(Integer, nullable=True, index=True)
    chapter_number = Column(Integer, nullable=True)
    
    # Промпты
    original_text = Column(Text, nullable=False)  # Исходный текст из книги
    enhanced_prompt = Column(Text, nullable=False)  # Улучшенный промпт
    negative_prompt = Column(Text, nullable=True)
    
    # Параметры генерации
    target_model = Column(String(50), nullable=False)  # midjourney, dalle3, sd, flux
    style = Column(String(100), nullable=True)
    parameters = Column(JSON, default=dict)  # Все параметры генерации
    
    # Результат анализа
    scene_type = Column(String(50), nullable=True)
    detected_characters = Column(JSON, default=list)
    detected_objects = Column(JSON, default=list)
    detected_location = Column(String(200), nullable=True)
    
    # Качество и метрики
    quality_score = Column(Integer, nullable=True)  # 0-100
    consistency_score = Column(Float, nullable=True)  # 0-1
    
    # Результат генерации изображения
    generated_image_url = Column(String(500), nullable=True)
    generation_status = Column(String(50), default="created")  # created, processing, completed, failed
    generation_error = Column(Text, nullable=True)
    
    # Feedback
    user_rating = Column(Integer, nullable=True)  # 1-5
    user_feedback = Column(Text, nullable=True)
    was_regenerated = Column(Integer, default=0)  # Сколько раз перегенерировали
    
    # Временные метки
    created_at = Column(DateTime, default=datetime.utcnow)
    generated_at = Column(DateTime, nullable=True)
    
    # Связи
    story = relationship("Story", back_populates="prompt_history")
    
    def __repr__(self):
        return f"<PromptHistory(id={self.id}, page={self.page_number})>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "story_id": self.story_id,
            "page_number": self.page_number,
            "chapter_number": self.chapter_number,
            "original_text": self.original_text[:200] + "..." if len(self.original_text) > 200 else self.original_text,
            "enhanced_prompt": self.enhanced_prompt,
            "negative_prompt": self.negative_prompt,
            "target_model": self.target_model,
            "style": self.style,
            "parameters": self.parameters,
            "scene_type": self.scene_type,
            "detected_characters": self.detected_characters,
            "quality_score": self.quality_score,
            "generated_image_url": self.generated_image_url,
            "generation_status": self.generation_status,
            "user_rating": self.user_rating,
            "created_at": self.created_at.isoformat() if self.created_at else None
        }
    
    def mark_completed(self, image_url: str):
        """Помечает как завершённый"""
        self.generated_image_url = image_url
        self.generation_status = "completed"
        self.generated_at = datetime.utcnow()
    
    def mark_failed(self, error: str):
        """Помечает как неудачный"""
        self.generation_status = "failed"
        self.generation_error = error