// src/shared/utils/storage.ts
// Local storage utilities with type safety

const PREFIX = 'nv_';

export const storage = {
  get<T>(key: string, defaultValue?: T): T | null {
    if (typeof window === 'undefined') return defaultValue ?? null;
    
    try {
      const item = localStorage.getItem(PREFIX + key);
      if (item === null) return defaultValue ?? null;
      return JSON.parse(item) as T;
    } catch {
      return defaultValue ?? null;
    }
  },

  set<T>(key: string, value: T): void {
    if (typeof window === 'undefined') return;
    
    try {
      localStorage.setItem(PREFIX + key, JSON.stringify(value));
    } catch (error) {
      console.error('Error saving to localStorage:', error);
    }
  },

  remove(key: string): void {
    if (typeof window === 'undefined') return;
    localStorage.removeItem(PREFIX + key);
  },

  clear(): void {
    if (typeof window === 'undefined') return;
    
    // Only clear items with our prefix
    Object.keys(localStorage)
      .filter((key) => key.startsWith(PREFIX))
      .forEach((key) => localStorage.removeItem(key));
  },

  has(key: string): boolean {
    if (typeof window === 'undefined') return false;
    return localStorage.getItem(PREFIX + key) !== null;
  },
};

// Session storage utilities
export const sessionStorage = {
  get<T>(key: string, defaultValue?: T): T | null {
    if (typeof window === 'undefined') return defaultValue ?? null;
    
    try {
      const item = window.sessionStorage.getItem(PREFIX + key);
      if (item === null) return defaultValue ?? null;
      return JSON.parse(item) as T;
    } catch {
      return defaultValue ?? null;
    }
  },

  set<T>(key: string, value: T): void {
    if (typeof window === 'undefined') return;
    
    try {
      window.sessionStorage.setItem(PREFIX + key, JSON.stringify(value));
    } catch (error) {
      console.error('Error saving to sessionStorage:', error);
    }
  },

  remove(key: string): void {
    if (typeof window === 'undefined') return;
    window.sessionStorage.removeItem(PREFIX + key);
  },

  clear(): void {
    if (typeof window === 'undefined') return;
    
    Object.keys(window.sessionStorage)
      .filter((key) => key.startsWith(PREFIX))
      .forEach((key) => window.sessionStorage.removeItem(key));
  },
};

export default storage;