# core/engines/style_engine.py
"""
Движок применения художественных стилей.

StyleEngine управляет:
- 50+ предустановленных стилей
- Комбинированием стилей
- Интенсивностью применения
- Стиль-специфичными негативами
- Рекомендациями по моделям

Категории стилей:
- Art Movements (impressionism, surrealism, baroque...)
- Photography (cinematic, documentary, portrait...)
- Illustration (comic, manga, concept art...)
- 3D/CGI (octane, unreal, cinema4d...)
- Experimental (glitch, vaporwave, abstract...)
"""

import json
from typing import Dict, List, Optional, Tuple
from pathlib import Path
from dataclasses import dataclass, field
from enum import Enum


class StyleCategory(Enum):
    """Категории стилей"""
    ART_MOVEMENTS = "art_movements"
    PHOTOGRAPHY = "photography"
    ILLUSTRATION = "illustration"
    RENDER_3D = "3d_render"
    EXPERIMENTAL = "experimental"
    CULTURAL = "cultural"
    HISTORICAL = "historical"
    GENRE = "genre"


@dataclass
class StylePreset:
    """Предустановка стиля"""
    id: str
    name: str
    category: StyleCategory
    description: str
    prompt_prefix: str  # Добавляется в начало
    prompt_suffix: str  # Добавляется в конец
    negative_additions: List[str] = field(default_factory=list)
    recommended_models: List[str] = field(default_factory=list)
    intensity_default: float = 1.0
    tags: List[str] = field(default_factory=list)
    
    def to_dict(self) -> Dict:
        return {
            "id": self.id,
            "name": self.name,
            "category": self.category.value,
            "description": self.description,
            "tags": self.tags
        }


class StyleEngine:
    """
    Движок применения художественных стилей.
    
    Использование:
        engine = StyleEngine()
        styled_prompt = engine.apply_style(prompt, "cinematic", intensity=0.8)
        
        # Комбинирование стилей
        styled = engine.combine_styles(prompt, ["noir", "cyberpunk"], [0.6, 0.4])
    """
    
    def __init__(self, presets_dir: Optional[str] = None):
        self.presets: Dict[str, StylePreset] = {}
        self._load_builtin_presets()
        
        if presets_dir:
            self._load_custom_presets(presets_dir)
    
    def _load_builtin_presets(self):
        """Загружает встроенные стили"""
        
        presets = {
            # ═══════════════════════════════════════════════════════════
            # ART MOVEMENTS
            # ═══════════════════════════════════════════════════════════
            "impressionism": StylePreset(
                id="impressionism",
                name="Impressionism",
                category=StyleCategory.ART_MOVEMENTS,
                description="Soft brushstrokes, emphasis on light and color",
                prompt_prefix="",
                prompt_suffix="impressionist painting style, soft visible brushstrokes, dappled light, vibrant colors, Claude Monet inspired, plein air aesthetic",
                negative_additions=["sharp lines", "photorealistic", "digital"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["painting", "classic", "soft"]
            ),
            
            "surrealism": StylePreset(
                id="surrealism",
                name="Surrealism",
                category=StyleCategory.ART_MOVEMENTS,
                description="Dreamlike, impossible, subconscious imagery",
                prompt_prefix="",
                prompt_suffix="surrealist art, dreamlike quality, impossible geometry, Salvador Dali inspired, subconscious imagery, melting forms, unexpected juxtapositions",
                negative_additions=["realistic", "ordinary", "mundane"],
                recommended_models=["midjourney", "dalle3"],
                tags=["dream", "abstract", "artistic"]
            ),
            
            "baroque": StylePreset(
                id="baroque",
                name="Baroque",
                category=StyleCategory.ART_MOVEMENTS,
                description="Dramatic, rich, ornate classical style",
                prompt_prefix="",
                prompt_suffix="baroque painting, dramatic chiaroscuro lighting, rich deep colors, ornate details, Caravaggio inspired, theatrical composition, gold accents",
                negative_additions=["minimalist", "modern", "simple"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["classical", "dramatic", "ornate"]
            ),
            
            "art_nouveau": StylePreset(
                id="art_nouveau",
                name="Art Nouveau",
                category=StyleCategory.ART_MOVEMENTS,
                description="Organic curves, decorative, nature-inspired",
                prompt_prefix="",
                prompt_suffix="art nouveau style, organic flowing lines, decorative elements, Alphonse Mucha inspired, ornamental borders, floral motifs, elegant curves",
                negative_additions=["geometric", "angular", "industrial"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["decorative", "elegant", "organic"]
            ),
            
            "renaissance": StylePreset(
                id="renaissance",
                name="Renaissance",
                category=StyleCategory.ART_MOVEMENTS,
                description="Classical realism, detailed, harmonious",
                prompt_prefix="",
                prompt_suffix="Renaissance painting, classical composition, sfumato technique, Leonardo da Vinci inspired, anatomical precision, golden ratio, religious iconography aesthetic",
                negative_additions=["modern", "abstract", "cartoon"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["classical", "realistic", "historical"]
            ),
            
            "romanticism": StylePreset(
                id="romanticism",
                name="Romanticism",
                category=StyleCategory.ART_MOVEMENTS,
                description="Emotional, dramatic nature, sublime",
                prompt_prefix="",
                prompt_suffix="romanticism painting, dramatic nature, emotional intensity, sublime landscape, Caspar David Friedrich inspired, stormy skies, heroic figures",
                negative_additions=["mundane", "urban", "modern"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["emotional", "nature", "dramatic"]
            ),
            
            "expressionism": StylePreset(
                id="expressionism",
                name="Expressionism",
                category=StyleCategory.ART_MOVEMENTS,
                description="Distorted reality, emotional intensity",
                prompt_prefix="",
                prompt_suffix="expressionist art, distorted forms, intense colors, emotional distortion, Edvard Munch inspired, psychological intensity, bold brushwork",
                negative_additions=["realistic", "calm", "photographic"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["emotional", "distorted", "intense"]
            ),
            
            "cubism": StylePreset(
                id="cubism",
                name="Cubism",
                category=StyleCategory.ART_MOVEMENTS,
                description="Geometric abstraction, multiple perspectives",
                prompt_prefix="",
                prompt_suffix="cubist art, geometric abstraction, fragmented forms, multiple perspectives, Pablo Picasso inspired, angular shapes, deconstructed reality",
                negative_additions=["realistic", "smooth", "photographic"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["abstract", "geometric", "modern"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # PHOTOGRAPHY
            # ═══════════════════════════════════════════════════════════
            "cinematic": StylePreset(
                id="cinematic",
                name="Cinematic",
                category=StyleCategory.PHOTOGRAPHY,
                description="Movie-like quality with dramatic lighting",
                prompt_prefix="cinematic shot,",
                prompt_suffix="movie still, anamorphic lens, dramatic lighting, film grain, color grading, shallow depth of field, widescreen composition",
                negative_additions=["amateur", "snapshot", "flat lighting"],
                recommended_models=["midjourney", "flux", "dalle3"],
                tags=["film", "dramatic", "professional"]
            ),
            
            "portrait": StylePreset(
                id="portrait",
                name="Portrait Photography",
                category=StyleCategory.PHOTOGRAPHY,
                description="Professional portrait with studio lighting",
                prompt_prefix="professional portrait,",
                prompt_suffix="studio lighting, shallow depth of field, 85mm lens, catchlights in eyes, soft skin, professional headshot quality",
                negative_additions=["wide angle distortion", "harsh shadows", "unflattering"],
                recommended_models=["midjourney", "flux"],
                tags=["portrait", "professional", "studio"]
            ),
            
            "documentary": StylePreset(
                id="documentary",
                name="Documentary",
                category=StyleCategory.PHOTOGRAPHY,
                description="Authentic, candid, journalistic",
                prompt_prefix="documentary photograph,",
                prompt_suffix="candid shot, natural lighting, photojournalism style, authentic moment, raw emotion, unposed",
                negative_additions=["staged", "artificial", "overproduced"],
                recommended_models=["flux", "dalle3"],
                tags=["authentic", "candid", "journalistic"]
            ),
            
            "noir": StylePreset(
                id="noir",
                name="Film Noir",
                category=StyleCategory.PHOTOGRAPHY,
                description="Dark, moody, high contrast black and white",
                prompt_prefix="film noir style,",
                prompt_suffix="black and white, high contrast, dramatic shadows, venetian blind shadows, cigarette smoke, mysterious atmosphere, 1940s aesthetic",
                negative_additions=["colorful", "bright", "cheerful", "saturated"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["dark", "dramatic", "classic"]
            ),
            
            "vintage": StylePreset(
                id="vintage",
                name="Vintage Photography",
                category=StyleCategory.PHOTOGRAPHY,
                description="Retro film aesthetic with faded colors",
                prompt_prefix="vintage photograph,",
                prompt_suffix="retro aesthetic, faded colors, film grain, nostalgic, Kodak Portra, light leaks, soft focus edges",
                negative_additions=["modern", "digital", "sharp", "HDR"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["retro", "nostalgic", "film"]
            ),
            
            "street": StylePreset(
                id="street",
                name="Street Photography",
                category=StyleCategory.PHOTOGRAPHY,
                description="Urban life, candid moments, gritty",
                prompt_prefix="street photography,",
                prompt_suffix="urban life, candid moment, natural lighting, gritty texture, decisive moment, Henri Cartier-Bresson inspired",
                negative_additions=["posed", "studio", "artificial"],
                recommended_models=["flux", "dalle3"],
                tags=["urban", "candid", "documentary"]
            ),
            
            "macro": StylePreset(
                id="macro",
                name="Macro Photography",
                category=StyleCategory.PHOTOGRAPHY,
                description="Extreme close-up, intricate details",
                prompt_prefix="macro photograph,",
                prompt_suffix="extreme close-up, intricate details, shallow depth of field, water droplets, texture emphasis, ring light",
                negative_additions=["wide shot", "distant", "blurry"],
                recommended_models=["midjourney", "flux"],
                tags=["detailed", "close-up", "scientific"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # ILLUSTRATION
            # ═══════════════════════════════════════════════════════════
            "anime": StylePreset(
                id="anime",
                name="Anime",
                category=StyleCategory.ILLUSTRATION,
                description="Japanese animation style",
                prompt_prefix="anime style,",
                prompt_suffix="cel shading, vibrant colors, large expressive eyes, clean lines, Studio Ghibli quality, Japanese animation",
                negative_additions=["realistic", "photographic", "3d render", "western cartoon"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["japanese", "animation", "stylized"]
            ),
            
            "manga": StylePreset(
                id="manga",
                name="Manga",
                category=StyleCategory.ILLUSTRATION,
                description="Japanese comic style, black and white",
                prompt_prefix="manga style,",
                prompt_suffix="black and white, screentone shading, dynamic lines, Japanese comic, expressive faces, action lines",
                negative_additions=["color", "western", "painted"],
                recommended_models=["stable-diffusion"],
                tags=["japanese", "comic", "monochrome"]
            ),
            
            "concept_art": StylePreset(
                id="concept_art",
                name="Concept Art",
                category=StyleCategory.ILLUSTRATION,
                description="Professional production design",
                prompt_prefix="concept art,",
                prompt_suffix="matte painting, artstation trending, production design, environment design, professional illustration, game art quality",
                negative_additions=["amateur", "sketch", "unfinished"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["professional", "game", "production"]
            ),
            
            "comic": StylePreset(
                id="comic",
                name="Comic Book",
                category=StyleCategory.ILLUSTRATION,
                description="Western comic book style",
                prompt_prefix="comic book art,",
                prompt_suffix="bold outlines, cel shading, dynamic poses, superhero comic style, halftone dots, action panel composition",
                negative_additions=["realistic", "soft edges", "painterly"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["western", "superhero", "bold"]
            ),
            
            "watercolor": StylePreset(
                id="watercolor",
                name="Watercolor",
                category=StyleCategory.ILLUSTRATION,
                description="Soft, flowing watercolor painting",
                prompt_prefix="watercolor painting,",
                prompt_suffix="wet on wet technique, soft edges, flowing colors, paper texture visible, transparent layers, delicate washes",
                negative_additions=["sharp lines", "digital", "opaque"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["soft", "traditional", "flowing"]
            ),
            
            "oil_painting": StylePreset(
                id="oil_painting",
                name="Oil Painting",
                category=StyleCategory.ILLUSTRATION,
                description="Classical oil painting technique",
                prompt_prefix="oil painting,",
                prompt_suffix="visible brushstrokes, canvas texture, rich impasto, classical technique, deep colors, layered glazes",
                negative_additions=["digital", "smooth", "flat", "photographic"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["classical", "traditional", "textured"]
            ),
            
            "sketch": StylePreset(
                id="sketch",
                name="Pencil Sketch",
                category=StyleCategory.ILLUSTRATION,
                description="Hand-drawn pencil artwork",
                prompt_prefix="pencil sketch,",
                prompt_suffix="graphite drawing, cross-hatching, paper texture, hand-drawn quality, shading techniques, artistic linework",
                negative_additions=["color", "digital", "painted", "rendered"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["traditional", "monochrome", "hand-drawn"]
            ),
            
            "pixel_art": StylePreset(
                id="pixel_art",
                name="Pixel Art",
                category=StyleCategory.ILLUSTRATION,
                description="Retro 8-bit and 16-bit game style",
                prompt_prefix="pixel art,",
                prompt_suffix="8-bit style, retro game aesthetic, limited color palette, crisp pixels, nostalgic, sprite art",
                negative_additions=["smooth", "realistic", "high resolution", "anti-aliased"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["retro", "gaming", "nostalgic"]
            ),
            
            "children_book": StylePreset(
                id="children_book",
                name="Children's Book Illustration",
                category=StyleCategory.ILLUSTRATION,
                description="Whimsical, friendly, storybook style",
                prompt_prefix="children's book illustration,",
                prompt_suffix="whimsical, friendly characters, soft colors, storybook aesthetic, warm and inviting, simple shapes",
                negative_additions=["scary", "dark", "violent", "complex"],
                recommended_models=["midjourney", "dalle3"],
                tags=["whimsical", "friendly", "colorful"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # 3D RENDER
            # ═══════════════════════════════════════════════════════════
            "3d_render": StylePreset(
                id="3d_render",
                name="3D Render",
                category=StyleCategory.RENDER_3D,
                description="Photorealistic 3D rendering",
                prompt_prefix="3D render,",
                prompt_suffix="octane render, ray tracing, realistic materials, subsurface scattering, global illumination, high quality CGI",
                negative_additions=["2d", "flat", "hand drawn", "sketch"],
                recommended_models=["midjourney", "flux"],
                tags=["cgi", "photorealistic", "technical"]
            ),
            
            "unreal_engine": StylePreset(
                id="unreal_engine",
                name="Unreal Engine",
                category=StyleCategory.RENDER_3D,
                description="Game engine cinematic quality",
                prompt_prefix="Unreal Engine 5,",
                prompt_suffix="nanite, lumen global illumination, photorealistic rendering, AAA game quality, real-time graphics",
                negative_additions=["low poly", "retro", "2d"],
                recommended_models=["midjourney"],
                tags=["gaming", "cinematic", "realistic"]
            ),
            
            "pixar": StylePreset(
                id="pixar",
                name="Pixar Style",
                category=StyleCategory.RENDER_3D,
                description="Animated movie 3D style",
                prompt_prefix="Pixar animation style,",
                prompt_suffix="3D animated, stylized characters, vibrant colors, Disney quality, expressive faces, subsurface scattering skin",
                negative_additions=["realistic", "dark", "gritty", "photographic"],
                recommended_models=["midjourney", "dalle3"],
                tags=["animated", "family-friendly", "stylized"]
            ),
            
            "low_poly": StylePreset(
                id="low_poly",
                name="Low Poly",
                category=StyleCategory.RENDER_3D,
                description="Geometric, faceted 3D style",
                prompt_prefix="low poly 3D,",
                prompt_suffix="geometric shapes, faceted surfaces, minimal polygons, clean aesthetic, isometric view, flat shading",
                negative_additions=["realistic", "detailed", "smooth", "organic"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["geometric", "minimal", "stylized"]
            ),
            
            "isometric": StylePreset(
                id="isometric",
                name="Isometric",
                category=StyleCategory.RENDER_3D,
                description="Isometric perspective view",
                prompt_prefix="isometric view,",
                prompt_suffix="isometric perspective, diorama style, miniature aesthetic, clean edges, game asset quality, 30 degree angle",
                negative_additions=["perspective distortion", "fisheye", "wide angle"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["technical", "clean", "game"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # EXPERIMENTAL / GENRE
            # ═══════════════════════════════════════════════════════════
            "cyberpunk": StylePreset(
                id="cyberpunk",
                name="Cyberpunk",
                category=StyleCategory.EXPERIMENTAL,
                description="Neon-lit dystopian future",
                prompt_prefix="cyberpunk,",
                prompt_suffix="neon lights, rain-slicked streets, holographic advertisements, dystopian future, Blade Runner inspired, high tech low life",
                negative_additions=["nature", "pastoral", "historical", "bright daylight"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["sci-fi", "neon", "dystopian"]
            ),
            
            "steampunk": StylePreset(
                id="steampunk",
                name="Steampunk",
                category=StyleCategory.EXPERIMENTAL,
                description="Victorian-era science fiction",
                prompt_prefix="steampunk,",
                prompt_suffix="brass and copper, gears and cogs, Victorian era, steam-powered machinery, goggles, clockwork mechanisms",
                negative_additions=["modern", "digital", "sleek", "minimal"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["victorian", "mechanical", "retro-futuristic"]
            ),
            
            "vaporwave": StylePreset(
                id="vaporwave",
                name="Vaporwave",
                category=StyleCategory.EXPERIMENTAL,
                description="Retro-futuristic aesthetic",
                prompt_prefix="vaporwave aesthetic,",
                prompt_suffix="pink and cyan gradient, greek statues, retro technology, palm trees, sunset gradient, glitch effects, 80s nostalgia",
                negative_additions=["natural", "realistic", "muted colors"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["retro", "aesthetic", "colorful"]
            ),
            
            "gothic": StylePreset(
                id="gothic",
                name="Gothic",
                category=StyleCategory.EXPERIMENTAL,
                description="Dark, ornate, medieval atmosphere",
                prompt_prefix="gothic art,",
                prompt_suffix="dark atmosphere, ornate architecture, medieval aesthetic, gargoyles, stained glass, moonlight, dramatic shadows",
                negative_additions=["bright", "cheerful", "modern", "minimal"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["dark", "medieval", "atmospheric"]
            ),
            
            "fantasy": StylePreset(
                id="fantasy",
                name="Epic Fantasy",
                category=StyleCategory.GENRE,
                description="Magical, epic fantasy world",
                prompt_prefix="epic fantasy,",
                prompt_suffix="magical atmosphere, ethereal lighting, mythical creatures, enchanted world, Lord of the Rings inspired, epic scale",
                negative_additions=["mundane", "realistic", "modern", "urban"],
                recommended_models=["midjourney", "stable-diffusion", "dalle3"],
                tags=["magical", "epic", "mythical"]
            ),
            
            "horror": StylePreset(
                id="horror",
                name="Horror",
                category=StyleCategory.GENRE,
                description="Dark, unsettling, frightening",
                prompt_prefix="horror atmosphere,",
                prompt_suffix="unsettling imagery, dark shadows, eerie lighting, ominous presence, psychological horror, Lovecraftian influence",
                negative_additions=["cheerful", "bright", "cute", "friendly"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["dark", "scary", "atmospheric"]
            ),
            
            "sci_fi": StylePreset(
                id="sci_fi",
                name="Science Fiction",
                category=StyleCategory.GENRE,
                description="Futuristic, technological",
                prompt_prefix="science fiction,",
                prompt_suffix="futuristic technology, space age, advanced civilization, sleek design, holographic displays, chrome and glass",
                negative_additions=["historical", "medieval", "rustic", "primitive"],
                recommended_models=["midjourney", "stable-diffusion", "flux"],
                tags=["futuristic", "technology", "space"]
            ),
            
            "western": StylePreset(
                id="western",
                name="Western",
                category=StyleCategory.GENRE,
                description="Wild West aesthetic",
                prompt_prefix="wild west,",
                prompt_suffix="dusty frontier, cowboy aesthetic, desert landscape, wooden saloons, sunset silhouettes, rugged terrain",
                negative_additions=["modern", "urban", "futuristic", "tropical"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["americana", "frontier", "rustic"]
            ),
            
            # ═══════════════════════════════════════════════════════════
            # CULTURAL
            # ═══════════════════════════════════════════════════════════
            "ukiyo_e": StylePreset(
                id="ukiyo_e",
                name="Ukiyo-e",
                category=StyleCategory.CULTURAL,
                description="Japanese woodblock print style",
                prompt_prefix="ukiyo-e style,",
                prompt_suffix="Japanese woodblock print, flat colors, bold outlines, Hokusai inspired, traditional Japanese art, wave patterns",
                negative_additions=["3d", "photographic", "western"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["japanese", "traditional", "artistic"]
            ),
            
            "art_deco": StylePreset(
                id="art_deco",
                name="Art Deco",
                category=StyleCategory.CULTURAL,
                description="1920s geometric glamour",
                prompt_prefix="art deco style,",
                prompt_suffix="geometric patterns, gold accents, 1920s glamour, symmetrical design, luxurious materials, Gatsby era aesthetic",
                negative_additions=["organic", "rustic", "minimal", "modern minimal"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["geometric", "luxurious", "vintage"]
            ),
            
            "pop_art": StylePreset(
                id="pop_art",
                name="Pop Art",
                category=StyleCategory.CULTURAL,
                description="Bold colors, comic-inspired",
                prompt_prefix="pop art style,",
                prompt_suffix="bold primary colors, comic book halftone, Andy Warhol inspired, graphic design, high contrast, Roy Lichtenstein dots",
                negative_additions=["muted", "subtle", "realistic", "dark"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["bold", "graphic", "colorful"]
            ),
            
            "minimalist": StylePreset(
                id="minimalist",
                name="Minimalist",
                category=StyleCategory.CULTURAL,
                description="Clean, simple, elegant",
                prompt_prefix="minimalist,",
                prompt_suffix="clean design, simple composition, negative space, elegant, less is more, monochromatic palette, essential elements only",
                negative_additions=["cluttered", "busy", "ornate", "detailed"],
                recommended_models=["midjourney", "dalle3", "flux"],
                tags=["clean", "simple", "modern"]
            ),
            
            "abstract": StylePreset(
                id="abstract",
                name="Abstract",
                category=StyleCategory.CULTURAL,
                description="Non-representational, shapes and colors",
                prompt_prefix="abstract art,",
                prompt_suffix="non-representational, shapes and colors, emotional expression, Kandinsky inspired, bold composition, modern art",
                negative_additions=["realistic", "figurative", "photographic"],
                recommended_models=["midjourney", "stable-diffusion"],
                tags=["modern", "artistic", "expressive"]
            ),
        }
        
        self.presets = presets
    
    def _load_custom_presets(self, presets_dir: str):
        """Загружает кастомные стили из директории"""
        
        path = Path(presets_dir)
        if not path.exists():
            return
        
        for file_path in path.glob("*.json"):
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                for preset_data in data.get("presets", []):
                    preset = StylePreset(
                        id=preset_data["id"],
                        name=preset_data["name"],
                        category=StyleCategory(preset_data.get("category", "experimental")),
                        description=preset_data.get("description", ""),
                        prompt_prefix=preset_data.get("prompt_prefix", ""),
                        prompt_suffix=preset_data.get("prompt_suffix", ""),
                        negative_additions=preset_data.get("negative_additions", []),
                        recommended_models=preset_data.get("recommended_models", []),
                        tags=preset_data.get("tags", [])
                    )
                    self.presets[preset.id] = preset
                    
            except Exception as e:
                print(f"Error loading preset file {file_path}: {e}")
    
    def apply_style(
        self,
        prompt: str,
        style_id: str,
        intensity: float = 1.0
    ) -> str:
        """
        Применяет стиль к промпту.
        
        Args:
            prompt: Исходный промпт
            style_id: ID стиля
            intensity: Интенсивность (0.0 - 1.0)
            
        Returns:
            Промпт со стилем
        """
        if style_id not in self.presets:
            return prompt
        
        style = self.presets[style_id]
        
        # Определяем сколько элементов стиля использовать
        suffix_parts = style.prompt_suffix.split(", ")
        
        if intensity >= 0.8:
            # Полное применение
            used_suffix = style.prompt_suffix
        elif intensity >= 0.5:
            # Частичное - берём ~60% элементов
            count = max(2, int(len(suffix_parts) * 0.6))
            used_suffix = ", ".join(suffix_parts[:count])
        elif intensity >= 0.3:
            # Минимальное - берём 2-3 элемента
            count = min(3, len(suffix_parts))
            used_suffix = ", ".join(suffix_parts[:count])
        else:
            # Очень лёгкое - только первый элемент
            used_suffix = suffix_parts[0] if suffix_parts else ""
        
        # Собираем результат
        parts = []
        
        if style.prompt_prefix:
            parts.append(style.prompt_prefix)
        
        parts.append(prompt)
        
        if used_suffix:
            parts.append(used_suffix)
        
        return " ".join(parts)
    
    def get_style(self, style_id: str) -> Optional[StylePreset]:
        """Получает стиль по ID"""
        return self.presets.get(style_id)
    
    def get_styles_by_category(self, category: StyleCategory) -> List[StylePreset]:
        """Получает стили по категории"""
        return [s for s in self.presets.values() if s.category == category]
    
    def get_all_styles(self) -> List[StylePreset]:
        """Получает все стили"""
        return list(self.presets.values())
    
    def get_categories(self) -> List[StyleCategory]:
        """Получает список категорий"""
        return list(StyleCategory)
    
    def search_styles(self, query: str) -> List[StylePreset]:
        """Поиск стилей по запросу"""
        
        query_lower = query.lower()
        results = []
        
        for style in self.presets.values():
            # Ищем в названии, описании и тегах
            if (query_lower in style.name.lower() or
                query_lower in style.description.lower() or
                any(query_lower in tag.lower() for tag in style.tags)):
                results.append(style)
        
        return results
    
    def combine_styles(
        self,
        prompt: str,
        style_ids: List[str],
        weights: Optional[List[float]] = None
    ) -> str:
        """
        Комбинирует несколько стилей.
        
        Args:
            prompt: Исходный промпт
            style_ids: Список ID стилей
            weights: Веса стилей (по умолчанию равные)
            
        Returns:
            Промпт с комбинированными стилями
        """
        if not style_ids:
            return prompt
        
        if weights is None:
            weights = [1.0 / len(style_ids)] * len(style_ids)
        
        # Нормализуем веса
        total_weight = sum(weights)
        weights = [w / total_weight for w in weights]
        
        combined_parts = []
        combined_prefix = []
        
        for style_id, weight in zip(style_ids, weights):
            if style_id not in self.presets or weight < 0.15:
                continue
            
            style = self.presets[style_id]
            
            # Берём элементы пропорционально весу
            suffix_parts = style.prompt_suffix.split(", ")
            num_parts = max(1, int(len(suffix_parts) * weight * 2))  # *2 для лучшего покрытия
            combined_parts.extend(suffix_parts[:num_parts])
            
            if style.prompt_prefix and weight > 0.3:
                combined_prefix.append(style.prompt_prefix)
        
        # Убираем дубликаты, сохраняя порядок
        seen = set()
        unique_parts = []
        for part in combined_parts:
            if part.lower() not in seen:
                seen.add(part.lower())
                unique_parts.append(part)
        
        # Собираем результат
        result_parts = []
        
        if combined_prefix:
            result_parts.append(" ".join(combined_prefix))
        
        result_parts.append(prompt)
        
        if unique_parts:
            result_parts.append(", ".join(unique_parts))
        
        return " ".join(result_parts)
    
    def get_negative_for_style(self, style_id: str) -> List[str]:
        """Возвращает негативы для стиля"""
        
        if style_id not in self.presets:
            return []
        
        return self.presets[style_id].negative_additions
    
    def get_recommended_models(self, style_id: str) -> List[str]:
        """Возвращает рекомендуемые модели для стиля"""
        
        if style_id not in self.presets:
            return ["midjourney", "stable-diffusion", "dalle3", "flux"]
        
        return self.presets[style_id].recommended_models or ["midjourney"]
    
    def to_dict(self) -> Dict[str, List[Dict]]:
        """Экспорт стилей в словарь по категориям"""
        
        result = {}
        
        for category in StyleCategory:
            styles = self.get_styles_by_category(category)
            result[category.value] = [s.to_dict() for s in styles]
        
        return result