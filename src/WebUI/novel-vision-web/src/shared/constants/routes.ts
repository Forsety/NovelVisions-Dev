// src/shared/constants/routes.ts
// NovelVision Application Routes

export const ROUTES = {
  // Public
  HOME: '/',
  CATALOG: '/catalog',
  BOOK: '/books/:id',
  BOOK_BY_ID: (id: string) => `/books/${id}`,
  READER: '/read/:bookId',
  READER_BY_ID: (bookId: string) => `/read/${bookId}`,
  AUTHORS: '/authors',
  AUTHOR: '/authors/:id',
  AUTHOR_BY_ID: (id: string) => `/authors/${id}`,
  GUTENBERG: '/gutenberg',
  
  // Auth
  LOGIN: '/login',
  REGISTER: '/register',
  FORGOT_PASSWORD: '/forgot-password',
  RESET_PASSWORD: '/reset-password',
  
  // Protected - User
  PROFILE: '/profile',
  SETTINGS: '/settings',
  BOOKMARKS: '/bookmarks',
  READING_HISTORY: '/history',
  
  // Protected - Author
  AUTHOR_DASHBOARD: '/author/dashboard',
  CREATE_BOOK: '/author/books/new',
  EDIT_BOOK: '/author/books/:id/edit',
  EDIT_BOOK_BY_ID: (id: string) => `/author/books/${id}/edit`,
  MANAGE_CHAPTERS: '/author/books/:id/chapters',
  MANAGE_CHAPTERS_BY_ID: (id: string) => `/author/books/${id}/chapters`,
  
  // Static
  ABOUT: '/about',
  HELP: '/help',
  TERMS: '/terms',
  PRIVACY: '/privacy',
  CONTACT: '/contact',
  
  // Errors
  NOT_FOUND: '/404',
  ERROR: '/error',
} as const;

// Navigation items for header/sidebar
export const NAV_ITEMS = {
  main: [
    { label: 'Home', path: ROUTES.HOME, icon: '🏠' },
    { label: 'Library', path: ROUTES.CATALOG, icon: '📚' },
    { label: 'Authors', path: ROUTES.AUTHORS, icon: '✍️' },
    { label: 'Gutenberg', path: ROUTES.GUTENBERG, icon: '📖' },
  ],
  author: [
    { label: 'Dashboard', path: ROUTES.AUTHOR_DASHBOARD, icon: '📊' },
    { label: 'Create Book', path: ROUTES.CREATE_BOOK, icon: '➕' },
  ],
  user: [
    { label: 'Profile', path: ROUTES.PROFILE, icon: '👤' },
    { label: 'Bookmarks', path: ROUTES.BOOKMARKS, icon: '🔖' },
    { label: 'History', path: ROUTES.READING_HISTORY, icon: '📜' },
    { label: 'Settings', path: ROUTES.SETTINGS, icon: '⚙️' },
  ],
  footer: [
    { label: 'About', path: ROUTES.ABOUT },
    { label: 'Help', path: ROUTES.HELP },
    { label: 'Terms', path: ROUTES.TERMS },
    { label: 'Privacy', path: ROUTES.PRIVACY },
    { label: 'Contact', path: ROUTES.CONTACT },
  ],
} as const;