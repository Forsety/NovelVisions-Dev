SUPPORTED_MODELS = ["midjourney", "dalle3", "stable-diffusion", "flux"]

DEFAULT_MODEL = "midjourney"

MODEL_MAX_LENGTHS = {
    "midjourney": 6000,
    "dalle3": 4000,
    "stable-diffusion": 380,
    "flux": 1000
}

# Style constants
DEFAULT_STYLES = [
    "anime", "realistic", "fantasy", "noir",
    "watercolor", "oil_painting", "cyberpunk", "steampunk"
]

# Quality levels
QUALITY_LEVELS = ["low", "medium", "high", "ultra"]

# Aspect ratios
ASPECT_RATIOS = {
    "square": "1:1",
    "portrait": "2:3",
    "landscape": "3:2",
    "wide": "16:9",
    "tall": "9:16",
    "ultrawide": "21:9"
}

# Cache TTL values (in seconds)
CACHE_TTL = {
    "prompt": 3600,      # 1 hour
    "character": 7200,   # 2 hours
    "scene": 7200,       # 2 hours
    "story": 86400,      # 24 hours
    "style": 86400       # 24 hours
}

# Rate limits
RATE_LIMITS = {
    "free": {
        "prompts_per_day": 50,
        "characters_per_month": 10,
        "stories_per_month": 5
    },
    "premium": {
        "prompts_per_day": 500,
        "characters_per_month": 100,
        "stories_per_month": 50
    }
}
