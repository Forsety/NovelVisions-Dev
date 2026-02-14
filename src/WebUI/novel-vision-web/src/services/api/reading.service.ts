// src/services/api/reading.service.ts
// Reading API Service - Progress tracking, bookmarks, reading sessions

import { apiClient } from './apiClient';
import { getEndpoints } from '../../shared/constants/api';
import type { ReadingProgress, Bookmark } from '../../types/api.types';

const endpoints = getEndpoints();

// ==================== TYPES ====================

export interface ReadingStats {
  totalBooksRead: number;
  totalBooksInProgress: number;
  totalPagesRead: number;
  totalReadingTime: string;
  totalReadingTimeMinutes: number;
  averagePagesPerDay: number;
  averageReadingTimePerDay: number;
  currentStreak: number;
  longestStreak: number;
  favoriteGenres: { genre: string; count: number; percentage: number }[];
  monthlyStats: MonthlyStats[];
  weeklyStats: WeeklyStats[];
  recentActivity: ReadingActivity[];
}

export interface MonthlyStats {
  month: string;
  year: number;
  booksRead: number;
  booksStarted: number;
  pagesRead: number;
  readingTimeMinutes: number;
  averagePagesPerDay: number;
}

export interface WeeklyStats {
  weekStart: string;
  weekEnd: string;
  pagesRead: number;
  readingTimeMinutes: number;
  daysActive: number;
}

export interface ReadingActivity {
  id: string;
  bookId: string;
  bookTitle: string;
  bookCoverUrl?: string;
  activityType: 'started' | 'progress' | 'completed' | 'bookmark' | 'visualization';
  description: string;
  timestamp: string;
  pagesRead?: number;
  chapterNumber?: number;
}

export interface ReadingSession {
  id: string;
  bookId: string;
  bookTitle?: string;
  startedAt: string;
  endedAt?: string;
  pagesRead: number;
  startPage: number;
  endPage?: number;
  durationMinutes: number;
  chapterIds: string[];
}

export interface ReadingGoal {
  id: string;
  type: 'daily_pages' | 'daily_time' | 'weekly_books' | 'monthly_books' | 'yearly_books';
  target: number;
  current: number;
  startDate: string;
  endDate?: string;
  isCompleted: boolean;
  completedAt?: string;
}

export interface BookmarkWithDetails extends Bookmark {
  bookTitle?: string;
  bookCoverUrl?: string;
  chapterTitle?: string;
  pageContent?: string;
}

export interface ReadingPreferences {
  defaultFontSize: number;
  defaultLineHeight: number;
  defaultTheme: 'dark' | 'light' | 'sepia';
  autoSaveProgress: boolean;
  showReadingTime: boolean;
  enableAnimations: boolean;
  defaultVisualizationProvider?: string;
  defaultVisualizationStyle?: string;
}

// ==================== SERVICE ====================

export const readingService = {
  // ==================== PROGRESS ====================

  /**
   * Get reading progress for a book
   */
  async getProgress(bookId: string): Promise<ReadingProgress | null> {
    try {
      return await apiClient.get<ReadingProgress>(endpoints.READING.PROGRESS(bookId));
    } catch {
      return null;
    }
  },

  /**
   * Update reading progress
   */
  async updateProgress(
    bookId: string,
    progress: {
      chapterId?: string;
      pageId?: string;
      currentPage: number;
      currentChapter?: number;
    }
  ): Promise<ReadingProgress> {
    return apiClient.post<ReadingProgress>(endpoints.READING.PROGRESS(bookId), progress);
  },

  /**
   * Get all reading progress for current user
   */
  async getAllProgress(): Promise<ReadingProgress[]> {
    return apiClient.get<ReadingProgress[]>('/reading/progress');
  },

  /**
   * Get recently read books
   */
  async getRecentlyRead(limit: number = 10): Promise<ReadingProgress[]> {
    return apiClient.get<ReadingProgress[]>('/reading/recent', { limit });
  },

  /**
   * Get books in progress
   */
  async getBooksInProgress(): Promise<ReadingProgress[]> {
    return apiClient.get<ReadingProgress[]>('/reading/in-progress');
  },

  /**
   * Get completed books
   */
  async getCompletedBooks(page: number = 1, pageSize: number = 20): Promise<{
    items: ReadingProgress[];
    totalCount: number;
  }> {
    return apiClient.get('/reading/completed', { page, pageSize });
  },

  /**
   * Mark book as completed
   */
  async markAsCompleted(bookId: string): Promise<ReadingProgress> {
    return apiClient.post<ReadingProgress>(`/reading/books/${bookId}/complete`);
  },

  /**
   * Reset reading progress
   */
  async resetProgress(bookId: string): Promise<void> {
    await apiClient.delete(`/reading/books/${bookId}/progress`);
  },

  // ==================== BOOKMARKS ====================

  /**
   * Get bookmarks for a book
   */
  async getBookmarks(bookId: string): Promise<Bookmark[]> {
    return apiClient.get<Bookmark[]>(`/reading/books/${bookId}/bookmarks`);
  },

  /**
   * Get all bookmarks for current user
   */
  async getAllBookmarks(page: number = 1, pageSize: number = 50): Promise<{
    items: BookmarkWithDetails[];
    totalCount: number;
  }> {
    return apiClient.get('/reading/bookmarks', { page, pageSize });
  },

  /**
   * Add bookmark
   */
  async addBookmark(
    bookId: string,
    bookmark: {
      chapterId?: string;
      pageId?: string;
      pageNumber: number;
      note?: string;
      color?: string;
      selectedText?: string;
    }
  ): Promise<Bookmark> {
    return apiClient.post<Bookmark>(`/reading/books/${bookId}/bookmarks`, bookmark);
  },

  /**
   * Update bookmark
   */
  async updateBookmark(
    bookId: string,
    bookmarkId: string,
    data: { note?: string; color?: string }
  ): Promise<Bookmark> {
    return apiClient.put<Bookmark>(
      `/reading/books/${bookId}/bookmarks/${bookmarkId}`,
      data
    );
  },

  /**
   * Delete bookmark
   */
  async deleteBookmark(bookId: string, bookmarkId: string): Promise<void> {
    await apiClient.delete(`/reading/books/${bookId}/bookmarks/${bookmarkId}`);
  },

  /**
   * Search bookmarks
   */
  async searchBookmarks(query: string): Promise<BookmarkWithDetails[]> {
    return apiClient.get<BookmarkWithDetails[]>('/reading/bookmarks/search', { q: query });
  },

  // ==================== READING SESSIONS ====================

  /**
   * Start reading session
   */
  async startSession(bookId: string, startPage: number): Promise<ReadingSession> {
    return apiClient.post<ReadingSession>(`/reading/books/${bookId}/sessions/start`, {
      startPage,
    });
  },

  /**
   * End reading session
   */
  async endSession(
    bookId: string,
    sessionId: string,
    data: { endPage: number; pagesRead: number }
  ): Promise<ReadingSession> {
    return apiClient.post<ReadingSession>(
      `/reading/books/${bookId}/sessions/${sessionId}/end`,
      data
    );
  },

  /**
   * Get active session
   */
  async getActiveSession(bookId: string): Promise<ReadingSession | null> {
    try {
      return await apiClient.get<ReadingSession>(`/reading/books/${bookId}/sessions/active`);
    } catch {
      return null;
    }
  },

  /**
   * Get session history
   */
  async getSessionHistory(bookId: string): Promise<ReadingSession[]> {
    return apiClient.get<ReadingSession[]>(`/reading/books/${bookId}/sessions`);
  },

  /**
   * Update session (heartbeat)
   */
  async updateSession(bookId: string, sessionId: string, currentPage: number): Promise<void> {
    await apiClient.put(`/reading/books/${bookId}/sessions/${sessionId}`, { currentPage });
  },

  // ==================== READING STATS ====================

  /**
   * Get reading statistics
   */
  async getReadingStats(): Promise<ReadingStats> {
    return apiClient.get<ReadingStats>('/reading/stats');
  },

  /**
   * Get stats for specific time period
   */
  async getStatsForPeriod(startDate: string, endDate: string): Promise<ReadingStats> {
    return apiClient.get<ReadingStats>('/reading/stats', { startDate, endDate });
  },

  /**
   * Get reading activity
   */
  async getActivity(limit: number = 20): Promise<ReadingActivity[]> {
    return apiClient.get<ReadingActivity[]>('/reading/activity', { limit });
  },

  // ==================== READING GOALS ====================

  /**
   * Get reading goals
   */
  async getGoals(): Promise<ReadingGoal[]> {
    return apiClient.get<ReadingGoal[]>('/reading/goals');
  },

  /**
   * Create reading goal
   */
  async createGoal(goal: {
    type: ReadingGoal['type'];
    target: number;
    startDate?: string;
    endDate?: string;
  }): Promise<ReadingGoal> {
    return apiClient.post<ReadingGoal>('/reading/goals', goal);
  },

  /**
   * Update reading goal
   */
  async updateGoal(goalId: string, data: { target?: number; endDate?: string }): Promise<ReadingGoal> {
    return apiClient.put<ReadingGoal>(`/reading/goals/${goalId}`, data);
  },

  /**
   * Delete reading goal
   */
  async deleteGoal(goalId: string): Promise<void> {
    await apiClient.delete(`/reading/goals/${goalId}`);
  },

  // ==================== PREFERENCES ====================

  /**
   * Get reading preferences
   */
  async getPreferences(): Promise<ReadingPreferences> {
    return apiClient.get<ReadingPreferences>('/reading/preferences');
  },

  /**
   * Update reading preferences
   */
  async updatePreferences(preferences: Partial<ReadingPreferences>): Promise<ReadingPreferences> {
    return apiClient.put<ReadingPreferences>('/reading/preferences', preferences);
  },

  // ==================== LIBRARY ====================

  /**
   * Add book to library
   */
  async addToLibrary(bookId: string): Promise<void> {
    await apiClient.post(`/reading/library/${bookId}`);
  },

  /**
   * Remove book from library
   */
  async removeFromLibrary(bookId: string): Promise<void> {
    await apiClient.delete(`/reading/library/${bookId}`);
  },

  /**
   * Check if book is in library
   */
  async isInLibrary(bookId: string): Promise<boolean> {
    try {
      const result = await apiClient.get<{ inLibrary: boolean }>(`/reading/library/${bookId}/check`);
      return result.inLibrary;
    } catch {
      return false;
    }
  },

  /**
   * Get library books
   */
  async getLibrary(page: number = 1, pageSize: number = 20): Promise<{
    items: ReadingProgress[];
    totalCount: number;
  }> {
    return apiClient.get('/reading/library', { page, pageSize });
  },
};

export default readingService;