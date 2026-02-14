// src/store/useAuthStore.ts
// Auth State Store using Zustand
// FIXED: Unified token storage with apiClient, correct response field names

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User } from '../types/api.types';
import { apiClient } from '../services/api/apiClient';
import { getEndpoints } from '../shared/constants/api';

const endpoints = getEndpoints();

// ============================================================
// UNIFIED STORAGE KEYS - Must match apiClient.ts
// ============================================================
const STORAGE_KEYS = {
  TOKEN: 'accessToken',
  REFRESH_TOKEN: 'refreshToken',
} as const;

export interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  
  // Actions
  setUser: (user: User | null) => void;
  updateUser: (user: User) => void;
  setTokens: (token: string, refreshToken: string) => void;
  clearAuth: () => void;
  logout: () => void;
  checkAuth: () => Promise<void>;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearError: () => void;
  login: (email: string, password: string) => Promise<boolean>;
  register: (data: RegisterData) => Promise<boolean>;
}

interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

// ============================================================
// API Response interfaces matching backend AuthenticationResult
// ============================================================
interface LoginResponse {
  succeeded?: boolean;       // Backend uses 'succeeded'
  success?: boolean;         // Fallback
  accessToken?: string;      // Backend uses 'accessToken' (not 'token')
  refreshToken?: string;
  expiresAt?: string;
  user?: User;
  message?: string;
  error?: string;
  errors?: string[];
}

interface RegisterResponse {
  succeeded?: boolean;
  success?: boolean;
  message?: string;
  error?: string;
  errors?: string[];
  user?: User;
  accessToken?: string;
  refreshToken?: string;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      setUser: (user) => set({ user, isAuthenticated: !!user }),
      
      updateUser: (user) => set({ user }),
      
      // ============================================================
      // UNIFIED setTokens - writes to both store AND localStorage
      // ============================================================
      setTokens: (token, refreshToken) => {
        // Write to localStorage (for apiClient to read)
        localStorage.setItem(STORAGE_KEYS.TOKEN, token);
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
        // Update store state
        set({ token, refreshToken, isAuthenticated: true });
      },
      
      // ============================================================
      // UNIFIED clearAuth - clears both store AND localStorage
      // ============================================================
      clearAuth: () => {
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        set({
          user: null,
          token: null,
          refreshToken: null,
          isAuthenticated: false,
          error: null,
        });
      },
      
      logout: () => {
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        set({
          user: null,
          token: null,
          refreshToken: null,
          isAuthenticated: false,
          error: null,
        });
      },
      
      checkAuth: async () => {
        // Check both store and localStorage
        const token = get().token || localStorage.getItem(STORAGE_KEYS.TOKEN);
        if (!token) {
          set({ isAuthenticated: false, user: null });
          return;
        }
        
        // Sync localStorage token to store if needed
        if (!get().token && token) {
          const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
          set({ 
            token, 
            refreshToken: refreshToken || get().refreshToken,
            isAuthenticated: true 
          });
        } else {
          set({ isAuthenticated: true });
        }
      },
      
      setLoading: (isLoading) => set({ isLoading }),
      
      setError: (error) => set({ error }),
      
      clearError: () => set({ error: null }),
      
      // ============================================================
      // LOGIN - Fixed to handle backend response correctly
      // ============================================================
      login: async (email: string, password: string): Promise<boolean> => {
        set({ isLoading: true, error: null });
        
        try {
          const response = await apiClient.post<LoginResponse>(endpoints.AUTH.LOGIN, {
            email,
            password,
          });
          
          console.log('[Auth] Login response:', response); // Debug logging
          
          // Check for success (backend returns "succeeded")
          if (response.succeeded || response.success) {
            const accessToken = response.accessToken;
            const refreshToken = response.refreshToken;
            const user = response.user;
            
            if (accessToken && refreshToken) {
              // Save tokens using unified method
              localStorage.setItem(STORAGE_KEYS.TOKEN, accessToken);
              localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
              
              // Update store state
              set({
                token: accessToken,
                refreshToken: refreshToken,
                user: user || null,
                isAuthenticated: true,
                isLoading: false,
                error: null,
              });
              
              console.log('[Auth] Login successful, tokens saved');
              return true;
            } else {
              console.warn('[Auth] Login succeeded but no tokens in response');
            }
          }
          
          // Login failed
          const errorMessage = response.error || response.message || response.errors?.[0] || 'Login failed';
          console.warn('[Auth] Login failed:', errorMessage);
          set({ error: errorMessage, isLoading: false });
          return false;
          
        } catch (error) {
          const errorMessage = (error as Error).message || 'Login failed';
          console.error('[Auth] Login error:', error);
          set({ error: errorMessage, isLoading: false });
          return false;
        }
      },
      
      // ============================================================
      // REGISTER - Fixed to handle backend response correctly
      // ============================================================
      register: async (data: RegisterData): Promise<boolean> => {
        set({ isLoading: true, error: null });
        
        try {
          const response = await apiClient.post<RegisterResponse>(endpoints.AUTH.REGISTER, {
            email: data.email,
            password: data.password,
            confirmPassword: data.password,
            firstName: data.firstName,
            lastName: data.lastName,
          });
          
          console.log('[Auth] Register response:', response); // Debug logging
          
          if (response.succeeded || response.success) {
            // If registration returns tokens, auto-login
            if (response.accessToken && response.refreshToken) {
              // Save tokens using unified method
              localStorage.setItem(STORAGE_KEYS.TOKEN, response.accessToken);
              localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, response.refreshToken);
              
              set({
                token: response.accessToken,
                refreshToken: response.refreshToken,
                user: response.user || null,
                isAuthenticated: true,
                isLoading: false,
                error: null,
              });
              
              console.log('[Auth] Registration successful with auto-login');
            } else {
              // Registration succeeded but no auto-login
              console.log('[Auth] Registration successful, no auto-login');
              set({ isLoading: false });
            }
            return true;
          }
          
          const errorMessage = response.error || response.message || response.errors?.[0] || 'Registration failed';
          console.warn('[Auth] Registration failed:', errorMessage);
          set({ error: errorMessage, isLoading: false });
          return false;
          
        } catch (error) {
          const errorMessage = (error as Error).message || 'Registration failed';
          console.error('[Auth] Registration error:', error);
          set({ error: errorMessage, isLoading: false });
          return false;
        }
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        token: state.token,
        refreshToken: state.refreshToken,
        user: state.user,
      }),
    }
  )
);

export default useAuthStore;