import re
from typing import Dict, Any, Optional, List, Tuple
from jinja2 import Template, Environment, FileSystemLoader, select_autoescape

class TemplateEngine:
    def __init__(self, template_dir: str = "templates"):
        self.env = Environment(
            loader=FileSystemLoader(template_dir),
            autoescape=select_autoescape(["html", "xml"]),
            trim_blocks=True,
            lstrip_blocks=True,
        )

    def render(self, template_name: str, context: Dict[str, Any]) -> str:
        return self.env.get_template(template_name).render(**context)

    def render_string(self, template_str: str, context: Dict[str, Any]) -> str:
        return Template(template_str).render(**context)

    def create_prompt_template(
        self,
        base_template: str,
        style_template: Optional[str] = None,
        model_template: Optional[str] = None,
    ) -> str:
        parts = [base_template]
        if style_template:
            parts.append(style_template)
        if model_template:
            parts.append(model_template)
        return "\n".join(parts)

    def extract_variables(self, template_str: str) -> List[str]:
        pattern = r"\{\{\s*(\w+)\s*\}\}"
        return list(set(re.findall(pattern, template_str)))

    def validate_template(self, template_str: str) -> Tuple[bool, Optional[str]]:
        try:
            Template(template_str).render()
            return True, None
        except Exception as e:
            return False, str(e)
