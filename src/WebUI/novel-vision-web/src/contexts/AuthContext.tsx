// src/contexts/AuthContext.tsx
import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import CatalogApiService, { UserDto } from '../services/catalog-api.service';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  role: string;
  isEmailConfirmed: boolean;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, firstName: string, lastName: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    try {
      setIsLoading(true);
      
      if (CatalogApiService.isAuthenticated()) {
        const currentUser = CatalogApiService.getCurrentUser();
        
        if (currentUser) {
          setUser({
            id: currentUser.id,
            email: currentUser.email,
            firstName: currentUser.firstName,
            lastName: currentUser.lastName,
            displayName: currentUser.displayName,
            role: currentUser.role,
            isEmailConfirmed: currentUser.isEmailConfirmed
          });
          setIsAuthenticated(true);
        } else {
          // Try to get profile from server
          try {
            const profile = await CatalogApiService.getProfile();
            setUser({
              id: profile.id,
              email: profile.email,
              firstName: profile.firstName,
              lastName: profile.lastName,
              displayName: profile.displayName,
              role: profile.role,
              isEmailConfirmed: profile.isEmailConfirmed
            });
            setIsAuthenticated(true);
          } catch (error) {
            console.error('Failed to get profile:', error);
            setIsAuthenticated(false);
          }
        }
      }
    } catch (error) {
      console.error('Auth check error:', error);
      setIsAuthenticated(false);
    } finally {
      setIsLoading(false);
    }
  };

  const login = async (email: string, password: string) => {
    try {
      const result = await CatalogApiService.login({ email, password });
      
      if (!result.succeeded) {
        throw new Error(result.error || 'Ошибка входа');
      }
      
      if (result.user) {
        setUser({
          id: result.user.id,
          email: result.user.email,
          firstName: result.user.firstName,
          lastName: result.user.lastName,
          displayName: result.user.displayName,
          role: result.user.role,
          isEmailConfirmed: result.user.isEmailConfirmed
        });
        setIsAuthenticated(true);
      }
    } catch (error: any) {
      console.error('Login error:', error);
      throw error;
    }
  };

  const register = async (
    email: string,
    password: string,
    firstName: string,
    lastName: string
  ) => {
    try {
      const result = await CatalogApiService.register({
        email,
        password,
        firstName,
        lastName
      });
      
      if (!result.succeeded) {
        throw new Error(result.error || 'Ошибка регистрации');
      }
      
      if (result.user) {
        setUser({
          id: result.user.id,
          email: result.user.email,
          firstName: result.user.firstName,
          lastName: result.user.lastName,
          displayName: result.user.displayName,
          role: result.user.role,
          isEmailConfirmed: result.user.isEmailConfirmed
        });
        setIsAuthenticated(true);
      }
    } catch (error: any) {
      console.error('Register error:', error);
      throw error;
    }
  };

  const logout = async () => {
    try {
      await CatalogApiService.logout();
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setUser(null);
      setIsAuthenticated(false);
    }
  };

  const refreshUser = async () => {
    if (!isAuthenticated) return;
    
    try {
      const profile = await CatalogApiService.getProfile();
      setUser({
        id: profile.id,
        email: profile.email,
        firstName: profile.firstName,
        lastName: profile.lastName,
        displayName: profile.displayName,
        role: profile.role,
        isEmailConfirmed: profile.isEmailConfirmed
      });
    } catch (error) {
      console.error('Failed to refresh user:', error);
    }
  };

  return (
    <AuthContext.Provider 
      value={{
        user,
        isAuthenticated,
        isLoading,
        login,
        register,
        logout,
        refreshUser
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};