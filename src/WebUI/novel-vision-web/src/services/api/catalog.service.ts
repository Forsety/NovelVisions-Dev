// src/services/api/catalog.service.ts
// Catalog API Service - Books, Authors, Chapters, Pages

import { apiClient } from './apiClient';
import { getEndpoints } from '../../shared/constants/api';
import type {
  Book,
  Author,
  Chapter,
  Page,
  PaginatedResponse,
  PaginationParams,
  CreateBookRequest,
  UpdateBookRequest,
} from '../../types/api.types';

const endpoints = getEndpoints();

// ==================== FILTERS ====================

export interface BookFilters {
  search?: string;
  genre?: string;
  language?: string;
  authorId?: string;
  status?: string;
  minPages?: number;
  maxPages?: number;
  source?: string;
  copyrightStatus?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface Genre {
  id: string;
  name: string;
  icon: string;
  color: string;
}

// ==================== READING TYPES ====================

export interface BookForReading {
  id: string;
  title: string;
  description: string;
  authorName: string;
  authorId: string;
  coverImageUrl?: string;
  language: string;
  totalPages: number;
  totalChapters: number;
  wordCount: number;
  estimatedReadingTime: string;
  visualizationMode: string;
  allowReaderVisualization: boolean;
  preferredStyle?: string;
  preferredProvider?: string;
  allowedVisualizationModes: string[];
  chapters: ChapterForReading[];
}

export interface ChapterForReading {
  id: string;
  chapterNumber: number;
  title: string;
  summary?: string;
  pageCount: number;
  wordCount: number;
  estimatedReadingTime: string;
  pages?: PageForReading[];
}

export interface PageForReading {
  id: string;
  pageNumber: number;
  content: string;
  wordCount: number;
  hasVisualization: boolean;
  visualizationImageUrl?: string;
  visualizationThumbnailUrl?: string;
  isVisualizationPoint: boolean;
}

export interface VisualizationPoint {
  pageId: string;
  pageNumber: number;
  chapterId: string;
  chapterNumber: number;
  hasVisualization: boolean;
  imageUrl?: string;
  thumbnailUrl?: string;
}

// ==================== SERVICE ====================

export const catalogService = {
  // ==================== BOOKS ====================

  /**
   * Get paginated list of books
   */
  async getBooks(filters: BookFilters = {}): Promise<PaginatedResponse<Book>> {
    const params: Record<string, unknown> = {
      pageNumber: filters.pageNumber || 1,
      pageSize: filters.pageSize || 20,
    };

    if (filters.search) params.searchTerm = filters.search;
    if (filters.genre) params.genre = filters.genre;
    if (filters.language) params.language = filters.language;
    if (filters.authorId) params.authorId = filters.authorId;
    if (filters.status) params.status = filters.status;
    if (filters.minPages) params.minPages = filters.minPages;
    if (filters.maxPages) params.maxPages = filters.maxPages;
    if (filters.source) params.source = filters.source;
    if (filters.copyrightStatus) params.copyrightStatus = filters.copyrightStatus;
    if (filters.sortBy) params.sortBy = filters.sortBy;
    if (filters.sortOrder) params.descending = filters.sortOrder === 'desc';

    return apiClient.get<PaginatedResponse<Book>>(endpoints.BOOKS.BASE, params);
  },

  /**
   * Search books
   */
  async searchBooks(query: string, filters: BookFilters = {}): Promise<PaginatedResponse<Book>> {
    return this.getBooks({ ...filters, search: query });
  },

  /**
   * Get book by ID
   */
  async getBookById(id: string): Promise<Book> {
    return apiClient.get<Book>(endpoints.BOOKS.BY_ID(id));
  },

  /**
   * Get book prepared for reading with chapters
   */
  async getBookForReading(bookId: string, chapterNumber?: number): Promise<BookForReading> {
    const params: Record<string, unknown> = {};
    if (chapterNumber) {
      params.chapterNumber = chapterNumber;
      params.includeChapterContent = true;
    }
    return apiClient.get<BookForReading>(endpoints.READING.BOOK(bookId), params);
  },

  /**
   * Get specific chapter for reading
   */
  async getChapterForReading(bookId: string, chapterNumber: number): Promise<ChapterForReading> {
    return apiClient.get<ChapterForReading>(
      endpoints.READING.CHAPTER(bookId, chapterNumber)
    );
  },

  /**
   * Get visualization points for a book
   */
  async getVisualizationPoints(bookId: string): Promise<VisualizationPoint[]> {
    return apiClient.get<VisualizationPoint[]>(
      endpoints.READING.VISUALIZATION_POINTS(bookId)
    );
  },

  /**
   * Get featured books
   */
  async getFeaturedBooks(limit: number = 10): Promise<Book[]> {
    const response = await apiClient.get<PaginatedResponse<Book>>(
      endpoints.BOOKS.FEATURED,
      { pageSize: limit }
    );
    return response.items || [];
  },

  /**
   * Get popular books
   */
  async getPopularBooks(limit: number = 10): Promise<Book[]> {
    const response = await apiClient.get<PaginatedResponse<Book>>(
      endpoints.BOOKS.POPULAR,
      { pageSize: limit }
    );
    return response.items || [];
  },

  /**
   * Get books by genre
   */
  async getBooksByGenre(genre: string, page: number = 1): Promise<PaginatedResponse<Book>> {
    return apiClient.get<PaginatedResponse<Book>>(endpoints.BOOKS.BY_GENRE, {
      genre,
      pageNumber: page,
    });
  },

  /**
   * Create new book (Author only)
   */
  async createBook(data: CreateBookRequest): Promise<Book> {
    return apiClient.post<Book>(endpoints.BOOKS.BASE, data);
  },

  /**
   * Update book (Author only)
   */
  async updateBook(id: string, data: UpdateBookRequest): Promise<Book> {
    return apiClient.put<Book>(endpoints.BOOKS.BY_ID(id), data);
  },

  /**
   * Delete book (Author only)
   */
  async deleteBook(id: string): Promise<void> {
    await apiClient.delete(endpoints.BOOKS.BY_ID(id));
  },

  /**
   * Publish book
   */
  async publishBook(id: string): Promise<Book> {
    return apiClient.post<Book>(`${endpoints.BOOKS.BY_ID(id)}/publish`);
  },

  /**
   * Unpublish book
   */
  async unpublishBook(id: string): Promise<Book> {
    return apiClient.post<Book>(`${endpoints.BOOKS.BY_ID(id)}/unpublish`);
  },

  // ==================== AUTHORS ====================

  /**
   * Get paginated list of authors
   */
  async getAuthors(params: PaginationParams & { verified?: boolean } = {}): Promise<PaginatedResponse<Author>> {
    const queryParams: Record<string, unknown> = {
      pageNumber: params.pageNumber || 1,
      pageSize: params.pageSize || 20,
    };

    if (params.search) queryParams.searchTerm = params.search;
    if (params.verified !== undefined) queryParams.verified = params.verified;

    return apiClient.get<PaginatedResponse<Author>>(endpoints.AUTHORS.BASE, queryParams);
  },

  /**
   * Get author by ID
   */
  async getAuthorById(id: string): Promise<Author> {
    return apiClient.get<Author>(endpoints.AUTHORS.BY_ID(id));
  },

  /**
   * Get books by author
   */
  async getAuthorBooks(authorId: string, page: number = 1): Promise<PaginatedResponse<Book>> {
    return apiClient.get<PaginatedResponse<Book>>(endpoints.AUTHORS.BOOKS(authorId), {
      pageNumber: page,
    });
  },

  /**
   * Get verified authors
   */
  async getVerifiedAuthors(limit: number = 10): Promise<Author[]> {
    const response = await apiClient.get<PaginatedResponse<Author>>(
      endpoints.AUTHORS.VERIFIED,
      { pageSize: limit, verified: true }
    );
    return response.items || [];
  },

  // ==================== CHAPTERS ====================

  /**
   * Get chapters for a book
   */
  async getChapters(bookId: string): Promise<Chapter[]> {
    const response = await apiClient.get<{ items: Chapter[] } | Chapter[]>(
      endpoints.CHAPTERS.BASE(bookId)
    );
    return Array.isArray(response) ? response : response.items || [];
  },

  /**
   * Get chapter by ID
   */
  async getChapterById(bookId: string, chapterId: string): Promise<Chapter> {
    return apiClient.get<Chapter>(endpoints.CHAPTERS.BY_ID(bookId, chapterId));
  },

  /**
   * Create chapter
   */
  async createChapter(bookId: string, data: { title: string; content: string; chapterNumber: number }): Promise<Chapter> {
    return apiClient.post<Chapter>(endpoints.CHAPTERS.CREATE(bookId), data);
  },

  /**
   * Update chapter
   */
  async updateChapter(bookId: string, chapterId: string, data: { title?: string; content?: string }): Promise<Chapter> {
    return apiClient.put<Chapter>(endpoints.CHAPTERS.BY_ID(bookId, chapterId), data);
  },

  /**
   * Delete chapter
   */
  async deleteChapter(bookId: string, chapterId: string): Promise<void> {
    await apiClient.delete(endpoints.CHAPTERS.BY_ID(bookId, chapterId));
  },

  /**
   * Reorder chapters
   */
  async reorderChapters(bookId: string, chapterIds: string[]): Promise<void> {
    await apiClient.post(endpoints.CHAPTERS.REORDER(bookId), { chapterIds });
  },

  // ==================== PAGES ====================

  /**
   * Get pages for a chapter
   */
  async getPages(bookId: string, chapterId: string): Promise<Page[]> {
    const response = await apiClient.get<{ items: Page[] } | Page[]>(
      endpoints.PAGES.BASE(bookId, chapterId)
    );
    return Array.isArray(response) ? response : response.items || [];
  },

  /**
   * Get page by ID
   */
  async getPageById(bookId: string, chapterId: string, pageId: string): Promise<Page> {
    return apiClient.get<Page>(endpoints.PAGES.BY_ID(bookId, chapterId, pageId));
  },

  /**
   * Create page
   */
  async createPage(bookId: string, chapterId: string, data: { content: string; pageNumber?: number }): Promise<Page> {
    return apiClient.post<Page>(endpoints.PAGES.CREATE(bookId, chapterId), data);
  },

  /**
   * Update page
   */
  async updatePage(bookId: string, chapterId: string, pageId: string, data: { content?: string; isVisualizationPoint?: boolean }): Promise<Page> {
    return apiClient.put<Page>(endpoints.PAGES.BY_ID(bookId, chapterId, pageId), data);
  },

  /**
   * Delete page
   */
  async deletePage(bookId: string, chapterId: string, pageId: string): Promise<void> {
    await apiClient.delete(endpoints.PAGES.BY_ID(bookId, chapterId, pageId));
  },

  /**
   * Mark page as visualization point
   */
  async markAsVisualizationPoint(bookId: string, chapterId: string, pageId: string, isPoint: boolean): Promise<Page> {
    return apiClient.post<Page>(endpoints.PAGES.VISUALIZATION(bookId, chapterId, pageId), {
      isVisualizationPoint: isPoint,
    });
  },

  // ==================== GENRES ====================

  /**
   * Get available genres
   */
  async getGenres(): Promise<Genre[]> {
    return [
      { id: 'fantasy', name: 'Fantasy', icon: '🐉', color: '#a78bfa' },
      { id: 'romance', name: 'Romance', icon: '💕', color: '#f472b6' },
      { id: 'mystery', name: 'Mystery', icon: '🔍', color: '#60a5fa' },
      { id: 'scifi', name: 'Sci-Fi', icon: '🚀', color: '#34d399' },
      { id: 'thriller', name: 'Thriller', icon: '😱', color: '#fbbf24' },
      { id: 'horror', name: 'Horror', icon: '👻', color: '#ef4444' },
      { id: 'classics', name: 'Classics', icon: '📜', color: '#f97316' },
      { id: 'adventure', name: 'Adventure', icon: '⚔️', color: '#14b8a6' },
      { id: 'historical', name: 'Historical', icon: '🏛️', color: '#8b5cf6' },
      { id: 'biography', name: 'Biography', icon: '👤', color: '#ec4899' },
      { id: 'poetry', name: 'Poetry', icon: '📝', color: '#06b6d4' },
      { id: 'philosophy', name: 'Philosophy', icon: '🤔', color: '#84cc16' },
    ];
  },

  /**
   * Get available languages
   */
  async getLanguages(): Promise<{ code: string; name: string }[]> {
    return [
      { code: 'en', name: 'English' },
      { code: 'es', name: 'Spanish' },
      { code: 'fr', name: 'French' },
      { code: 'de', name: 'German' },
      { code: 'it', name: 'Italian' },
      { code: 'pt', name: 'Portuguese' },
      { code: 'ru', name: 'Russian' },
      { code: 'zh', name: 'Chinese' },
      { code: 'ja', name: 'Japanese' },
      { code: 'ko', name: 'Korean' },
      { code: 'nl', name: 'Dutch' },
      { code: 'pl', name: 'Polish' },
      { code: 'fi', name: 'Finnish' },
      { code: 'sv', name: 'Swedish' },
    ];
  },

  // ==================== REVIEWS ====================

  /**
   * Get book reviews
   */
  async getBookReviews(bookId: string, page: number = 1): Promise<PaginatedResponse<BookReview>> {
    return apiClient.get<PaginatedResponse<BookReview>>(`${endpoints.BOOKS.BY_ID(bookId)}/reviews`, {
      pageNumber: page,
    });
  },

  /**
   * Add review
   */
  async addReview(bookId: string, data: { rating: number; comment?: string }): Promise<BookReview> {
    return apiClient.post<BookReview>(`${endpoints.BOOKS.BY_ID(bookId)}/reviews`, data);
  },

  /**
   * Update review
   */
  async updateReview(bookId: string, reviewId: string, data: { rating?: number; comment?: string }): Promise<BookReview> {
    return apiClient.put<BookReview>(`${endpoints.BOOKS.BY_ID(bookId)}/reviews/${reviewId}`, data);
  },

  /**
   * Delete review
   */
  async deleteReview(bookId: string, reviewId: string): Promise<void> {
    await apiClient.delete(`${endpoints.BOOKS.BY_ID(bookId)}/reviews/${reviewId}`);
  },
};

// ==================== ADDITIONAL TYPES ====================

export interface BookReview {
  id: string;
  bookId: string;
  userId: string;
  userName: string;
  userAvatarUrl?: string;
  rating: number;
  comment?: string;
  createdAt: string;
  updatedAt?: string;
}

export default catalogService;