# models/domain/__init__.py
"""
Domain models package.

РЕФАКТОРИНГ: Удалены User и Story модели.
- User: Identity теперь в Catalog.API
- Story: Книги хранятся в Catalog.API, PromptGen только генерирует промпты

Оставлены модели для консистентности визуализации:
- Character: персонажи книги
- Scene: локации/сцены
- StoryObject: значимые объекты
- PromptHistory: история генераций
- BookContext: runtime контекст книги (не SQLAlchemy модель)
"""

from models.domain.character import Character
from models.domain.scene import Scene
from models.domain.story_object import StoryObject
from models.domain.prompt_history import PromptHistory
from models.domain.book_context import BookContext, CharacterProfile, SceneContext

__all__ = [
    # SQLAlchemy models
    "Character",
    "Scene", 
    "StoryObject",
    "PromptHistory",
    
    # Dataclasses (runtime context)
    "BookContext",
    "CharacterProfile",
    "SceneContext",
]