// src/services/books.service.ts
import apiService from './api.service';
import { API_CONFIG } from '../config/api.config';
import { Book, Author, Chapter, Page, PaginatedResult } from '../types';

interface GetBooksParams {
  page?: number;
  pageSize?: number;
  genre?: string;
  searchTerm?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

interface CreateBookData {
  title: string;
  description: string;
  authorId: string;
  coverImageUrl?: string;
  language: string;
  genres: string[];
  tags: string[];
}

interface CreateChapterData {
  title: string;
  content?: string;
}

interface CreatePageData {
  content: string;
}

class BooksService {
  // ==================== BOOKS ====================
  async getBooks(params: GetBooksParams = {}): Promise<PaginatedResult<Book>> {
    const { page = 1, pageSize = 12, ...rest } = params;
    return apiService.get<PaginatedResult<Book>>(
      API_CONFIG.ENDPOINTS.BOOKS.BASE,
      { page, pageSize, ...rest }
    );
  }

  async getBookById(id: string): Promise<Book> {
    return apiService.get<Book>(API_CONFIG.ENDPOINTS.BOOKS.BY_ID(id));
  }

  async getFeaturedBooks(): Promise<Book[]> {
    return apiService.get<Book[]>(API_CONFIG.ENDPOINTS.BOOKS.FEATURED);
  }

  async getPopularBooks(): Promise<Book[]> {
    return apiService.get<Book[]>(API_CONFIG.ENDPOINTS.BOOKS.POPULAR);
  }

  async searchBooks(query: string): Promise<Book[]> {
    return apiService.get<Book[]>(API_CONFIG.ENDPOINTS.BOOKS.SEARCH, { q: query });
  }

  async getBooksByGenre(genre: string): Promise<Book[]> {
    return apiService.get<Book[]>(API_CONFIG.ENDPOINTS.BOOKS.BY_GENRE, { genre });
  }

  async createBook(data: CreateBookData): Promise<Book> {
    return apiService.post<Book>(API_CONFIG.ENDPOINTS.BOOKS.BASE, data);
  }

  async updateBook(id: string, data: Partial<CreateBookData>): Promise<Book> {
    return apiService.put<Book>(API_CONFIG.ENDPOINTS.BOOKS.BY_ID(id), data);
  }

  async deleteBook(id: string): Promise<void> {
    await apiService.delete(API_CONFIG.ENDPOINTS.BOOKS.BY_ID(id));
  }

  // ==================== AUTHORS ====================
  async getAuthors(): Promise<Author[]> {
    return apiService.get<Author[]>(API_CONFIG.ENDPOINTS.AUTHORS.BASE);
  }

  async getAuthorById(id: string): Promise<Author> {
    return apiService.get<Author>(API_CONFIG.ENDPOINTS.AUTHORS.BY_ID(id));
  }

  async getAuthorBooks(authorId: string): Promise<Book[]> {
    return apiService.get<Book[]>(API_CONFIG.ENDPOINTS.AUTHORS.BOOKS(authorId));
  }

  async getVerifiedAuthors(): Promise<Author[]> {
    return apiService.get<Author[]>(API_CONFIG.ENDPOINTS.AUTHORS.VERIFIED);
  }

  // ==================== CHAPTERS ====================
  async getChapters(bookId: string): Promise<Chapter[]> {
    return apiService.get<Chapter[]>(API_CONFIG.ENDPOINTS.CHAPTERS.BASE(bookId));
  }

  async getChapterById(bookId: string, chapterId: string): Promise<Chapter> {
    return apiService.get<Chapter>(
      API_CONFIG.ENDPOINTS.CHAPTERS.BY_ID(bookId, chapterId)
    );
  }

  async createChapter(bookId: string, data: CreateChapterData): Promise<Chapter> {
    return apiService.post<Chapter>(
      API_CONFIG.ENDPOINTS.CHAPTERS.ADD(bookId),
      data
    );
  }

  // ==================== PAGES ====================
  async getPages(bookId: string, chapterId: string): Promise<Page[]> {
    return apiService.get<Page[]>(
      API_CONFIG.ENDPOINTS.PAGES.BASE(bookId, chapterId)
    );
  }

  async getPageById(bookId: string, chapterId: string, pageId: string): Promise<Page> {
    return apiService.get<Page>(
      API_CONFIG.ENDPOINTS.PAGES.BY_ID(bookId, chapterId, pageId)
    );
  }

  async createPage(bookId: string, chapterId: string, data: CreatePageData): Promise<Page> {
    return apiService.post<Page>(
      API_CONFIG.ENDPOINTS.PAGES.ADD(bookId, chapterId),
      data
    );
  }

  // ==================== READING ====================
  async getReadingData(bookId: string): Promise<any> {
    return apiService.get(API_CONFIG.ENDPOINTS.READING.BOOK(bookId));
  }

  async getChapterContent(bookId: string, chapterNumber: number): Promise<any> {
    return apiService.get(API_CONFIG.ENDPOINTS.READING.CHAPTER(bookId, chapterNumber));
  }

  async updateReadingProgress(bookId: string, progress: any): Promise<void> {
    await apiService.post(API_CONFIG.ENDPOINTS.READING.PROGRESS(bookId), progress);
  }

  // ==================== GUTENBERG ====================
  async searchGutenberg(query: string, page = 1): Promise<any> {
    return apiService.get(API_CONFIG.ENDPOINTS.GUTENBERG.SEARCH, { query, page });
  }

  async importFromGutenberg(gutenbergId: string, authorId: string): Promise<Book> {
    return apiService.post<Book>(API_CONFIG.ENDPOINTS.GUTENBERG.IMPORT, {
      gutenbergId,
      authorId
    });
  }

  async getGutenbergBook(gutenbergId: string): Promise<any> {
    return apiService.get(API_CONFIG.ENDPOINTS.GUTENBERG.BY_ID(gutenbergId));
  }
}

export const booksService = new BooksService();
export default booksService;