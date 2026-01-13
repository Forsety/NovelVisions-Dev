// src/services/catalog-api.service.ts
import axios, { AxiosInstance, AxiosError } from 'axios';
import { API_CONFIG } from '../config/api.config';

// ==================== AUTH TYPES ====================
interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role?: 'Reader' | 'Author';
}

interface AuthenticationResult {
  succeeded: boolean;
  token?: string;
  refreshToken?: string;
  expiresIn?: number;
  user?: UserDto;
  error?: string;
}

interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  role: string;
  isEmailConfirmed: boolean;
  createdAt: string;
}

interface RefreshTokenRequest {
  refreshToken: string;
}

interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  displayName?: string;
  biography?: string;
}

// ==================== BOOK TYPES ====================
interface BookDto {
  id: string;
  title: string;
  description: string;
  authorId: string;
  authorName?: string;
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
  createdAt: string;
  updatedAt: string;
}

interface CreateBookCommand {
  title: string;
  description: string;
  authorId: string;
  coverImageUrl?: string;
  language: string;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  edition?: string;
  genres: string[];
  tags: string[];
}

interface UpdateBookCommand {
  title?: string;
  description?: string;
  coverImageUrl?: string;
  language?: string;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  edition?: string;
  genres?: string[];
  tags?: string[];
}

// ==================== AUTHOR TYPES ====================
interface AuthorDto {
  id: string;
  displayName: string;
  email: string;
  biography?: string;
  isVerified: boolean;
  socialLinks?: Record<string, string>;
  createdAt: string;
  updatedAt: string;
}

interface CreateAuthorCommand {
  displayName: string;
  email: string;
  biography?: string;
  socialLinks?: Record<string, string>;
}

// ==================== CHAPTER & PAGE TYPES ====================
interface ChapterDto {
  id: string;
  bookId: string;
  chapterNumber: number;
  title: string;
  content?: string;
  pageCount: number;
  createdAt: string;
}

interface PageDto {
  id: string;
  chapterId: string;
  pageNumber: number;
  content: string;
  wordCount: number;
}

interface CreateChapterCommand {
  title: string;
  content?: string;
}

interface AddPageCommand {
  content: string;
}

// ==================== RESPONSE TYPES ====================
interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

class CatalogApiService {
  private api: AxiosInstance;
  private authToken: string | null = null;
  private refreshTokenValue: string | null = null;

  constructor() {
    this.api = axios.create({
      baseURL: API_CONFIG.CATALOG_API_URL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    });

    // Request interceptor - добавляем токен
    this.api.interceptors.request.use(
      (config) => {
        const token = this.getAuthToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor - обработка ошибок и refresh token
    this.api.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest: any = error.config;
        
        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;
          
          try {
            await this.refreshAccessToken();
            const newToken = this.getAuthToken();
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return this.api(originalRequest);
          } catch (refreshError) {
            this.clearTokens();
            window.location.href = '/login';
            return Promise.reject(refreshError);
          }
        }
        
        return Promise.reject(error);
      }
    );
  }

  // ==================== TOKEN MANAGEMENT ====================
  
  private setTokens(token: string, refreshToken: string) {
    this.authToken = token;
    this.refreshTokenValue = refreshToken;
    localStorage.setItem('authToken', token);
    localStorage.setItem('refreshToken', refreshToken);
  }

  private getAuthToken(): string | null {
    if (!this.authToken) {
      this.authToken = localStorage.getItem('authToken');
    }
    return this.authToken;
  }

  private getRefreshToken(): string | null {
    if (!this.refreshTokenValue) {
      this.refreshTokenValue = localStorage.getItem('refreshToken');
    }
    return this.refreshTokenValue;
  }

  private clearTokens() {
    this.authToken = null;
    this.refreshTokenValue = null;
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  // ==================== AUTH METHODS ====================

  async login(request: LoginRequest): Promise<AuthenticationResult> {
    try {
      const response = await this.api.post(API_CONFIG.ENDPOINTS.AUTH.LOGIN, request);
      const result = response.data;
      
      if (result.succeeded && result.token && result.refreshToken) {
        this.setTokens(result.token, result.refreshToken);
        localStorage.setItem('user', JSON.stringify(result.user));
      }
      
      return result;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async register(request: RegisterRequest): Promise<AuthenticationResult> {
    try {
      const response = await this.api.post(API_CONFIG.ENDPOINTS.AUTH.REGISTER, request);
      const result = response.data;
      
      if (result.succeeded && result.token && result.refreshToken) {
        this.setTokens(result.token, result.refreshToken);
        localStorage.setItem('user', JSON.stringify(result.user));
      }
      
      return result;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async logout(): Promise<void> {
    try {
      const refreshToken = this.getRefreshToken();
      if (refreshToken) {
        await this.api.post(API_CONFIG.ENDPOINTS.AUTH.REVOKE_TOKEN, { 
          refreshToken 
        });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      this.clearTokens();
    }
  }

  async refreshAccessToken(): Promise<string> {
    const refreshToken = this.getRefreshToken();
    
    if (!refreshToken) {
      throw new Error('No refresh token');
    }

    try {
      const response = await this.api.post(API_CONFIG.ENDPOINTS.AUTH.REFRESH_TOKEN, {
        refreshToken
      });
      
      const result = response.data;
      if (result.succeeded && result.token && result.refreshToken) {
        this.setTokens(result.token, result.refreshToken);
        return result.token;
      }
      
      throw new Error('Failed to refresh token');
    } catch (error) {
      this.clearTokens();
      throw error;
    }
  }

  async getProfile(): Promise<UserDto> {
    try {
      const response = await this.api.get(API_CONFIG.ENDPOINTS.AUTH.PROFILE);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async updateProfile(request: UpdateProfileRequest): Promise<void> {
    try {
      await this.api.put(API_CONFIG.ENDPOINTS.AUTH.PROFILE, request);
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async changePassword(request: ChangePasswordRequest): Promise<void> {
    try {
      await this.api.post(API_CONFIG.ENDPOINTS.AUTH.CHANGE_PASSWORD, request);
    } catch (error) {
      throw this.handleError(error);
    }
  }

  // ==================== BOOKS METHODS ====================

  async getBooks(page = 1, pageSize = 20, genre?: string, searchTerm?: string): Promise<PaginatedResult<BookDto>> {
    try {
      const params = { page, pageSize, genre, searchTerm };
      const response = await this.api.get(API_CONFIG.ENDPOINTS.BOOKS.BASE, { params });
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async getBookById(id: string): Promise<BookDto> {
    try {
      const response = await this.api.get(API_CONFIG.ENDPOINTS.BOOKS.BY_ID(id));
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async createBook(command: CreateBookCommand): Promise<BookDto> {
    try {
      const response = await this.api.post(API_CONFIG.ENDPOINTS.BOOKS.BASE, command);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async updateBook(id: string, command: UpdateBookCommand): Promise<BookDto> {
    try {
      const response = await this.api.put(API_CONFIG.ENDPOINTS.BOOKS.BY_ID(id), command);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async deleteBook(id: string): Promise<void> {
    try {
      await this.api.delete(API_CONFIG.ENDPOINTS.BOOKS.BY_ID(id));
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async searchBooks(query: string): Promise<BookDto[]> {
    try {
      const response = await this.api.get(API_CONFIG.ENDPOINTS.BOOKS.SEARCH, {
        params: { q: query }
      });
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  // ==================== AUTHORS METHODS ====================

  async getAuthors(): Promise<AuthorDto[]> {
    try {
      const response = await this.api.get(API_CONFIG.ENDPOINTS.AUTHORS.BASE);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async getAuthorById(id: string): Promise<AuthorDto> {
    try {
      const response = await this.api.get(API_CONFIG.ENDPOINTS.AUTHORS.BY_ID(id));
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async createAuthor(command: CreateAuthorCommand): Promise<AuthorDto> {
    try {
      const response = await this.api.post(API_CONFIG.ENDPOINTS.AUTHORS.BASE, command);
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async getBooksByAuthor(authorId: string): Promise<BookDto[]> {
    try {
      const response = await this.api.get(API_CONFIG.ENDPOINTS.AUTHORS.BOOKS(authorId));
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  // ==================== CHAPTERS METHODS ====================

  async getChapters(bookId: string): Promise<ChapterDto[]> {
    try {
      const response = await this.api.get(API_CONFIG.ENDPOINTS.CHAPTERS.BASE(bookId));
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async createChapter(bookId: string, command: CreateChapterCommand): Promise<ChapterDto> {
    try {
      const response = await this.api.post(
        API_CONFIG.ENDPOINTS.CHAPTERS.ADD(bookId), 
        command
      );
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  // ==================== PAGES METHODS ====================

  async getPages(bookId: string, chapterId: string): Promise<PageDto[]> {
    try {
      const response = await this.api.get(
        API_CONFIG.ENDPOINTS.PAGES.BASE(bookId, chapterId)
      );
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async addPage(bookId: string, chapterId: string, command: AddPageCommand): Promise<PageDto> {
    try {
      const response = await this.api.post(
        API_CONFIG.ENDPOINTS.PAGES.ADD(bookId, chapterId),
        command
      );
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  // ==================== HELPERS ====================

  isAuthenticated(): boolean {
    return !!this.getAuthToken();
  }

  getCurrentUser(): UserDto | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  private handleError(error: any): Error {
    if (axios.isAxiosError(error)) {
      const axiosError = error as AxiosError<any>;
      
      if (axiosError.response) {
        const data = axiosError.response.data;
        const message = data?.error || data?.message || 
                       `Ошибка сервера: ${axiosError.response.status}`;
        
        console.error('API Error:', {
          status: axiosError.response.status,
          message,
          data
        });
        
        return new Error(message);
      } else if (axiosError.request) {
        console.error('Network Error:', axiosError.message);
        return new Error('Сервер недоступен. Проверьте подключение.');
      }
    }
    
    console.error('Unknown Error:', error);
    return new Error('Произошла неизвестная ошибка');
  }
}

// Export types
export type {
  LoginRequest,
  RegisterRequest,
  AuthenticationResult,
  UserDto,
  BookDto,
  AuthorDto,
  ChapterDto,
  PageDto,
  CreateBookCommand,
  CreateAuthorCommand,
  PaginatedResult
};

// Export singleton instance
export default new CatalogApiService();