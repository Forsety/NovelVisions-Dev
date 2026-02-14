import hashlib
import secrets
import string
import re
from typing import Optional, Any, Dict, List
from datetime import timedelta

class Helpers:
    @staticmethod
    def generate_id() -> str:
        import uuid
        return str(uuid.uuid4())

    @staticmethod
    def generate_token(length: int = 32) -> str:
        return secrets.token_urlsafe(length)

    @staticmethod
    def generate_password(length: int = 16) -> str:
        alphabet = string.ascii_letters + string.digits + string.punctuation
        return "".join(secrets.choice(alphabet) for _ in range(length))

    @staticmethod
    def hash_string(text: str) -> str:
        return hashlib.sha256(text.encode()).hexdigest()

    @staticmethod
    def calculate_reading_time(text: str, wpm: int = 200) -> int:
        return max(1, len(text.split()) // wpm)

    @staticmethod
    def parse_time_delta(s: str) -> timedelta:
        units = {"s": "seconds", "m": "minutes", "h": "hours", "d": "days", "w": "weeks"}
        m = re.match(r"(\d+)([smhdw])", s.lower())
        return timedelta(**{units[m.group(2)]: int(m.group(1))}) if m else timedelta(0)
