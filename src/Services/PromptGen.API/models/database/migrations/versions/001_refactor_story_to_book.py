# models/database/migrations/versions/001_refactor_story_to_book.py
"""
Refactor story_id to book_id

Revision ID: 001_refactor_story_to_book
Revises: 
Create Date: 2025-01-15

Этот migration:
1. Переименовывает story_id → book_id во всех таблицах
2. Удаляет FK constraints на stories
3. Удаляет таблицы users и stories
4. Добавляет новые индексы
"""

from alembic import op
import sqlalchemy as sa
from sqlalchemy.dialects import postgresql

# revision identifiers
revision = '001_refactor_story_to_book'
down_revision = None  # Или ID предыдущей миграции
branch_labels = None
depends_on = None


def upgrade() -> None:
    """
    Upgrade: story_id → book_id, удаление user/story таблиц
    """
    
    # ===========================================
    # 1. CHARACTERS TABLE
    # ===========================================
    
    # Удаляем FK constraint если есть
    try:
        op.drop_constraint('characters_story_id_fkey', 'characters', type_='foreignkey')
    except Exception:
        pass  # FK может не существовать
    
    try:
        op.drop_constraint('characters_user_id_fkey', 'characters', type_='foreignkey')
    except Exception:
        pass
    
    # Переименовываем колонку story_id → book_id
    op.alter_column('characters', 'story_id',
                    new_column_name='book_id',
                    existing_type=sa.String(36),
                    existing_nullable=True)
    
    # Делаем book_id NOT NULL
    op.alter_column('characters', 'book_id',
                    existing_type=sa.String(36),
                    nullable=False)
    
    # Удаляем user_id колонку (если есть и не нужна)
    # op.drop_column('characters', 'user_id')
    
    # Добавляем новые индексы
    op.create_index('ix_characters_book_id', 'characters', ['book_id'])
    op.create_index('ix_characters_book_id_name', 'characters', ['book_id', 'name'])
    
    # Удаляем старые индексы
    try:
        op.drop_index('ix_characters_story_id', 'characters')
    except Exception:
        pass
    
    # ===========================================
    # 2. SCENES TABLE
    # ===========================================
    
    try:
        op.drop_constraint('scenes_story_id_fkey', 'scenes', type_='foreignkey')
    except Exception:
        pass
    
    try:
        op.drop_constraint('scenes_user_id_fkey', 'scenes', type_='foreignkey')
    except Exception:
        pass
    
    op.alter_column('scenes', 'story_id',
                    new_column_name='book_id',
                    existing_type=sa.String(36),
                    existing_nullable=True)
    
    op.alter_column('scenes', 'book_id',
                    existing_type=sa.String(36),
                    nullable=False)
    
    op.create_index('ix_scenes_book_id', 'scenes', ['book_id'])
    op.create_index('ix_scenes_book_id_name', 'scenes', ['book_id', 'name'])
    
    try:
        op.drop_index('ix_scenes_story_id', 'scenes')
    except Exception:
        pass
    
    # ===========================================
    # 3. STORY_OBJECTS TABLE
    # ===========================================
    
    try:
        op.drop_constraint('story_objects_story_id_fkey', 'story_objects', type_='foreignkey')
    except Exception:
        pass
    
    try:
        op.drop_constraint('story_objects_user_id_fkey', 'story_objects', type_='foreignkey')
    except Exception:
        pass
    
    op.alter_column('story_objects', 'story_id',
                    new_column_name='book_id',
                    existing_type=sa.String(36),
                    existing_nullable=True)
    
    op.alter_column('story_objects', 'book_id',
                    existing_type=sa.String(36),
                    nullable=False)
    
    op.create_index('ix_story_objects_book_id', 'story_objects', ['book_id'])
    op.create_index('ix_story_objects_book_id_name', 'story_objects', ['book_id', 'name'])
    
    try:
        op.drop_index('ix_story_objects_story_id', 'story_objects')
    except Exception:
        pass
    
    # ===========================================
    # 4. PROMPT_HISTORY TABLE
    # ===========================================
    
    try:
        op.drop_constraint('prompt_history_story_id_fkey', 'prompt_history', type_='foreignkey')
    except Exception:
        pass
    
    try:
        op.drop_constraint('prompt_history_user_id_fkey', 'prompt_history', type_='foreignkey')
    except Exception:
        pass
    
    op.alter_column('prompt_history', 'story_id',
                    new_column_name='book_id',
                    existing_type=sa.String(36),
                    existing_nullable=True)
    
    op.alter_column('prompt_history', 'book_id',
                    existing_type=sa.String(36),
                    nullable=False)
    
    # Добавляем page_id и chapter_id колонки если их нет
    op.add_column('prompt_history', sa.Column('page_id', sa.String(36), nullable=True))
    op.add_column('prompt_history', sa.Column('chapter_id', sa.String(36), nullable=True))
    op.add_column('prompt_history', sa.Column('text_hash', sa.String(64), nullable=True))
    
    op.create_index('ix_prompt_history_book_id', 'prompt_history', ['book_id'])
    op.create_index('ix_prompt_history_book_page', 'prompt_history', ['book_id', 'page_id'])
    op.create_index('ix_prompt_history_text_hash', 'prompt_history', ['text_hash'])
    
    try:
        op.drop_index('ix_prompt_history_story_id', 'prompt_history')
    except Exception:
        pass
    
    # ===========================================
    # 5. DROP PROMPTS TABLE (if exists) - had FK to users/stories
    # ===========================================
    
    try:
        op.drop_table('prompts')
    except Exception:
        pass
    
    # ===========================================
    # 6. DROP STYLES TABLE (if exists) - had FK to users
    # ===========================================
    
    try:
        op.drop_table('styles')
    except Exception:
        pass
    
    # ===========================================
    # 7. DROP STORIES TABLE
    # ===========================================
    
    try:
        op.drop_table('stories')
    except Exception:
        pass
    
    # ===========================================
    # 8. DROP USERS TABLE
    # ===========================================
    
    try:
        op.drop_table('users')
    except Exception:
        pass


def downgrade() -> None:
    """
    Downgrade: Восстановление старой структуры
    WARNING: Данные из удалённых таблиц будут потеряны!
    """
    
    # ===========================================
    # 1. RECREATE USERS TABLE
    # ===========================================
    
    op.create_table(
        'users',
        sa.Column('id', sa.String(36), primary_key=True),
        sa.Column('email', sa.String(255), unique=True, nullable=False),
        sa.Column('username', sa.String(100), unique=True, nullable=False),
        sa.Column('preferences', sa.JSON, nullable=True),
        sa.Column('api_keys', sa.JSON, nullable=True),
        sa.Column('is_active', sa.Boolean, default=True),
        sa.Column('is_premium', sa.Boolean, default=False),
        sa.Column('created_at', sa.DateTime),
        sa.Column('updated_at', sa.DateTime),
        sa.Column('last_login', sa.DateTime),
    )
    
    # ===========================================
    # 2. RECREATE STORIES TABLE
    # ===========================================
    
    op.create_table(
        'stories',
        sa.Column('id', sa.String(36), primary_key=True),
        sa.Column('user_id', sa.String(36), sa.ForeignKey('users.id'), nullable=False),
        sa.Column('external_book_id', sa.String(36), nullable=True),
        sa.Column('title', sa.String(500), nullable=False),
        sa.Column('author', sa.String(300), nullable=True),
        sa.Column('description', sa.Text, nullable=True),
        sa.Column('genre', sa.String(100), nullable=True),
        sa.Column('language', sa.String(10), default='en'),
        sa.Column('default_style', sa.String(100), nullable=True),
        sa.Column('default_model', sa.String(50), default='midjourney'),
        sa.Column('visualization_mode', sa.String(50), default='per_page'),
        sa.Column('generation_settings', sa.JSON, default=dict),
        sa.Column('metadata', sa.JSON, default=dict),
        sa.Column('created_at', sa.DateTime),
        sa.Column('updated_at', sa.DateTime),
    )
    
    # ===========================================
    # 3. REVERT CHARACTERS
    # ===========================================
    
    op.drop_index('ix_characters_book_id', 'characters')
    op.drop_index('ix_characters_book_id_name', 'characters')
    
    op.alter_column('characters', 'book_id',
                    new_column_name='story_id',
                    existing_type=sa.String(36))
    
    op.alter_column('characters', 'story_id',
                    existing_type=sa.String(36),
                    nullable=True)
    
    op.create_foreign_key('characters_story_id_fkey', 'characters', 
                          'stories', ['story_id'], ['id'])
    
    op.create_index('ix_characters_story_id', 'characters', ['story_id'])
    
    # ===========================================
    # 4. REVERT SCENES
    # ===========================================
    
    op.drop_index('ix_scenes_book_id', 'scenes')
    op.drop_index('ix_scenes_book_id_name', 'scenes')
    
    op.alter_column('scenes', 'book_id',
                    new_column_name='story_id',
                    existing_type=sa.String(36))
    
    op.alter_column('scenes', 'story_id',
                    existing_type=sa.String(36),
                    nullable=True)
    
    op.create_foreign_key('scenes_story_id_fkey', 'scenes',
                          'stories', ['story_id'], ['id'])
    
    op.create_index('ix_scenes_story_id', 'scenes', ['story_id'])
    
    # ===========================================
    # 5. REVERT STORY_OBJECTS
    # ===========================================
    
    op.drop_index('ix_story_objects_book_id', 'story_objects')
    op.drop_index('ix_story_objects_book_id_name', 'story_objects')
    
    op.alter_column('story_objects', 'book_id',
                    new_column_name='story_id',
                    existing_type=sa.String(36))
    
    op.alter_column('story_objects', 'story_id',
                    existing_type=sa.String(36),
                    nullable=True)
    
    op.create_foreign_key('story_objects_story_id_fkey', 'story_objects',
                          'stories', ['story_id'], ['id'])
    
    op.create_index('ix_story_objects_story_id', 'story_objects', ['story_id'])
    
    # ===========================================
    # 6. REVERT PROMPT_HISTORY
    # ===========================================
    
    op.drop_index('ix_prompt_history_book_id', 'prompt_history')
    op.drop_index('ix_prompt_history_book_page', 'prompt_history')
    op.drop_index('ix_prompt_history_text_hash', 'prompt_history')
    
    op.drop_column('prompt_history', 'page_id')
    op.drop_column('prompt_history', 'chapter_id')
    op.drop_column('prompt_history', 'text_hash')
    
    op.alter_column('prompt_history', 'book_id',
                    new_column_name='story_id',
                    existing_type=sa.String(36))
    
    op.alter_column('prompt_history', 'story_id',
                    existing_type=sa.String(36),
                    nullable=True)
    
    op.create_foreign_key('prompt_history_story_id_fkey', 'prompt_history',
                          'stories', ['story_id'], ['id'])
    
    op.create_index('ix_prompt_history_story_id', 'prompt_history', ['story_id'])