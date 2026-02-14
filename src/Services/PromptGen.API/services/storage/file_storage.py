import os
import aiofiles
import hashlib
from typing import Optional, List
from pathlib import Path

class FileStorageService:
    def __init__(self, base_path: str = "storage"):
        self.base_path = Path(base_path)
        self.base_path.mkdir(parents=True, exist_ok=True)

    async def save(self, file_content: bytes, filename: str, folder: Optional[str] = None) -> str:
        folder_path = self.base_path / folder if folder else self.base_path
        folder_path.mkdir(parents=True, exist_ok=True)
        file_hash = hashlib.md5(file_content).hexdigest()
        unique = f"{file_hash}{Path(filename).suffix}"
        path = folder_path / unique
        async with aiofiles.open(path, "wb") as f:
            await f.write(file_content)
        return str(path.relative_to(self.base_path))

    async def read(self, file_path: str) -> Optional[bytes]:
        path = self.base_path / file_path
        if not path.exists():
            return None
        async with aiofiles.open(path, "rb") as f:
            return await f.read()

    async def delete(self, file_path: str) -> bool:
        path = self.base_path / file_path
        if path.exists():
            path.unlink()
            return True
        return False

    def get_url(self, file_path: str, base_url: str = "/storage") -> str:
        return f"{base_url}/{file_path}"

    async def list_files(self, folder: Optional[str] = None) -> List[str]:
        path = self.base_path / folder if folder else self.base_path
        if not path.exists():
            return []
        return [
            str(p.relative_to(self.base_path))
            for p in path.iterdir()
            if p.is_file()
        ]
