// src/layouts/MainLayout/MainLayout.tsx
// ╔══════════════════════════════════════════════════════════════════════════════════════════════════╗
// ║   NOVELVISION MAIN LAYOUT v3.0                                                                   ║
// ║   Premium Library - Application Shell with Header, Footer & Navigation                           ║
// ╚══════════════════════════════════════════════════════════════════════════════════════════════════╝

import React, { useState, useEffect, useCallback, useRef } from 'react';
import { Outlet, Link, NavLink, useNavigate, useLocation } from 'react-router-dom';
import { motion, AnimatePresence, useScroll, useTransform } from 'framer-motion';
import { useAuthStore } from '../../store';
import { useUIStore } from '../../store/useUIStore';
import { ROUTES } from '../../shared/constants/routes';
import styles from './MainLayout.module.css';

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// TYPES & INTERFACES
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

interface NavItem {
  label: string;
  path: string;
  icon: string;
  badge?: number;
}

interface UserDropdownProps {
  user: any;
  onLogout: () => void;
  onClose: () => void;
}

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// CONSTANTS
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const NAV_ITEMS: NavItem[] = [
  { label: 'Home', path: ROUTES.HOME, icon: '🏠' },
  { label: 'Library', path: ROUTES.CATALOG, icon: '📚' },
  { label: 'Authors', path: ROUTES.AUTHORS, icon: '✍️' },
  { label: 'Gutenberg', path: ROUTES.GUTENBERG, icon: '📖' },
];

const USER_NAV_ITEMS: NavItem[] = [
  { label: 'My Library', path: ROUTES.PROFILE, icon: '📚' },
  { label: 'Bookmarks', path: ROUTES.BOOKMARKS, icon: '🔖' },
  { label: 'Settings', path: ROUTES.SETTINGS, icon: '⚙️' },
];

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// HEADER COMPONENT
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const Header: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, isAuthenticated, logout } = useAuthStore();
  const { theme, setTheme } = useUIStore();
  
  const [isScrolled, setIsScrolled] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  
  const userMenuRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);

  // Check if on home page for transparent header
  const isHomePage = location.pathname === ROUTES.HOME;
  const showTransparent = isHomePage && !isScrolled;

  // Handle scroll
  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Close menus on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) {
        setIsUserMenuOpen(false);
      }
    };
    
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Close mobile menu on route change
  useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [location.pathname]);

  // Focus search input when opened
  useEffect(() => {
    if (isSearchOpen && searchInputRef.current) {
      searchInputRef.current.focus();
    }
  }, [isSearchOpen]);

  // Handle search submit
  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`${ROUTES.CATALOG}?search=${encodeURIComponent(searchQuery.trim())}`);
      setSearchQuery('');
      setIsSearchOpen(false);
    }
  };

  // Handle logout
  const handleLogout = () => {
    logout();
    setIsUserMenuOpen(false);
    navigate(ROUTES.HOME);
  };

  // Toggle theme
  const toggleTheme = () => {
    const themes = ['dark', 'light', 'sepia'] as const;
    const currentIndex = themes.indexOf(theme as any);
    const nextIndex = (currentIndex + 1) % themes.length;
    setTheme(themes[nextIndex]);
  };

  // Get user initials
  const getUserInitials = (name: string) => {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <>
      <header 
        className={`${styles.header} ${isScrolled ? styles.scrolled : ''} ${showTransparent ? styles.transparent : ''}`}
      >
        <div className={styles.headerInner}>
          {/* Logo */}
          <Link to={ROUTES.HOME} className={styles.logo}>
            <div className={styles.logoIcon}>
              <span>📚</span>
            </div>
            <div className={styles.logoText}>
              <span className={styles.logoTitle}>NovelVision</span>
              <span className={styles.logoSubtitle}>AI-Powered Reading</span>
            </div>
          </Link>

          {/* Desktop Navigation */}
          <nav className={styles.nav}>
            {NAV_ITEMS.map((item) => (
              <NavLink
                key={item.path}
                to={item.path}
                className={({ isActive }) => 
                  `${styles.navLink} ${isActive ? styles.active : ''}`
                }
              >
                <span className={styles.navIcon}>{item.icon}</span>
                <span className={styles.navLabel}>{item.label}</span>
                {item.badge && item.badge > 0 && (
                  <span className={styles.navBadge}>{item.badge}</span>
                )}
              </NavLink>
            ))}
          </nav>

          {/* Right Actions */}
          <div className={styles.actions}>
            {/* Search */}
            <div className={styles.searchWrapper}>
              <AnimatePresence>
                {isSearchOpen && (
                  <motion.form
                    className={styles.searchForm}
                    onSubmit={handleSearchSubmit}
                    initial={{ width: 0, opacity: 0 }}
                    animate={{ width: 280, opacity: 1 }}
                    exit={{ width: 0, opacity: 0 }}
                    transition={{ duration: 0.25 }}
                  >
                    <input
                      ref={searchInputRef}
                      type="text"
                      placeholder="Search books, authors..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      className={styles.searchInput}
                    />
                    <button type="submit" className={styles.searchSubmit}>
                      🔍
                    </button>
                  </motion.form>
                )}
              </AnimatePresence>
              <button
                className={styles.iconBtn}
                onClick={() => setIsSearchOpen(!isSearchOpen)}
                aria-label="Search"
              >
                {isSearchOpen ? '✕' : '🔍'}
              </button>
            </div>

            {/* Theme Toggle */}
            <button
              className={styles.iconBtn}
              onClick={toggleTheme}
              aria-label="Toggle theme"
              title={`Theme: ${theme}`}
            >
              {theme === 'dark' ? '🌙' : theme === 'light' ? '☀️' : '📜'}
            </button>

            {/* Auth Section */}
            {isAuthenticated && user ? (
              <div className={styles.userSection} ref={userMenuRef}>
                <button
                  className={styles.userBtn}
                  onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                  aria-expanded={isUserMenuOpen}
                >
                  <div className={styles.userAvatar}>
                    {user.avatarUrl ? (
                      <img src={user.avatarUrl} alt={user.displayName} />
                    ) : (
                      <span>{getUserInitials(user.displayName || user.email)}</span>
                    )}
                  </div>
                  <span className={styles.userName}>{user.displayName || 'User'}</span>
                  <span className={`${styles.userChevron} ${isUserMenuOpen ? styles.open : ''}`}>
                    ▼
                  </span>
                </button>

                <AnimatePresence>
                  {isUserMenuOpen && (
                    <motion.div
                      className={styles.userDropdown}
                      initial={{ opacity: 0, y: -10, scale: 0.95 }}
                      animate={{ opacity: 1, y: 0, scale: 1 }}
                      exit={{ opacity: 0, y: -10, scale: 0.95 }}
                      transition={{ duration: 0.15 }}
                    >
                      <div className={styles.dropdownHeader}>
                        <div className={styles.dropdownAvatar}>
                          {user.avatarUrl ? (
                            <img src={user.avatarUrl} alt={user.displayName} />
                          ) : (
                            <span>{getUserInitials(user.displayName || user.email)}</span>
                          )}
                        </div>
                        <div className={styles.dropdownInfo}>
                          <span className={styles.dropdownName}>{user.displayName}</span>
                          <span className={styles.dropdownEmail}>{user.email}</span>
                        </div>
                      </div>

                      <div className={styles.dropdownDivider} />

                      <nav className={styles.dropdownNav}>
                        {USER_NAV_ITEMS.map((item) => (
                          <Link
                            key={item.path}
                            to={item.path}
                            className={styles.dropdownItem}
                            onClick={() => setIsUserMenuOpen(false)}
                          >
                            <span className={styles.dropdownIcon}>{item.icon}</span>
                            <span>{item.label}</span>
                          </Link>
                        ))}
                      </nav>

                      <div className={styles.dropdownDivider} />

                      {/* Author Dashboard for authors */}
                      {user.roles?.includes('Author') && (
                        <>
                          <Link
                            to={ROUTES.AUTHOR_DASHBOARD}
                            className={styles.dropdownItem}
                            onClick={() => setIsUserMenuOpen(false)}
                          >
                            <span className={styles.dropdownIcon}>✍️</span>
                            <span>Author Dashboard</span>
                          </Link>
                          <div className={styles.dropdownDivider} />
                        </>
                      )}

                      <button
                        className={`${styles.dropdownItem} ${styles.danger}`}
                        onClick={handleLogout}
                      >
                        <span className={styles.dropdownIcon}>🚪</span>
                        <span>Sign Out</span>
                      </button>
                    </motion.div>
                  )}
                </AnimatePresence>
              </div>
            ) : (
              <div className={styles.authButtons}>
                <Link to={ROUTES.LOGIN} className={styles.loginBtn}>
                  Sign In
                </Link>
                <Link to={ROUTES.REGISTER} className={styles.registerBtn}>
                  Get Started
                </Link>
              </div>
            )}

            {/* Mobile Menu Toggle */}
            <button
              className={styles.mobileToggle}
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              aria-label="Toggle menu"
              aria-expanded={isMobileMenuOpen}
            >
              <span className={`${styles.hamburger} ${isMobileMenuOpen ? styles.open : ''}`}>
                <span />
                <span />
                <span />
              </span>
            </button>
          </div>
        </div>
      </header>

      {/* Mobile Menu */}
      <AnimatePresence>
        {isMobileMenuOpen && (
          <>
            <motion.div
              className={styles.mobileOverlay}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setIsMobileMenuOpen(false)}
            />
            <motion.div
              className={styles.mobileMenu}
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              transition={{ type: 'spring', damping: 25, stiffness: 300 }}
            >
              <div className={styles.mobileMenuHeader}>
                <span className={styles.mobileMenuTitle}>Menu</span>
                <button
                  className={styles.mobileClose}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  ✕
                </button>
              </div>

              {/* Mobile Search */}
              <form className={styles.mobileSearch} onSubmit={handleSearchSubmit}>
                <input
                  type="text"
                  placeholder="Search books..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
                <button type="submit">🔍</button>
              </form>

              {/* Mobile Nav */}
              <nav className={styles.mobileNav}>
                {NAV_ITEMS.map((item, index) => (
                  <motion.div
                    key={item.path}
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: index * 0.05 }}
                  >
                    <NavLink
                      to={item.path}
                      className={({ isActive }) =>
                        `${styles.mobileNavLink} ${isActive ? styles.active : ''}`
                      }
                    >
                      <span className={styles.mobileNavIcon}>{item.icon}</span>
                      <span>{item.label}</span>
                    </NavLink>
                  </motion.div>
                ))}
              </nav>

              {/* Mobile User Section */}
              {isAuthenticated && user ? (
                <div className={styles.mobileUserSection}>
                  <div className={styles.mobileUserInfo}>
                    <div className={styles.mobileUserAvatar}>
                      {user.avatarUrl ? (
                        <img src={user.avatarUrl} alt={user.displayName} />
                      ) : (
                        <span>{getUserInitials(user.displayName || user.email)}</span>
                      )}
                    </div>
                    <div>
                      <span className={styles.mobileUserName}>{user.displayName}</span>
                      <span className={styles.mobileUserEmail}>{user.email}</span>
                    </div>
                  </div>

                  <nav className={styles.mobileUserNav}>
                    {USER_NAV_ITEMS.map((item) => (
                      <NavLink
                        key={item.path}
                        to={item.path}
                        className={styles.mobileNavLink}
                      >
                        <span className={styles.mobileNavIcon}>{item.icon}</span>
                        <span>{item.label}</span>
                      </NavLink>
                    ))}
                  </nav>

                  <button
                    className={styles.mobileLogout}
                    onClick={handleLogout}
                  >
                    <span>🚪</span>
                    <span>Sign Out</span>
                  </button>
                </div>
              ) : (
                <div className={styles.mobileAuth}>
                  <Link to={ROUTES.LOGIN} className={styles.mobileLoginBtn}>
                    Sign In
                  </Link>
                  <Link to={ROUTES.REGISTER} className={styles.mobileRegisterBtn}>
                    Create Account
                  </Link>
                </div>
              )}
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// FOOTER COMPONENT
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className={styles.footer}>
      <div className={styles.footerInner}>
        {/* Footer Brand */}
        <div className={styles.footerBrand}>
          <Link to={ROUTES.HOME} className={styles.footerLogo}>
            <span className={styles.footerLogoIcon}>📚</span>
            <span className={styles.footerLogoText}>NovelVision</span>
          </Link>
          <p className={styles.footerTagline}>
            Where Stories Come to Life with AI
          </p>
        </div>

        {/* Footer Links */}
        <div className={styles.footerLinks}>
          <div className={styles.footerColumn}>
            <h4>Explore</h4>
            <Link to={ROUTES.CATALOG}>Library</Link>
            <Link to={ROUTES.AUTHORS}>Authors</Link>
            <Link to={ROUTES.GUTENBERG}>Free Books</Link>
          </div>
          <div className={styles.footerColumn}>
            <h4>Account</h4>
            <Link to={ROUTES.LOGIN}>Sign In</Link>
            <Link to={ROUTES.REGISTER}>Create Account</Link>
            <Link to={ROUTES.SETTINGS}>Settings</Link>
          </div>
          <div className={styles.footerColumn}>
            <h4>Support</h4>
            <a href="#">Help Center</a>
            <a href="#">Contact Us</a>
            <a href="#">Feedback</a>
          </div>
        </div>

        {/* Footer Bottom */}
        <div className={styles.footerBottom}>
          <p className={styles.footerCopyright}>
            © {currentYear} NovelVision. Crafted with ❤️ for book lovers.
          </p>
          <div className={styles.footerSocial}>
            <a href="#" aria-label="Twitter">𝕏</a>
            <a href="#" aria-label="GitHub">🐙</a>
            <a href="#" aria-label="Discord">💬</a>
          </div>
        </div>
      </div>
    </footer>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// SCROLL TO TOP BUTTON
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const ScrollToTop: React.FC = () => {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const toggleVisibility = () => {
      setIsVisible(window.scrollY > 500);
    };

    window.addEventListener('scroll', toggleVisibility, { passive: true });
    return () => window.removeEventListener('scroll', toggleVisibility);
  }, []);

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <AnimatePresence>
      {isVisible && (
        <motion.button
          className={styles.scrollToTop}
          onClick={scrollToTop}
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          exit={{ opacity: 0, scale: 0.8 }}
          whileHover={{ scale: 1.1 }}
          whileTap={{ scale: 0.9 }}
          aria-label="Scroll to top"
        >
          ↑
        </motion.button>
      )}
    </AnimatePresence>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// MAIN LAYOUT COMPONENT
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const MainLayout: React.FC = () => {
  const location = useLocation();
  const { theme } = useUIStore();

  // Set theme on document
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
  }, [theme]);

  return (
    <div className={styles.layout} data-theme={theme}>
      <Header />
      
      <main className={styles.main}>
        <AnimatePresence mode="wait">
          <motion.div
            key={location.pathname}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.3 }}
          >
            <Outlet />
          </motion.div>
        </AnimatePresence>
      </main>

      <Footer />
      <ScrollToTop />
    </div>
  );
};

export default MainLayout;