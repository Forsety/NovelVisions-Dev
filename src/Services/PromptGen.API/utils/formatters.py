from typing import Any, Dict, List, Optional
from datetime import datetime
import json


class Formatters:
    """Collection of formatting functions"""
    
    @staticmethod
    def format_datetime(dt: datetime, format: str = "%Y-%m-%d %H:%M:%S") -> str:
        """Format datetime to string"""
        return dt.strftime(format) if dt else ""
    
    @staticmethod
    def format_prompt_for_display(prompt: str, max_length: int = 100) -> str:
        """Format prompt for display (truncate if needed)"""
        if len(prompt) <= max_length:
            return prompt
        return prompt[:max_length-3] + "..."
    
    @staticmethod
    def format_model_parameters(params: Dict[str, Any], model: str) -> str:
        """Format model parameters for display"""
        if model == "midjourney":
            parts = []
            if "ar" in params:
                parts.append(f"--ar {params['ar']}")
            if "v" in params:
                parts.append(f"--v {params['v']}")
            if "q" in params:
                parts.append(f"--q {params['q']}")
            return " ".join(parts)
        
        elif model == "stable-diffusion":
            parts = []
            if "steps" in params:
                parts.append(f"Steps: {params['steps']}")
            if "cfg_scale" in params:
                parts.append(f"CFG: {params['cfg_scale']}")
            return ", ".join(parts)
        
        return json.dumps(params, indent=2)
    
    @staticmethod
    def format_file_size(size_bytes: int) -> str:
        """Format file size for display"""
        for unit in ['B', 'KB', 'MB', 'GB']:
            if size_bytes < 1024.0:
                return f"{size_bytes:.2f} {unit}"
            size_bytes /= 1024.0
        return f"{size_bytes:.2f} TB"