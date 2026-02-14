// src/services/api/apiClient.ts
// NovelVision API Client with Axios
// FIXED: Unified token storage - uses 'accessToken' and 'refreshToken' keys

import axios, { 
  AxiosInstance, 
  AxiosError, 
  InternalAxiosRequestConfig,
  AxiosResponse 
} from 'axios';
import { API_CONFIG, getEndpoints } from '../../shared/constants/api';
import type { ApiError } from '../../types';

// ============================================================
// UNIFIED STORAGE KEYS - Same as useAuthStore uses
// ============================================================
const STORAGE_KEYS = {
  TOKEN: 'accessToken',           // ← Was 'nv_token' - NOW UNIFIED
  REFRESH_TOKEN: 'refreshToken',  // ← Was 'nv_refresh_token' - NOW UNIFIED
  USER: 'nv_user',
} as const;

// Zustand persist storage key (for reading state directly if needed)
const ZUSTAND_STORAGE_KEY = 'auth-storage';

class ApiClient {
  private client: AxiosInstance;
  private isRefreshing = false;
  private failedQueue: Array<{
    resolve: (value?: unknown) => void;
    reject: (error?: unknown) => void;
  }> = [];

  constructor() {
    this.client = axios.create({
      baseURL: API_CONFIG.USE_GATEWAY ? API_CONFIG.GATEWAY_URL : API_CONFIG.CATALOG_API_URL,
      timeout: API_CONFIG.TIMEOUT,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  // ==================== INTERCEPTORS ====================

  private setupInterceptors(): void {
    // Request interceptor - add auth token
    this.client.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        const token = this.getToken();
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor - handle errors & token refresh
    this.client.interceptors.response.use(
      (response: AxiosResponse) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        // Handle 401 - try to refresh token
        if (error.response?.status === 401 && !originalRequest._retry) {
          if (this.isRefreshing) {
            // Queue the request while refreshing
            return new Promise((resolve, reject) => {
              this.failedQueue.push({ resolve, reject });
            })
              .then((token) => {
                if (originalRequest.headers) {
                  originalRequest.headers.Authorization = `Bearer ${token}`;
                }
                return this.client(originalRequest);
              })
              .catch((err) => Promise.reject(err));
          }

          originalRequest._retry = true;
          this.isRefreshing = true;

          try {
            const newToken = await this.refreshTokenRequest();
            this.processQueue(null, newToken);
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${newToken}`;
            }
            return this.client(originalRequest);
          } catch (refreshError) {
            this.processQueue(refreshError, null);
            this.clearAuth();
            // Redirect to login
            if (typeof window !== 'undefined') {
              window.location.href = '/login';
            }
            return Promise.reject(refreshError);
          } finally {
            this.isRefreshing = false;
          }
        }

        return Promise.reject(this.normalizeError(error));
      }
    );
  }

  private processQueue(error: unknown, token: string | null): void {
    this.failedQueue.forEach(({ resolve, reject }) => {
      if (error) {
        reject(error);
      } else {
        resolve(token);
      }
    });
    this.failedQueue = [];
  }

  // ==================== TOKEN MANAGEMENT ====================
  // Now uses unified keys that match useAuthStore

  getToken(): string | null {
    if (typeof window === 'undefined') return null;
    
    // Primary: Read from localStorage directly (where useAuthStore writes)
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    if (token) return token;
    
    // Fallback: Try reading from Zustand persisted state
    try {
      const zustandState = localStorage.getItem(ZUSTAND_STORAGE_KEY);
      if (zustandState) {
        const parsed = JSON.parse(zustandState);
        return parsed?.state?.token || null;
      }
    } catch {
      // Ignore parse errors
    }
    
    return null;
  }

  getRefreshToken(): string | null {
    if (typeof window === 'undefined') return null;
    
    // Primary: Read from localStorage directly
    const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
    if (refreshToken) return refreshToken;
    
    // Fallback: Try reading from Zustand persisted state
    try {
      const zustandState = localStorage.getItem(ZUSTAND_STORAGE_KEY);
      if (zustandState) {
        const parsed = JSON.parse(zustandState);
        return parsed?.state?.refreshToken || null;
      }
    } catch {
      // Ignore parse errors
    }
    
    return null;
  }

  setTokens(token: string, refreshToken: string): void {
    if (typeof window === 'undefined') return;
    localStorage.setItem(STORAGE_KEYS.TOKEN, token);
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
  }

  setUser(user: unknown): void {
    if (typeof window === 'undefined') return;
    localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
  }

  getUser<T = unknown>(): T | null {
    if (typeof window === 'undefined') return null;
    const user = localStorage.getItem(STORAGE_KEYS.USER);
    return user ? JSON.parse(user) : null;
  }

  clearAuth(): void {
    if (typeof window === 'undefined') return;
    localStorage.removeItem(STORAGE_KEYS.TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.USER);
    // Also clear Zustand persisted state
    localStorage.removeItem(ZUSTAND_STORAGE_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private async refreshTokenRequest(): Promise<string> {
    const refreshToken = this.getRefreshToken();
    const accessToken = this.getToken();
    
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const endpoints = getEndpoints();
    const response = await axios.post(
      `${this.client.defaults.baseURL}${endpoints.AUTH.REFRESH}`,
      { 
        accessToken: accessToken,  // Backend expects both tokens
        refreshToken: refreshToken 
      }
    );

    // Backend returns accessToken, not token
    const newAccessToken = response.data.accessToken;
    const newRefreshToken = response.data.refreshToken;
    
    if (!newAccessToken || !newRefreshToken) {
      throw new Error('Invalid refresh response');
    }

    this.setTokens(newAccessToken, newRefreshToken);
    return newAccessToken;
  }

  // ==================== ERROR HANDLING ====================

  private normalizeError(error: AxiosError): ApiError {
    if (error.response) {
      const data = error.response.data as Record<string, unknown>;
      const message = 
        (data?.error as string) || 
        (data?.message as string) || 
        (data?.title as string) || 
        `Server error: ${error.response.status}`;
      
      return {
        status: error.response.status,
        message,
        errors: data?.errors as string[] | undefined,
      };
    }

    if (error.request) {
      return {
        status: 0,
        message: 'Unable to connect to server. Please check your connection.',
      };
    }

    return {
      status: 0,
      message: error.message || 'An unexpected error occurred',
    };
  }

  // ==================== HTTP METHODS ====================

  private extractData<T>(response: AxiosResponse): T {
    const data = response.data;
    
    // Handle wrapped API responses
    if (data && typeof data === 'object') {
      // If response has success/data structure
      if ('data' in data && data.data !== undefined) {
        return data.data as T;
      }
      // If response has items (paginated)
      if ('items' in data) {
        return data as T;
      }
    }
    
    return data as T;
  }

  async get<T>(url: string, params?: Record<string, unknown>): Promise<T> {
    const response = await this.client.get<T>(url, { params });
    return this.extractData<T>(response);
  }

  async post<T>(url: string, data?: unknown): Promise<T> {
    const response = await this.client.post<T>(url, data);
    return this.extractData<T>(response);
  }

  async put<T>(url: string, data?: unknown): Promise<T> {
    const response = await this.client.put<T>(url, data);
    return this.extractData<T>(response);
  }

  async patch<T>(url: string, data?: unknown): Promise<T> {
    const response = await this.client.patch<T>(url, data);
    return this.extractData<T>(response);
  }

  async delete<T>(url: string): Promise<T> {
    const response = await this.client.delete<T>(url);
    return this.extractData<T>(response);
  }

  async upload<T>(url: string, formData: FormData, onProgress?: (progress: number) => void): Promise<T> {
    const response = await this.client.post<T>(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total);
          onProgress(progress);
        }
      },
    });
    return this.extractData<T>(response);
  }
}

// Export singleton instance
export const apiClient = new ApiClient();
export default apiClient;