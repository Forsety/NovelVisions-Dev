// src/pages/index.ts
// NovelVision Page Exports

// ============================================================
// PUBLIC PAGES
// ============================================================
export { default as HomePage } from './HomePage/HomePage';
export { default as CatalogPage } from './CatalogPage/CatalogPage';
export { default as BookPage } from './BookPage';
export { default as ReaderPage } from './ReaderPage';
export { default as GutenbergPage } from './GutenbergPage';

// ============================================================
// AUTHOR PAGES (from AuthorPages folder)
// ============================================================
export { default as AuthorsPage } from './AuthorPages/AuthorsPage';
export { default as DashboardPage } from './AuthorPages/DashboardPage';
export { default as CreateBookPage } from './AuthorPages/CreateBookPage';

// ============================================================
// AUTHOR PROFILE PAGE
// ============================================================
export { default as AuthorProfilePage } from './AuthorProfilePage';

// ============================================================
// AUTH PAGES
// ============================================================
export { default as LoginPage } from './AuthPages/LoginPage';
export { default as RegisterPage } from './AuthPages/RegisterPage';
export { default as ForgotPasswordPage } from './ForgotPasswordPage';
export { default as ResetPasswordPage } from './ResetPasswordPage';

// ============================================================
// USER PAGES (Protected)
// ============================================================
export { default as ProfilePage } from './ProfilePage';
export { default as SettingsPage } from './SettingsPage';
export { default as BookmarksPage } from './BookmarksPage';

// ============================================================
// ERROR PAGES
// ============================================================
export { default as NotFoundPage } from './ErrorPages/NotFoundPage';