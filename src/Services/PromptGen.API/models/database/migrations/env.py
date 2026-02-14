# models/database/migrations/env.py
"""
Alembic migrations environment.
Поддержка SQLite, PostgreSQL, SQL Server.
"""

import asyncio
import sys
import os
from logging.config import fileConfig

# Добавляем корень проекта в путь
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))))

from sqlalchemy import pool
from sqlalchemy.engine import Connection
from sqlalchemy.ext.asyncio import create_async_engine
from alembic import context

# Импорт настроек
from app.config import settings

# Импорт Base - ТОЛЬКО Base, без других импортов из этого модуля
from sqlalchemy.orm import DeclarativeBase


class Base(DeclarativeBase):
    """Временный Base для миграций"""
    pass


# Alembic Config object
config = context.config

# Interpret the config file for Python logging
if config.config_file_name is not None:
    fileConfig(config.config_file_name)

# ===========================================
# Import all models AFTER Base is defined
# ===========================================
try:
    from models.domain.character import Character
    from models.domain.scene import Scene
    from models.domain.story_object import StoryObject
    from models.domain.prompt_history import PromptHistory
    
    # Получаем metadata из реального Base
    from models.database.base import Base as RealBase
    target_metadata = RealBase.metadata
except ImportError as e:
    print(f"Warning: Could not import models: {e}")
    target_metadata = Base.metadata


def get_database_url() -> str:
    """Получает URL базы данных"""
    
    if settings.DATABASE_URL:
        url = settings.DATABASE_URL
        
        # Конвертируем sync URL в async
        if url.startswith("postgresql://"):
            return url.replace("postgresql://", "postgresql+asyncpg://")
        elif url.startswith("sqlite:///") and "+aiosqlite" not in url:
            return url.replace("sqlite:///", "sqlite+aiosqlite:///")
        
        return url
    
    # Default SQLite
    return "sqlite+aiosqlite:///./data/promptgen.db"


def run_migrations_offline() -> None:
    """Run migrations in 'offline' mode."""
    url = get_database_url()
    
    # Для offline используем sync драйвер
    if "+aiosqlite" in url:
        url = url.replace("+aiosqlite", "")
    elif "+asyncpg" in url:
        url = url.replace("+asyncpg", "")
    
    context.configure(
        url=url,
        target_metadata=target_metadata,
        literal_binds=True,
        dialect_opts={"paramstyle": "named"},
        render_as_batch=True,  # Важно для SQLite
    )

    with context.begin_transaction():
        context.run_migrations()


def do_run_migrations(connection: Connection) -> None:
    """Run migrations with the given connection."""
    context.configure(
        connection=connection, 
        target_metadata=target_metadata,
        compare_type=True,
        compare_server_default=True,
        render_as_batch=True,  # Важно для SQLite
    )

    with context.begin_transaction():
        context.run_migrations()


async def run_async_migrations() -> None:
    """Run migrations in 'online' mode with async engine."""
    
    database_url = get_database_url()
    is_sqlite = "sqlite" in database_url
    
    if is_sqlite:
        from sqlalchemy.pool import StaticPool
        connectable = create_async_engine(
            database_url,
            poolclass=StaticPool,
            connect_args={"check_same_thread": False}
        )
    else:
        connectable = create_async_engine(
            database_url,
            poolclass=pool.NullPool,
        )

    async with connectable.connect() as connection:
        await connection.run_sync(do_run_migrations)

    await connectable.dispose()


def run_migrations_online() -> None:
    """Run migrations in 'online' mode."""
    asyncio.run(run_async_migrations())


if context.is_offline_mode():
    run_migrations_offline()
else:
    run_migrations_online()