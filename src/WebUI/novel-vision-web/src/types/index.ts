// src/types/index.ts
// Re-export all types

export * from './api.types';

// Additional common types that might not be in api.types

export interface Genre {
  id: string;
  name: string;
  icon?: string;
  color?: string;
  bookCount?: number;
}

export interface Language {
  code: string;
  name: string;
}

export interface Theme {
  id: 'dark' | 'light' | 'sepia';
  name: string;
  colors: {
    primary: string;
    background: string;
    text: string;
  };
}

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number;
}

export interface Modal {
  id: string;
  isOpen: boolean;
  data?: unknown;
}