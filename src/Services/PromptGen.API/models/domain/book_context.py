# models/domain/book_context.py
"""
Runtime контекст книги для генерации промптов.

Это НЕ SQLAlchemy модель, а dataclass для хранения контекста в памяти/кэше.
Загружается из БД при первом запросе и кэшируется в Redis.
"""

from dataclasses import dataclass, field
from typing import Optional, Dict, List, Any
from datetime import datetime


@dataclass
class CharacterProfile:
    """
    Профиль персонажа для консистентности.
    Используется при генерации промптов для поддержания одинаковой внешности.
    """
    
    name: str
    book_id: str
    
    # Физические характеристики
    gender: Optional[str] = None
    age: Optional[str] = None
    height: Optional[str] = None
    build: Optional[str] = None
    
    # Внешность
    appearance: str = ""  # Полное описание
    hair: Optional[str] = None
    eyes: Optional[str] = None
    skin: Optional[str] = None
    facial_features: Optional[str] = None
    
    # Одежда
    clothing: Optional[str] = None
    accessories: Optional[str] = None
    distinguishing_features: Optional[str] = None
    
    # Консистентность
    base_prompt: Optional[str] = None  # Зафиксированный промпт
    generation_count: int = 0
    is_established: bool = False  # True если описание зафиксировано автором
    
    def to_prompt_fragment(self) -> str:
        """
        Генерирует фрагмент промпта для персонажа.
        Используется для вставки в общий промпт сцены.
        """
        # Если есть base_prompt - используем его
        if self.base_prompt:
            return self.base_prompt
        
        parts = []
        
        # Имя
        if self.name:
            parts.append(self.name)
        
        # Возраст и пол
        if self.age:
            parts.append(self.age)
        if self.gender:
            parts.append(self.gender)
        
        # Телосложение
        if self.build:
            parts.append(f"{self.build} build")
        
        # Волосы и глаза
        if self.hair:
            parts.append(f"{self.hair} hair")
        if self.eyes:
            parts.append(f"{self.eyes} eyes")
        if self.skin:
            parts.append(f"{self.skin} skin")
        
        # Особенности
        if self.distinguishing_features:
            parts.append(self.distinguishing_features)
        
        # Одежда
        if self.clothing:
            parts.append(f"wearing {self.clothing}")
        
        # Полное описание имеет приоритет
        if self.appearance and len(self.appearance) > 20:
            return self.appearance
        
        return ", ".join(parts) if parts else self.name
    
    def to_dict(self) -> Dict[str, Any]:
        """Сериализация для кэша."""
        return {
            "name": self.name,
            "book_id": self.book_id,
            "gender": self.gender,
            "age": self.age,
            "height": self.height,
            "build": self.build,
            "appearance": self.appearance,
            "hair": self.hair,
            "eyes": self.eyes,
            "skin": self.skin,
            "facial_features": self.facial_features,
            "clothing": self.clothing,
            "accessories": self.accessories,
            "distinguishing_features": self.distinguishing_features,
            "base_prompt": self.base_prompt,
            "generation_count": self.generation_count,
            "is_established": self.is_established
        }
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'CharacterProfile':
        """Десериализация из кэша."""
        return cls(**data)


@dataclass
class SceneContext:
    """
    Контекст сцены/локации для консистентности.
    """
    
    name: str
    book_id: str
    
    # Описание
    description: str = ""
    location_type: Optional[str] = None  # interior, exterior
    setting_type: Optional[str] = None  # castle, forest, city
    
    # Атмосфера
    atmosphere: Optional[str] = None
    mood: Optional[str] = None
    
    # Освещение и время
    lighting: Optional[str] = None
    time_of_day: Optional[str] = None
    weather: Optional[str] = None
    
    # Детали
    key_elements: List[str] = field(default_factory=list)
    
    # Консистентность
    base_prompt: Optional[str] = None
    is_established: bool = False
    
    def to_prompt_fragment(self) -> str:
        """Генерирует фрагмент промпта для сцены."""
        if self.base_prompt:
            return self.base_prompt
        
        parts = []
        
        if self.setting_type:
            parts.append(self.setting_type)
        
        if self.name:
            parts.append(self.name)
        
        if self.atmosphere:
            parts.append(f"{self.atmosphere} atmosphere")
        
        if self.lighting:
            parts.append(self.lighting)
        
        if self.time_of_day:
            parts.append(self.time_of_day)
        
        if self.weather:
            parts.append(f"{self.weather} weather")
        
        if self.key_elements:
            elements = ", ".join(self.key_elements[:3])
            parts.append(f"with {elements}")
        
        if self.description and len(self.description) > 50:
            return self.description
        
        return ", ".join(parts) if parts else self.name
    
    def to_dict(self) -> Dict[str, Any]:
        return {
            "name": self.name,
            "book_id": self.book_id,
            "description": self.description,
            "location_type": self.location_type,
            "setting_type": self.setting_type,
            "atmosphere": self.atmosphere,
            "mood": self.mood,
            "lighting": self.lighting,
            "time_of_day": self.time_of_day,
            "weather": self.weather,
            "key_elements": self.key_elements,
            "base_prompt": self.base_prompt,
            "is_established": self.is_established
        }
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'SceneContext':
        return cls(**data)


@dataclass
class ObjectContext:
    """Контекст объекта для консистентности."""
    
    name: str
    book_id: str
    
    appearance: str = ""
    materials: Optional[str] = None
    colors: List[str] = field(default_factory=list)
    size: Optional[str] = None
    details: Optional[str] = None
    effects: Optional[str] = None
    
    base_prompt: Optional[str] = None
    is_established: bool = False
    
    def to_prompt_fragment(self) -> str:
        if self.base_prompt:
            return self.base_prompt
        
        parts = [self.name]
        
        if self.materials:
            parts.append(f"made of {self.materials}")
        if self.colors:
            parts.append(" and ".join(self.colors[:2]))
        if self.size:
            parts.append(self.size)
        if self.details:
            parts.append(self.details)
        if self.effects:
            parts.append(self.effects)
        
        if self.appearance:
            return self.appearance
        
        return ", ".join(parts)
    
    def to_dict(self) -> Dict[str, Any]:
        return {
            "name": self.name,
            "book_id": self.book_id,
            "appearance": self.appearance,
            "materials": self.materials,
            "colors": self.colors,
            "size": self.size,
            "details": self.details,
            "effects": self.effects,
            "base_prompt": self.base_prompt,
            "is_established": self.is_established
        }
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'ObjectContext':
        return cls(**data)


@dataclass
class BookContext:
    """
    Полный контекст книги для генерации промптов.
    
    Это runtime структура, которая:
    1. Загружается из БД при первом запросе
    2. Кэшируется в Redis
    3. Обновляется при добавлении/изменении персонажей/сцен
    
    НЕ хранит саму книгу - книга в Catalog.API!
    Хранит только данные для консистентности визуализации.
    """
    
    book_id: str
    
    # Персонажи (name -> profile)
    characters: Dict[str, CharacterProfile] = field(default_factory=dict)
    
    # Сцены (name -> context)
    scenes: Dict[str, SceneContext] = field(default_factory=dict)
    
    # Объекты (name -> context)
    objects: Dict[str, ObjectContext] = field(default_factory=dict)
    
    # Настройки визуализации
    default_style: Optional[str] = None
    default_model: str = "dalle3"
    
    # Метаданные
    created_at: datetime = field(default_factory=datetime.utcnow)
    updated_at: datetime = field(default_factory=datetime.utcnow)
    
    # ===========================================
    # CHARACTER METHODS
    # ===========================================
    
    def get_character(self, name: str) -> Optional[CharacterProfile]:
        """Получить профиль персонажа по имени (case-insensitive)."""
        name_lower = name.lower()
        for char_name, profile in self.characters.items():
            if char_name.lower() == name_lower:
                return profile
            # Проверяем алиасы если есть
        return None
    
    def add_character(self, profile: CharacterProfile):
        """Добавить или обновить персонажа."""
        self.characters[profile.name] = profile
        self.updated_at = datetime.utcnow()
    
    def remove_character(self, name: str) -> bool:
        """Удалить персонажа."""
        if name in self.characters:
            del self.characters[name]
            self.updated_at = datetime.utcnow()
            return True
        return False
    
    # ===========================================
    # SCENE METHODS
    # ===========================================
    
    def get_scene(self, name: str) -> Optional[SceneContext]:
        """Получить контекст сцены по имени."""
        name_lower = name.lower()
        for scene_name, context in self.scenes.items():
            if scene_name.lower() == name_lower:
                return context
        return None
    
    def add_scene(self, context: SceneContext):
        """Добавить или обновить сцену."""
        self.scenes[context.name] = context
        self.updated_at = datetime.utcnow()
    
    def remove_scene(self, name: str) -> bool:
        """Удалить сцену."""
        if name in self.scenes:
            del self.scenes[name]
            self.updated_at = datetime.utcnow()
            return True
        return False
    
    # ===========================================
    # OBJECT METHODS
    # ===========================================
    
    def get_object(self, name: str) -> Optional[ObjectContext]:
        """Получить контекст объекта по имени."""
        name_lower = name.lower()
        for obj_name, context in self.objects.items():
            if obj_name.lower() == name_lower:
                return context
        return None
    
    def add_object(self, context: ObjectContext):
        """Добавить или обновить объект."""
        self.objects[context.name] = context
        self.updated_at = datetime.utcnow()
    
    # ===========================================
    # SERIALIZATION
    # ===========================================
    
    def to_dict(self) -> Dict[str, Any]:
        """Сериализация для Redis кэша."""
        return {
            "book_id": self.book_id,
            "characters": {
                name: profile.to_dict() 
                for name, profile in self.characters.items()
            },
            "scenes": {
                name: scene.to_dict() 
                for name, scene in self.scenes.items()
            },
            "objects": {
                name: obj.to_dict() 
                for name, obj in self.objects.items()
            },
            "default_style": self.default_style,
            "default_model": self.default_model,
            "created_at": self.created_at.isoformat(),
            "updated_at": self.updated_at.isoformat()
        }
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'BookContext':
        """Десериализация из Redis кэша."""
        context = cls(
            book_id=data["book_id"],
            default_style=data.get("default_style"),
            default_model=data.get("default_model", "dalle3")
        )
        
        # Восстанавливаем персонажей
        for name, char_data in data.get("characters", {}).items():
            context.characters[name] = CharacterProfile.from_dict(char_data)
        
        # Восстанавливаем сцены
        for name, scene_data in data.get("scenes", {}).items():
            context.scenes[name] = SceneContext.from_dict(scene_data)
        
        # Восстанавливаем объекты
        for name, obj_data in data.get("objects", {}).items():
            context.objects[name] = ObjectContext.from_dict(obj_data)
        
        # Timestamps
        if "created_at" in data:
            context.created_at = datetime.fromisoformat(data["created_at"])
        if "updated_at" in data:
            context.updated_at = datetime.fromisoformat(data["updated_at"])
        
        return context
    
    # ===========================================
    # UTILITY
    # ===========================================
    
    def get_all_character_names(self) -> List[str]:
        """Получить список имён всех персонажей."""
        return list(self.characters.keys())
    
    def get_all_scene_names(self) -> List[str]:
        """Получить список названий всех сцен."""
        return list(self.scenes.keys())
    
    def get_established_characters(self) -> List[CharacterProfile]:
        """Получить персонажей с зафиксированным описанием."""
        return [p for p in self.characters.values() if p.is_established]
    
    def stats(self) -> Dict[str, int]:
        """Статистика контекста."""
        return {
            "characters": len(self.characters),
            "scenes": len(self.scenes),
            "objects": len(self.objects),
            "established_characters": len(self.get_established_characters())
        }