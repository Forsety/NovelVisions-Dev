# core/templates/template_engine.py
"""
Движок шаблонов промптов.

TemplateEngine управляет шаблонами для разных типов сцен:
- Character portraits
- Action scenes
- Establishing shots
- Dialogue scenes
- Emotional moments
- Battle sequences

Каждый шаблон содержит:
- Структуру промпта
- Рекомендации по композиции
- Рекомендации по освещению
"""

from typing import Dict, List, Optional, Any
from dataclasses import dataclass, field
from enum import Enum
import re


class TemplateType(Enum):
    """Типы шаблонов"""
    # Персонажи
    CHARACTER_PORTRAIT = "character_portrait"
    CHARACTER_FULL_BODY = "character_full_body"
    CHARACTER_ACTION = "character_action"
    CHARACTER_GROUP = "character_group"
    
    # Сцены
    SCENE_ESTABLISHING = "scene_establishing"
    SCENE_INTERIOR = "scene_interior"
    SCENE_EXTERIOR = "scene_exterior"
    SCENE_CLOSE_UP = "scene_close_up"
    
    # Действие
    ACTION_DYNAMIC = "action_dynamic"
    ACTION_BATTLE = "action_battle"
    ACTION_CHASE = "action_chase"
    
    # Эмоции
    EMOTIONAL_INTIMATE = "emotional_intimate"
    EMOTIONAL_DRAMATIC = "emotional_dramatic"
    EMOTIONAL_TENSION = "emotional_tension"
    
    # Диалоги
    DIALOGUE_TWO_SHOT = "dialogue_two_shot"
    DIALOGUE_OVER_SHOULDER = "dialogue_over_shoulder"
    
    # Объекты
    OBJECT_FOCUS = "object_focus"
    OBJECT_DETAIL = "object_detail"
    
    # Атмосфера
    ATMOSPHERIC = "atmospheric"
    ATMOSPHERIC_WEATHER = "atmospheric_weather"


@dataclass
class PromptTemplate:
    """Шаблон промпта"""
    id: str
    name: str
    type: TemplateType
    description: str
    
    # Структура промпта с плейсхолдерами
    structure: str
    
    # Переменные которые нужно заполнить
    variables: List[str]
    
    # Переменные со значениями по умолчанию
    defaults: Dict[str, str] = field(default_factory=dict)
    
    # Рекомендации
    shot_suggestion: str = "medium shot"
    angle_suggestion: str = "eye level"
    lighting_suggestion: str = "natural lighting"
    composition_notes: str = ""
    
    # Теги для поиска
    tags: List[str] = field(default_factory=list)


class TemplateEngine:
    """
    Движок шаблонов для разных типов сцен.
    
    Использование:
        engine = TemplateEngine()
        template = engine.get_template(TemplateType.CHARACTER_PORTRAIT)
        prompt = engine.fill_template(template, {"character": "young woman"})
    """
    
    def __init__(self):
        self.templates: Dict[TemplateType, PromptTemplate] = {}
        self._init_templates()
    
    def _init_templates(self):
        """Инициализация всех шаблонов"""
        
        templates = {
            # ═══════════════════════════════════════════════════════════
            # CHARACTER TEMPLATES
            # ═══════════════════════════════════════════════════════════
            TemplateType.CHARACTER_PORTRAIT: PromptTemplate(
                id="char_portrait",
                name="Character Portrait",
                type=TemplateType.CHARACTER_PORTRAIT,
                description="Close-up or medium close-up portrait of a character",
                structure="{character_description}, {expression}, {pose}, portrait shot, {lighting}, {background}, {atmosphere}",
                variables=["character_description", "expression", "pose", "lighting", "background", "atmosphere"],
                defaults={
                    "expression": "neutral expression",
                    "pose": "facing camera",
                    "lighting": "soft studio lighting",
                    "background": "blurred background",
                    "atmosphere": "professional portrait"
                },
                shot_suggestion="close-up or medium close-up",
                angle_suggestion="eye level, slightly above",
                lighting_suggestion="Rembrandt lighting, rim light for separation",
                composition_notes="Rule of thirds, eyes on upper third line",
                tags=["portrait", "character", "face"]
            ),
            
            TemplateType.CHARACTER_FULL_BODY: PromptTemplate(
                id="char_full_body",
                name="Character Full Body",
                type=TemplateType.CHARACTER_FULL_BODY,
                description="Full body shot showing character's complete appearance",
                structure="{character_description}, full body shot, {pose}, {clothing}, {action}, {environment}, {lighting}",
                variables=["character_description", "pose", "clothing", "action", "environment", "lighting"],
                defaults={
                    "pose": "standing",
                    "clothing": "detailed attire",
                    "action": "",
                    "environment": "contextual background",
                    "lighting": "dramatic lighting"
                },
                shot_suggestion="full body or medium full shot",
                angle_suggestion="eye level or slightly low angle for heroic feel",
                lighting_suggestion="Three-point lighting, strong key light",
                tags=["full body", "character", "costume"]
            ),
            
            TemplateType.CHARACTER_ACTION: PromptTemplate(
                id="char_action",
                name="Character in Action",
                type=TemplateType.CHARACTER_ACTION,
                description="Character performing a dynamic action",
                structure="{character_description} {action}, dynamic pose, {motion_effect}, {environment}, {atmosphere}, action shot",
                variables=["character_description", "action", "motion_effect", "environment", "atmosphere"],
                defaults={
                    "action": "in motion",
                    "motion_effect": "motion blur",
                    "environment": "dynamic background",
                    "atmosphere": "intense atmosphere"
                },
                shot_suggestion="medium or wide shot for context",
                angle_suggestion="dynamic angle, dutch tilt optional",
                lighting_suggestion="Dramatic, directional, high contrast",
                tags=["action", "dynamic", "movement"]
            ),
            
            TemplateType.CHARACTER_GROUP: PromptTemplate(
                id="char_group",
                name="Group of Characters",
                type=TemplateType.CHARACTER_GROUP,
                description="Multiple characters in a scene together",
                structure="{characters_description}, group composition, {interaction}, {arrangement}, {environment}, {lighting}, {atmosphere}",
                variables=["characters_description", "interaction", "arrangement", "environment", "lighting", "atmosphere"],
                defaults={
                    "interaction": "interacting naturally",
                    "arrangement": "balanced composition",
                    "environment": "appropriate setting",
                    "lighting": "even lighting",
                    "atmosphere": "cohesive mood"
                },
                shot_suggestion="wide or medium wide shot",
                angle_suggestion="eye level for natural feel",
                lighting_suggestion="Even lighting to show all characters",
                tags=["group", "multiple", "ensemble"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # SCENE TEMPLATES
            # ═══════════════════════════════════════════════════════════
            TemplateType.SCENE_ESTABLISHING: PromptTemplate(
                id="scene_establish",
                name="Establishing Shot",
                type=TemplateType.SCENE_ESTABLISHING,
                description="Wide shot introducing location and setting",
                structure="wide establishing shot of {location}, {time_of_day}, {weather}, {atmosphere}, cinematic, epic scale, {additional_details}",
                variables=["location", "time_of_day", "weather", "atmosphere", "additional_details"],
                defaults={
                    "time_of_day": "golden hour",
                    "weather": "clear sky",
                    "atmosphere": "grand and majestic",
                    "additional_details": "detailed environment"
                },
                shot_suggestion="extreme wide shot or wide shot",
                angle_suggestion="high angle or eye level",
                lighting_suggestion="Natural, atmospheric, emphasizing scale",
                composition_notes="Show scale, context, and mood of location",
                tags=["establishing", "location", "wide"]
            ),
            
            TemplateType.SCENE_INTERIOR: PromptTemplate(
                id="scene_interior",
                name="Interior Scene",
                type=TemplateType.SCENE_INTERIOR,
                description="Indoor environment with specific atmosphere",
                structure="interior of {location}, {lighting_type} lighting, {decorations}, {atmosphere}, detailed environment, {architectural_details}",
                variables=["location", "lighting_type", "decorations", "atmosphere", "architectural_details"],
                defaults={
                    "lighting_type": "warm ambient",
                    "decorations": "period-appropriate furnishings",
                    "atmosphere": "immersive atmosphere",
                    "architectural_details": "architectural details"
                },
                shot_suggestion="wide interior or medium wide",
                angle_suggestion="eye level, slight low angle for grandeur",
                lighting_suggestion="Practical lighting sources, ambient fill",
                tags=["interior", "indoor", "room"]
            ),
            
            TemplateType.SCENE_EXTERIOR: PromptTemplate(
                id="scene_exterior",
                name="Exterior Scene",
                type=TemplateType.SCENE_EXTERIOR,
                description="Outdoor environment with natural elements",
                structure="exterior view of {location}, {time_of_day}, {weather}, {natural_elements}, {atmosphere}, {lighting}",
                variables=["location", "time_of_day", "weather", "natural_elements", "atmosphere", "lighting"],
                defaults={
                    "time_of_day": "daytime",
                    "weather": "clear",
                    "natural_elements": "environmental details",
                    "atmosphere": "natural atmosphere",
                    "lighting": "natural sunlight"
                },
                shot_suggestion="wide or medium wide",
                angle_suggestion="varies by content",
                lighting_suggestion="Natural lighting based on time of day",
                tags=["exterior", "outdoor", "landscape"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # ACTION TEMPLATES
            # ═══════════════════════════════════════════════════════════
            TemplateType.ACTION_BATTLE: PromptTemplate(
                id="action_battle",
                name="Battle Scene",
                type=TemplateType.ACTION_BATTLE,
                description="Combat or battle sequence",
                structure="epic battle scene, {combatants}, {action}, {weapons}, dynamic composition, {effects}, dramatic lighting, {atmosphere}",
                variables=["combatants", "action", "weapons", "effects", "atmosphere"],
                defaults={
                    "combatants": "warriors in combat",
                    "action": "fierce fighting",
                    "weapons": "weapons clashing",
                    "effects": "dust and debris, sparks",
                    "atmosphere": "intense and chaotic"
                },
                shot_suggestion="dynamic wide or medium shot",
                angle_suggestion="dynamic angle, dutch tilt, low angle",
                lighting_suggestion="Dramatic, high contrast, directional",
                composition_notes="Capture peak action moment, show movement",
                tags=["battle", "combat", "fight", "war"]
            ),
            
            TemplateType.ACTION_CHASE: PromptTemplate(
                id="action_chase",
                name="Chase Scene",
                type=TemplateType.ACTION_CHASE,
                description="Pursuit or chase sequence",
                structure="{pursuer} chasing {target}, high speed action, {environment}, motion blur, {obstacles}, intense atmosphere, dynamic camera angle",
                variables=["pursuer", "target", "environment", "obstacles"],
                defaults={
                    "pursuer": "pursuer",
                    "target": "target fleeing",
                    "environment": "urban environment",
                    "obstacles": "environmental obstacles"
                },
                shot_suggestion="tracking shot feel, medium or wide",
                angle_suggestion="dynamic, from behind or side",
                lighting_suggestion="Motion-enhanced, streaking lights",
                tags=["chase", "pursuit", "speed", "action"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # EMOTIONAL TEMPLATES
            # ═══════════════════════════════════════════════════════════
            TemplateType.EMOTIONAL_INTIMATE: PromptTemplate(
                id="emotional_intimate",
                name="Intimate Moment",
                type=TemplateType.EMOTIONAL_INTIMATE,
                description="Close, personal, romantic or tender moment",
                structure="{characters} in intimate moment, {action}, {expressions}, soft lighting, {atmosphere}, romantic, {setting}",
                variables=["characters", "action", "expressions", "atmosphere", "setting"],
                defaults={
                    "characters": "two figures",
                    "action": "close together",
                    "expressions": "tender expressions",
                    "atmosphere": "warm and romantic",
                    "setting": "private setting"
                },
                shot_suggestion="close-up or medium close-up",
                angle_suggestion="eye level, intimate perspective",
                lighting_suggestion="Soft, warm, candlelight, golden hour",
                composition_notes="Shallow depth of field, focus on connection",
                tags=["intimate", "romantic", "tender", "emotional"]
            ),
            
            TemplateType.EMOTIONAL_DRAMATIC: PromptTemplate(
                id="emotional_dramatic",
                name="Dramatic Moment",
                type=TemplateType.EMOTIONAL_DRAMATIC,
                description="High emotional intensity scene",
                structure="{character} experiencing {emotion}, {expression}, {body_language}, dramatic lighting, {atmosphere}, emotional impact, {environment}",
                variables=["character", "emotion", "expression", "body_language", "atmosphere", "environment"],
                defaults={
                    "character": "figure",
                    "emotion": "intense emotion",
                    "expression": "powerful expression",
                    "body_language": "expressive posture",
                    "atmosphere": "emotionally charged",
                    "environment": "contextual setting"
                },
                shot_suggestion="close-up on face, or wide for isolation",
                angle_suggestion="varies - low for power, high for vulnerability",
                lighting_suggestion="Chiaroscuro, single source, dramatic shadows",
                tags=["dramatic", "emotional", "intense", "powerful"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # DIALOGUE TEMPLATES
            # ═══════════════════════════════════════════════════════════
            TemplateType.DIALOGUE_TWO_SHOT: PromptTemplate(
                id="dialogue_two_shot",
                name="Dialogue Two-Shot",
                type=TemplateType.DIALOGUE_TWO_SHOT,
                description="Two characters in conversation",
                structure="two-shot of {character1} and {character2}, {interaction}, {expressions}, {location}, {lighting}, conversational atmosphere",
                variables=["character1", "character2", "interaction", "expressions", "location", "lighting"],
                defaults={
                    "interaction": "in conversation",
                    "expressions": "engaged expressions",
                    "location": "appropriate setting",
                    "lighting": "natural lighting"
                },
                shot_suggestion="medium two-shot",
                angle_suggestion="eye level",
                lighting_suggestion="Even, natural, shows both faces clearly",
                tags=["dialogue", "conversation", "two-shot"]
            ),
            
            TemplateType.DIALOGUE_OVER_SHOULDER: PromptTemplate(
                id="dialogue_ots",
                name="Over-the-Shoulder Shot",
                type=TemplateType.DIALOGUE_OVER_SHOULDER,
                description="Dialogue from one character's perspective",
                structure="over-the-shoulder shot, {foreground_character} in foreground, {background_character} facing camera, {expression}, {setting}, {lighting}",
                variables=["foreground_character", "background_character", "expression", "setting", "lighting"],
                defaults={
                    "foreground_character": "character's shoulder visible",
                    "background_character": "character speaking",
                    "expression": "engaged expression",
                    "setting": "contextual background",
                    "lighting": "natural lighting"
                },
                shot_suggestion="over-the-shoulder medium shot",
                angle_suggestion="eye level, slight angle",
                lighting_suggestion="Focus light on speaking character",
                tags=["dialogue", "over-shoulder", "ots", "conversation"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # OBJECT TEMPLATES
            # ═══════════════════════════════════════════════════════════
            TemplateType.OBJECT_FOCUS: PromptTemplate(
                id="object_focus",
                name="Object Focus",
                type=TemplateType.OBJECT_FOCUS,
                description="Focus on a significant object",
                structure="close-up of {object}, {details}, {material}, {lighting}, {background}, {atmosphere}, product photography quality",
                variables=["object", "details", "material", "lighting", "background", "atmosphere"],
                defaults={
                    "details": "intricate details",
                    "material": "visible textures",
                    "lighting": "dramatic lighting",
                    "background": "complementary background",
                    "atmosphere": "significant atmosphere"
                },
                shot_suggestion="close-up or macro",
                angle_suggestion="hero angle, slight above",
                lighting_suggestion="Product lighting, highlights details",
                tags=["object", "detail", "macro", "focus"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # ATMOSPHERIC TEMPLATES
            # ═══════════════════════════════════════════════════════════
            TemplateType.ATMOSPHERIC: PromptTemplate(
                id="atmospheric",
                name="Atmospheric Scene",
                type=TemplateType.ATMOSPHERIC,
                description="Mood and atmosphere focused scene",
                structure="{description}, {atmosphere}, {weather}, {lighting}, mood: {mood}, {environmental_details}, {style}",
                variables=["description", "atmosphere", "weather", "lighting", "mood", "environmental_details", "style"],
                defaults={
                    "atmosphere": "immersive atmosphere",
                    "weather": "atmospheric conditions",
                    "lighting": "moody lighting",
                    "mood": "evocative",
                    "environmental_details": "rich environmental details",
                    "style": "artistic"
                },
                shot_suggestion="varies by mood",
                angle_suggestion="varies by content",
                lighting_suggestion="Atmospheric, moody, expressive",
                tags=["atmospheric", "mood", "ambiance"]
            ),
            
            TemplateType.ATMOSPHERIC_WEATHER: PromptTemplate(
                id="atmospheric_weather",
                name="Weather Atmosphere",
                type=TemplateType.ATMOSPHERIC_WEATHER,
                description="Weather-focused atmospheric scene",
                structure="{scene} during {weather}, {weather_effects}, {lighting}, {atmosphere}, {visibility}, dramatic weather",
                variables=["scene", "weather", "weather_effects", "lighting", "atmosphere", "visibility"],
                defaults={
                    "scene": "landscape",
                    "weather": "storm",
                    "weather_effects": "rain, wind effects",
                    "lighting": "dramatic stormy light",
                    "atmosphere": "powerful atmosphere",
                    "visibility": "atmospheric perspective"
                },
                shot_suggestion="wide for scale",
                angle_suggestion="varies",
                lighting_suggestion="Weather-appropriate, dramatic",
                tags=["weather", "storm", "rain", "atmosphere"]
            ),
        }
        
        self.templates = templates
    
    def get_template(self, template_type: TemplateType) -> Optional[PromptTemplate]:
        """Получает шаблон по типу"""
        return self.templates.get(template_type)
    
    def get_template_by_scene_type(self, scene_type: str) -> PromptTemplate:
        """
        Получает шаблон на основе типа сцены.
        
        Маппинг scene_type (из анализа) -> TemplateType
        """
        type_mapping = {
            "establishing": TemplateType.SCENE_ESTABLISHING,
            "character_intro": TemplateType.CHARACTER_PORTRAIT,
            "action": TemplateType.CHARACTER_ACTION,
            "dialogue": TemplateType.DIALOGUE_TWO_SHOT,
            "emotional": TemplateType.EMOTIONAL_DRAMATIC,
            "revelation": TemplateType.EMOTIONAL_DRAMATIC,
            "atmospheric": TemplateType.ATMOSPHERIC,
            "object_focus": TemplateType.OBJECT_FOCUS,
            "battle": TemplateType.ACTION_BATTLE,
            "intimate": TemplateType.EMOTIONAL_INTIMATE,
            "horror": TemplateType.ATMOSPHERIC,
            "death": TemplateType.EMOTIONAL_DRAMATIC,
            "chase": TemplateType.ACTION_CHASE,
            "celebration": TemplateType.CHARACTER_GROUP,
            "mystery": TemplateType.ATMOSPHERIC
        }
        
        template_type = type_mapping.get(scene_type.lower(), TemplateType.ATMOSPHERIC)
        return self.templates.get(template_type, self.templates[TemplateType.ATMOSPHERIC])
    
    def fill_template(
        self,
        template: PromptTemplate,
        variables: Dict[str, str],
        use_defaults: bool = True
    ) -> str:
        """
        Заполняет шаблон переменными.
        
        Args:
            template: Шаблон для заполнения
            variables: Словарь переменных
            use_defaults: Использовать ли значения по умолчанию
            
        Returns:
            Заполненный промпт
        """
        result = template.structure
        
        # Объединяем с дефолтами если нужно
        all_vars = {}
        if use_defaults:
            all_vars.update(template.defaults)
        all_vars.update(variables)
        
        # Заменяем переменные
        for var, value in all_vars.items():
            placeholder = f"{{{var}}}"
            if value:
                result = result.replace(placeholder, value)
            else:
                result = result.replace(placeholder, "")
        
        # Убираем оставшиеся пустые плейсхолдеры
        result = re.sub(r'\{[^}]+\}', '', result)
        
        # Чистим результат
        result = re.sub(r',\s*,', ',', result)  # Убираем двойные запятые
        result = re.sub(r'\s+', ' ', result)    # Убираем лишние пробелы
        result = result.strip(' ,')             # Убираем запятые по краям
        
        return result
    
    def fill_by_scene_type(
        self,
        scene_type: str,
        variables: Dict[str, str]
    ) -> str:
        """Заполняет шаблон на основе типа сцены"""
        template = self.get_template_by_scene_type(scene_type)
        return self.fill_template(template, variables)
    
    def get_composition_suggestions(self, template_type: TemplateType) -> Dict[str, str]:
        """Возвращает рекомендации по композиции для шаблона"""
        template = self.templates.get(template_type)
        
        if not template:
            return {}
        
        return {
            "shot": template.shot_suggestion,
            "angle": template.angle_suggestion,
            "lighting": template.lighting_suggestion,
            "notes": template.composition_notes
        }
    
    def list_templates(self) -> List[Dict[str, Any]]:
        """Возвращает список всех шаблонов"""
        return [
            {
                "id": t.id,
                "name": t.name,
                "type": t.type.value,
                "description": t.description,
                "variables": t.variables,
                "tags": t.tags
            }
            for t in self.templates.values()
        ]
    
    def search_templates(self, query: str) -> List[PromptTemplate]:
        """Поиск шаблонов по запросу"""
        query_lower = query.lower()
        results = []
        
        for template in self.templates.values():
            if (query_lower in template.name.lower() or
                query_lower in template.description.lower() or
                any(query_lower in tag for tag in template.tags)):
                results.append(template)
        
        return results