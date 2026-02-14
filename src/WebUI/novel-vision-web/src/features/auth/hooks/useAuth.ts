// src/features/auth/hooks/useAuth.ts
// FIXED: Correct response field names (accessToken instead of token)

import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../../store';
import { authService } from '../services/authService';
import { ROUTES } from '../../../shared/constants/routes';

interface LoginCredentials {
  email: string;
  password: string;
}

interface RegisterData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

export const useAuth = () => {
  const navigate = useNavigate();
  const {
    user,
    isAuthenticated,
    isLoading,
    error,
    setUser,
    setTokens,
    setLoading,
    setError,
    logout: storeLogout,
  } = useAuthStore();

  const clearError = useCallback(() => {
    setError(null);
  }, [setError]);

  const login = useCallback(
    async (credentials: LoginCredentials) => {
      setLoading(true);
      setError(null);
      try {
        const response = await authService.login(credentials);
        
        // FIXED: Backend returns 'accessToken', not 'token'
        if (response.accessToken && response.refreshToken) {
          setTokens(response.accessToken, response.refreshToken);
        }
        if (response.user) {
          setUser(response.user);
        }
        
        // Only navigate if login was successful
        if (response.succeeded || response.success) {
          navigate(ROUTES.HOME);
        }
        
        return response;
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Login failed';
        setError(message);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [navigate, setLoading, setError, setTokens, setUser]
  );

  const register = useCallback(
    async (data: RegisterData) => {
      setLoading(true);
      setError(null);
      try {
        const response = await authService.register(data);
        
        // FIXED: Backend returns 'accessToken', not 'token'
        if (response.accessToken && response.refreshToken) {
          setTokens(response.accessToken, response.refreshToken);
        }
        if (response.user) {
          setUser(response.user);
        }
        
        // Only navigate if registration was successful
        if (response.succeeded || response.success) {
          navigate(ROUTES.HOME);
        }
        
        return response;
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Registration failed';
        setError(message);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [navigate, setLoading, setError, setTokens, setUser]
  );

  const logout = useCallback(async () => {
    try {
      await authService.logout();
    } catch {
      // Ignore logout API errors
    }
    storeLogout();
    navigate(ROUTES.LOGIN);
  }, [navigate, storeLogout]);

  const refreshUser = useCallback(async () => {
    try {
      const user = await authService.getCurrentUser();
      setUser(user);
      return user;
    } catch (err) {
      // If getting user fails, might need to re-login
      console.error('Failed to refresh user:', err);
      throw err;
    }
  }, [setUser]);

  return {
    user,
    isAuthenticated,
    isLoading,
    error,
    login,
    register,
    logout,
    clearError,
    refreshUser,
  };
};

export default useAuth;