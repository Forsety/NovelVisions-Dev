import aiohttp
import hmac
import hashlib
import json
from typing import Dict, Optional, List, Any
from datetime import datetime


class WebhookService:
    """Service for webhook notifications"""
    
    def __init__(self, secret_key: Optional[str] = None):
        self.secret_key = secret_key
        self.session = None
    
    async def send_webhook(
        self,
        url: str,
        event: str,
        data: Dict,
        headers: Optional[Dict] = None
    ) -> bool:
        """Send webhook notification"""
        
        if not self.session:
            self.session = aiohttp.ClientSession()
        
        payload = {
            "event": event,
            "timestamp": datetime.utcnow().isoformat(),
            "data": data
        }
        
        # Add signature if secret key is configured
        request_headers = headers or {}
        if self.secret_key:
            signature = self._generate_signature(json.dumps(payload))
            request_headers["X-Webhook-Signature"] = signature
        
        try:
            async with self.session.post(
                url,
                json=payload,
                headers=request_headers,
                timeout=aiohttp.ClientTimeout(total=30)
            ) as response:
                return response.status in [200, 201, 202, 204]
        except Exception as e:
            print(f"Webhook error: {e}")
            return False
    
    def _generate_signature(self, payload: str) -> str:
        """Generate HMAC signature for payload"""
        
        if not self.secret_key:
            return ""
        
        signature = hmac.new(
            self.secret_key.encode(),
            payload.encode(),
            hashlib.sha256
        ).hexdigest()
        
        return f"sha256={signature}"
    
    async def notify_prompt_generated(
        self,
        webhook_url: str,
        prompt_data: Dict
    ):
        """Notify when prompt is generated"""
        
        await self.send_webhook(
            url=webhook_url,
            event="prompt.generated",
            data=prompt_data
        )
    
    async def notify_character_created(
        self,
        webhook_url: str,
        character_data: Dict
    ):
        """Notify when character is created"""
        
        await self.send_webhook(
            url=webhook_url,
            event="character.created",
            data=character_data
        )
    
    async def notify_story_analyzed(
        self,
        webhook_url: str,
        analysis_data: Dict
    ):
        """Notify when story is analyzed"""
        
        await self.send_webhook(
            url=webhook_url,
            event="story.analyzed",
            data=analysis_data
        )
    
    async def close(self):
        """Close session"""
        
        if self.session:
            await self.session.close()
            self.session = None
