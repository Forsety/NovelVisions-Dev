// src/types/reader.types.ts
// Types for Reader functionality

import { VisualizationMode, VisualizationSettings, GeneratedImage } from './visualization.types';

export interface ReaderSettings {
  fontSize: number;
  fontFamily: string;
  lineHeight: number;
  theme: 'light' | 'dark' | 'sepia';
  pageWidth: 'narrow' | 'medium' | 'wide';
  textAlign: 'left' | 'justify';
  showProgress: boolean;
  autoScroll: boolean;
  scrollSpeed: number;
  visualization: VisualizationSettings;
}

export interface ReadingProgress {
  bookId: string;
  userId: string;
  currentChapterId: string;
  currentPageId: string;
  currentPageNumber: number;
  totalPages: number;
  percentComplete: number;
  lastReadAt: string;
  readingTime: number; // in seconds
  bookmarks: Bookmark[];
}

export interface Bookmark {
  id: string;
  pageId: string;
  pageNumber: number;
  chapterTitle: string;
  note?: string;
  createdAt: string;
}

export interface PageContent {
  id: string;
  chapterId: string;
  bookId: string;
  pageNumber: number;
  content: string;
  wordCount: number;
  hasVisualization: boolean;
  visualizationUrl?: string;
  visualizations?: GeneratedImage[];
}

export interface ChapterInfo {
  id: string;
  bookId: string;
  chapterNumber: number;
  title: string;
  pageCount: number;
  wordCount: number;
  estimatedReadTime: number; // in minutes
  isCompleted: boolean;
  pages?: PageContent[];
}

export interface BookReaderData {
  book: {
    id: string;
    title: string;
    authorName: string;
    coverImageUrl?: string;
    visualizationMode: VisualizationMode;
    preferredStyle?: string;
    totalPages: number;
    totalChapters: number;
  };
  chapters: ChapterInfo[];
  progress?: ReadingProgress;
}

export interface ReaderState {
  isLoading: boolean;
  error: string | null;
  book: BookReaderData | null;
  currentChapter: ChapterInfo | null;
  currentPage: PageContent | null;
  settings: ReaderSettings;
  isFullscreen: boolean;
  showChapterList: boolean;
  showSettings: boolean;
  showBookmarks: boolean;
  visualizationInProgress: boolean;
  currentVisualization: GeneratedImage | null;
}

// Default reader settings
export const DEFAULT_READER_SETTINGS: ReaderSettings = {
  fontSize: 18,
  fontFamily: 'Georgia, serif',
  lineHeight: 1.8,
  theme: 'dark',
  pageWidth: 'medium',
  textAlign: 'justify',
  showProgress: true,
  autoScroll: false,
  scrollSpeed: 50,
  visualization: {
    mode: 'UserSelected',
    preferredProvider: 'dalle3',
    artStyle: 'realistic',
    autoVisualize: false,
    showGenerationProgress: true
  }
};

// Theme configurations
export const READER_THEMES = {
  light: {
    background: '#ffffff',
    text: '#1a1a1a',
    secondary: '#666666',
    accent: '#6366f1',
    surface: '#f5f5f5',
    border: '#e0e0e0'
  },
  dark: {
    background: '#0f0f1a',
    text: '#e8e8e8',
    secondary: '#a0a0a0',
    accent: '#a78bfa',
    surface: '#1a1a2e',
    border: '#2d2d44'
  },
  sepia: {
    background: '#f4ecd8',
    text: '#5c4b37',
    secondary: '#8b7355',
    accent: '#c9a227',
    surface: '#ebe3d0',
    border: '#d4c9b0'
  }
};

// Font options
export const FONT_OPTIONS = [
  { value: 'Georgia, serif', label: 'Georgia' },
  { value: '"Merriweather", serif', label: 'Merriweather' },
  { value: '"Lora", serif', label: 'Lora' },
  { value: '"Crimson Text", serif', label: 'Crimson Text' },
  { value: '"Source Sans Pro", sans-serif', label: 'Source Sans Pro' },
  { value: '"Open Sans", sans-serif', label: 'Open Sans' },
  { value: '"Roboto", sans-serif', label: 'Roboto' },
  { value: 'system-ui, sans-serif', label: 'System Font' }
];

// Page width options
export const PAGE_WIDTH_OPTIONS = {
  narrow: '600px',
  medium: '800px',
  wide: '1000px'
};