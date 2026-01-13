// src/components/ChapterListSidebar.tsx
import React, { useState } from 'react';
import { ChapterInfo } from '../types/reader.types';
import './ChapterListSidebar.css';

interface ChapterListSidebarProps {
  isOpen: boolean;
  onClose: () => void;
  chapters: ChapterInfo[];
  currentChapterId?: string;
  onSelectChapter: (chapterId: string) => void;
}

const ChapterListSidebar: React.FC<ChapterListSidebarProps> = ({
  isOpen,
  onClose,
  chapters,
  currentChapterId,
  onSelectChapter
}) => {
  const [searchQuery, setSearchQuery] = useState('');

  const filteredChapters = chapters.filter(ch =>
    ch.title.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const totalPages = chapters.reduce((sum, ch) => sum + ch.pageCount, 0);
  const completedChapters = chapters.filter(ch => ch.isCompleted).length;
  const progress = chapters.length > 0 ? (completedChapters / chapters.length) * 100 : 0;

  return (
    <>
      {/* Overlay */}
      <div 
        className={`sidebar-overlay ${isOpen ? 'visible' : ''}`}
        onClick={onClose}
      />

      {/* Sidebar */}
      <aside className={`chapter-sidebar ${isOpen ? 'open' : ''}`}>
        {/* Header */}
        <div className="sidebar-header">
          <h2>
            <span className="header-icon">📑</span>
            Chapters
          </h2>
          <button className="btn-close" onClick={onClose}>×</button>
        </div>

        {/* Progress */}
        <div className="reading-stats">
          <div className="stat-row">
            <span className="stat-label">Progress</span>
            <span className="stat-value">{Math.round(progress)}%</span>
          </div>
          <div className="progress-bar">
            <div className="progress-fill" style={{ width: `${progress}%` }}></div>
          </div>
          <div className="stat-details">
            <span>{completedChapters} of {chapters.length} chapters</span>
            <span>{totalPages} pages total</span>
          </div>
        </div>

        {/* Search */}
        <div className="search-container">
          <span className="search-icon">🔍</span>
          <input
            type="text"
            placeholder="Search chapters..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="search-input"
          />
          {searchQuery && (
            <button 
              className="clear-search" 
              onClick={() => setSearchQuery('')}
            >
              ×
            </button>
          )}
        </div>

        {/* Chapter List */}
        <div className="chapter-list">
          {filteredChapters.length === 0 ? (
            <div className="no-chapters">
              <span className="no-chapters-icon">📚</span>
              <p>No chapters found</p>
            </div>
          ) : (
            filteredChapters.map((chapter, index) => (
              <button
                key={chapter.id}
                className={`chapter-item ${currentChapterId === chapter.id ? 'active' : ''} ${chapter.isCompleted ? 'completed' : ''}`}
                onClick={() => onSelectChapter(chapter.id)}
              >
                <div className="chapter-number">
                  {chapter.isCompleted ? (
                    <span className="check-icon">✓</span>
                  ) : (
                    <span>{chapter.chapterNumber}</span>
                  )}
                </div>
                
                <div className="chapter-content">
                  <h4 className="chapter-title">{chapter.title}</h4>
                  <div className="chapter-meta">
                    <span className="meta-item">
                      <span className="meta-icon">📄</span>
                      {chapter.pageCount} pages
                    </span>
                    <span className="meta-item">
                      <span className="meta-icon">⏱️</span>
                      ~{chapter.estimatedReadTime} min
                    </span>
                  </div>
                </div>

                {currentChapterId === chapter.id && (
                  <div className="current-indicator">
                    <span className="reading-badge">Reading</span>
                  </div>
                )}
              </button>
            ))
          )}
        </div>

        {/* Footer */}
        <div className="sidebar-footer">
          <button className="btn-bookmark">
            <span className="btn-icon">🔖</span>
            Bookmarks
          </button>
          <button className="btn-notes">
            <span className="btn-icon">📝</span>
            Notes
          </button>
        </div>
      </aside>
    </>
  );
};

export default ChapterListSidebar;