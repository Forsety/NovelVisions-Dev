import aiohttp
from typing import Optional, Dict, Any, List

class LocalLLMService:
    def __init__(self, base_url: str = "http://localhost:11434"):
        self.base_url = base_url
        self.model = "llama2"

    async def generate(
        self,
        prompt: str,
        model: Optional[str] = None,
        max_tokens: int = 500,
        temperature: float = 0.7,
    ) -> str:
        async with aiohttp.ClientSession() as session:
            payload = {
                "model": model or self.model,
                "prompt": prompt,
                "options": {"num_predict": max_tokens, "temperature": temperature},
            }
            async with session.post(f"{self.base_url}/api/generate", json=payload) as r:
                if r.status == 200:
                    data = await r.json()
                    return data.get("response", "")
                raise Exception(f"Local LLM error: {r.status}")

    async def list_models(self) -> List[str]:
        async with aiohttp.ClientSession() as session:
            async with session.get(f"{self.base_url}/api/tags") as r:
                if r.status == 200:
                    data = await r.json()
                    return [m["name"] for m in data.get("models", [])]
                return []
