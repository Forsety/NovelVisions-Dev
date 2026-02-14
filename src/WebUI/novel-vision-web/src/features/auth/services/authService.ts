// src/features/auth/services/authService.ts
// FIXED: Added becomeAuthor method for upgrading Reader to Author role

import { apiClient } from '../../../services/api/apiClient';
import type { User } from '../../../types/api.types';

// ============================================================
// UNIFIED STORAGE KEYS - Must match apiClient.ts and useAuthStore.ts
// ============================================================
const STORAGE_KEYS = {
  TOKEN: 'accessToken',
  REFRESH_TOKEN: 'refreshToken',
} as const;

// ============================================================
// Request/Response Types
// ============================================================
interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

interface AuthResponse {
  // Success indicators
  succeeded?: boolean;
  success?: boolean;
  
  // Tokens
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: string;
  
  // User data
  user?: User;
  
  // Error fields
  message?: string;
  error?: string;
  errors?: string[];
}

interface BecomeAuthorResponse {
  success: boolean;
  message: string;
  user: User;
}

// ============================================================
// Auth Service
// ============================================================
export const authService = {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    return apiClient.post<AuthResponse>('/auth/login', credentials);
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    return apiClient.post<AuthResponse>('/auth/register', data);
  },

  async refreshToken(refreshToken: string, accessToken?: string): Promise<AuthResponse> {
    // Backend RefreshTokenRequest expects both tokens
    return apiClient.post<AuthResponse>('/auth/refresh', { 
      accessToken: accessToken || localStorage.getItem(STORAGE_KEYS.TOKEN) || '',
      refreshToken 
    });
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/revoke', {
        refreshToken: localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN) || ''
      });
    } catch {
      // Ignore logout errors - still clear local storage
    }
    // Clear using UNIFIED keys
    localStorage.removeItem(STORAGE_KEYS.TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    // Also clear Zustand persisted state
    localStorage.removeItem('auth-storage');
  },

  async getCurrentUser(): Promise<User> {
    return apiClient.get<User>('/auth/profile');
  },

  async updateProfile(data: Partial<User>): Promise<User> {
    return apiClient.put<User>('/auth/profile', data);
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await apiClient.post('/auth/change-password', {
      currentPassword,
      newPassword,
      confirmNewPassword: newPassword,
    });
  },

  async forgotPassword(email: string): Promise<void> {
    await apiClient.post('/auth/forgot-password', { email });
  },

  async resetPassword(token: string, email: string, newPassword: string): Promise<void> {
    await apiClient.post('/auth/reset-password', {
      token,
      email,
      newPassword,
      confirmPassword: newPassword,
    });
  },

  /**
   * Upgrade current user to Author role
   * Allows any authenticated Reader to become an author
   */
  async becomeAuthor(): Promise<BecomeAuthorResponse> {
    return apiClient.post<BecomeAuthorResponse>('/auth/become-author');
  },

  // Helper to check if user is authenticated
  isAuthenticated(): boolean {
    return !!localStorage.getItem(STORAGE_KEYS.TOKEN);
  },

  // Helper to get current token
  getToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.TOKEN);
  },
};

export default authService;