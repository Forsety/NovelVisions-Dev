// src/types/api.types.ts
// NovelVision API Type Definitions
// FIXED: User supports both 'role' (string) and 'roles' (array)

// ==================== AUTH ====================

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  avatarUrl?: string;
  biography?: string;
  // Single role (fallback for compatibility)
  role?: 'Reader' | 'Author' | 'Admin' | string;
  // Roles array (backend actually returns this)
  roles?: string[];
  isEmailConfirmed?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  acceptTerms?: boolean;
}

export interface AuthResponse {
  succeeded?: boolean;
  success?: boolean;
  accessToken?: string;
  token?: string; // alias for accessToken
  refreshToken?: string;
  expiresAt?: string;
  user?: User;
  message?: string;
  error?: string;
  errors?: string[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
  accessToken?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  displayName?: string;
  biography?: string;
  avatarUrl?: string;
}

// ==================== BOOKS ====================

export type VisualizationMode = 
  | 'None' 
  | 'PerPage' 
  | 'PerChapter' 
  | 'UserSelected' 
  | 'AuthorDefined';

export type CopyrightStatus = 
  | 'Unknown'
  | 'PublicDomain'
  | 'Copyrighted'
  | 'CreativeCommons';

export type BookSource = 
  | 'UserCreated'
  | 'Gutenberg'
  | 'OpenLibrary'
  | 'Import';

export interface Book {
  id: string;
  title: string;
  description: string;
  authorId: string;
  authorName?: string;
  author?: Author;
  coverImageUrl?: string;
  hasCover?: boolean;
  language: string;
  
  // Content stats
  pageCount: number;
  chapterCount: number;
  wordCount: number;
  readingTimeMinutes: number;
  
  // Publication
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  edition?: string;
  
  // Categories
  genres: string[];
  tags: string[];
  
  // Ratings & stats
  rating: number;
  averageRating?: number;
  reviewCount: number;
  downloadCount?: number;
  viewCount?: number;
  
  // Status
  status?: string;
  isPublished: boolean;
  isFeatured?: boolean;
  isFree?: boolean;
  isFreeToUse?: boolean;
  
  // Source & Copyright
  source?: BookSource | string;
  copyrightStatus?: CopyrightStatus | string;
  externalId?: string;
  externalSourceUrl?: string;
  isImported?: boolean;
  
  // Visualization
  hasVisualization: boolean;
  visualizationEnabled?: boolean;
  visualizationMode?: VisualizationMode | string;
  
  // Timestamps
  createdAt: string;
  updatedAt: string;
}

export interface CreateBookRequest {
  title: string;
  description: string;
  authorId?: string;
  coverImageUrl?: string;
  language?: string;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  genres?: string[];
  tags?: string[];
  visualizationMode?: VisualizationMode;
}

export interface UpdateBookRequest {
  title?: string;
  description?: string;
  coverImageUrl?: string;
  language?: string;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  genres?: string[];
  tags?: string[];
  isPublished?: boolean;
  visualizationMode?: VisualizationMode;
}

// ==================== AUTHORS ====================

export interface Author {
  id: string;
  userId?: string;
  displayName: string;
  email?: string;
  biography?: string;
  avatarUrl?: string;
  isVerified: boolean;
  verifiedAt?: string;
  bookCount?: number;
  followerCount?: number;
  socialLinks?: Record<string, string>;
  // Life info (for historical authors)
  birthYear?: number;
  deathYear?: number;
  lifeSpan?: string;
  nationality?: string;
  isAlive?: boolean;
  isHistorical?: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateAuthorRequest {
  displayName: string;
  email?: string;
  biography?: string;
  socialLinks?: Record<string, string>;
}

// ==================== CHAPTERS & PAGES ====================

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
  updatedAt?: string;
}

export interface CreateChapterRequest {
  title: string;
  content?: string;
  summary?: string;
  chapterNumber?: number;
}

export interface Page {
  id: string;
  chapterId: string;
  bookId?: string;
  pageNumber: number;
  globalPageNumber?: number;
  content: string;
  wordCount: number;
  hasVisualization?: boolean;
  visualizationUrl?: string;
  createdAt?: string;
}

export interface CreatePageRequest {
  content: string;
  pageNumber?: number;
}

// ==================== VISUALIZATION ====================

export type VisualizationTrigger = 'Manual' | 'Automatic' | 'OnRead';
export type AIProvider = 'dalle3' | 'midjourney' | 'stable-diffusion' | 'flux';
export type JobStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed' | 'Cancelled';

export interface VisualizationJob {
  id: string;
  bookId: string;
  chapterId?: string;
  pageId?: string;
  status: JobStatus;
  provider: AIProvider;
  prompt: string;
  enhancedPrompt?: string;
  style?: string;
  imageUrl?: string;
  thumbnailUrl?: string;
  errorMessage?: string;
  createdAt: string;
  completedAt?: string;
}

export interface GenerateVisualizationRequest {
  pageId: string;
  provider?: AIProvider;
  style?: string;
  customPrompt?: string;
  enhancePrompt?: boolean;
}

export interface VisualizationStyle {
  id: string;
  name: string;
  description: string;
  previewUrl?: string;
  promptModifiers: string;
}

// ==================== READING ====================

export interface ReadingProgress {
  id: string;
  userId: string;
  bookId: string;
  chapterId?: string;
  pageId?: string;
  currentPage: number;
  totalPages: number;
  progressPercent: number;
  lastReadAt: string;
  startedAt: string;
  completedAt?: string;
}

export interface Bookmark {
  id: string;
  userId: string;
  bookId: string;
  chapterId?: string;
  pageId?: string;
  pageNumber: number;
  note?: string;
  color?: string;
  createdAt: string;
}

// ==================== PAGINATION ====================

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  search?: string;
}

// ==================== API RESPONSE ====================

export interface ApiResponse<T> {
  success: boolean;
  succeeded?: boolean;
  data?: T;
  message?: string;
  error?: string;
  errors?: string[];
  timestamp?: string;
}

export interface ApiError {
  status: number;
  message: string;
  errors?: string[];
  details?: Record<string, unknown>;
}

// ==================== SUBJECTS (Genres/Categories) ====================

export interface Subject {
  id: string;
  name: string;
  slug: string;
  type: 'Genre' | 'Tag' | 'Category';
  description?: string;
  parentId?: string;
  bookCount: number;
  createdAt: string;
}

// ==================== GUTENBERG ====================

export interface GutenbergBook {
  id: number;
  title: string;
  authors: GutenbergAuthor[];
  subjects: string[];
  bookshelves: string[];
  languages: string[];
  copyright: boolean;
  mediaType: string;
  downloadCount: number;
  formats: Record<string, string>;
}

export interface GutenbergAuthor {
  name: string;
  birthYear?: number;
  deathYear?: number;
}

export interface GutenbergSearchResult {
  count: number;
  next?: string;
  previous?: string;
  results: GutenbergBook[];
}

export interface ImportGutenbergRequest {
  gutenbergId: number;
  enableVisualization?: boolean;
  visualizationMode?: VisualizationMode;
}

// ==================== BECOME AUTHOR ====================

export interface BecomeAuthorRequest {
  displayName?: string;
  biography?: string;
}

export interface BecomeAuthorResponse {
  success: boolean;
  succeeded?: boolean;
  message?: string;
  error?: string;
  authorId?: string;
  user?: User;
}