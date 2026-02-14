// src/app/App.tsx
// Main Application Component

import React, { useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { useAuthStore, useUIStore } from '../store';
import AppRouter from './AppRouter';

const App: React.FC = () => {
  const checkAuth = useAuthStore((state) => state.checkAuth);
  const setIsMobile = useUIStore((state) => state.setIsMobile);
  const theme = useUIStore((state) => state.theme);

  // Check authentication on mount
  useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  // Handle responsive detection
  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 768);
    };

    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, [setIsMobile]);

  // Apply theme
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
  }, [theme]);

  return (
    <BrowserRouter>
      <AppRouter />
    </BrowserRouter>
  );
};

export default App;