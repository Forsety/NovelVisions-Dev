// src/shared/constants/api.ts
// NovelVision API Configuration - FIXED VERSION with all endpoints including Visualization API

export const API_CONFIG = {
  // Base URLs
  GATEWAY_URL: process.env.REACT_APP_GATEWAY_URL || 'http://localhost:5000',
  CATALOG_API_URL: process.env.REACT_APP_CATALOG_API_URL || 'http://localhost:5231',
  VISUALIZATION_API_URL: process.env.REACT_APP_VISUALIZATION_API_URL || 'https://localhost:7130',
  PROMPTGEN_API_URL: process.env.REACT_APP_PROMPTGEN_API_URL || 'http://localhost:8000',
  
  // Use Gateway or Direct
  USE_GATEWAY: process.env.REACT_APP_USE_GATEWAY === 'true',
  
  // Timeouts
  TIMEOUT: 30000,
  UPLOAD_TIMEOUT: 120000,
} as const;

// Get base URL based on configuration
export const getBaseUrl = (): string => 
  API_CONFIG.USE_GATEWAY ? API_CONFIG.GATEWAY_URL : API_CONFIG.CATALOG_API_URL;

export const getVisualizationUrl = (): string =>
  API_CONFIG.USE_GATEWAY ? API_CONFIG.GATEWAY_URL : API_CONFIG.VISUALIZATION_API_URL;

export const getPromptGenUrl = (): string =>
  API_CONFIG.USE_GATEWAY ? API_CONFIG.GATEWAY_URL : API_CONFIG.PROMPTGEN_API_URL;

// ============================================================================
// DIRECT API ENDPOINTS (когда идём напрямую к сервисам, минуя Gateway)
// Catalog.API: http://localhost:5231
// Visualization.API: https://localhost:7130
// ============================================================================
export const API_ENDPOINTS = {
  // Auth - контроллер AuthController
  AUTH: {
    LOGIN: '/api/v1/Auth/login',
    REGISTER: '/api/v1/Auth/register',
    REFRESH: '/api/v1/Auth/refresh',
    REVOKE: '/api/v1/Auth/revoke',
    PROFILE: '/api/v1/Auth/profile',
    CHANGE_PASSWORD: '/api/v1/Auth/change-password',
    FORGOT_PASSWORD: '/api/v1/Auth/forgot-password',
    RESET_PASSWORD: '/api/v1/Auth/reset-password',
    CONFIRM_EMAIL: '/api/v1/Auth/confirm-email',
  },
  
  // Books - контроллер BooksController
  BOOKS: {
    BASE: '/api/v1/Books',
    BY_ID: (id: string) => `/api/v1/Books/${id}`,
    PUBLISH: (id: string) => `/api/v1/Books/${id}/publish`,
    CHAPTERS: (id: string) => `/api/v1/Books/${id}/chapters`,
    SEARCH: '/api/v1/Books/search/advanced',
    POPULAR: '/api/v1/Books/popular',
    RECENT: '/api/v1/Books/recent',
    FEATURED: '/api/v1/Books/featured',
    BY_GENRE: '/api/v1/Books/by-genre',
    BY_SOURCE: (source: string) => `/api/v1/Books/by-source/${source}`,
  },
  
  // Authors - контроллер AuthorsController
  AUTHORS: {
    BASE: '/api/v1/Authors',
    BY_ID: (id: string) => `/api/v1/Authors/${id}`,
    VERIFY: (id: string) => `/api/v1/Authors/${id}/verify`,
    BOOKS: (authorId: string) => `/api/v1/Authors/${authorId}/books`,
    VERIFIED: '/api/v1/Authors/verified',
  },
  
  // Chapters - контроллер ChaptersController
  CHAPTERS: {
    BASE: (bookId: string) => `/api/v1/books/${bookId}/chapters`,
    CREATE: (bookId: string) => `/api/v1/books/${bookId}/chapters`,
    BY_ID: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}`,
    REORDER: (bookId: string) => `/api/v1/books/${bookId}/chapters/reorder`,
  },
  
  // Pages - контроллер PagesController
  PAGES: {
    BASE: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages`,
    CREATE: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages`,
    BY_ID: (bookId: string, chapterId: string, pageId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages/${pageId}`,
    VISUALIZATION: (bookId: string, chapterId: string, pageId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages/${pageId}/visualization`,
  },
  
  // Books Visualization - контроллер BooksVisualizationController
  BOOKS_VISUALIZATION: {
    SETTINGS: (bookId: string) => `/api/v1/books/${bookId}/visualization/settings`,
    PAGES: (bookId: string) => `/api/v1/books/${bookId}/visualization/pages`,
    SET_POINT: (bookId: string, chapterId: string, pageId: string) => 
      `/api/v1/books/${bookId}/chapters/${chapterId}/pages/${pageId}/visualization-point`,
    SET_VISUALIZATION: (bookId: string, chapterId: string, pageId: string) => 
      `/api/v1/books/${bookId}/chapters/${chapterId}/pages/${pageId}/visualization`,
  },
  
  // Reading (Public) - контроллер ReadingController
  READING: {
    BOOK: (bookId: string) => `/api/v1/reading/books/${bookId}`,
    CHAPTER: (bookId: string, chapterNum: number) => `/api/v1/reading/books/${bookId}/chapters/${chapterNum}`,
    TOC: (bookId: string) => `/api/v1/reading/books/${bookId}/toc`,
    VISUALIZATION_POINTS: (bookId: string) => `/api/v1/reading/books/${bookId}/visualization-points`,
  },
  
  // Import/Gutenberg - контроллер ImportController
  GUTENBERG: {
    SEARCH: '/api/v1/Import/gutenberg/search',
    POPULAR: '/api/v1/Import/gutenberg/popular',
    PREVIEW: (id: number) => `/api/v1/Import/gutenberg/${id}/preview`,
    IMPORT: (id: number) => `/api/v1/Import/gutenberg/${id}`,
    BULK_IMPORT: '/api/v1/Import/gutenberg/bulk',
    HEALTH: '/api/v1/Import/gutenberg/health',
  },
  
  // Subjects - контроллер SubjectsController
  SUBJECTS: {
    BASE: '/api/v1/Subjects',
    HIERARCHY: '/api/v1/Subjects/hierarchy',
    BY_ID: (id: string) => `/api/v1/Subjects/${id}`,
    BY_SLUG: (slug: string) => `/api/v1/Subjects/by-slug/${slug}`,
    BOOKS_BY_ID: (id: string) => `/api/v1/Subjects/${id}/books`,
    BOOKS_BY_SLUG: (slug: string) => `/api/v1/Subjects/by-slug/${slug}/books`,
  },
  
  // ==================== VISUALIZATION API (Direct) ====================
  // Visualization.API: https://localhost:7130
  VISUALIZATION: {
    // Jobs
    JOBS: '/api/v1/visualization/jobs',
    JOB_BY_ID: (id: string) => `/api/v1/visualization/jobs/${id}`,
    MY_JOBS: '/api/v1/visualization/jobs/my',
    
    // Create jobs by type
    CREATE_PAGE: '/api/v1/visualization/jobs/page',
    CREATE_CHAPTER: '/api/v1/visualization/jobs/chapter',
    CREATE_SELECTION: '/api/v1/visualization/jobs/selection',
    CREATE_BATCH: '/api/v1/visualization/jobs/batch',
    
    // Job actions
    CANCEL: (id: string) => `/api/v1/visualization/jobs/${id}/cancel`,
    RETRY: (id: string) => `/api/v1/visualization/jobs/${id}/retry`,
    
    // Images
    IMAGES: (jobId: string) => `/api/v1/visualization/jobs/${jobId}/images`,
    SELECT_IMAGE: (jobId: string, imageId: string) => `/api/v1/visualization/jobs/${jobId}/images/${imageId}/select`,
    DELETE_IMAGE: (jobId: string, imageId: string) => `/api/v1/visualization/jobs/${jobId}/images/${imageId}`,
    
    // Queue
    QUEUE_STATUS: '/api/v1/visualization/queue/status',
    QUEUE_POSITION: (jobId: string) => `/api/v1/visualization/queue/position/${jobId}`,
    
    // Book visualizations
    BOOK_VISUALIZATIONS: (bookId: string) => `/api/v1/visualization/books/${bookId}/visualizations`,
    PAGE_VISUALIZATION: (bookId: string, pageId: string) => `/api/v1/visualization/books/${bookId}/pages/${pageId}/visualization`,
    
    // Providers
    PROVIDERS: '/api/v1/visualization/providers',
    PROVIDER_STATUS: (provider: string) => `/api/v1/visualization/providers/${provider}/status`,
    
    // Health
    HEALTH: '/health',
    PING: '/ping',
  },
  
  // Health
  HEALTH: '/health',
} as const;

// ============================================================================
// GATEWAY ENDPOINTS (когда идём через Ocelot Gateway на порте 5000)
// ============================================================================
export const GATEWAY_ENDPOINTS = {
  // Auth -> /auth/...
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    REFRESH: '/auth/refresh',
    REVOKE: '/auth/revoke',
    PROFILE: '/auth/profile',
    CHANGE_PASSWORD: '/auth/change-password',
    FORGOT_PASSWORD: '/auth/forgot-password',
    RESET_PASSWORD: '/auth/reset-password',
    CONFIRM_EMAIL: '/auth/confirm-email',
  },
  
  // Books -> /catalog/books/...
  BOOKS: {
    BASE: '/catalog/books',
    BY_ID: (id: string) => `/catalog/books/${id}`,
    PUBLISH: (id: string) => `/catalog/books/${id}/publish`,
    CHAPTERS: (id: string) => `/catalog/books/${id}/chapters`,
    SEARCH: '/catalog/books/search/advanced',
    POPULAR: '/catalog/books/popular',
    RECENT: '/catalog/books/recent',
    FEATURED: '/catalog/books/featured',
    BY_GENRE: '/catalog/books/by-genre',
    BY_SOURCE: (source: string) => `/catalog/books/by-source/${source}`,
  },
  
  // Authors -> /catalog/authors/...
  AUTHORS: {
    BASE: '/catalog/authors',
    BY_ID: (id: string) => `/catalog/authors/${id}`,
    VERIFY: (id: string) => `/catalog/authors/${id}/verify`,
    BOOKS: (authorId: string) => `/catalog/authors/${authorId}/books`,
    VERIFIED: '/catalog/authors/verified',
  },
  
  // Chapters -> /catalog/chapters/...
  CHAPTERS: {
    BASE: (bookId: string) => `/catalog/books/${bookId}/chapters`,
    CREATE: (bookId: string) => `/catalog/books/${bookId}/chapters`,
    BY_ID: (bookId: string, chapterId: string) => `/catalog/books/${bookId}/chapters/${chapterId}`,
    REORDER: (bookId: string) => `/catalog/books/${bookId}/chapters/reorder`,
  },
  
  // Pages -> /catalog/pages/...
  PAGES: {
    BASE: (bookId: string, chapterId: string) => `/catalog/books/${bookId}/chapters/${chapterId}/pages`,
    CREATE: (bookId: string, chapterId: string) => `/catalog/books/${bookId}/chapters/${chapterId}/pages`,
    BY_ID: (bookId: string, chapterId: string, pageId: string) => `/catalog/books/${bookId}/chapters/${chapterId}/pages/${pageId}`,
    VISUALIZATION: (bookId: string, chapterId: string, pageId: string) => `/catalog/books/${bookId}/chapters/${chapterId}/pages/${pageId}/visualization`,
  },
  
  // Books Visualization -> /catalog/books/{bookId}/visualization/...
  BOOKS_VISUALIZATION: {
    SETTINGS: (bookId: string) => `/catalog/books/${bookId}/visualization/settings`,
    PAGES: (bookId: string) => `/catalog/books/${bookId}/visualization/pages`,
    SET_POINT: (bookId: string, chapterId: string, pageId: string) => 
      `/catalog/books/${bookId}/chapters/${chapterId}/pages/${pageId}/visualization-point`,
    SET_VISUALIZATION: (bookId: string, chapterId: string, pageId: string) => 
      `/catalog/books/${bookId}/chapters/${chapterId}/pages/${pageId}/visualization`,
  },
  
  // Reading -> /reading/...
  READING: {
    BOOK: (bookId: string) => `/reading/books/${bookId}`,
    CHAPTER: (bookId: string, chapterNum: number) => `/reading/books/${bookId}/chapters/${chapterNum}`,
    TOC: (bookId: string) => `/reading/books/${bookId}/toc`,
    VISUALIZATION_POINTS: (bookId: string) => `/reading/books/${bookId}/visualization-points`,
  },
  
  // Gutenberg -> /catalog/gutenberg/...
  GUTENBERG: {
    SEARCH: '/catalog/gutenberg/search',
    POPULAR: '/catalog/gutenberg/popular',
    PREVIEW: (id: number) => `/catalog/gutenberg/${id}/preview`,
    IMPORT: (id: number) => `/catalog/gutenberg/${id}`,
    BULK_IMPORT: '/catalog/gutenberg/bulk',
    HEALTH: '/catalog/gutenberg/health',
  },
  
  // Subjects -> /catalog/subjects/...
  SUBJECTS: {
    BASE: '/catalog/subjects',
    HIERARCHY: '/catalog/subjects/hierarchy',
    BY_ID: (id: string) => `/catalog/subjects/${id}`,
    BY_SLUG: (slug: string) => `/catalog/subjects/by-slug/${slug}`,
    BOOKS_BY_ID: (id: string) => `/catalog/subjects/${id}/books`,
    BOOKS_BY_SLUG: (slug: string) => `/catalog/subjects/by-slug/${slug}/books`,
  },
  
  // ==================== VISUALIZATION API (через Gateway) ====================
  // Gateway route: /visualization/... -> Visualization.API
  VISUALIZATION: {
    // Jobs
    JOBS: '/visualization/jobs',
    JOB_BY_ID: (id: string) => `/visualization/jobs/${id}`,
    MY_JOBS: '/visualization/jobs/my',
    
    // Create jobs by type
    GENERATE: '/visualization/generate',
    GENERATE_PAGE: '/visualization/generate/page',
    GENERATE_TEXT: '/visualization/generate/text-selection',
    GENERATE_CHAPTER: '/visualization/generate/chapter',
    CREATE_BATCH: '/visualization/batch',
    
    // Job actions
    CANCEL: (id: string) => `/visualization/jobs/${id}/cancel`,
    RETRY: (id: string) => `/visualization/jobs/${id}/retry`,
    
    // Images
    IMAGES: (jobId: string) => `/visualization/jobs/${jobId}/images`,
    SELECT_IMAGE: (jobId: string, imageId: string) => `/visualization/jobs/${jobId}/images/${imageId}/select`,
    DELETE_IMAGE: (jobId: string, imageId: string) => `/visualization/jobs/${jobId}/images/${imageId}`,
    
    // Queue
    QUEUE: '/visualization/queue',
    QUEUE_STATUS: '/visualization/queue/status',
    QUEUE_POSITION: (jobId: string) => `/visualization/queue/position/${jobId}`,
    
    // Book visualizations
    BOOK_VISUALIZATIONS: (bookId: string) => `/visualization/books/${bookId}/visualizations`,
    PAGE_VISUALIZATION: (bookId: string, pageId: string) => `/visualization/books/${bookId}/pages/${pageId}/visualization`,
    
    // Providers
    PROVIDERS: '/visualization/providers',
    PROVIDER_STATUS: (provider: string) => `/visualization/providers/${provider}/status`,
    
    // Health
    HEALTH: '/visualization/health',
  },
  
  // PromptGen API -> /promptgen/...
  PROMPTGEN: {
    PROMPTS: '/promptgen/prompts',
    ENHANCE: '/promptgen/prompts/enhance',
    QUICK: '/promptgen/prompts/quick',
    STYLES: '/promptgen/styles',
    CHARACTERS: '/promptgen/characters',
    VISUALIZATION: '/promptgen/visualization',
    GENERATE_PROMPTS: '/promptgen/visualization/generate-prompts',
    ENHANCE_PROMPT: '/promptgen/visualization/enhance-prompt',
    CHARACTER_CONSISTENCY: '/promptgen/visualization/character-consistency',
    BATCH_GENERATE: '/promptgen/visualization/batch-generate',
    HEALTH: '/promptgen/health',
  },
  
  // Health
  HEALTH: '/health',
  CATALOG_HEALTH: '/catalog/health',
  SERVICES: '/services',
} as const;

// ============================================================================
// Helper function - возвращает нужные эндпоинты в зависимости от конфигурации
// ============================================================================
export const getEndpoints = () => 
  API_CONFIG.USE_GATEWAY ? GATEWAY_ENDPOINTS : API_ENDPOINTS;

// ============================================================================
// Type exports
// ============================================================================
export type ApiEndpoints = typeof API_ENDPOINTS;
export type GatewayEndpoints = typeof GATEWAY_ENDPOINTS;