// src/services/api.service.ts
// Base API Service with Axios

import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { getBaseUrl, API_CONFIG } from '../config/api.config';

class ApiService {
  private client: AxiosInstance;
  private isRefreshing = false;
  private failedQueue: Array<{
    resolve: (value?: any) => void;
    reject: (error?: any) => void;
  }> = [];

  constructor(baseURL?: string) {
    this.client = axios.create({
      baseURL: baseURL || getBaseUrl(),
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    });

    this.setupInterceptors();
  }

  private setupInterceptors() {
    // Request interceptor
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

    // Response interceptor
    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as any;

        if (error.response?.status === 401 && !originalRequest._retry) {
          if (this.isRefreshing) {
            return new Promise((resolve, reject) => {
              this.failedQueue.push({ resolve, reject });
            })
              .then((token) => {
                originalRequest.headers.Authorization = `Bearer ${token}`;
                return this.client(originalRequest);
              })
              .catch((err) => Promise.reject(err));
          }

          originalRequest._retry = true;
          this.isRefreshing = true;

          try {
            const newToken = await this.refreshToken();
            this.processQueue(null, newToken);
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return this.client(originalRequest);
          } catch (refreshError) {
            this.processQueue(refreshError, null);
            this.logout();
            return Promise.reject(refreshError);
          } finally {
            this.isRefreshing = false;
          }
        }

        return Promise.reject(this.handleError(error));
      }
    );
  }

  private processQueue(error: any, token: string | null) {
    this.failedQueue.forEach(({ resolve, reject }) => {
      if (error) {
        reject(error);
      } else {
        resolve(token);
      }
    });
    this.failedQueue = [];
  }

  private getToken(): string | null {
    return localStorage.getItem('token');
  }

  private getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  private setTokens(token: string, refreshToken: string) {
    localStorage.setItem('token', token);
    localStorage.setItem('refreshToken', refreshToken);
  }

  private async refreshToken(): Promise<string> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) throw new Error('No refresh token');

    const response = await axios.post(
      `${getBaseUrl()}${API_CONFIG.ENDPOINTS.AUTH.REFRESH_TOKEN}`,
      { refreshToken }
    );

    const { token, refreshToken: newRefreshToken } = response.data;
    this.setTokens(token, newRefreshToken);
    return token;
  }

  private logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    window.location.href = '/login';
  }

  private handleError(error: AxiosError): Error {
    if (error.response) {
      const data = error.response.data as any;
      const message = data?.error || data?.message || data?.title || 
        `Server error: ${error.response.status}`;
      return new Error(message);
    }
    if (error.request) {
      return new Error('Server unavailable. Please check your connection.');
    }
    return new Error(error.message || 'An unexpected error occurred');
  }

  // HTTP Methods
  async get<T>(url: string, params?: any): Promise<T> {
    const response = await this.client.get<T>(url, { params });
    return this.extractData(response.data);
  }

  async post<T>(url: string, data?: any): Promise<T> {
    const response = await this.client.post<T>(url, data);
    return this.extractData(response.data);
  }

  async put<T>(url: string, data?: any): Promise<T> {
    const response = await this.client.put<T>(url, data);
    return this.extractData(response.data);
  }

  async patch<T>(url: string, data?: any): Promise<T> {
    const response = await this.client.patch<T>(url, data);
    return this.extractData(response.data);
  }

  async delete<T>(url: string): Promise<T> {
    const response = await this.client.delete<T>(url);
    return this.extractData(response.data);
  }

  private extractData<T>(data: any): T {
    // Handle wrapped responses
    if (data && typeof data === 'object') {
      if ('data' in data && data.data !== undefined) {
        return data.data;
      }
    }
    return data;
  }

  // Auth helpers
  setAuth(token: string, refreshToken: string, user?: any) {
    this.setTokens(token, refreshToken);
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
    }
  }

  clearAuth() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getUser(): any | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }
}

export const apiService = new ApiService();
export default apiService;