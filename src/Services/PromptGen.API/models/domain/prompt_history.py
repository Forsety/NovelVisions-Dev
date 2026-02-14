# models/domain/prompt_history.py
"""
Domain model для PromptHistory (История промптов).
РЕФАКТОРИНГ: story_id → book_id (внешний ID из Catalog.API, НЕ FK)
"""

from datetime import datetime
from typing import Optional, Dict, Any
from sqlalchemy import Column, String, Text, DateTime, JSON, Integer, Float, Index
import uuid

from models.database.base import Base


class PromptHistory(Base):
    """
    Модель истории промптов.
    
    PromptHistory сохраняет все сгенерированные промпты для:
    - Аналитики и улучшения качества
    - Возможности повторить генерацию
    - Отслеживания эволюции стиля
    - Кэширования (не генерировать повторно тот же текст)
    
    Привязан к book_id и page_id - внешним ID из Catalog.API.
    """
    
    __tablename__ = "prompt_history"
    
    # ===========================================
    # PRIMARY KEYS & EXTERNAL REFERENCES
    # ===========================================
    
    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    
    # Внешние ID из Catalog.API (НЕ FK!)
    book_id = Column(String(36), nullable=False, index=True)
    page_id = Column(String(36), nullable=True, index=True)  # ID страницы
    chapter_id = Column(String(36), nullable=True, index=True)  # ID главы
    
    # Номера для удобства
    page_number = Column(Integer, nullable=True, index=True)
    chapter_number = Column(Integer, nullable=True)
    
    # ===========================================
    # ORIGINAL TEXT
    # ===========================================
    
    original_text = Column(Text, nullable=False)  # Исходный текст из книги
    text_hash = Column(String(64), nullable=True, index=True)  # SHA256 для дедупликации
    context_before = Column(Text, nullable=True)  # Контекст до
    context_after = Column(Text, nullable=True)  # Контекст после
    
    # ===========================================
    # GENERATED PROMPTS
    # ===========================================
    
    enhanced_prompt = Column(Text, nullable=False)  # Улучшенный промпт
    negative_prompt = Column(Text, nullable=True)  # Негативный промпт
    
    # ===========================================
    # GENERATION PARAMETERS
    # ===========================================
    
    target_model = Column(String(50), nullable=False)  # midjourney, dalle3, sd, flux
    style = Column(String(100), nullable=True)  # fantasy, realistic, manga, etc.
    parameters = Column(JSON, default=dict)  # Все параметры генерации
    # Пример parameters:
    # {
    #     "aspect_ratio": "16:9",
    #     "quality": "high",
    #     "seed": 12345,
    #     "guidance_scale": 7.5
    # }
    
    # ===========================================
    # ANALYSIS RESULTS
    # ===========================================
    
    scene_type = Column(String(50), nullable=True)  # action, dialogue, establishing, etc.
    detected_characters = Column(JSON, default=list)  # ["Harry", "Hermione"]
    detected_objects = Column(JSON, default=list)  # ["wand", "book"]
    detected_location = Column(String(200), nullable=True)  # "Hogwarts Great Hall"
    mood = Column(String(100), nullable=True)  # "tense", "joyful", "mysterious"
    
    # ===========================================
    # QUALITY METRICS
    # ===========================================
    
    quality_score = Column(Integer, nullable=True)  # 0-100 автоматическая оценка
    consistency_score = Column(Float, nullable=True)  # 0-1 соответствие персонажам
    complexity_score = Column(Float, nullable=True)  # 0-1 сложность сцены
    
    # ===========================================
    # GENERATION RESULT
    # ===========================================
    
    generated_image_url = Column(String(500), nullable=True)  # URL результата
    thumbnail_url = Column(String(500), nullable=True)  # URL превью
    generation_status = Column(String(50), default="created")  # created, processing, completed, failed
    generation_error = Column(Text, nullable=True)  # Текст ошибки
    generation_duration_ms = Column(Integer, nullable=True)  # Время генерации в ms
    
    # ===========================================
    # USER FEEDBACK
    # ===========================================
    
    user_rating = Column(Integer, nullable=True)  # 1-5 звёзд
    user_feedback = Column(Text, nullable=True)  # Текстовый отзыв
    was_regenerated = Column(Integer, default=0)  # Сколько раз перегенерировали
    is_selected = Column(String(1), default='0')  # '1' если выбран пользователем
    
    # ===========================================
    # TIMESTAMPS
    # ===========================================
    
    created_at = Column(DateTime, default=datetime.utcnow)
    generated_at = Column(DateTime, nullable=True)  # Когда изображение было создано
    
    # ===========================================
    # INDEXES
    # ===========================================
    
    __table_args__ = (
        Index('ix_prompt_history_book_page', 'book_id', 'page_id'),
        Index('ix_prompt_history_status', 'generation_status'),
        Index('ix_prompt_history_text_hash', 'text_hash'),
    )
    
    # ===========================================
    # METHODS
    # ===========================================
    
    def __repr__(self):
        return f"<PromptHistory(id={self.id}, book_id={self.book_id}, page={self.page_number})>"
    
    def to_dict(self) -> Dict[str, Any]:
        """Конвертация в словарь"""
        return {
            "id": self.id,
            "book_id": self.book_id,
            "page_id": self.page_id,
            "chapter_id": self.chapter_id,
            "page_number": self.page_number,
            "chapter_number": self.chapter_number,
            "original_text": self.original_text,
            "enhanced_prompt": self.enhanced_prompt,
            "negative_prompt": self.negative_prompt,
            "target_model": self.target_model,
            "style": self.style,
            "parameters": self.parameters or {},
            "scene_type": self.scene_type,
            "detected_characters": self.detected_characters or [],
            "detected_objects": self.detected_objects or [],
            "detected_location": self.detected_location,
            "mood": self.mood,
            "quality_score": self.quality_score,
            "consistency_score": self.consistency_score,
            "generated_image_url": self.generated_image_url,
            "thumbnail_url": self.thumbnail_url,
            "generation_status": self.generation_status,
            "generation_error": self.generation_error,
            "generation_duration_ms": self.generation_duration_ms,
            "user_rating": self.user_rating,
            "user_feedback": self.user_feedback,
            "was_regenerated": self.was_regenerated,
            "is_selected": self.is_selected == '1',
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "generated_at": self.generated_at.isoformat() if self.generated_at else None
        }
    
    def mark_completed(self, image_url: str, thumbnail_url: str = None, duration_ms: int = None):
        """Помечает генерацию как завершённую"""
        self.generation_status = "completed"
        self.generated_image_url = image_url
        self.thumbnail_url = thumbnail_url
        self.generation_duration_ms = duration_ms
        self.generated_at = datetime.utcnow()
    
    def mark_failed(self, error: str):
        """Помечает генерацию как неудачную"""
        self.generation_status = "failed"
        self.generation_error = error
        self.generated_at = datetime.utcnow()
    
    def increment_regeneration(self):
        """Увеличивает счётчик перегенераций"""
        self.was_regenerated = (self.was_regenerated or 0) + 1
    
    def set_user_feedback(self, rating: int, feedback: str = None):
        """Устанавливает обратную связь от пользователя"""
        if 1 <= rating <= 5:
            self.user_rating = rating
        self.user_feedback = feedback
    
    @staticmethod
    def compute_text_hash(text: str) -> str:
        """Вычисляет хэш текста для дедупликации"""
        import hashlib
        normalized = text.strip().lower()
        return hashlib.sha256(normalized.encode()).hexdigest()