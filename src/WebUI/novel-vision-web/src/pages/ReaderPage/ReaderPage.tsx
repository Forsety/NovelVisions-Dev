// src/pages/ReaderPage/ReaderPage.tsx
// ╔══════════════════════════════════════════════════════════════════════════════╗
// ║  NOVELVISION ULTIMATE READER 2.0                                             ║
// ║  Premium Book Reading Experience with AI Visualization Integration           ║
// ║  Diploma-Quality Professional Implementation                                  ║
// ╚══════════════════════════════════════════════════════════════════════════════╝

import React, { 
  useState, useEffect, useCallback, useRef, useMemo, 
  createContext, useContext, memo 
} from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { motion, AnimatePresence, useMotionValue, useTransform, PanInfo } from 'framer-motion';
import styles from './ReaderPage.module.css';

// ==================== API IMPORTS ====================
import { catalogService } from '../../services/api/catalog.service';
import { readingService } from '../../services/api/reading.service';
import { useVisualization } from '../../shared/hooks/useVisualization';
import type { Visualization } from '../../shared/hooks/useVisualization';

// ==================== TYPES ====================

type VisualizationMode = 'None' | 'PerPage' | 'PerChapter' | 'UserSelected';
type ImagePosition = 'left' | 'right' | 'center' | 'alternate';
type ImageSize = 'small' | 'medium' | 'large';
type ReaderTheme = 'dark' | 'light' | 'sepia' | 'midnight' | 'forest' | 'ocean';
type ViewMode = 'scroll' | 'paginated' | 'twoColumn';
type FontFamily = 'serif' | 'sans' | 'mono' | 'dyslexic';
type FocusMode = 'off' | 'paragraph' | 'sentence' | 'line';

interface Page {
  id: string;
  chapterId: string;
  number: number;
  content: string;
  wordCount: number;
  visualizations?: Visualization[];
}

interface Chapter {
  id: string;
  bookId: string;
  title: string;
  number: number;
  pageCount: number;
  wordCount?: number;
  pages?: Page[];
}

interface Book {
  id: string;
  title: string;
  author: { id: string; name: string };
  coverImageUrl?: string;
  visualizationMode: VisualizationMode;
  chapters: Chapter[];
  totalPages?: number;
  totalWords?: number;
}

interface Bookmark {
  id: string;
  pageId: string;
  chapterIndex: number;
  pageIndex: number;
  note?: string;
  color: string;
  createdAt: Date;
  selectedText?: string;
}

interface Annotation {
  id: string;
  pageId: string;
  text: string;
  note: string;
  color: string;
  startOffset: number;
  endOffset: number;
  createdAt: Date;
}

interface ReadingStats {
  totalTimeSeconds: number;
  pagesRead: number;
  wordsRead: number;
  averageSpeed: number; // words per minute
  currentSessionStart: Date;
  currentSessionPages: number;
}

interface ReaderSettings {
  theme: ReaderTheme;
  fontSize: number;
  lineHeight: number;
  fontFamily: FontFamily;
  textAlign: 'left' | 'justify';
  margins: 'narrow' | 'normal' | 'wide';
  viewMode: ViewMode;
  showProgress: boolean;
  showTime: boolean;
  autoHideToolbar: boolean;
  pageAnimation: 'slide' | 'fade' | 'flip' | 'none';
  enableSounds: boolean;
  enableMusic: boolean;
  // New advanced settings
  focusMode: FocusMode;
  autoScroll: boolean;
  autoScrollSpeed: number; // words per minute
  paragraphSpacing: 'compact' | 'normal' | 'relaxed';
  highlightLinks: boolean;
  showParagraphNumbers: boolean;
  immersiveMode: boolean; // hides everything except text
}

interface TextSelection {
  text: string;
  pageId: string;
  rect?: DOMRect;
  contextBefore?: string;
  contextAfter?: string;
  startOffset: number;
  endOffset: number;
}

interface UserPreferences {
  mode: VisualizationMode;
  imagePosition: ImagePosition;
  imageSize: ImageSize;
  autoGenerate: boolean;
}

// ==================== CONSTANTS ====================

const THEMES: Record<ReaderTheme, { name: string; icon: string; bg: string; text: string }> = {
  dark: { name: 'Dark', icon: '🌙', bg: '#0d0b0e', text: '#f5f0e8' },
  light: { name: 'Light', icon: '☀️', bg: '#faf7f2', text: '#2d2426' },
  sepia: { name: 'Sepia', icon: '📜', bg: '#f4e8d3', text: '#4a3f32' },
  midnight: { name: 'Midnight', icon: '🌌', bg: '#0a0a1a', text: '#e8e8f0' },
  forest: { name: 'Forest', icon: '🌲', bg: '#0d1a0f', text: '#e0f0e0' },
  ocean: { name: 'Ocean', icon: '🌊', bg: '#0a1520', text: '#e0f0fa' },
};

const FONTS: Record<FontFamily, { name: string; family: string }> = {
  serif: { name: 'Classic Serif', family: '"Crimson Pro", "Georgia", serif' },
  sans: { name: 'Modern Sans', family: '"DM Sans", "Segoe UI", sans-serif' },
  mono: { name: 'Monospace', family: '"JetBrains Mono", "Consolas", monospace' },
  dyslexic: { name: 'OpenDyslexic', family: '"OpenDyslexic", "Comic Sans MS", sans-serif' },
};

const BOOKMARK_COLORS = ['#d4a574', '#8b3a4c', '#4a7c6f', '#6b5b95', '#d4574a', '#4a90d4'];

const DEFAULT_SETTINGS: ReaderSettings = {
  theme: 'dark',
  fontSize: 18,
  lineHeight: 1.8,
  fontFamily: 'serif',
  textAlign: 'justify',
  margins: 'normal',
  viewMode: 'scroll',
  showProgress: true,
  showTime: true,
  autoHideToolbar: true,
  pageAnimation: 'slide',
  enableSounds: false,
  enableMusic: false,
  // New defaults
  focusMode: 'off',
  autoScroll: false,
  autoScrollSpeed: 200,
  paragraphSpacing: 'normal',
  highlightLinks: true,
  showParagraphNumbers: false,
  immersiveMode: false,
};

// ==================== CONTEXT ====================

interface ReaderContextType {
  book: Book | null;
  settings: ReaderSettings;
  updateSettings: (settings: Partial<ReaderSettings>) => void;
  currentChapterIndex: number;
  currentPageIndex: number;
  goToPage: (chapterIndex: number, pageIndex: number) => void;
  bookmarks: Bookmark[];
  addBookmark: (bookmark: Omit<Bookmark, 'id' | 'createdAt'>) => void;
  removeBookmark: (id: string) => void;
  annotations: Annotation[];
  addAnnotation: (annotation: Omit<Annotation, 'id' | 'createdAt'>) => void;
  removeAnnotation: (id: string) => void;
  stats: ReadingStats;
  searchResults: SearchResult[];
  searchQuery: string;
  setSearchQuery: (query: string) => void;
}

const ReaderContext = createContext<ReaderContextType | null>(null);

const useReader = () => {
  const context = useContext(ReaderContext);
  if (!context) throw new Error('useReader must be used within ReaderProvider');
  return context;
};

// ==================== SEARCH ====================

interface SearchResult {
  chapterIndex: number;
  pageIndex: number;
  excerpt: string;
  matchIndex: number;
}

// ==================== UTILITY FUNCTIONS ====================

const getStoredSettings = (): ReaderSettings => {
  try {
    const stored = localStorage.getItem('reader-settings');
    return stored ? { ...DEFAULT_SETTINGS, ...JSON.parse(stored) } : DEFAULT_SETTINGS;
  } catch {
    return DEFAULT_SETTINGS;
  }
};

const saveSettings = (settings: ReaderSettings) => {
  localStorage.setItem('reader-settings', JSON.stringify(settings));
};

const getStoredBookmarks = (bookId: string): Bookmark[] => {
  try {
    const stored = localStorage.getItem(`bookmarks-${bookId}`);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
};

const saveBookmarks = (bookId: string, bookmarks: Bookmark[]) => {
  localStorage.setItem(`bookmarks-${bookId}`, JSON.stringify(bookmarks));
};

const getStoredAnnotations = (bookId: string): Annotation[] => {
  try {
    const stored = localStorage.getItem(`annotations-${bookId}`);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
};

const saveAnnotations = (bookId: string, annotations: Annotation[]) => {
  localStorage.setItem(`annotations-${bookId}`, JSON.stringify(annotations));
};

const formatTime = (seconds: number): string => {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  if (hours > 0) return `${hours}h ${minutes}m`;
  return `${minutes}m`;
};

const formatReadingSpeed = (wpm: number): string => {
  if (wpm < 150) return 'Relaxed';
  if (wpm < 250) return 'Average';
  if (wpm < 400) return 'Fast';
  return 'Speed Reader';
};

const generateId = () => `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

// ==================== SUB-COMPONENTS ====================

// ---------- Reading Progress Wheel ----------
const ReadingProgressWheel: React.FC<{ 
  progress: number; 
  size?: number;
  showLabel?: boolean;
}> = memo(({ progress, size = 120, showLabel = true }) => {
  const strokeWidth = size * 0.08;
  const radius = (size - strokeWidth) / 2;
  const circumference = radius * 2 * Math.PI;
  const offset = circumference - (progress / 100) * circumference;

  return (
    <div className={styles.readingProgressWheel} style={{ width: size, height: size }}>
      <svg width={size} height={size}>
        <defs>
          <linearGradient id="progressGradient" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="var(--reader-accent)" />
            <stop offset="100%" stopColor="var(--reader-accent-secondary)" />
          </linearGradient>
        </defs>
        <circle
          className={styles.wheelBg}
          strokeWidth={strokeWidth}
          fill="transparent"
          r={radius}
          cx={size / 2}
          cy={size / 2}
        />
        <circle
          className={styles.wheelFill}
          strokeWidth={strokeWidth}
          fill="transparent"
          r={radius}
          cx={size / 2}
          cy={size / 2}
          style={{ 
            strokeDasharray: circumference, 
            strokeDashoffset: offset,
            stroke: 'url(#progressGradient)'
          }}
        />
      </svg>
      <div className={styles.wheelContent}>
        <span className={styles.wheelPercent}>{Math.round(progress)}%</span>
        {showLabel && <span className={styles.wheelLabel}>complete</span>}
      </div>
    </div>
  );
});

// ---------- Progress Ring ----------
const ProgressRing: React.FC<{ progress: number; size?: number; strokeWidth?: number }> = memo(({ 
  progress, size = 44, strokeWidth = 3 
}) => {
  const radius = (size - strokeWidth) / 2;
  const circumference = radius * 2 * Math.PI;
  const offset = circumference - (progress / 100) * circumference;

  return (
    <svg className={styles.progressRing} width={size} height={size}>
      <circle
        className={styles.progressRingBg}
        strokeWidth={strokeWidth}
        fill="transparent"
        r={radius}
        cx={size / 2}
        cy={size / 2}
      />
      <circle
        className={styles.progressRingFill}
        strokeWidth={strokeWidth}
        fill="transparent"
        r={radius}
        cx={size / 2}
        cy={size / 2}
        style={{ strokeDasharray: circumference, strokeDashoffset: offset }}
      />
      <text
        x="50%"
        y="50%"
        textAnchor="middle"
        dominantBaseline="middle"
        className={styles.progressRingText}
      >
        {Math.round(progress)}%
      </text>
    </svg>
  );
});

// ---------- Mini Map ----------
const MiniMap: React.FC<{ 
  book: Book; 
  currentChapter: number; 
  currentPage: number;
  onNavigate: (chapter: number, page: number) => void;
}> = memo(({ book, currentChapter, currentPage, onNavigate }) => {
  const totalPages = book.chapters.reduce((sum, ch) => sum + ch.pageCount, 0);
  const currentAbsolute = book.chapters
    .slice(0, currentChapter)
    .reduce((sum, ch) => sum + ch.pageCount, 0) + currentPage;

  return (
    <div className={styles.miniMap}>
      <div className={styles.miniMapTrack}>
        {book.chapters.map((chapter, chIdx) => (
          <div 
            key={chapter.id}
            className={styles.miniMapChapter}
            style={{ flex: chapter.pageCount }}
          >
            {Array.from({ length: chapter.pageCount }, (_, pIdx) => {
              const isActive = chIdx === currentChapter && pIdx === currentPage;
              const isPast = chIdx < currentChapter || (chIdx === currentChapter && pIdx < currentPage);
              return (
                <button
                  key={pIdx}
                  className={`${styles.miniMapPage} ${isActive ? styles.active : ''} ${isPast ? styles.past : ''}`}
                  onClick={() => onNavigate(chIdx, pIdx)}
                  title={`Chapter ${chapter.number}, Page ${pIdx + 1}`}
                />
              );
            })}
          </div>
        ))}
      </div>
      <div className={styles.miniMapInfo}>
        <span>Page {currentAbsolute + 1} of {totalPages}</span>
      </div>
    </div>
  );
});

// ---------- Toolbar ----------
const Toolbar: React.FC<{
  book: Book;
  visible: boolean;
  onBack: () => void;
  onToggleSidebar: () => void;
  onToggleSettings: () => void;
  onToggleSearch: () => void;
  onToggleBookmarks: () => void;
  sidebarOpen: boolean;
  settingsOpen: boolean;
  searchOpen: boolean;
  bookmarksOpen: boolean;
}> = memo(({ 
  book, visible, onBack, onToggleSidebar, onToggleSettings, 
  onToggleSearch, onToggleBookmarks, sidebarOpen, settingsOpen, searchOpen, bookmarksOpen 
}) => {
  const { settings, currentChapterIndex, currentPageIndex, stats } = useReader();
  const currentChapter = book.chapters[currentChapterIndex];
  
  const totalPages = book.chapters.reduce((sum, ch) => sum + ch.pageCount, 0);
  const currentAbsolute = book.chapters
    .slice(0, currentChapterIndex)
    .reduce((sum, ch) => sum + ch.pageCount, 0) + currentPageIndex;
  const progress = totalPages > 0 ? (currentAbsolute / totalPages) * 100 : 0;

  return (
    <motion.header
      className={`${styles.toolbar} ${!visible ? styles.toolbarHidden : ''}`}
      initial={{ y: -80 }}
      animate={{ y: visible ? 0 : -80 }}
      transition={{ duration: 0.3, ease: 'easeOut' }}
    >
      {/* Left Section */}
      <div className={styles.toolbarLeft}>
        <button className={styles.toolbarBtn} onClick={onBack} aria-label="Go back">
          <span>←</span>
        </button>
        <div className={styles.bookInfo}>
          <h1>{book.title}</h1>
          <span>{book.author.name}</span>
        </div>
      </div>

      {/* Center Section - Chapter & Progress */}
      <div className={styles.toolbarCenter}>
        <div className={styles.chapterIndicator}>
          {currentChapter ? (
            <>
              <span className={styles.chapterLabel}>Chapter {currentChapter.number}</span>
              <span className={styles.chapterTitle}>{currentChapter.title}</span>
            </>
          ) : (
            <span>Loading...</span>
          )}
        </div>
        {settings.showProgress && (
          <div className={styles.progressSection}>
            <div className={styles.progressBar}>
              <motion.div 
                className={styles.progressFill}
                initial={{ width: 0 }}
                animate={{ width: `${progress}%` }}
                transition={{ duration: 0.5, ease: 'easeOut' }}
              />
            </div>
            <span className={styles.progressText}>{Math.round(progress)}%</span>
          </div>
        )}
      </div>

      {/* Right Section - Actions */}
      <div className={styles.toolbarRight}>
        {settings.showTime && (
          <div className={styles.readingTime}>
            <span className={styles.timeIcon}>⏱️</span>
            <span>{formatTime(stats.totalTimeSeconds)}</span>
          </div>
        )}
        
        <div className={styles.toolbarActions}>
          <button 
            className={`${styles.toolbarBtn} ${searchOpen ? styles.active : ''}`}
            onClick={onToggleSearch}
            aria-label="Search"
          >
            🔍
          </button>
          <button 
            className={`${styles.toolbarBtn} ${bookmarksOpen ? styles.active : ''}`}
            onClick={onToggleBookmarks}
            aria-label="Bookmarks"
          >
            🔖
          </button>
          <button 
            className={`${styles.toolbarBtn} ${sidebarOpen ? styles.active : ''}`}
            onClick={onToggleSidebar}
            aria-label="Table of Contents"
          >
            📑
          </button>
          <button 
            className={`${styles.toolbarBtn} ${settingsOpen ? styles.active : ''}`}
            onClick={onToggleSettings}
            aria-label="Settings"
          >
            ⚙️
          </button>
        </div>
      </div>
    </motion.header>
  );
});

// ---------- Chapter Sidebar ----------
const ChapterSidebar: React.FC<{
  book: Book;
  isOpen: boolean;
  onClose: () => void;
  onNavigate: (chapterIndex: number, pageIndex?: number) => void;
}> = memo(({ book, isOpen, onClose, onNavigate }) => {
  const { currentChapterIndex, currentPageIndex } = useReader();

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          <motion.div
            className={styles.sidebarOverlay}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
          />
          <motion.aside
            className={styles.sidebar}
            initial={{ x: -360 }}
            animate={{ x: 0 }}
            exit={{ x: -360 }}
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
          >
            <div className={styles.sidebarHeader}>
              <h2>📚 Contents</h2>
              <button onClick={onClose} className={styles.closeBtn}>✕</button>
            </div>
            
            {/* Book Cover Mini */}
            <div className={styles.sidebarBookCard}>
              {book.coverImageUrl && (
                <img src={book.coverImageUrl} alt={book.title} className={styles.sidebarCover} />
              )}
              <div className={styles.sidebarBookInfo}>
                <h3>{book.title}</h3>
                <p>{book.author.name}</p>
              </div>
            </div>

            {/* Chapter List */}
            <nav className={styles.chapterList}>
              {book.chapters.map((chapter, idx) => (
                <button
                  key={chapter.id}
                  className={`${styles.chapterItem} ${idx === currentChapterIndex ? styles.active : ''}`}
                  onClick={() => onNavigate(idx, 0)}
                >
                  <span className={styles.chapterNum}>{chapter.number}</span>
                  <div className={styles.chapterContent}>
                    <strong>{chapter.title}</strong>
                    <span className={styles.chapterMeta}>
                      {chapter.pageCount} pages
                      {chapter.wordCount && ` • ${(chapter.wordCount / 1000).toFixed(1)}k words`}
                    </span>
                    {idx === currentChapterIndex && (
                      <div className={styles.chapterProgress}>
                        <div 
                          className={styles.chapterProgressFill}
                          style={{ width: `${((currentPageIndex + 1) / chapter.pageCount) * 100}%` }}
                        />
                      </div>
                    )}
                  </div>
                  {idx < currentChapterIndex && <span className={styles.checkMark}>✓</span>}
                </button>
              ))}
            </nav>
          </motion.aside>
        </>
      )}
    </AnimatePresence>
  );
});

// ---------- Settings Panel ----------
const SettingsPanel: React.FC<{
  isOpen: boolean;
  onClose: () => void;
}> = memo(({ isOpen, onClose }) => {
  const { settings, updateSettings } = useReader();

  const fontSizeOptions = [14, 16, 18, 20, 22, 24, 28];
  const lineHeightOptions = [1.4, 1.6, 1.8, 2.0, 2.2];

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          className={styles.settingsPanel}
          initial={{ opacity: 0, x: 40, scale: 0.95 }}
          animate={{ opacity: 1, x: 0, scale: 1 }}
          exit={{ opacity: 0, x: 40, scale: 0.95 }}
          transition={{ duration: 0.25, ease: 'easeOut' }}
        >
          <div className={styles.settingsHeader}>
            <h3>⚙️ Reader Settings</h3>
            <button onClick={onClose} className={styles.closeBtn}>✕</button>
          </div>

          <div className={styles.settingsContent}>
            {/* Theme Selection */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Theme</label>
              <div className={styles.themeGrid}>
                {(Object.entries(THEMES) as [ReaderTheme, typeof THEMES[ReaderTheme]][]).map(([key, theme]) => (
                  <button
                    key={key}
                    className={`${styles.themeBtn} ${settings.theme === key ? styles.active : ''}`}
                    onClick={() => updateSettings({ theme: key })}
                    style={{ '--theme-bg': theme.bg, '--theme-text': theme.text } as React.CSSProperties}
                  >
                    <span className={styles.themeIcon}>{theme.icon}</span>
                    <span className={styles.themeName}>{theme.name}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Font Family */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Font</label>
              <div className={styles.fontOptions}>
                {(Object.entries(FONTS) as [FontFamily, typeof FONTS[FontFamily]][]).map(([key, font]) => (
                  <button
                    key={key}
                    className={`${styles.fontBtn} ${settings.fontFamily === key ? styles.active : ''}`}
                    onClick={() => updateSettings({ fontFamily: key })}
                    style={{ fontFamily: font.family }}
                  >
                    {font.name}
                  </button>
                ))}
              </div>
            </div>

            {/* Font Size */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>
                Font Size: <strong>{settings.fontSize}px</strong>
              </label>
              <div className={styles.sizeControls}>
                <button 
                  className={styles.sizeBtn}
                  onClick={() => updateSettings({ fontSize: Math.max(12, settings.fontSize - 2) })}
                >
                  A-
                </button>
                <div className={styles.sizeSlider}>
                  <input
                    type="range"
                    min="12"
                    max="32"
                    value={settings.fontSize}
                    onChange={(e) => updateSettings({ fontSize: parseInt(e.target.value) })}
                  />
                </div>
                <button 
                  className={styles.sizeBtn}
                  onClick={() => updateSettings({ fontSize: Math.min(32, settings.fontSize + 2) })}
                >
                  A+
                </button>
              </div>
            </div>

            {/* Line Height */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Line Spacing</label>
              <div className={styles.spacingOptions}>
                {lineHeightOptions.map((height) => (
                  <button
                    key={height}
                    className={`${styles.spacingBtn} ${settings.lineHeight === height ? styles.active : ''}`}
                    onClick={() => updateSettings({ lineHeight: height })}
                  >
                    <span className={styles.spacingLines} style={{ gap: height * 4 }}>
                      <span /><span /><span />
                    </span>
                    <span>{height}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Text Align */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Text Alignment</label>
              <div className={styles.alignOptions}>
                <button
                  className={`${styles.alignBtn} ${settings.textAlign === 'left' ? styles.active : ''}`}
                  onClick={() => updateSettings({ textAlign: 'left' })}
                >
                  <span className={styles.alignIcon}>≡</span>
                  Left
                </button>
                <button
                  className={`${styles.alignBtn} ${settings.textAlign === 'justify' ? styles.active : ''}`}
                  onClick={() => updateSettings({ textAlign: 'justify' })}
                >
                  <span className={styles.alignIcon}>☰</span>
                  Justify
                </button>
              </div>
            </div>

            {/* Margins */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Page Margins</label>
              <div className={styles.marginOptions}>
                {(['narrow', 'normal', 'wide'] as const).map((margin) => (
                  <button
                    key={margin}
                    className={`${styles.marginBtn} ${settings.margins === margin ? styles.active : ''}`}
                    onClick={() => updateSettings({ margins: margin })}
                  >
                    <span className={styles.marginPreview} data-margin={margin}>
                      <span />
                    </span>
                    {margin.charAt(0).toUpperCase() + margin.slice(1)}
                  </button>
                ))}
              </div>
            </div>

            {/* View Mode */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>View Mode</label>
              <div className={styles.viewModeOptions}>
                {([
                  { key: 'scroll', icon: '📜', label: 'Scroll' },
                  { key: 'paginated', icon: '📖', label: 'Pages' },
                  { key: 'twoColumn', icon: '📰', label: 'Columns' },
                ] as const).map(({ key, icon, label }) => (
                  <button
                    key={key}
                    className={`${styles.viewModeBtn} ${settings.viewMode === key ? styles.active : ''}`}
                    onClick={() => updateSettings({ viewMode: key })}
                  >
                    <span>{icon}</span>
                    <span>{label}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Focus Mode - NEW */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Focus Mode</label>
              <div className={styles.focusModeOptions}>
                {([
                  { key: 'off', label: 'Off' },
                  { key: 'paragraph', label: 'Paragraph' },
                  { key: 'sentence', label: 'Sentence' },
                  { key: 'line', label: 'Line' },
                ] as const).map(({ key, label }) => (
                  <button
                    key={key}
                    className={`${styles.optionBtn} ${settings.focusMode === key ? styles.active : ''}`}
                    onClick={() => updateSettings({ focusMode: key })}
                  >
                    {label}
                  </button>
                ))}
              </div>
              {settings.focusMode !== 'off' && (
                <p className={styles.settingHint}>
                  💡 Click on text to focus. Press Esc to exit.
                </p>
              )}
            </div>

            {/* Paragraph Spacing - NEW */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Paragraph Spacing</label>
              <div className={styles.spacingOptions}>
                {([
                  { key: 'compact', label: 'Compact' },
                  { key: 'normal', label: 'Normal' },
                  { key: 'relaxed', label: 'Relaxed' },
                ] as const).map(({ key, label }) => (
                  <button
                    key={key}
                    className={`${styles.optionBtn} ${settings.paragraphSpacing === key ? styles.active : ''}`}
                    onClick={() => updateSettings({ paragraphSpacing: key })}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>

            {/* Auto-Scroll - NEW */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Auto-Scroll</label>
              <div className={styles.autoScrollControls}>
                <label className={styles.toggle}>
                  <input
                    type="checkbox"
                    checked={settings.autoScroll}
                    onChange={(e) => updateSettings({ autoScroll: e.target.checked })}
                  />
                  <span className={styles.toggleSlider} />
                  Enable
                </label>
                {settings.autoScroll && (
                  <div className={styles.speedControl}>
                    <span>Speed: {settings.autoScrollSpeed} wpm</span>
                    <input
                      type="range"
                      min="50"
                      max="500"
                      step="25"
                      value={settings.autoScrollSpeed}
                      onChange={(e) => updateSettings({ autoScrollSpeed: parseInt(e.target.value) })}
                    />
                  </div>
                )}
              </div>
            </div>

            {/* Page Animation */}
            {settings.viewMode === 'paginated' && (
              <div className={styles.settingGroup}>
                <label className={styles.settingLabel}>Page Animation</label>
                <div className={styles.animationOptions}>
                  {([
                    { key: 'slide', label: 'Slide' },
                    { key: 'fade', label: 'Fade' },
                    { key: 'flip', label: 'Flip' },
                    { key: 'none', label: 'None' },
                  ] as const).map(({ key, label }) => (
                    <button
                      key={key}
                      className={`${styles.optionBtn} ${settings.pageAnimation === key ? styles.active : ''}`}
                      onClick={() => updateSettings({ pageAnimation: key })}
                    >
                      {label}
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Toggles */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Display Options</label>
              <div className={styles.toggles}>
                <label className={styles.toggle}>
                  <input
                    type="checkbox"
                    checked={settings.showProgress}
                    onChange={(e) => updateSettings({ showProgress: e.target.checked })}
                  />
                  <span className={styles.toggleSlider} />
                  Show Progress
                </label>
                <label className={styles.toggle}>
                  <input
                    type="checkbox"
                    checked={settings.showTime}
                    onChange={(e) => updateSettings({ showTime: e.target.checked })}
                  />
                  <span className={styles.toggleSlider} />
                  Show Reading Time
                </label>
                <label className={styles.toggle}>
                  <input
                    type="checkbox"
                    checked={settings.autoHideToolbar}
                    onChange={(e) => updateSettings({ autoHideToolbar: e.target.checked })}
                  />
                  <span className={styles.toggleSlider} />
                  Auto-hide Toolbar
                </label>
                <label className={styles.toggle}>
                  <input
                    type="checkbox"
                    checked={settings.showParagraphNumbers}
                    onChange={(e) => updateSettings({ showParagraphNumbers: e.target.checked })}
                  />
                  <span className={styles.toggleSlider} />
                  Paragraph Numbers
                </label>
                <label className={styles.toggle}>
                  <input
                    type="checkbox"
                    checked={settings.immersiveMode}
                    onChange={(e) => updateSettings({ immersiveMode: e.target.checked })}
                  />
                  <span className={styles.toggleSlider} />
                  Immersive Mode
                </label>
              </div>
            </div>

            {/* Keyboard Shortcuts Help */}
            <div className={styles.settingGroup}>
              <label className={styles.settingLabel}>Keyboard Shortcuts</label>
              <div className={styles.shortcutsGrid}>
                <div className={styles.shortcut}><kbd>←</kbd><span>Previous page</span></div>
                <div className={styles.shortcut}><kbd>→</kbd><span>Next page</span></div>
                <div className={styles.shortcut}><kbd>Ctrl+F</kbd><span>Search</span></div>
                <div className={styles.shortcut}><kbd>Ctrl+S</kbd><span>Settings</span></div>
                <div className={styles.shortcut}><kbd>Ctrl+T</kbd><span>Contents</span></div>
                <div className={styles.shortcut}><kbd>Esc</kbd><span>Close panels</span></div>
                <div className={styles.shortcut}><kbd>F</kbd><span>Toggle fullscreen</span></div>
                <div className={styles.shortcut}><kbd>Space</kbd><span>Next page</span></div>
              </div>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
});

// ---------- Search Panel ----------
const SearchPanel: React.FC<{
  isOpen: boolean;
  onClose: () => void;
  onNavigate: (chapterIndex: number, pageIndex: number) => void;
}> = memo(({ isOpen, onClose, onNavigate }) => {
  const { searchQuery, setSearchQuery, searchResults } = useReader();
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          className={styles.searchPanel}
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -20 }}
          transition={{ duration: 0.2 }}
        >
          <div className={styles.searchHeader}>
            <div className={styles.searchInputWrapper}>
              <span className={styles.searchIcon}>🔍</span>
              <input
                ref={inputRef}
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search in book..."
                className={styles.searchInput}
              />
              {searchQuery && (
                <button 
                  className={styles.searchClear}
                  onClick={() => setSearchQuery('')}
                >
                  ✕
                </button>
              )}
            </div>
            <button onClick={onClose} className={styles.closeBtn}>✕</button>
          </div>

          {searchResults.length > 0 && (
            <div className={styles.searchResults}>
              <div className={styles.searchResultsHeader}>
                {searchResults.length} result{searchResults.length !== 1 ? 's' : ''} found
              </div>
              <div className={styles.searchResultsList}>
                {searchResults.map((result, idx) => (
                  <button
                    key={idx}
                    className={styles.searchResultItem}
                    onClick={() => {
                      onNavigate(result.chapterIndex, result.pageIndex);
                      onClose();
                    }}
                  >
                    <span className={styles.searchResultLocation}>
                      Ch. {result.chapterIndex + 1}, Page {result.pageIndex + 1}
                    </span>
                    <p 
                      className={styles.searchResultExcerpt}
                      dangerouslySetInnerHTML={{ 
                        __html: result.excerpt.replace(
                          new RegExp(`(${searchQuery})`, 'gi'),
                          '<mark>$1</mark>'
                        )
                      }}
                    />
                  </button>
                ))}
              </div>
            </div>
          )}

          {searchQuery && searchResults.length === 0 && (
            <div className={styles.searchEmpty}>
              <span>🔍</span>
              <p>No results found for "{searchQuery}"</p>
            </div>
          )}
        </motion.div>
      )}
    </AnimatePresence>
  );
});

// ---------- Bookmarks Panel ----------
const BookmarksPanel: React.FC<{
  isOpen: boolean;
  onClose: () => void;
  onNavigate: (chapterIndex: number, pageIndex: number) => void;
}> = memo(({ isOpen, onClose, onNavigate }) => {
  const { book, bookmarks, removeBookmark, annotations, removeAnnotation } = useReader();
  const [activeTab, setActiveTab] = useState<'bookmarks' | 'annotations'>('bookmarks');

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          <motion.div
            className={styles.sidebarOverlay}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
          />
          <motion.aside
            className={styles.bookmarksPanel}
            initial={{ x: 360 }}
            animate={{ x: 0 }}
            exit={{ x: 360 }}
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
          >
            <div className={styles.bookmarksPanelHeader}>
              <h2>🔖 Notes & Bookmarks</h2>
              <button onClick={onClose} className={styles.closeBtn}>✕</button>
            </div>

            <div className={styles.bookmarksTabs}>
              <button
                className={`${styles.bookmarksTab} ${activeTab === 'bookmarks' ? styles.active : ''}`}
                onClick={() => setActiveTab('bookmarks')}
              >
                Bookmarks ({bookmarks.length})
              </button>
              <button
                className={`${styles.bookmarksTab} ${activeTab === 'annotations' ? styles.active : ''}`}
                onClick={() => setActiveTab('annotations')}
              >
                Notes ({annotations.length})
              </button>
            </div>

            <div className={styles.bookmarksList}>
              {activeTab === 'bookmarks' ? (
                bookmarks.length > 0 ? (
                  bookmarks.map((bookmark) => (
                    <div 
                      key={bookmark.id} 
                      className={styles.bookmarkItem}
                      style={{ '--bookmark-color': bookmark.color } as React.CSSProperties}
                    >
                      <div className={styles.bookmarkColor} />
                      <div 
                        className={styles.bookmarkContent}
                        onClick={() => {
                          onNavigate(bookmark.chapterIndex, bookmark.pageIndex);
                          onClose();
                        }}
                      >
                        <span className={styles.bookmarkLocation}>
                          Chapter {bookmark.chapterIndex + 1}, Page {bookmark.pageIndex + 1}
                        </span>
                        {bookmark.selectedText && (
                          <p className={styles.bookmarkText}>"{bookmark.selectedText}"</p>
                        )}
                        {bookmark.note && (
                          <p className={styles.bookmarkNote}>{bookmark.note}</p>
                        )}
                      </div>
                      <button 
                        className={styles.bookmarkDelete}
                        onClick={() => removeBookmark(bookmark.id)}
                      >
                        🗑️
                      </button>
                    </div>
                  ))
                ) : (
                  <div className={styles.emptyBookmarks}>
                    <span>🔖</span>
                    <p>No bookmarks yet</p>
                    <small>Select text to add a bookmark</small>
                  </div>
                )
              ) : (
                annotations.length > 0 ? (
                  annotations.map((annotation) => (
                    <div 
                      key={annotation.id} 
                      className={styles.annotationItem}
                      style={{ '--annotation-color': annotation.color } as React.CSSProperties}
                    >
                      <div className={styles.annotationHighlight}>
                        "{annotation.text}"
                      </div>
                      <p className={styles.annotationNote}>{annotation.note}</p>
                      <button 
                        className={styles.annotationDelete}
                        onClick={() => removeAnnotation(annotation.id)}
                      >
                        🗑️
                      </button>
                    </div>
                  ))
                ) : (
                  <div className={styles.emptyBookmarks}>
                    <span>📝</span>
                    <p>No notes yet</p>
                    <small>Highlight text to add notes</small>
                  </div>
                )
              )}
            </div>
          </motion.aside>
        </>
      )}
    </AnimatePresence>
  );
});

// ---------- Visualization Block ----------
const VisualizationBlock: React.FC<{
  visualization: Visualization & { position: ImagePosition };
  size: ImageSize;
  onRemove: () => void;
  onRegenerate: () => void;
  isGenerating: boolean;
}> = memo(({ visualization, size, onRemove, onRegenerate, isGenerating }) => {
  const [isHovered, setIsHovered] = useState(false);
  const [imageLoaded, setImageLoaded] = useState(false);
  const [showLightbox, setShowLightbox] = useState(false);

  const positionClass = {
    left: styles.vizLeft,
    right: styles.vizRight,
    center: styles.vizCenter,
    alternate: styles.vizRight,
  }[visualization.position];

  const sizeClass = {
    small: styles.vizSmall,
    medium: styles.vizMedium,
    large: styles.vizLarge,
  }[size];

  if (isGenerating || visualization.status === 'generating') {
    return (
      <div className={`${styles.visualizationBlock} ${positionClass} ${sizeClass} ${styles.vizGenerating}`}>
        <div className={styles.vizLoadingContent}>
          <div className={styles.vizSpinner} />
          <span>Creating visualization...</span>
          <div className={styles.vizLoadingPulse} />
        </div>
      </div>
    );
  }

  if (visualization.status === 'failed') {
    return (
      <div className={`${styles.visualizationBlock} ${positionClass} ${sizeClass} ${styles.vizFailed}`}>
        <div className={styles.vizErrorContent}>
          <span>⚠️</span>
          <p>Failed to generate</p>
          <button onClick={onRegenerate} className={styles.vizRetryBtn}>
            🔄 Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <>
      <motion.div
        className={`${styles.visualizationBlock} ${positionClass} ${sizeClass}`}
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4 }}
      >
        <div className={styles.vizImageWrapper}>
          {!imageLoaded && <div className={styles.vizImagePlaceholder} />}
          <img
            src={visualization.imageUrl}
            alt="AI Generated Visualization"
            onLoad={() => setImageLoaded(true)}
            onClick={() => setShowLightbox(true)}
            style={{ opacity: imageLoaded ? 1 : 0 }}
          />
          <AnimatePresence>
            {isHovered && (
              <motion.div
                className={styles.vizOverlay}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
              >
                <button 
                  className={styles.vizActionBtn}
                  onClick={() => setShowLightbox(true)}
                  title="View fullscreen"
                >
                  🔍
                </button>
                <button 
                  className={styles.vizActionBtn}
                  onClick={onRegenerate}
                  title="Regenerate"
                >
                  🔄
                </button>
                <button 
                  className={styles.vizActionBtn}
                  onClick={onRemove}
                  title="Remove"
                >
                  🗑️
                </button>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
        {visualization.selectedText && (
          <div className={styles.vizCaption}>
            <span className={styles.quoteStart}>"</span>
            {visualization.selectedText.substring(0, 100)}
            {visualization.selectedText.length > 100 ? '...' : ''}
            <span className={styles.quoteEnd}>"</span>
          </div>
        )}
      </motion.div>

      {/* Lightbox */}
      <AnimatePresence>
        {showLightbox && (
          <motion.div
            className={styles.lightbox}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={() => setShowLightbox(false)}
          >
            <motion.img
              src={visualization.imageUrl}
              alt="AI Generated Visualization"
              initial={{ scale: 0.8 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0.8 }}
            />
            <button className={styles.lightboxClose}>✕</button>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
});

// ---------- Page Content ----------
const PageContent: React.FC<{
  page: Page;
  visualizations: Visualization[];
  preferences: UserPreferences;
  onRemoveVisualization: (id: string) => void;
  onRegenerateVisualization: (id: string) => void;
  isFirstPage?: boolean;
  chapterTitle?: string;
  chapterNumber?: number;
  annotations: Annotation[];
}> = memo(({
  page,
  visualizations,
  preferences,
  onRemoveVisualization,
  onRegenerateVisualization,
  isFirstPage,
  chapterTitle,
  chapterNumber,
  annotations
}) => {
  const { settings } = useReader();
  const [focusedParagraph, setFocusedParagraph] = useState<number | null>(null);
  const paragraphs = page.content.split('\n\n').filter(p => p.trim());
  
  const getVizPosition = (index: number): ImagePosition => {
    if (preferences.imagePosition === 'alternate') {
      return index % 2 === 0 ? 'right' : 'left';
    }
    return preferences.imagePosition;
  };

  // Paragraph spacing class
  const spacingClass = {
    compact: styles.spacingCompact,
    normal: styles.spacingNormal,
    relaxed: styles.spacingRelaxed,
  }[settings.paragraphSpacing];

  // Handle paragraph click for focus mode
  const handleParagraphClick = (index: number) => {
    if (settings.focusMode !== 'off') {
      setFocusedParagraph(focusedParagraph === index ? null : index);
    }
  };

  const renderContent = () => {
    const elements: React.ReactNode[] = [];
    let vizIndex = 0;
    
    // Chapter header
    if (isFirstPage && chapterTitle) {
      elements.push(
        <header key="chapter-header" className={styles.chapterHeader}>
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, ease: 'easeOut' }}
          >
            <span className={styles.chapterLabel}>Chapter {chapterNumber}</span>
            <h2 className={styles.chapterHeading}>{chapterTitle}</h2>
            <div className={styles.chapterOrnament}>❧</div>
          </motion.div>
        </header>
      );
    }

    paragraphs.forEach((paragraph, pIdx) => {
      // Insert visualization before some paragraphs
      if (vizIndex < visualizations.length && pIdx > 0 && pIdx % 3 === 0) {
        const viz = visualizations[vizIndex];
        const position = getVizPosition(vizIndex);
        
        elements.push(
          <VisualizationBlock
            key={viz.id}
            visualization={{ ...viz, position }}
            size={preferences.imageSize}
            onRemove={() => onRemoveVisualization(viz.id)}
            onRegenerate={() => onRegenerateVisualization(viz.id)}
            isGenerating={viz.status === 'generating' || viz.status === 'pending'}
          />
        );
        vizIndex++;
      }

      // Determine if this paragraph is focused
      const isFocused = settings.focusMode !== 'off' && focusedParagraph === pIdx;
      const isDimmed = settings.focusMode !== 'off' && focusedParagraph !== null && focusedParagraph !== pIdx;

      elements.push(
        <motion.p 
          key={`p-${pIdx}`} 
          className={`
            ${pIdx === 0 && isFirstPage ? styles.firstParagraph : styles.paragraph}
            ${spacingClass}
            ${isFocused ? styles.paragraphFocused : ''}
            ${isDimmed ? styles.paragraphDimmed : ''}
          `}
          initial={{ opacity: 0 }}
          animate={{ opacity: isDimmed ? 0.3 : 1 }}
          transition={{ delay: pIdx * 0.02, duration: 0.3 }}
          onClick={() => handleParagraphClick(pIdx)}
          style={{
            fontSize: settings.fontSize,
            lineHeight: settings.lineHeight,
            fontFamily: FONTS[settings.fontFamily].family,
            textAlign: settings.textAlign,
            cursor: settings.focusMode !== 'off' ? 'pointer' : 'auto',
          }}
          data-paragraph={pIdx + 1}
        >
          {settings.showParagraphNumbers && (
            <span className={styles.paragraphNumber}>{pIdx + 1}</span>
          )}
          {paragraph}
        </motion.p>
      );
    });

    // Remaining visualizations
    while (vizIndex < visualizations.length) {
      const viz = visualizations[vizIndex];
      const position = getVizPosition(vizIndex);
      
      elements.push(
        <VisualizationBlock
          key={viz.id}
          visualization={{ ...viz, position }}
          size={preferences.imageSize}
          onRemove={() => onRemoveVisualization(viz.id)}
          onRegenerate={() => onRegenerateVisualization(viz.id)}
          isGenerating={viz.status === 'generating' || viz.status === 'pending'}
        />
      );
      vizIndex++;
    }

    return elements;
  };

  return (
    <article 
      className={`${styles.pageContent} ${settings.viewMode === 'twoColumn' ? styles.twoColumnLayout : ''}`}
    >
      {renderContent()}
      <div className={styles.clearFloat} />
    </article>
  );
});

// ---------- Selection Popup ----------
const SelectionPopup: React.FC<{
  selection: TextSelection | null;
  onVisualize: () => void;
  onBookmark: () => void;
  onAnnotate: () => void;
  onClose: () => void;
  isGenerating: boolean;
  preferences: UserPreferences;
}> = memo(({ selection, onVisualize, onBookmark, onAnnotate, onClose, isGenerating, preferences }) => {
  if (!selection || !selection.rect) return null;

  return (
    <motion.div
      className={styles.selectionPopup}
      initial={{ opacity: 0, y: 10, scale: 0.9 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      exit={{ opacity: 0, y: 10, scale: 0.9 }}
      style={{
        top: selection.rect.top - 80,
        left: selection.rect.left + selection.rect.width / 2,
      }}
    >
      <div className={styles.popupPreview}>
        "{selection.text.substring(0, 80)}{selection.text.length > 80 ? '...' : ''}"
      </div>
      <div className={styles.popupActions}>
        <button
          className={styles.visualizeButton}
          onClick={onVisualize}
          disabled={isGenerating}
        >
          {isGenerating ? (
            <>
              <span className={styles.btnSpinner} />
              Creating...
            </>
          ) : (
            <>
              ✨ Visualize
              <span className={styles.positionHint}>({preferences.imagePosition})</span>
            </>
          )}
        </button>
        <button className={styles.popupBtn} onClick={onBookmark} title="Bookmark">
          🔖
        </button>
        <button className={styles.popupBtn} onClick={onAnnotate} title="Add Note">
          📝
        </button>
        <button className={styles.closePopupBtn} onClick={onClose}>
          ✕
        </button>
      </div>
    </motion.div>
  );
});

// ---------- Reading Stats Footer ----------
const ReadingStatsFooter: React.FC = memo(() => {
  const { stats, book, currentChapterIndex, currentPageIndex, settings } = useReader();
  const currentChapter = book?.chapters[currentChapterIndex];
  
  // Calculate progress
  const totalPages = book?.chapters.reduce((sum, ch) => sum + ch.pageCount, 0) || 0;
  const currentAbsolute = book?.chapters
    .slice(0, currentChapterIndex)
    .reduce((sum, ch) => sum + ch.pageCount, 0) || 0;
  const progress = totalPages > 0 ? ((currentAbsolute + currentPageIndex + 1) / totalPages) * 100 : 0;

  const estimatedTimeLeft = useMemo(() => {
    if (!book || stats.averageSpeed === 0) return 'Calculating...';
    const totalWords = book.chapters.reduce((sum, ch) => sum + (ch.wordCount || 0), 0);
    const wordsRead = stats.wordsRead;
    const wordsLeft = totalWords - wordsRead;
    const minutesLeft = Math.ceil(wordsLeft / Math.max(stats.averageSpeed, 150));
    if (minutesLeft < 60) return `${minutesLeft}m left`;
    return `${Math.floor(minutesLeft / 60)}h ${minutesLeft % 60}m left`;
  }, [book, stats]);

  return (
    <div className={styles.statsFooter}>
      <div className={styles.statItem}>
        <span className={styles.statIcon}>📖</span>
        <span>{stats.pagesRead} pages</span>
      </div>
      <div className={styles.statItem}>
        <span className={styles.statIcon}>⚡</span>
        <span>{stats.averageSpeed || '—'} wpm</span>
      </div>
      <div className={styles.statItem}>
        <span className={styles.statIcon}>⏱️</span>
        <span>{formatTime(stats.totalTimeSeconds)}</span>
      </div>
      <div className={styles.statItem}>
        <span className={styles.statIcon}>⏳</span>
        <span>{estimatedTimeLeft}</span>
      </div>
      <div className={styles.statItem}>
        <span className={styles.statIcon}>📊</span>
        <span>{Math.round(progress)}% complete</span>
      </div>
    </div>
  );
});

// ---------- Page Navigation ----------
const PageNavigation: React.FC<{
  onPrev: () => void;
  onNext: () => void;
  canPrev: boolean;
  canNext: boolean;
  currentPage: number;
  totalPages: number;
}> = memo(({ onPrev, onNext, canPrev, canNext, currentPage, totalPages }) => {
  return (
    <nav className={styles.pageNav}>
      <button
        className={`${styles.navBtn} ${styles.navPrev}`}
        onClick={onPrev}
        disabled={!canPrev}
      >
        <span>←</span>
        <span>Previous</span>
      </button>
      <div className={styles.pageIndicator}>
        <span className={styles.pageNum}>{currentPage}</span>
        <span className={styles.pageSeparator}>/</span>
        <span className={styles.pageTotal}>{totalPages}</span>
      </div>
      <button
        className={`${styles.navBtn} ${styles.navNext}`}
        onClick={onNext}
        disabled={!canNext}
      >
        <span>Next</span>
        <span>→</span>
      </button>
    </nav>
  );
});

// ==================== MAIN READER COMPONENT ====================

const ReaderPage: React.FC = () => {
  const { bookId } = useParams<{ bookId: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  // Core State
  const [book, setBook] = useState<Book | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Navigation State
  const [currentChapterIndex, setCurrentChapterIndex] = useState(
    parseInt(searchParams.get('chapter') || '0')
  );
  const [currentPageIndex, setCurrentPageIndex] = useState(0);

  // UI State
  const [settings, setSettings] = useState<ReaderSettings>(getStoredSettings);
  const [toolbarVisible, setToolbarVisible] = useState(true);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const [bookmarksOpen, setBookmarksOpen] = useState(false);

  // Features State
  const [bookmarks, setBookmarks] = useState<Bookmark[]>([]);
  const [annotations, setAnnotations] = useState<Annotation[]>([]);
  const [textSelection, setTextSelection] = useState<TextSelection | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);

  // Stats State
  const [stats, setStats] = useState<ReadingStats>({
    totalTimeSeconds: 0,
    pagesRead: 0,
    wordsRead: 0,
    averageSpeed: 200,
    currentSessionStart: new Date(),
    currentSessionPages: 0,
  });

  // Visualization Preferences
  const [preferences, setPreferences] = useState<UserPreferences>({
    mode: 'UserSelected',
    imagePosition: 'right',
    imageSize: 'medium',
    autoGenerate: false,
  });

  // Refs
  const contentRef = useRef<HTMLDivElement>(null);
  const lastScrollY = useRef(0);
  const readingTimer = useRef<NodeJS.Timeout | null>(null);
  const lastInteraction = useRef(Date.now());

  // ==================== VISUALIZATION HOOK ====================
  const {
    isGenerating,
    error: vizError,
    generateFromSelection,
    removeVisualization,
    regenerateVisualization,
    getVisualizationsForPage,
    loadExistingVisualizations,
    clearError: clearVizError,
  } = useVisualization({
    bookId: bookId || '',
    defaultPosition: preferences.imagePosition,
  });

  // Current data
  const currentChapter = book?.chapters[currentChapterIndex];
  const currentPage = currentChapter?.pages?.[currentPageIndex];
  const currentVizs = currentPage ? getVisualizationsForPage(currentPage.id) : [];

  // ==================== LOAD BOOK ====================
  useEffect(() => {
    const loadBook = async () => {
      if (!bookId) {
        setError('No book ID');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const chapterNum = parseInt(searchParams.get('chapter') || '1');
        
        let bookData;
        try {
          bookData = await catalogService.getBookForReading(bookId, chapterNum);
        } catch {
          const basicBook = await catalogService.getBookById(bookId);
          bookData = {
            id: basicBook.id,
            title: basicBook.title,
            authorId: basicBook.author?.id || basicBook.authorId || '',
            authorName: basicBook.author?.displayName || basicBook.authorName || 'Unknown',
            coverImageUrl: basicBook.coverImageUrl,
            visualizationMode: basicBook.visualizationMode || 'UserSelected',
            chapters: [],
          };
        }
        
        const mappedChapters: Chapter[] = (bookData.chapters || []).map(ch => ({
          id: ch.id,
          bookId: bookData.id,
          title: ch.title,
          number: ch.chapterNumber,
          pageCount: ch.pageCount,
          wordCount: ch.wordCount,
          pages: ch.pages?.map(p => ({
            id: p.id,
            chapterId: ch.id,
            number: p.pageNumber,
            content: p.content,
            wordCount: p.wordCount,
            visualizations: [],
          })),
        }));
        
        setBook({
          id: bookData.id,
          title: bookData.title,
          author: { 
            id: bookData.authorId || '', 
            name: bookData.authorName || 'Unknown Author' 
          },
          coverImageUrl: bookData.coverImageUrl,
          visualizationMode: (bookData.visualizationMode as VisualizationMode) || 'UserSelected',
          chapters: mappedChapters,
          totalPages: mappedChapters.reduce((sum, ch) => sum + ch.pageCount, 0),
          totalWords: mappedChapters.reduce((sum, ch) => sum + (ch.wordCount || 0), 0),
        });

        // Load bookmarks and annotations
        setBookmarks(getStoredBookmarks(bookId));
        setAnnotations(getStoredAnnotations(bookId));
        
        // Try to load existing visualizations (optional - may fail if not authorized)
        try {
          await loadExistingVisualizations();
        } catch (vizErr) {
          console.log('Could not load existing visualizations:', vizErr);
          // This is fine - user can still generate new ones when authorized
        }
        
      } catch (err) {
        console.error('Failed to load book:', err);
        setError(err instanceof Error ? err.message : 'Failed to load book');
      } finally {
        setLoading(false);
      }
    };

    loadBook();
  }, [bookId, searchParams]); // eslint-disable-line react-hooks/exhaustive-deps

  // ==================== SETTINGS PERSISTENCE ====================
  const updateSettings = useCallback((newSettings: Partial<ReaderSettings>) => {
    setSettings(prev => {
      const updated = { ...prev, ...newSettings };
      saveSettings(updated);
      return updated;
    });
  }, []);

  // ==================== BOOKMARKS ====================
  const addBookmark = useCallback((bookmark: Omit<Bookmark, 'id' | 'createdAt'>) => {
    if (!bookId) return;
    const newBookmark: Bookmark = {
      ...bookmark,
      id: generateId(),
      createdAt: new Date(),
    };
    setBookmarks(prev => {
      const updated = [...prev, newBookmark];
      saveBookmarks(bookId, updated);
      return updated;
    });
  }, [bookId]);

  const removeBookmark = useCallback((id: string) => {
    if (!bookId) return;
    setBookmarks(prev => {
      const updated = prev.filter(b => b.id !== id);
      saveBookmarks(bookId, updated);
      return updated;
    });
  }, [bookId]);

  // ==================== ANNOTATIONS ====================
  const addAnnotation = useCallback((annotation: Omit<Annotation, 'id' | 'createdAt'>) => {
    if (!bookId) return;
    const newAnnotation: Annotation = {
      ...annotation,
      id: generateId(),
      createdAt: new Date(),
    };
    setAnnotations(prev => {
      const updated = [...prev, newAnnotation];
      saveAnnotations(bookId, updated);
      return updated;
    });
  }, [bookId]);

  const removeAnnotation = useCallback((id: string) => {
    if (!bookId) return;
    setAnnotations(prev => {
      const updated = prev.filter(a => a.id !== id);
      saveAnnotations(bookId, updated);
      return updated;
    });
  }, [bookId]);

  // ==================== NAVIGATION ====================
  const goToPage = useCallback((chapterIndex: number, pageIndex: number) => {
    setCurrentChapterIndex(chapterIndex);
    setCurrentPageIndex(pageIndex);
    window.scrollTo({ top: 0, behavior: 'smooth' });
    
    // Update stats
    setStats(prev => ({
      ...prev,
      pagesRead: prev.pagesRead + 1,
      currentSessionPages: prev.currentSessionPages + 1,
    }));
  }, []);

  const goToPrevPage = useCallback(() => {
    if (currentPageIndex > 0) {
      goToPage(currentChapterIndex, currentPageIndex - 1);
    } else if (currentChapterIndex > 0) {
      const prevChapter = book?.chapters[currentChapterIndex - 1];
      if (prevChapter) {
        goToPage(currentChapterIndex - 1, prevChapter.pageCount - 1);
      }
    }
  }, [currentChapterIndex, currentPageIndex, book, goToPage]);

  const goToNextPage = useCallback(() => {
    const currentChapter = book?.chapters[currentChapterIndex];
    if (!currentChapter) return;

    if (currentPageIndex < currentChapter.pageCount - 1) {
      goToPage(currentChapterIndex, currentPageIndex + 1);
    } else if (currentChapterIndex < (book?.chapters.length || 0) - 1) {
      goToPage(currentChapterIndex + 1, 0);
    }
  }, [currentChapterIndex, currentPageIndex, book, goToPage]);

  const canGoPrev = currentChapterIndex > 0 || currentPageIndex > 0;
  const canGoNext = book 
    ? currentChapterIndex < book.chapters.length - 1 || 
      currentPageIndex < (book.chapters[currentChapterIndex]?.pageCount || 0) - 1
    : false;

  // ==================== SEARCH ====================
  useEffect(() => {
    if (!searchQuery || !book) {
      setSearchResults([]);
      return;
    }

    const query = searchQuery.toLowerCase();
    const results: SearchResult[] = [];

    book.chapters.forEach((chapter, chIdx) => {
      chapter.pages?.forEach((page, pIdx) => {
        const content = page.content.toLowerCase();
        let matchIndex = content.indexOf(query);
        
        while (matchIndex !== -1) {
          const start = Math.max(0, matchIndex - 50);
          const end = Math.min(content.length, matchIndex + query.length + 50);
          const excerpt = '...' + page.content.substring(start, end) + '...';
          
          results.push({
            chapterIndex: chIdx,
            pageIndex: pIdx,
            excerpt,
            matchIndex,
          });
          
          matchIndex = content.indexOf(query, matchIndex + 1);
        }
      });
    });

    setSearchResults(results.slice(0, 50)); // Limit results
  }, [searchQuery, book]);

  // ==================== READING TIMER ====================
  useEffect(() => {
    readingTimer.current = setInterval(() => {
      const now = Date.now();
      if (now - lastInteraction.current < 60000) { // Active within 1 minute
        setStats(prev => ({
          ...prev,
          totalTimeSeconds: prev.totalTimeSeconds + 1,
        }));
      }
    }, 1000);

    return () => {
      if (readingTimer.current) clearInterval(readingTimer.current);
    };
  }, []);

  // Update interaction timestamp
  useEffect(() => {
    const handleInteraction = () => {
      lastInteraction.current = Date.now();
    };

    window.addEventListener('mousemove', handleInteraction);
    window.addEventListener('keydown', handleInteraction);
    window.addEventListener('scroll', handleInteraction);
    window.addEventListener('touchstart', handleInteraction);

    return () => {
      window.removeEventListener('mousemove', handleInteraction);
      window.removeEventListener('keydown', handleInteraction);
      window.removeEventListener('scroll', handleInteraction);
      window.removeEventListener('touchstart', handleInteraction);
    };
  }, []);

  // ==================== TOOLBAR AUTO-HIDE ====================
  useEffect(() => {
    if (!settings.autoHideToolbar) return;

    const handleScroll = () => {
      const currentScrollY = window.scrollY;
      if (currentScrollY > lastScrollY.current && currentScrollY > 100) {
        setToolbarVisible(false);
      } else {
        setToolbarVisible(true);
      }
      lastScrollY.current = currentScrollY;
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, [settings.autoHideToolbar]);

  // ==================== KEYBOARD SHORTCUTS ====================
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Don't handle if typing in input
      if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) return;

      switch (e.key) {
        case 'ArrowLeft':
        case 'PageUp':
          e.preventDefault();
          goToPrevPage();
          break;
        case 'ArrowRight':
        case 'PageDown':
        case ' ':
          if (!e.shiftKey) {
            e.preventDefault();
            goToNextPage();
          }
          break;
        case 'f':
          if (e.ctrlKey || e.metaKey) {
            e.preventDefault();
            setSearchOpen(true);
          } else {
            // Toggle fullscreen
            e.preventDefault();
            toggleFullscreen();
          }
          break;
        case 'Escape':
          setSearchOpen(false);
          setSettingsOpen(false);
          setSidebarOpen(false);
          setBookmarksOpen(false);
          setTextSelection(null);
          // Exit fullscreen if active
          if (document.fullscreenElement) {
            document.exitFullscreen();
          }
          break;
        case 's':
          if (e.ctrlKey || e.metaKey) {
            e.preventDefault();
            setSettingsOpen(prev => !prev);
          }
          break;
        case 't':
          if (e.ctrlKey || e.metaKey) {
            e.preventDefault();
            setSidebarOpen(prev => !prev);
          }
          break;
        case 'i':
          // Toggle immersive mode
          if (e.ctrlKey || e.metaKey) {
            e.preventDefault();
            updateSettings({ immersiveMode: !settings.immersiveMode });
          }
          break;
        case 'Home':
          e.preventDefault();
          goToPage(0, 0);
          break;
        case 'End':
          e.preventDefault();
          if (book) {
            const lastChapter = book.chapters.length - 1;
            const lastPage = book.chapters[lastChapter].pageCount - 1;
            goToPage(lastChapter, lastPage);
          }
          break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [goToPrevPage, goToNextPage, goToPage, book, settings.immersiveMode, updateSettings]);

  // ==================== FULLSCREEN ====================
  const toggleFullscreen = useCallback(() => {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen().catch(console.error);
    } else {
      document.exitFullscreen().catch(console.error);
    }
  }, []);

  // ==================== AUTO-SCROLL ====================
  useEffect(() => {
    if (!settings.autoScroll) return;

    const scrollSpeed = settings.autoScrollSpeed / 60; // Convert WPM to pixels per second approximation
    const pixelsPerFrame = scrollSpeed * 0.5;
    let animationId: number;

    const scroll = () => {
      window.scrollBy(0, pixelsPerFrame);
      
      // Check if we've reached the bottom
      if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 100) {
        goToNextPage();
        window.scrollTo(0, 0);
      }
      
      animationId = requestAnimationFrame(scroll);
    };

    animationId = requestAnimationFrame(scroll);
    
    return () => cancelAnimationFrame(animationId);
  }, [settings.autoScroll, settings.autoScrollSpeed, goToNextPage]);

  // ==================== TEXT SELECTION ====================
  const handleTextSelection = useCallback(() => {
    const selection = window.getSelection();
    if (!selection || selection.isCollapsed || !currentPage) return;

    const text = selection.toString().trim();
    if (text.length < 5) return;

    const range = selection.getRangeAt(0);
    const rect = range.getBoundingClientRect();
    const contentRect = contentRef.current?.getBoundingClientRect();

    if (!contentRect) return;

    setTextSelection({
      text,
      pageId: currentPage.id,
      rect: new DOMRect(
        rect.left - contentRect.left,
        rect.top - contentRect.top + window.scrollY,
        rect.width,
        rect.height
      ),
      startOffset: range.startOffset,
      endOffset: range.endOffset,
    });
  }, [currentPage]);

  useEffect(() => {
    document.addEventListener('mouseup', handleTextSelection);
    return () => document.removeEventListener('mouseup', handleTextSelection);
  }, [handleTextSelection]);

  // ==================== VISUALIZATION ACTIONS ====================
  const handleVisualize = useCallback(async () => {
    if (!textSelection || !currentPage) return;
    
    // Convert 'alternate' to actual position for API call
    const actualPosition = preferences.imagePosition === 'alternate' 
      ? 'right' 
      : preferences.imagePosition;
    
    await generateFromSelection(
      textSelection.text,
      currentPage.id,
      { position: actualPosition }
    );
    
    setTextSelection(null);
    window.getSelection()?.removeAllRanges();
  }, [textSelection, currentPage, generateFromSelection, preferences.imagePosition]);

  const handleBookmarkSelection = useCallback(() => {
    if (!textSelection || !currentPage) return;
    
    addBookmark({
      pageId: currentPage.id,
      chapterIndex: currentChapterIndex,
      pageIndex: currentPageIndex,
      selectedText: textSelection.text,
      color: BOOKMARK_COLORS[Math.floor(Math.random() * BOOKMARK_COLORS.length)],
    });
    
    setTextSelection(null);
    window.getSelection()?.removeAllRanges();
  }, [textSelection, currentPage, currentChapterIndex, currentPageIndex, addBookmark]);

  const handleAnnotateSelection = useCallback(() => {
    if (!textSelection || !currentPage) return;
    
    const note = prompt('Add a note:');
    if (!note) return;
    
    addAnnotation({
      pageId: currentPage.id,
      text: textSelection.text,
      note,
      color: BOOKMARK_COLORS[Math.floor(Math.random() * BOOKMARK_COLORS.length)],
      startOffset: textSelection.startOffset,
      endOffset: textSelection.endOffset,
    });
    
    setTextSelection(null);
    window.getSelection()?.removeAllRanges();
  }, [textSelection, currentPage, addAnnotation]);

  // ==================== CONTEXT VALUE ====================
  const contextValue: ReaderContextType = useMemo(() => ({
    book,
    settings,
    updateSettings,
    currentChapterIndex,
    currentPageIndex,
    goToPage,
    bookmarks,
    addBookmark,
    removeBookmark,
    annotations,
    addAnnotation,
    removeAnnotation,
    stats,
    searchResults,
    searchQuery,
    setSearchQuery,
  }), [
    book, settings, updateSettings, currentChapterIndex, currentPageIndex, goToPage,
    bookmarks, addBookmark, removeBookmark, annotations, addAnnotation, removeAnnotation,
    stats, searchResults, searchQuery
  ]);

  // ==================== MARGINS CALCULATION ====================
  const marginClass = {
    narrow: styles.marginsNarrow,
    normal: styles.marginsNormal,
    wide: styles.marginsWide,
  }[settings.margins];

  // ==================== RENDER ====================

  // Loading
  if (loading) {
    return (
      <div className={styles.loadingState} data-theme={settings.theme}>
        <div className={styles.loadingContent}>
          <div className={styles.loadingSpinner} />
          <h2>Opening your book...</h2>
          <p>Preparing your reading experience</p>
        </div>
      </div>
    );
  }

  // Error
  if (error || !book) {
    return (
      <div className={styles.errorState} data-theme={settings.theme}>
        <span className={styles.errorIcon}>📚</span>
        <h2>Unable to Load Book</h2>
        <p>{error || 'Book not found'}</p>
        <button onClick={() => navigate(-1)} className={styles.backBtn}>
          ← Go Back
        </button>
      </div>
    );
  }

  // Calculate total pages for navigation
  const totalPagesInChapter = currentChapter?.pageCount || 0;
  const totalPagesInBook = book.chapters.reduce((sum, ch) => sum + ch.pageCount, 0);
  const currentAbsolutePage = book.chapters
    .slice(0, currentChapterIndex)
    .reduce((sum, ch) => sum + ch.pageCount, 0) + currentPageIndex + 1;

  return (
    <ReaderContext.Provider value={contextValue}>
      <div 
        className={`${styles.readerPage} ${settings.immersiveMode ? styles.immersiveMode : ''}`}
        data-theme={settings.theme}
        data-view={settings.viewMode}
        data-focus={settings.focusMode !== 'off'}
      >
        {/* Auto-scroll indicator */}
        {settings.autoScroll && (
          <div className={styles.autoScrollIndicator}>
            <span>⏩</span> Auto-scrolling at {settings.autoScrollSpeed} wpm
            <button onClick={() => updateSettings({ autoScroll: false })}>Stop</button>
          </div>
        )}

        {/* Toolbar */}
        {!settings.immersiveMode && (
          <Toolbar
            book={book}
            visible={toolbarVisible}
            onBack={() => navigate(`/books/${bookId}`)}
            onToggleSidebar={() => setSidebarOpen(prev => !prev)}
            onToggleSettings={() => setSettingsOpen(prev => !prev)}
            onToggleSearch={() => setSearchOpen(prev => !prev)}
            onToggleBookmarks={() => setBookmarksOpen(prev => !prev)}
            sidebarOpen={sidebarOpen}
            settingsOpen={settingsOpen}
            searchOpen={searchOpen}
            bookmarksOpen={bookmarksOpen}
          />
        )}

        {/* Chapter Sidebar */}
        <ChapterSidebar
          book={book}
          isOpen={sidebarOpen}
          onClose={() => setSidebarOpen(false)}
          onNavigate={(chIdx, pIdx) => {
            goToPage(chIdx, pIdx || 0);
            setSidebarOpen(false);
          }}
        />

        {/* Settings Panel */}
        <SettingsPanel
          isOpen={settingsOpen}
          onClose={() => setSettingsOpen(false)}
        />

        {/* Search Panel */}
        <SearchPanel
          isOpen={searchOpen}
          onClose={() => setSearchOpen(false)}
          onNavigate={(chIdx, pIdx) => {
            goToPage(chIdx, pIdx);
            setSearchOpen(false);
          }}
        />

        {/* Bookmarks Panel */}
        <BookmarksPanel
          isOpen={bookmarksOpen}
          onClose={() => setBookmarksOpen(false)}
          onNavigate={(chIdx, pIdx) => {
            goToPage(chIdx, pIdx);
            setBookmarksOpen(false);
          }}
        />

        {/* Mini Map */}
        <MiniMap
          book={book}
          currentChapter={currentChapterIndex}
          currentPage={currentPageIndex}
          onNavigate={goToPage}
        />

        {/* Main Content */}
        <main className={`${styles.readerMain} ${marginClass}`}>
          <div 
            ref={contentRef}
            className={styles.readerContent}
          >
            {currentPage ? (
              <PageContent
                page={currentPage}
                visualizations={currentVizs}
                preferences={preferences}
                onRemoveVisualization={(id) => removeVisualization(currentPage.id, id)}
                onRegenerateVisualization={(id) => regenerateVisualization(currentPage.id, id)}
                isFirstPage={currentPageIndex === 0}
                chapterTitle={currentChapter?.title}
                chapterNumber={currentChapter?.number}
                annotations={annotations.filter(a => a.pageId === currentPage.id)}
              />
            ) : (
              <div className={styles.noContent}>
                <p>No content available for this page.</p>
              </div>
            )}

            {/* Visualization Prompt */}
            {currentVizs.length === 0 && book.visualizationMode === 'UserSelected' && (
              <div className={styles.vizPrompt}>
                <span>✨</span>
                <p>Select text to generate AI visualizations</p>
              </div>
            )}
          </div>

          {/* Page Navigation */}
          <PageNavigation
            onPrev={goToPrevPage}
            onNext={goToNextPage}
            canPrev={canGoPrev}
            canNext={canGoNext}
            currentPage={currentAbsolutePage}
            totalPages={totalPagesInBook}
          />
        </main>

        {/* Reading Stats Footer */}
        {!settings.immersiveMode && <ReadingStatsFooter />}

        {/* Floating Action Buttons */}
        {!settings.immersiveMode && (
          <div className={styles.floatingActions}>
            <button 
              className={`${styles.fab} ${settings.autoScroll ? styles.active : ''}`}
              onClick={() => updateSettings({ autoScroll: !settings.autoScroll })}
              title="Auto-scroll"
            >
              {settings.autoScroll ? '⏸️' : '▶️'}
              <span className={styles.fabTooltip}>
                {settings.autoScroll ? 'Stop auto-scroll' : 'Start auto-scroll'}
              </span>
            </button>
            <button 
              className={styles.fab}
              onClick={toggleFullscreen}
              title="Fullscreen"
            >
              ⛶
              <span className={styles.fabTooltip}>Fullscreen (F)</span>
            </button>
            <button 
              className={`${styles.fab} ${settings.immersiveMode ? styles.active : ''}`}
              onClick={() => updateSettings({ immersiveMode: !settings.immersiveMode })}
              title="Immersive mode"
            >
              🧘
              <span className={styles.fabTooltip}>Immersive mode (Ctrl+I)</span>
            </button>
          </div>
        )}

        {/* Selection Popup */}
        <AnimatePresence>
          {textSelection && (
            <SelectionPopup
              selection={textSelection}
              onVisualize={handleVisualize}
              onBookmark={handleBookmarkSelection}
              onAnnotate={handleAnnotateSelection}
              onClose={() => {
                setTextSelection(null);
                window.getSelection()?.removeAllRanges();
              }}
              isGenerating={isGenerating}
              preferences={preferences}
            />
          )}
        </AnimatePresence>

        {/* Click Areas for Navigation */}
        {settings.viewMode === 'paginated' && (
          <div className={styles.clickAreas}>
            <div 
              className={styles.clickPrev} 
              onClick={goToPrevPage}
              style={{ cursor: canGoPrev ? 'w-resize' : 'default' }}
            />
            <div 
              className={styles.clickNext} 
              onClick={goToNextPage}
              style={{ cursor: canGoNext ? 'e-resize' : 'default' }}
            />
          </div>
        )}
      </div>
    </ReaderContext.Provider>
  );
};

export default ReaderPage;
