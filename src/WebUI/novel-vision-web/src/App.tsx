// src/App.tsx
import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';

// Pages
import HomePage from './pages/HomePage';
import CatalogPage from './pages/CatalogPage';
import BookDetailPage from './pages/BookDetailPage';
import ReaderPage from './pages/ReaderPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ProfilePage from './pages/ProfilePage';
import AuthorsPage from './pages/AuthorsPage';
import CreateBookPage from './pages/CreateBookPage';
import AuthorDashboardPage from './pages/AuthorDashboardPage';

// Context
import { AuthProvider, useAuth } from './contexts/AuthContext';

// Styles
import './App.css';

// ========================================
// NAVBAR COMPONENT
// ========================================
const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <nav className="navbar">
      <div className="nav-container">
        {/* Logo */}
        <Link to="/" className="nav-brand">
          <span className="brand-icon">🔮</span>
          <span className="brand-text">NovelVision</span>
        </Link>

        {/* Navigation Links */}
        <div className="nav-links">
          <Link to="/" className="nav-link">Home</Link>
          <Link to="/catalog" className="nav-link">Library</Link>
          <Link to="/authors" className="nav-link">Authors</Link>
          {isAuthenticated && user?.role === 'Author' && (
            <Link to="/author/dashboard" className="nav-link">Dashboard</Link>
          )}
        </div>

        {/* User Actions */}
        <div className="nav-actions">
          {isAuthenticated ? (
            <div className="user-menu">
              <button className="user-avatar" title={user?.displayName || user?.email}>
                <span className="avatar-letter">
                  {user?.firstName?.charAt(0).toUpperCase() || '?'}
                </span>
              </button>
              <div className="user-dropdown">
                <div className="dropdown-header">
                  <span className="user-name">{user?.displayName || `${user?.firstName} ${user?.lastName}`}</span>
                  <span className="user-email">{user?.email}</span>
                  <span className="user-role">{user?.role}</span>
                </div>
                <div className="dropdown-divider"></div>
                <Link to="/profile" className="dropdown-item">
                  <span className="item-icon">👤</span>
                  Profile
                </Link>
                {user?.role === 'Author' && (
                  <Link to="/author/dashboard" className="dropdown-item">
                    <span className="item-icon">✍️</span>
                    Author Dashboard
                  </Link>
                )}
                <Link to="/profile" className="dropdown-item">
                  <span className="item-icon">⚙️</span>
                  Settings
                </Link>
                <div className="dropdown-divider"></div>
                <button onClick={logout} className="dropdown-item logout">
                  <span className="item-icon">🚪</span>
                  Sign Out
                </button>
              </div>
            </div>
          ) : (
            <div className="auth-buttons">
              <Link to="/login" className="btn-login">Sign In</Link>
              <Link to="/register" className="btn-register">Get Started</Link>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
};

// ========================================
// PROTECTED ROUTE COMPONENT
// ========================================
const ProtectedRoute: React.FC<{ 
  children: React.ReactNode;
  requiredRole?: string;
}> = ({ children, requiredRole }) => {
  const { isAuthenticated, isLoading, user } = useAuth();
  
  if (isLoading) {
    return (
      <div className="loading-screen">
        <div className="loading-content">
          <div className="loading-spinner"></div>
          <p>Loading...</p>
        </div>
      </div>
    );
  }
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && user?.role !== requiredRole) {
    return <Navigate to="/" replace />;
  }
  
  return <>{children}</>;
};

// ========================================
// FOOTER COMPONENT
// ========================================
const Footer: React.FC = () => {
  return (
    <footer className="footer">
      <div className="footer-content">
        <div className="footer-main">
          <div className="footer-brand">
            <Link to="/" className="brand-link">
              <span className="brand-icon">🔮</span>
              <span className="brand-text">NovelVision</span>
            </Link>
            <p className="brand-tagline">
              Experience stories with AI-powered visualization
            </p>
          </div>

          <div className="footer-links">
            <div className="link-group">
              <h4>Explore</h4>
              <Link to="/catalog">Library</Link>
              <Link to="/authors">Authors</Link>
              <Link to="/genres">Genres</Link>
            </div>
            <div className="link-group">
              <h4>Create</h4>
              <Link to="/publish">Publish</Link>
              <Link to="/author/dashboard">Dashboard</Link>
              <Link to="/pricing">Pricing</Link>
            </div>
            <div className="link-group">
              <h4>Support</h4>
              <Link to="/help">Help Center</Link>
              <Link to="/contact">Contact</Link>
              <Link to="/faq">FAQ</Link>
            </div>
            <div className="link-group">
              <h4>Legal</h4>
              <Link to="/terms">Terms of Service</Link>
              <Link to="/privacy">Privacy Policy</Link>
              <Link to="/cookies">Cookie Policy</Link>
            </div>
          </div>
        </div>

        <div className="footer-bottom">
          <p className="copyright">
            © 2024 NovelVision. All rights reserved.
          </p>
          <div className="api-status">
            <span className="status-indicator online"></span>
            <span>All systems operational</span>
          </div>
        </div>
      </div>
    </footer>
  );
};

// ========================================
// MAIN APP COMPONENT
// ========================================
function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="app">
          <Navbar />

          <main className="main-content">
            <Routes>
              {/* Public Routes */}
              <Route path="/" element={<HomePage />} />
              <Route path="/catalog" element={<CatalogPage />} />
              <Route path="/authors" element={<AuthorsPage />} />
              <Route path="/books/:id" element={<BookDetailPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />

              {/* Reader Route (Public but tracks progress for authenticated users) */}
              <Route path="/read/:bookId" element={<ReaderPage />} />

              {/* Protected Routes - Any authenticated user */}
              <Route 
                path="/profile" 
                element={
                  <ProtectedRoute>
                    <ProfilePage />
                  </ProtectedRoute>
                } 
              />

              {/* Protected Routes - Authors only */}
              <Route 
                path="/publish" 
                element={
                  <ProtectedRoute requiredRole="Author">
                    <CreateBookPage />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/author/dashboard" 
                element={
                  <ProtectedRoute requiredRole="Author">
                    <AuthorDashboardPage />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/author/books/new" 
                element={
                  <ProtectedRoute requiredRole="Author">
                    <CreateBookPage />
                  </ProtectedRoute>
                } 
              />
              <Route 
                path="/author/books/:id/edit" 
                element={
                  <ProtectedRoute requiredRole="Author">
                    <CreateBookPage />
                  </ProtectedRoute>
                } 
              />

              {/* Placeholder Routes */}
              <Route path="/genres" element={<PlaceholderPage title="Genres" icon="📚" />} />
              <Route path="/pricing" element={<PlaceholderPage title="Pricing" icon="💎" />} />
              <Route path="/help" element={<PlaceholderPage title="Help Center" icon="❓" />} />
              <Route path="/contact" element={<PlaceholderPage title="Contact Us" icon="📧" />} />
              <Route path="/faq" element={<PlaceholderPage title="FAQ" icon="💬" />} />
              <Route path="/terms" element={<PlaceholderPage title="Terms of Service" icon="📋" />} />
              <Route path="/privacy" element={<PlaceholderPage title="Privacy Policy" icon="🔒" />} />
              <Route path="/cookies" element={<PlaceholderPage title="Cookie Policy" icon="🍪" />} />

              {/* Catch-all redirect */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </main>

          <Footer />
        </div>
      </Router>
    </AuthProvider>
  );
}

// ========================================
// PLACEHOLDER PAGE COMPONENT
// ========================================
const PlaceholderPage: React.FC<{ title: string; icon: string }> = ({ title, icon }) => {
  return (
    <div className="placeholder-page">
      <div className="placeholder-content">
        <span className="placeholder-icon">{icon}</span>
        <h1>{title}</h1>
        <p>This page is coming soon</p>
        <Link to="/" className="btn-back-home">
          ← Back to Home
        </Link>
      </div>
    </div>
  );
};

export default App;