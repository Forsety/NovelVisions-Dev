// src/store/useUIStore.ts
// UI State Store using Zustand

import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { ThemeMode } from '../shared/constants/theme';

interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number;
}

// Export interface for use in other files
export interface UIState {
  // Theme
  theme: ThemeMode;
  setTheme: (theme: ThemeMode) => void;
  toggleTheme: () => void;

  // Sidebar
  sidebarOpen: boolean;
  sidebarCollapsed: boolean;
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  setSidebarCollapsed: (collapsed: boolean) => void;

  // Modal
  activeModal: string | null;
  modalData: unknown;
  openModal: (modalId: string, data?: unknown) => void;
  closeModal: () => void;

  // Toast notifications
  toasts: Toast[];
  addToast: (toast: Omit<Toast, 'id'>) => void;
  removeToast: (id: string) => void;
  clearToasts: () => void;

  // Loading states
  globalLoading: boolean;
  setGlobalLoading: (loading: boolean) => void;

  // Mobile
  isMobile: boolean;
  setIsMobile: (isMobile: boolean) => void;
}

export const useUIStore = create<UIState>()(
  persist(
    (set, get) => ({
      // Theme
      theme: 'dark' as ThemeMode,
      setTheme: (theme: ThemeMode) => {
        document.documentElement.setAttribute('data-theme', theme);
        set({ theme });
      },
      toggleTheme: () => {
        const themes: ThemeMode[] = ['dark', 'light', 'sepia'];
        const currentIndex = themes.indexOf(get().theme);
        const nextTheme = themes[(currentIndex + 1) % themes.length];
        document.documentElement.setAttribute('data-theme', nextTheme);
        set({ theme: nextTheme });
      },

      // Sidebar
      sidebarOpen: false,
      sidebarCollapsed: false,
      toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
      setSidebarOpen: (open: boolean) => set({ sidebarOpen: open }),
      setSidebarCollapsed: (collapsed: boolean) => set({ sidebarCollapsed: collapsed }),

      // Modal
      activeModal: null,
      modalData: null,
      openModal: (modalId: string, data?: unknown) => set({ activeModal: modalId, modalData: data }),
      closeModal: () => set({ activeModal: null, modalData: null }),

      // Toast
      toasts: [],
      addToast: (toast: Omit<Toast, 'id'>) => {
        const id = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const newToast = { ...toast, id };
        set((state) => ({ toasts: [...state.toasts, newToast] }));

        // Auto remove after duration
        const duration = toast.duration ?? 5000;
        if (duration > 0) {
          setTimeout(() => {
            get().removeToast(id);
          }, duration);
        }
      },
      removeToast: (id: string) => set((state) => ({ 
        toasts: state.toasts.filter((t) => t.id !== id) 
      })),
      clearToasts: () => set({ toasts: [] }),

      // Loading
      globalLoading: false,
      setGlobalLoading: (loading: boolean) => set({ globalLoading: loading }),

      // Mobile
      isMobile: false,
      setIsMobile: (isMobile: boolean) => set({ isMobile }),
    }),
    {
      name: 'nv-ui-storage',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({ 
        theme: state.theme,
        sidebarCollapsed: state.sidebarCollapsed 
      }),
    }
  )
);

// Initialize theme on load
if (typeof window !== 'undefined') {
  const stored = localStorage.getItem('nv-ui-storage');
  if (stored) {
    try {
      const { state } = JSON.parse(stored);
      if (state?.theme) {
        document.documentElement.setAttribute('data-theme', state.theme);
      }
    } catch {
      document.documentElement.setAttribute('data-theme', 'dark');
    }
  }
}

export default useUIStore;