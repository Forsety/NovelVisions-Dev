import aiohttp
import base64
from typing import Dict, Optional, List, Any


class ImageAPIClient:
    """Client for communicating with Image Generation API"""
    
    def __init__(self, base_url: str = "http://localhost:8002"):
        self.base_url = base_url
        self.session = None
    
    async def __aenter__(self):
        self.session = aiohttp.ClientSession()
        return self
    
    async def __aexit__(self, exc_type, exc_val, exc_tb):
        if self.session:
            await self.session.close()
    
    async def generate_image(
        self,
        prompt: str,
        model: str = "dalle3",
        parameters: Optional[Dict] = None
    ) -> Optional[Dict]:
        """Request image generation"""
        
        if not self.session:
            self.session = aiohttp.ClientSession()
        
        payload = {
            "prompt": prompt,
            "model": model,
            "parameters": parameters or {}
        }
        
        try:
            async with self.session.post(
                f"{self.base_url}/api/v1/generate",
                json=payload,
                timeout=aiohttp.ClientTimeout(total=300)
            ) as response:
                if response.status == 200:
                    data = await response.json()
                    return data.get("data")
                return None
        except Exception as e:
            print(f"Error generating image: {e}")
            return None
    
    async def get_generation_status(
        self,
        job_id: str
    ) -> Optional[Dict]:
        """Check image generation status"""
        
        if not self.session:
            self.session = aiohttp.ClientSession()
        
        try:
            async with self.session.get(
                f"{self.base_url}/api/v1/status/{job_id}"
            ) as response:
                if response.status == 200:
                    data = await response.json()
                    return data.get("data")
                return None
        except Exception as e:
            print(f"Error checking status: {e}")
            return None
    
    async def get_image(self, image_id: str) -> Optional[bytes]:
        """Get generated image"""
        
        if not self.session:
            self.session = aiohttp.ClientSession()
        
        try:
            async with self.session.get(
                f"{self.base_url}/api/v1/images/{image_id}"
            ) as response:
                if response.status == 200:
                    return await response.read()
                return None
        except Exception as e:
            print(f"Error fetching image: {e}")
            return None
