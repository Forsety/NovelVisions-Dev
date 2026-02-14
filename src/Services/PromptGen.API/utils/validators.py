import re
from typing import Optional, List, Dict, Any


class Validators:
    """Collection of validation functions"""
    
    @staticmethod
    def validate_email(email: str) -> bool:
        """Validate email format"""
        pattern = r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'
        return re.match(pattern, email) is not None
    
    @staticmethod
    def validate_username(username: str) -> bool:
        """Validate username format"""
        # 3-20 characters, alphanumeric and underscore only
        pattern = r'^[a-zA-Z0-9_]{3,20}$'
        return re.match(pattern, username) is not None
    
    @staticmethod
    def validate_uuid(uuid_str: str) -> bool:
        """Validate UUID format"""
        pattern = r'^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$'
        return re.match(pattern, uuid_str.lower()) is not None
    
    @staticmethod
    def validate_prompt_length(prompt: str, max_length: int = 2000) -> bool:
        """Validate prompt length"""
        return 0 < len(prompt) <= max_length
    
    @staticmethod
    def validate_model_name(model: str) -> bool:
        """Validate AI model name"""
        allowed_models = ["midjourney", "dalle3", "stable-diffusion", "flux"]
        return model in allowed_models
    
    @staticmethod
    def sanitize_filename(filename: str) -> str:
        """Sanitize filename for safe storage"""
        # Remove or replace invalid characters
        sanitized = re.sub(r'[<>:"/\\|?*]', '_', filename)
        # Limit length
        name, ext = sanitized.rsplit('.', 1) if '.' in sanitized else (sanitized, '')
        if len(name) > 100:
            name = name[:100]
        return f"{name}.{ext}" if ext else name
