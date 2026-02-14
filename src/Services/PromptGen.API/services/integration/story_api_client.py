import aiohttp
from typing import Dict, Optional, List, Any


class StoryAPIClient:
    """Client for communicating with Story Management API"""
    
    def __init__(self, base_url: str = "http://localhost:8001"):
        self.base_url = base_url
        self.session = None
    
    async def __aenter__(self):
        self.session = aiohttp.ClientSession()
        return self
    
    async def __aexit__(self, exc_type, exc_val, exc_tb):
        if self.session:
            await self.session.close()
    
    async def get_story(self, story_id: str) -> Optional[Dict]:
        """Get story details"""
        
        if not self.session:
            self.session = aiohttp.ClientSession()
        
        try:
            async with self.session.get(
                f"{self.base_url}/api/v1/stories/{story_id}"
            ) as response:
                if response.status == 200:
                    data = await response.json()
                    return data.get("data")
                return None
        except Exception as e:
            print(f"Error fetching story: {e}")
            return None
    
    async def get_story_page(
        self,
        story_id: str,
        page_number: int
    ) -> Optional[Dict]:
        """Get specific page of story"""
        
        if not self.session:
            self.session = aiohttp.ClientSession()
        
        try:
            async with self.session.get(
                f"{self.base_url}/api/v1/stories/{story_id}/pages/{page_number}"
            ) as response:
                if response.status == 200:
                    data = await response.json()
                    return data.get("data")
                return None
        except Exception as e:
            print(f"Error fetching story page: {e}")
            return None
    
    async def update_story_metadata(
        self,
        story_id: str,
        metadata: Dict
    ) -> bool:
        """Update story metadata"""
        
        if not self.session:
            self.session = aiohttp.ClientSession()
        
        try:
            async with self.session.patch(
                f"{self.base_url}/api/v1/stories/{story_id}/metadata",
                json=metadata
            ) as response:
                return response.status == 200
        except Exception:
            return False

