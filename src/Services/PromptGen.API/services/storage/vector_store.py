# services/storage/vector_store.py
"""
Векторное хранилище для семантического поиска.

Поддерживает бэкенды:
- In-memory (для разработки и тестов)
- ChromaDB (локальное векторное хранилище) - опционально
- Qdrant (распределённое хранилище) - опционально
"""

import json
import uuid
import hashlib
from typing import Dict, List, Optional, Any, Union
from dataclasses import dataclass, field
from datetime import datetime
import numpy as np

from app.config import settings


# Опциональные импорты ChromaDB
try:
    import chromadb
    from chromadb.config import Settings as ChromaSettings
    CHROMA_AVAILABLE = True
except ImportError:
    chromadb = None  # type: ignore
    ChromaSettings = None  # type: ignore
    CHROMA_AVAILABLE = False

# Опциональные импорты Qdrant
try:
    from qdrant_client import QdrantClient
    from qdrant_client.models import (
        VectorParams, 
        Distance, 
        PointStruct,
        Filter,
        FieldCondition,
        MatchValue,
        PointIdsList
    )
    QDRANT_AVAILABLE = True
except ImportError:
    QdrantClient = None  # type: ignore
    VectorParams = None  # type: ignore
    Distance = None  # type: ignore
    PointStruct = None  # type: ignore
    Filter = None  # type: ignore
    FieldCondition = None  # type: ignore
    MatchValue = None  # type: ignore
    PointIdsList = None  # type: ignore
    QDRANT_AVAILABLE = False


@dataclass
class VectorDocument:
    """Документ в векторном хранилище"""
    id: str
    content: str
    vector: List[float]
    metadata: Dict[str, Any] = field(default_factory=dict)
    created_at: datetime = field(default_factory=datetime.utcnow)


@dataclass
class SearchResult:
    """Результат поиска"""
    id: str
    score: float
    content: str
    metadata: Dict[str, Any]
    vector: Optional[List[float]] = None


class VectorStore:
    """
    Универсальное векторное хранилище.
    
    Поддерживает несколько бэкендов с единым интерфейсом.
    По умолчанию использует in-memory хранилище.
    """
    
    def __init__(
        self, 
        backend: str = "memory",
        embedding_dimension: int = 384
    ):
        """
        Инициализация хранилища.
        
        Args:
            backend: Тип бэкенда (memory, chroma, qdrant)
            embedding_dimension: Размерность векторов
        """
        self.backend = backend
        self.dimension = embedding_dimension
        self._collections: Dict[str, Dict[str, VectorDocument]] = {}
        self._client: Any = None
        
        if backend == "chroma":
            self._init_chroma()
        elif backend == "qdrant":
            self._init_qdrant()
        # memory работает без инициализации
    
    def _init_chroma(self) -> None:
        """Инициализация ChromaDB"""
        if not CHROMA_AVAILABLE or chromadb is None:
            print("ChromaDB not installed, falling back to memory backend")
            self.backend = "memory"
            return
        
        try:
            persist_dir = getattr(settings, 'CHROMA_PERSIST_DIR', './storage/chroma')
            
            self._client = chromadb.Client(ChromaSettings(
                chroma_db_impl="duckdb+parquet",
                persist_directory=persist_dir,
                anonymized_telemetry=False
            ))
            print(f"ChromaDB initialized at {persist_dir}")
        except Exception as e:
            print(f"ChromaDB init error: {e}, falling back to memory")
            self.backend = "memory"
    
    def _init_qdrant(self) -> None:
        """Инициализация Qdrant"""
        if not QDRANT_AVAILABLE or QdrantClient is None:
            print("Qdrant client not installed, falling back to memory backend")
            self.backend = "memory"
            return
        
        try:
            host = getattr(settings, 'QDRANT_HOST', 'localhost')
            port = getattr(settings, 'QDRANT_PORT', 6333)
            
            self._client = QdrantClient(host=host, port=port)
            print(f"Qdrant connected at {host}:{port}")
        except Exception as e:
            print(f"Qdrant connection error: {e}, falling back to memory")
            self.backend = "memory"
    
    async def create_collection(
        self,
        name: str,
        dimension: Optional[int] = None
    ) -> bool:
        """Создаёт коллекцию."""
        dim = dimension or self.dimension
        
        if self.backend == "memory":
            if name not in self._collections:
                self._collections[name] = {}
            return True
        
        elif self.backend == "chroma" and self._client is not None:
            try:
                self._client.get_or_create_collection(
                    name=name,
                    metadata={"dimension": dim}
                )
                return True
            except Exception as e:
                print(f"ChromaDB create collection error: {e}")
                return False
        
        elif self.backend == "qdrant" and self._client is not None and VectorParams is not None:
            try:
                # Проверяем существование
                collections = self._client.get_collections().collections
                exists = any(c.name == name for c in collections)
                
                if not exists:
                    self._client.create_collection(
                        collection_name=name,
                        vectors_config=VectorParams(
                            size=dim,
                            distance=Distance.COSINE
                        )
                    )
                return True
            except Exception as e:
                print(f"Qdrant create collection error: {e}")
                return False
        
        return False
    
    async def delete_collection(self, name: str) -> bool:
        """Удаляет коллекцию"""
        
        if self.backend == "memory":
            if name in self._collections:
                del self._collections[name]
            return True
        
        elif self.backend == "chroma" and self._client is not None:
            try:
                self._client.delete_collection(name)
                return True
            except Exception:
                return False
        
        elif self.backend == "qdrant" and self._client is not None:
            try:
                self._client.delete_collection(name)
                return True
            except Exception:
                return False
        
        return False
    
    async def insert(
        self,
        collection: str,
        id: str,
        content: str,
        vector: List[float],
        metadata: Optional[Dict[str, Any]] = None
    ) -> bool:
        """Вставляет документ в коллекцию."""
        metadata = metadata or {}
        
        # Создаём коллекцию если не существует
        await self.create_collection(collection, len(vector))
        
        if self.backend == "memory":
            self._collections[collection][id] = VectorDocument(
                id=id,
                content=content,
                vector=vector,
                metadata=metadata
            )
            return True
        
        elif self.backend == "chroma" and self._client is not None:
            try:
                coll = self._client.get_collection(collection)
                coll.upsert(
                    ids=[id],
                    embeddings=[vector],
                    metadatas=[{**metadata, "content": content}],
                    documents=[content]
                )
                return True
            except Exception as e:
                print(f"ChromaDB insert error: {e}")
                return False
        
        elif self.backend == "qdrant" and self._client is not None and PointStruct is not None:
            try:
                self._client.upsert(
                    collection_name=collection,
                    points=[PointStruct(
                        id=id if isinstance(id, int) else hash(id) % (2**63),
                        vector=vector,
                        payload={**metadata, "content": content, "original_id": id}
                    )]
                )
                return True
            except Exception as e:
                print(f"Qdrant insert error: {e}")
                return False
        
        return False
    
    async def insert_many(
        self,
        collection: str,
        documents: List[Dict[str, Any]]
    ) -> int:
        """Пакетная вставка документов."""
        success_count = 0
        
        for doc in documents:
            result = await self.insert(
                collection=collection,
                id=doc["id"],
                content=doc["content"],
                vector=doc["vector"],
                metadata=doc.get("metadata")
            )
            if result:
                success_count += 1
        
        return success_count
    
    async def search(
        self,
        collection: str,
        query_vector: List[float],
        limit: int = 10,
        filter: Optional[Dict[str, Any]] = None,
        min_score: float = 0.0
    ) -> List[SearchResult]:
        """Поиск похожих документов."""
        if self.backend == "memory":
            return await self._memory_search(
                collection, query_vector, limit, filter, min_score
            )
        
        elif self.backend == "chroma":
            return await self._chroma_search(
                collection, query_vector, limit, filter, min_score
            )
        
        elif self.backend == "qdrant":
            return await self._qdrant_search(
                collection, query_vector, limit, filter, min_score
            )
        
        return []
    
    async def _memory_search(
        self,
        collection: str,
        query_vector: List[float],
        limit: int,
        filter_dict: Optional[Dict[str, Any]],
        min_score: float
    ) -> List[SearchResult]:
        """In-memory поиск с косинусным сходством"""
        
        if collection not in self._collections:
            return []
        
        docs = self._collections[collection]
        if not docs:
            return []
        
        query_np = np.array(query_vector)
        query_norm = np.linalg.norm(query_np)
        
        if query_norm == 0:
            return []
        
        results: List[SearchResult] = []
        
        for doc_id, doc in docs.items():
            # Применяем фильтр
            if filter_dict:
                match = all(
                    doc.metadata.get(k) == v 
                    for k, v in filter_dict.items()
                )
                if not match:
                    continue
            
            # Вычисляем косинусное сходство
            doc_np = np.array(doc.vector)
            doc_norm = np.linalg.norm(doc_np)
            
            if doc_norm == 0:
                continue
            
            similarity = float(np.dot(query_np, doc_np) / (query_norm * doc_norm))
            
            if similarity >= min_score:
                results.append(SearchResult(
                    id=doc.id,
                    score=similarity,
                    content=doc.content,
                    metadata=doc.metadata,
                    vector=doc.vector
                ))
        
        # Сортируем по score
        results.sort(key=lambda x: x.score, reverse=True)
        
        return results[:limit]
    
    async def _chroma_search(
        self,
        collection: str,
        query_vector: List[float],
        limit: int,
        filter_dict: Optional[Dict[str, Any]],
        min_score: float
    ) -> List[SearchResult]:
        """ChromaDB поиск"""
        
        if self._client is None:
            return []
        
        try:
            coll = self._client.get_collection(collection)
            
            where = filter_dict if filter_dict else None
            
            results = coll.query(
                query_embeddings=[query_vector],
                n_results=limit,
                where=where,
                include=["documents", "metadatas", "distances"]
            )
            
            search_results: List[SearchResult] = []
            
            if results['ids'] and results['ids'][0]:
                for i, doc_id in enumerate(results['ids'][0]):
                    # ChromaDB возвращает distance, конвертируем в similarity
                    distance = results['distances'][0][i] if results['distances'] else 0
                    similarity = 1 - distance  # Для косинусного расстояния
                    
                    if similarity >= min_score:
                        metadata = results['metadatas'][0][i] if results['metadatas'] else {}
                        content = results['documents'][0][i] if results['documents'] else ""
                        
                        search_results.append(SearchResult(
                            id=doc_id,
                            score=similarity,
                            content=content,
                            metadata=metadata
                        ))
            
            return search_results
            
        except Exception as e:
            print(f"ChromaDB search error: {e}")
            return []
    
    async def _qdrant_search(
        self,
        collection: str,
        query_vector: List[float],
        limit: int,
        filter_dict: Optional[Dict[str, Any]],
        min_score: float
    ) -> List[SearchResult]:
        """Qdrant поиск"""
        
        if self._client is None or Filter is None:
            return []
        
        try:
            # Конвертируем фильтр
            qdrant_filter = None
            if filter_dict and FieldCondition is not None and MatchValue is not None:
                conditions = [
                    FieldCondition(key=k, match=MatchValue(value=v))
                    for k, v in filter_dict.items()
                ]
                qdrant_filter = Filter(must=conditions)
            
            results = self._client.search(
                collection_name=collection,
                query_vector=query_vector,
                limit=limit,
                query_filter=qdrant_filter,
                score_threshold=min_score
            )
            
            return [
                SearchResult(
                    id=str(r.payload.get("original_id", r.id)),
                    score=r.score,
                    content=r.payload.get("content", ""),
                    metadata={k: v for k, v in r.payload.items() 
                             if k not in ["content", "original_id"]}
                )
                for r in results
            ]
            
        except Exception as e:
            print(f"Qdrant search error: {e}")
            return []
    
    async def get(
        self,
        collection: str,
        id: str
    ) -> Optional[VectorDocument]:
        """Получает документ по ID"""
        
        if self.backend == "memory":
            if collection in self._collections:
                return self._collections[collection].get(id)
            return None
        
        elif self.backend == "chroma" and self._client is not None:
            try:
                coll = self._client.get_collection(collection)
                result = coll.get(ids=[id], include=["embeddings", "metadatas", "documents"])
                
                if result['ids']:
                    return VectorDocument(
                        id=result['ids'][0],
                        content=result['documents'][0] if result['documents'] else "",
                        vector=result['embeddings'][0] if result['embeddings'] else [],
                        metadata=result['metadatas'][0] if result['metadatas'] else {}
                    )
                return None
            except Exception:
                return None
        
        elif self.backend == "qdrant" and self._client is not None:
            try:
                results = self._client.retrieve(
                    collection_name=collection,
                    ids=[hash(id) % (2**63)],
                    with_vectors=True
                )
                
                if results:
                    r = results[0]
                    return VectorDocument(
                        id=str(r.payload.get("original_id", r.id)),
                        content=r.payload.get("content", ""),
                        vector=list(r.vector) if r.vector else [],
                        metadata={k: v for k, v in r.payload.items() 
                                 if k not in ["content", "original_id"]}
                    )
                return None
            except Exception:
                return None
        
        return None
    
    async def delete(
        self,
        collection: str,
        id: str
    ) -> bool:
        """Удаляет документ"""
        
        if self.backend == "memory":
            if collection in self._collections:
                if id in self._collections[collection]:
                    del self._collections[collection][id]
                    return True
            return False
        
        elif self.backend == "chroma" and self._client is not None:
            try:
                coll = self._client.get_collection(collection)
                coll.delete(ids=[id])
                return True
            except Exception:
                return False
        
        elif self.backend == "qdrant" and self._client is not None and PointIdsList is not None:
            try:
                self._client.delete(
                    collection_name=collection,
                    points_selector=PointIdsList(
                        points=[hash(id) % (2**63)]
                    )
                )
                return True
            except Exception:
                return False
        
        return False
    
    async def update(
        self,
        collection: str,
        id: str,
        content: Optional[str] = None,
        vector: Optional[List[float]] = None,
        metadata: Optional[Dict[str, Any]] = None
    ) -> bool:
        """Обновляет документ"""
        
        # Получаем существующий документ
        existing = await self.get(collection, id)
        if not existing:
            return False
        
        # Мержим данные
        new_content = content if content is not None else existing.content
        new_vector = vector if vector is not None else existing.vector
        new_metadata = {**existing.metadata, **(metadata or {})}
        
        # Вставляем обновлённый (upsert)
        return await self.insert(
            collection=collection,
            id=id,
            content=new_content,
            vector=new_vector,
            metadata=new_metadata
        )
    
    async def count(self, collection: str) -> int:
        """Возвращает количество документов в коллекции"""
        
        if self.backend == "memory":
            return len(self._collections.get(collection, {}))
        
        elif self.backend == "chroma" and self._client is not None:
            try:
                coll = self._client.get_collection(collection)
                return coll.count()
            except Exception:
                return 0
        
        elif self.backend == "qdrant" and self._client is not None:
            try:
                info = self._client.get_collection(collection)
                return info.points_count
            except Exception:
                return 0
        
        return 0
    
    async def list_collections(self) -> List[str]:
        """Возвращает список коллекций"""
        
        if self.backend == "memory":
            return list(self._collections.keys())
        
        elif self.backend == "chroma" and self._client is not None:
            try:
                collections = self._client.list_collections()
                return [c.name for c in collections]
            except Exception:
                return []
        
        elif self.backend == "qdrant" and self._client is not None:
            try:
                collections = self._client.get_collections().collections
                return [c.name for c in collections]
            except Exception:
                return []
        
        return []
    
    async def get_stats(self, collection: str) -> Dict[str, Any]:
        """Статистика коллекции"""
        
        count = await self.count(collection)
        
        return {
            "collection": collection,
            "backend": self.backend,
            "count": count,
            "dimension": self.dimension
        }