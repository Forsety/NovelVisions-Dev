// src/store/index.ts
// Re-export all stores

import useAuthStore from './useAuthStore';
import useUIStore from './useUIStore';

export { useAuthStore, useUIStore };
export type { AuthState } from './useAuthStore';
export type { UIState } from './useUIStore';