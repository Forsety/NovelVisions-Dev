// src/services/api/gutenberg.service.ts
// Gutenberg API Service for searching and importing public domain books

import { apiClient } from './apiClient';
import { getEndpoints } from '../../shared/constants/api';
import type {
  GutenbergBook,
  GutenbergSearchResult,
  ImportBookRequest,
  ImportResult,
} from '../../types/api.types';

const endpoints = getEndpoints();

// ==================== TYPES ====================

export interface GutenbergSearchParams {
  query?: string;
  author?: string;
  title?: string;
  topic?: string;
  language?: string;
  page?: number;
}

export interface GutenbergBookPreview {
  id: number;
  title: string;
  authors: GutenbergAuthor[];
  subjects: string[];
  bookshelves: string[];
  languages: string[];
  copyright: boolean;
  mediaType: string;
  downloadCount: number;
  coverUrl?: string;
  textUrl?: string;
}

export interface GutenbergAuthor {
  name: string;
  birth_year?: number;
  death_year?: number;
}

export interface ImportStatus {
  isImported: boolean;
  bookId?: string;
  importedAt?: string;
  importedBy?: string;
}

export interface ImportProgress {
  jobId: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  progress: number;
  currentStep?: string;
  bookId?: string;
  error?: string;
  startedAt: string;
  completedAt?: string;
  estimatedTimeRemaining?: number;
}

export interface BulkImportResult {
  totalRequested: number;
  successCount: number;
  failedCount: number;
  skippedCount: number;
  results: {
    gutenbergId: number;
    success: boolean;
    bookId?: string;
    error?: string;
  }[];
}

// ==================== SERVICE ====================

export const gutenbergService = {
  /**
   * Search books in Project Gutenberg
   */
  async search(params: GutenbergSearchParams): Promise<GutenbergSearchResult> {
    const searchParams: Record<string, unknown> = {};
    
    if (params.query) searchParams.search = params.query;
    if (params.author) searchParams.author = params.author;
    if (params.title) searchParams.title = params.title;
    if (params.topic) searchParams.topic = params.topic;
    if (params.language) searchParams.languages = params.language;
    if (params.page) searchParams.page = params.page;

    return apiClient.get<GutenbergSearchResult>(endpoints.GUTENBERG.SEARCH, searchParams);
  },

  /**
   * Get popular books from Project Gutenberg
   */
  async getPopular(page: number = 1, language?: string): Promise<GutenbergSearchResult> {
    const params: Record<string, unknown> = { page };
    if (language) params.language = language;
    
    return apiClient.get<GutenbergSearchResult>(endpoints.GUTENBERG.POPULAR, params);
  },

  /**
   * Get book details by Gutenberg ID
   */
  async getBook(gutenbergId: number): Promise<GutenbergBook> {
    return apiClient.get<GutenbergBook>(endpoints.GUTENBERG.PREVIEW(gutenbergId));
  },

  /**
   * Preview book content before importing
   */
  async previewBook(gutenbergId: number): Promise<GutenbergBookPreview> {
    return apiClient.get<GutenbergBookPreview>(endpoints.GUTENBERG.PREVIEW(gutenbergId));
  },

  /**
   * Import a book from Gutenberg to the platform
   */
  /**
 * Import a book from Gutenberg to the platform
 */
async importBook(request: ImportBookRequest): Promise<ImportResult> {
  // Завжди передаємо importFullText: true для повного імпорту
  const importRequest = {
    ...request,
    importFullText: true,        // ← ДОДАНО
    parseChapters: true,         // ← ДОДАНО
    wordsPerPage: 300,           // ← ДОДАНО
  };
  return apiClient.post<ImportResult>(
    endpoints.GUTENBERG.IMPORT(request.gutenbergId), 
    importRequest
  );
},

  /**
   * Bulk import multiple books
   */
  async bulkImport(gutenbergIds: number[], options?: { 
    skipExisting?: boolean; 
    defaultVisualizationMode?: string;
  }): Promise<BulkImportResult> {
    return apiClient.post<BulkImportResult>(endpoints.GUTENBERG.BULK_IMPORT, {
      gutenbergIds,
      ...options,
    });
  },

  /**
   * Check if a book is already imported
   */
  async checkImported(gutenbergId: number): Promise<ImportStatus> {
    try {
      const result = await apiClient.get<ImportStatus>(
        `${endpoints.GUTENBERG.PREVIEW(gutenbergId)}/status`
      );
      return result;
    } catch {
      return { isImported: false };
    }
  },

  /**
   * Check multiple books import status
   */
  async checkMultipleImported(gutenbergIds: number[]): Promise<Map<number, ImportStatus>> {
    const results = await apiClient.post<{ statuses: Record<number, ImportStatus> }>(
      `${endpoints.GUTENBERG.SEARCH}/check-imported`,
      { gutenbergIds }
    );
    return new Map(Object.entries(results.statuses).map(([k, v]) => [parseInt(k), v]));
  },

  /**
   * Get import progress
   */
  async getImportProgress(jobId: string): Promise<ImportProgress> {
    return apiClient.get<ImportProgress>(`${endpoints.GUTENBERG.BULK_IMPORT}/${jobId}/progress`);
  },

  /**
   * Cancel import
   */
  async cancelImport(jobId: string): Promise<void> {
    await apiClient.delete(`${endpoints.GUTENBERG.BULK_IMPORT}/${jobId}`);
  },

  /**
   * Check Gutenberg API health
   */
  async checkHealth(): Promise<{ status: string; latency: number }> {
    return apiClient.get<{ status: string; latency: number }>(endpoints.GUTENBERG.HEALTH);
  },

  // ==================== HELPER METHODS ====================

  /**
   * Get cover image URL for a Gutenberg book
   */
  getCoverUrl(book: GutenbergBook): string | null {
    if (!book.formats) return null;
    
    // Try different possible keys
    const formats = book.formats as Record<string, string>;
    return (
      formats['image/jpeg'] ||
      formats['imageJpeg'] ||
      formats.imageJpeg ||
      null
    );
  },

  /**
   * Get text URL for a Gutenberg book
   */
  getTextUrl(book: GutenbergBook): string | null {
    if (!book.formats) return null;
    
    const formats = book.formats as Record<string, string>;
    return (
      formats['text/plain; charset=utf-8'] ||
      formats['text/plain'] ||
      formats['text/html; charset=utf-8'] ||
      formats['text/html'] ||
      formats.textPlainUtf8 ||
      formats.textPlain ||
      null
    );
  },

  /**
   * Get HTML URL for a Gutenberg book
   */
  getHtmlUrl(book: GutenbergBook): string | null {
    if (!book.formats) return null;
    
    const formats = book.formats as Record<string, string>;
    return (
      formats['text/html; charset=utf-8'] ||
      formats['text/html'] ||
      formats.textHtml ||
      null
    );
  },

  /**
   * Get EPUB URL for a Gutenberg book
   */
  getEpubUrl(book: GutenbergBook): string | null {
    if (!book.formats) return null;
    
    const formats = book.formats as Record<string, string>;
    return (
      formats['application/epub+zip'] ||
      formats.applicationEpub ||
      null
    );
  },

  /**
   * Format author name (Gutenberg uses "Last, First" format)
   */
  formatAuthorName(author: { name: string; birth_year?: number; death_year?: number }): string {
    if (!author?.name) return 'Unknown Author';
    
    const parts = author.name.split(', ');
    if (parts.length === 2) {
      return `${parts[1]} ${parts[0]}`;
    }
    return author.name;
  },

  /**
   * Get life years string for author
   */
  getAuthorLifeYears(author: { birth_year?: number; death_year?: number }): string | null {
    if (!author.birth_year && !author.death_year) return null;
    
    const birth = author.birth_year?.toString() || '?';
    const death = author.death_year?.toString() || '';
    return death ? `${birth}–${death}` : `${birth}–`;
  },

  /**
   * Get Gutenberg page URL
   */
  getGutenbergUrl(gutenbergId: number): string {
    return `https://www.gutenberg.org/ebooks/${gutenbergId}`;
  },

  /**
   * Get Gutenberg read online URL
   */
  getReadOnlineUrl(gutenbergId: number): string {
    return `https://www.gutenberg.org/ebooks/${gutenbergId}.html.images`;
  },

  /**
   * Parse subjects into categories
   */
  parseSubjects(subjects: string[]): { category: string; subject: string }[] {
    if (!subjects) return [];
    
    return subjects.map(subject => {
      const parts = subject.split(' -- ');
      if (parts.length > 1) {
        return { category: parts[0], subject: parts.slice(1).join(' - ') };
      }
      return { category: 'General', subject };
    });
  },

  /**
   * Get primary language
   */
  getPrimaryLanguage(languages: string[]): string {
    return languages?.[0] || 'en';
  },

  /**
   * Format download count
   */
  formatDownloadCount(count: number): string {
    if (count >= 1000000) {
      return `${(count / 1000000).toFixed(1)}M`;
    }
    if (count >= 1000) {
      return `${(count / 1000).toFixed(1)}K`;
    }
    return count.toString();
  },
};

export default gutenbergService;