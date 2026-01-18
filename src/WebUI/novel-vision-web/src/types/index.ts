// src/types/index.ts
// NovelVision Type Definitions

// ==================== AUTH ====================
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  displayName?: string;
  role?: 'Reader' | 'Author';
}

export interface AuthResult {
  succeeded: boolean;
  token?: string;
  refreshToken?: string;
  expiresIn?: number;
  user?: User;
  error?: string;
  errors?: string[];
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  avatarUrl?: string;
  role: 'Reader' | 'Author' | 'Admin';
  isEmailConfirmed: boolean;
  createdAt: string;
  updatedAt?: string;
}

// ==================== BOOKS ====================
export interface Book {
  id: string;
  title: string;
  description: string;
  authorId: string;
  authorName?: string;
  author?: Author;
  coverImageUrl?: string;
  language: string;
  pageCount: number;
  wordCount: number;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  edition?: string;
  genres: string[];
  tags: string[];
  rating: number;
  reviewCount: number;
  isPublished: boolean;
  isFeatured?: boolean;
  visualizationEnabled?: boolean;
  visualizationMode?: VisualizationMode;
  createdAt: string;
  updatedAt: string;
}

export interface Author {
  id: string;
  displayName: string;
  email: string;
  biography?: string;
  avatarUrl?: string;
  isVerified: boolean;
  bookCount?: number;
  followerCount?: number;
  socialLinks?: Record<string, string>;
  createdAt: string;
  updatedAt?: string;
}

export interface Chapter {
  id: string;
  bookId: string;
  chapterNumber: number;
  title: string;
  content?: string;
  summary?: string;
  pageCount: number;
  wordCount?: number;
  estimatedReadTime?: number;
  hasVisualization?: boolean;
  createdAt: string;
}

export interface Page {
  id: string;
  chapterId: string;
  bookId?: string;
  pageNumber: number;
  content: string;
  wordCount: number;
  hasVisualization?: boolean;
  visualizationUrl?: string;
}

// ==================== VISUALIZATION ====================
export type VisualizationMode = 
  | 'None' 
  | 'PerPage' 
  | 'PerChapter' 
  | 'UserSelected' 
  | 'AuthorDefined';

export type AIProvider = 
  | 'dalle3' 
  | 'midjourney' 
  | 'stable-diffusion' 
  | 'flux';

export type ArtStyle = 
  | 'realistic' 
  | 'anime' 
  | 'oil_painting' 
  | 'watercolor' 
  | 'fantasy' 
  | 'sketch' 
  | 'concept_art' 
  | 'digital_art'
  | 'cinematic'
  | '3d';

export type JobStatus = 
  | 'Pending' 
  | 'Queued' 
  | 'GeneratingPrompt' 
  | 'Processing' 
  | 'Uploading' 
  | 'Completed' 
  | 'Failed' 
  | 'Cancelled';

export type VisualizationTrigger = 
  | 'Button' 
  | 'TextSelection' 
  | 'AutoNovel' 
  | 'AuthorDefined' 
  | 'PerChapter' 
  | 'PerPage';

export interface VisualizationJob {
  id: string;
  bookId: string;
  pageId?: string;
  chapterId?: string;
  userId: string;
  trigger: VisualizationTrigger;
  status: JobStatus;
  provider: AIProvider;
  style?: ArtStyle;
  images: GeneratedImage[];
  promptData?: PromptData;
  queuePosition?: number;
  progress?: number;
  errorMessage?: string;
  createdAt: string;
  completedAt?: string;
}

export interface GeneratedImage {
  id: string;
  imageUrl: string;
  thumbnailUrl?: string;
  width: number;
  height: number;
  isSelected: boolean;
  createdAt: string;
}

export interface PromptData {
  originalText: string;
  enhancedPrompt: string;
  negativePrompt?: string;
  targetModel: string;
  style?: string;
}

export interface TextSelection {
  selectedText: string;
  startPosition: number;
  endPosition: number;
  pageId: string;
  chapterId?: string;
}

export interface VisualizationSettings {
  mode: VisualizationMode;
  preferredProvider: AIProvider;
  artStyle: ArtStyle;
  autoVisualize: boolean;
  showProgress: boolean;
}

// ==================== READER ====================
export type ReaderTheme = 'light' | 'dark' | 'sepia' | 'midnight';
export type PageWidth = 'narrow' | 'medium' | 'wide';
export type TextAlign = 'left' | 'justify';

export interface ReaderSettings {
  fontSize: number;
  fontFamily: string;
  lineHeight: number;
  theme: ReaderTheme;
  pageWidth: PageWidth;
  textAlign: TextAlign;
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
  totalReadingTime: number;
}

export interface Bookmark {
  id: string;
  pageId: string;
  pageNumber: number;
  chapterTitle: string;
  note?: string;
  createdAt: string;
}

// ==================== API RESPONSES ====================
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// ==================== UI ====================
export interface SelectOption {
  value: string;
  label: string;
  icon?: string;
  description?: string;
  disabled?: boolean;
}

export interface TabItem {
  id: string;
  label: string;
  icon?: React.ReactNode;
  badge?: string | number;
}

export interface MenuItem {
  id: string;
  label: string;
  icon?: React.ReactNode;
  href?: string;
  onClick?: () => void;
  children?: MenuItem[];
  badge?: string | number;
  disabled?: boolean;
}

// ==================== CONSTANTS ====================
export const READER_THEMES = {
  light: {
    background: '#ffffff',
    text: '#1a1a1a',
    secondary: '#666666',
    accent: '#a855f7',
    surface: '#f5f5f5',
    border: '#e0e0e0'
  },
  dark: {
    background: '#09090f',
    text: '#fafafa',
    secondary: '#a1a1aa',
    accent: '#a855f7',
    surface: '#16161f',
    border: '#27272a'
  },
  sepia: {
    background: '#f4ecd8',
    text: '#5c4b37',
    secondary: '#8b7355',
    accent: '#c9a227',
    surface: '#ebe3d0',
    border: '#d4c9b0'
  },
  midnight: {
    background: '#0a0a12',
    text: '#d4d4dc',
    secondary: '#8888a0',
    accent: '#818cf8',
    surface: '#12121e',
    border: '#1e1e30'
  }
} as const;

export const PAGE_WIDTHS = {
  narrow: '580px',
  medium: '720px',
  wide: '900px'
} as const;

export const FONT_OPTIONS: SelectOption[] = [
  { value: 'Georgia, serif', label: 'Georgia' },
  { value: '"Merriweather", serif', label: 'Merriweather' },
  { value: '"Lora", serif', label: 'Lora' },
  { value: '"Playfair Display", serif', label: 'Playfair Display' },
  { value: '"Inter", sans-serif', label: 'Inter' },
  { value: '"Source Sans Pro", sans-serif', label: 'Source Sans Pro' },
  { value: 'system-ui, sans-serif', label: 'System Font' }
];

export const ART_STYLE_OPTIONS: SelectOption[] = [
  { value: 'realistic', label: 'Realistic', icon: '📷', description: 'Photorealistic' },
  { value: 'anime', label: 'Anime', icon: '🎌', description: 'Japanese style' },
  { value: 'oil_painting', label: 'Oil Painting', icon: '🖼️', description: 'Classic art' },
  { value: 'watercolor', label: 'Watercolor', icon: '🎨', description: 'Soft colors' },
  { value: 'fantasy', label: 'Fantasy', icon: '🐉', description: 'Epic scenes' },
  { value: 'digital_art', label: 'Digital Art', icon: '💻', description: 'Modern style' },
  { value: 'cinematic', label: 'Cinematic', icon: '🎬', description: 'Movie-like' },
  { value: 'concept_art', label: 'Concept Art', icon: '🎭', description: 'Game/movie' },
  { value: 'sketch', label: 'Sketch', icon: '✏️', description: 'Hand-drawn' },
  { value: '3d', label: '3D Render', icon: '💎', description: '3D graphics' }
];

export const AI_PROVIDER_OPTIONS: SelectOption[] = [
  { value: 'dalle3', label: 'DALL-E 3', icon: '🎨', description: 'Best quality' },
  { value: 'midjourney', label: 'Midjourney', icon: '🌟', description: 'Artistic' },
  { value: 'stable-diffusion', label: 'Stable Diffusion', icon: '⚡', description: 'Fast' },
  { value: 'flux', label: 'Flux', icon: '✨', description: 'Newest' }
];

export const VISUALIZATION_MODE_OPTIONS: SelectOption[] = [
  { value: 'PerPage', label: 'Every Page', icon: '🖼️', description: 'AI creates illustration for each page' },
  { value: 'PerChapter', label: 'Every Chapter', icon: '📑', description: 'One illustration per chapter' },
  { value: 'UserSelected', label: 'On Demand', icon: '✋', description: 'You choose what to visualize' },
  { value: 'None', label: 'Classic Reading', icon: '📖', description: 'No AI visualization' }
];

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
    showProgress: true
  }
};