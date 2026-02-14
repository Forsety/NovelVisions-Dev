// src/app/AppRouter.tsx
// NovelVision Application Router - FIXED для roles[] массива

import React, { Suspense, lazy } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { ROUTES } from '../shared/constants/routes';
import { useAuthStore } from '../store';
import { AuthLayout, MainLayout } from '../layouts';

// ============================================================
// LAZY LOAD ALL PAGES
// ============================================================

// Public Pages
const HomePage = lazy(() => import('../pages/HomePage/HomePage'));
const CatalogPage = lazy(() => import('../pages/CatalogPage/CatalogPage'));
const BookPage = lazy(() => import('../pages/BookPage'));
const ReaderPage = lazy(() => import('../pages/ReaderPage'));
const GutenbergPage = lazy(() => import('../pages/GutenbergPage'));

// Authors Pages
const AuthorsPage = lazy(() => import('../pages/AuthorPages/AuthorsPage'));
const AuthorProfilePage = lazy(() => import('../pages/AuthorProfilePage'));

// Auth Pages
const LoginPage = lazy(() => import('../pages/AuthPages/LoginPage'));
const RegisterPage = lazy(() => import('../pages/AuthPages/RegisterPage'));
const ForgotPasswordPage = lazy(() => import('../pages/ForgotPasswordPage'));
const ResetPasswordPage = lazy(() => import('../pages/ResetPasswordPage'));

// User Pages (Protected)
const ProfilePage = lazy(() => import('../pages/ProfilePage'));
const SettingsPage = lazy(() => import('../pages/SettingsPage'));
const BookmarksPage = lazy(() => import('../pages/BookmarksPage'));

// Author Pages (Protected - Author Role)
const AuthorDashboardPage = lazy(() => import('../pages/AuthorPages/DashboardPage'));
const CreateBookPage = lazy(() => import('../pages/AuthorPages/CreateBookPage'));

// Error Pages
const NotFoundPage = lazy(() => import('../pages/ErrorPages/NotFoundPage'));

// ============================================================
// PAGE LOADER COMPONENT
// ============================================================
const PageLoader: React.FC = () => (
  <div className="page-loader">
    <div className="loader-spinner" />
    <p>Loading...</p>
  </div>
);

// ============================================================
// HELPER: Check if user has required role
// ============================================================
const hasRole = (user: any, requiredRole: string): boolean => {
  if (!user) return false;
  
  // Check roles array (from backend)
  if (user.roles && Array.isArray(user.roles)) {
    if (user.roles.includes(requiredRole)) return true;
    if (user.roles.includes('Admin')) return true;
  }
  
  // Check single role string (fallback)
  if (user.role) {
    if (user.role === requiredRole) return true;
    if (user.role === 'Admin') return true;
  }
  
  return false;
};

// ============================================================
// PROTECTED ROUTE WRAPPER - FIXED для roles[]
// ============================================================
interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: 'Author' | 'Admin';
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRole }) => {
  const { isAuthenticated, isLoading, user } = useAuthStore();

  if (isLoading) return <PageLoader />;
  if (!isAuthenticated) return <Navigate to={ROUTES.LOGIN} replace />;
  
  // Check role if required
  if (requiredRole && !hasRole(user, requiredRole)) {
    console.log('ProtectedRoute: User lacks required role', { 
      requiredRole, 
      userRole: user?.role,
      userRoles: user?.roles 
    });
    return <Navigate to={ROUTES.HOME} replace />;
  }

  return <>{children}</>;
};

// ============================================================
// GUEST ROUTE WRAPPER (for auth pages - redirect if logged in)
// ============================================================
const GuestRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuthStore();

  if (isLoading) return <PageLoader />;
  if (isAuthenticated) return <Navigate to={ROUTES.HOME} replace />;

  return <>{children}</>;
};

// ============================================================
// MAIN APP ROUTER
// ============================================================
const AppRouter: React.FC = () => {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* ==================== PUBLIC ROUTES WITH MAIN LAYOUT ==================== */}
        <Route element={<MainLayout />}>
          {/* Home */}
          <Route path={ROUTES.HOME} element={<HomePage />} />
          
          {/* Catalog / Library */}
          <Route path={ROUTES.CATALOG} element={<CatalogPage />} />
          
          {/* Book Details */}
          <Route path={ROUTES.BOOK} element={<BookPage />} />
          
          {/* Authors List */}
          <Route path={ROUTES.AUTHORS} element={<AuthorsPage />} />
          
          {/* Author Profile */}
          <Route path={ROUTES.AUTHOR} element={<AuthorProfilePage />} />
          
          {/* Gutenberg Import */}
          <Route path={ROUTES.GUTENBERG} element={<GutenbergPage />} />

          {/* ==================== USER PROTECTED ROUTES ==================== */}
          
          {/* Profile */}
          <Route 
            path={ROUTES.PROFILE} 
            element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} 
          />
          
          {/* Settings */}
          <Route 
            path={ROUTES.SETTINGS} 
            element={<ProtectedRoute><SettingsPage /></ProtectedRoute>} 
          />
          
          {/* Bookmarks */}
          <Route 
            path={ROUTES.BOOKMARKS} 
            element={<ProtectedRoute><BookmarksPage /></ProtectedRoute>} 
          />

          {/* ==================== AUTHOR PROTECTED ROUTES ==================== */}
          
          {/* Author Dashboard */}
          <Route 
            path={ROUTES.AUTHOR_DASHBOARD} 
            element={<ProtectedRoute requiredRole="Author"><AuthorDashboardPage /></ProtectedRoute>} 
          />
          
          {/* Create Book */}
          <Route 
            path={ROUTES.CREATE_BOOK} 
            element={<ProtectedRoute requiredRole="Author"><CreateBookPage /></ProtectedRoute>} 
          />
          
          {/* Edit Book (same component as Create) */}
          <Route 
            path={ROUTES.EDIT_BOOK} 
            element={<ProtectedRoute requiredRole="Author"><CreateBookPage /></ProtectedRoute>} 
          />

          {/* ==================== ERROR ROUTES ==================== */}
          <Route path={ROUTES.NOT_FOUND} element={<NotFoundPage />} />
        </Route>

        {/* ==================== READER (Full screen, no layout) ==================== */}
        <Route path={ROUTES.READER} element={<ReaderPage />} />

        {/* ==================== AUTH ROUTES (Guest Only) ==================== */}
        
        {/* Auth Layout for Login/Register */}
        <Route element={<GuestRoute><AuthLayout /></GuestRoute>}>
          <Route path={ROUTES.LOGIN} element={<LoginPage />} />
          <Route path={ROUTES.REGISTER} element={<RegisterPage />} />
        </Route>
        
        {/* Password Recovery (standalone pages with their own styling) */}
        <Route 
          path={ROUTES.FORGOT_PASSWORD} 
          element={<GuestRoute><ForgotPasswordPage /></GuestRoute>} 
        />
        <Route 
          path={ROUTES.RESET_PASSWORD} 
          element={<GuestRoute><ResetPasswordPage /></GuestRoute>} 
        />
        
        {/* Catch all - redirect to 404 */}
        <Route path="*" element={<Navigate to={ROUTES.NOT_FOUND} replace />} />
      </Routes>
    </Suspense>
  );
};

export default AppRouter;