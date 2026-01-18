// src/config/api.config.ts
// NovelVision API Configuration

type EndpointFunc = (...args: string[]) => string;

interface AuthEndpoints {
  LOGIN: string;
  REGISTER: string;
  REFRESH_TOKEN: string;
  REVOKE_TOKEN: string;
  PROFILE: string;
  CHANGE_PASSWORD: string;
  FORGOT_PASSWORD: string;
  CONFIRM_EMAIL: string;
}

interface BookEndpoints {
  BASE: string;
  BY_ID: EndpointFunc;
  SEARCH: string;
  BY_GENRE: string;
  FEATURED: string;
  POPULAR: string;
}

interface AuthorEndpoints {
  BASE: string;
  BY_ID: EndpointFunc;
  BOOKS: EndpointFunc;
  VERIFIED: string;
}

interface ChapterEndpoints {
  BASE: EndpointFunc;
  BY_ID: (bookId: string, chapterId: string) => string;
  ADD: EndpointFunc;
}

interface PageEndpoints {
  BASE: (bookId: string, chapterId: string) => string;
  BY_ID: (bookId: string, chapterId: string, pageId: string) => string;
  ADD: (bookId: string, chapterId: string) => string;
}

interface ReadingEndpoints {
  BOOK: EndpointFunc;
  CHAPTER: (bookId: string, chapterNumber: number) => string;
  PROGRESS: EndpointFunc;
}

interface VisualizationEndpoints {
  JOBS: string;
  JOB_BY_ID: EndpointFunc;
  GENERATE_PAGE: string;
  GENERATE_TEXT: string;
  GENERATE_CHAPTER: string;
  PROVIDERS: string;
  STYLES: string;
}

interface GutenbergEndpoints {
  SEARCH: string;
  IMPORT: string;
  BY_ID: EndpointFunc;
  HEALTH: string;
}

interface Endpoints {
  AUTH: AuthEndpoints;
  BOOKS: BookEndpoints;
  AUTHORS: AuthorEndpoints;
  CHAPTERS: ChapterEndpoints;
  PAGES: PageEndpoints;
  READING: ReadingEndpoints;
  VISUALIZATION: VisualizationEndpoints;
  GUTENBERG: GutenbergEndpoints;
  HEALTH: string;
  SERVICES: string;
}

interface ApiConfig {
  GATEWAY_URL: string;
  CATALOG_API_URL: string;
  VISUALIZATION_API_URL: string;
  PROMPTGEN_API_URL: string;
  USE_GATEWAY: boolean;
  GATEWAY_ENDPOINTS: Endpoints;
  DIRECT_ENDPOINTS: Endpoints;
  readonly ENDPOINTS: Endpoints;
}

const createGatewayEndpoints = (): Endpoints => ({
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    REFRESH_TOKEN: '/auth/refresh',
    REVOKE_TOKEN: '/auth/revoke',
    PROFILE: '/auth/profile',
    CHANGE_PASSWORD: '/auth/change-password',
    FORGOT_PASSWORD: '/auth/forgot-password',
    CONFIRM_EMAIL: '/auth/confirm-email'
  },
  BOOKS: {
    BASE: '/catalog/books',
    BY_ID: (id: string) => `/catalog/books/${id}`,
    SEARCH: '/catalog/books/search',
    BY_GENRE: '/catalog/books/by-genre',
    FEATURED: '/catalog/books/featured',
    POPULAR: '/catalog/books/popular'
  },
  AUTHORS: {
    BASE: '/catalog/authors',
    BY_ID: (id: string) => `/catalog/authors/${id}`,
    BOOKS: (authorId: string) => `/catalog/authors/${authorId}/books`,
    VERIFIED: '/catalog/authors/verified'
  },
  CHAPTERS: {
    BASE: (bookId: string) => `/catalog/books/${bookId}/chapters`,
    BY_ID: (bookId: string, chapterId: string) => `/catalog/books/${bookId}/chapters/${chapterId}`,
    ADD: (bookId: string) => `/catalog/books/${bookId}/chapters`
  },
  PAGES: {
    BASE: (bookId: string, chapterId: string) => `/catalog/books/${bookId}/chapters/${chapterId}/pages`,
    BY_ID: (bookId: string, chapterId: string, pageId: string) => 
      `/catalog/books/${bookId}/chapters/${chapterId}/pages/${pageId}`,
    ADD: (bookId: string, chapterId: string) => `/catalog/books/${bookId}/chapters/${chapterId}/pages`
  },
  READING: {
    BOOK: (bookId: string) => `/reading/books/${bookId}`,
    CHAPTER: (bookId: string, chapterNumber: number) => `/reading/books/${bookId}/chapters/${chapterNumber}`,
    PROGRESS: (bookId: string) => `/reading/books/${bookId}/progress`
  },
  VISUALIZATION: {
    JOBS: '/visualization/jobs',
    JOB_BY_ID: (jobId: string) => `/visualization/jobs/${jobId}`,
    GENERATE_PAGE: '/visualization/generate/page',
    GENERATE_TEXT: '/visualization/generate/text-selection',
    GENERATE_CHAPTER: '/visualization/generate/chapter',
    PROVIDERS: '/visualization/providers',
    STYLES: '/promptgen/styles'
  },
  GUTENBERG: {
    SEARCH: '/catalog/gutenberg/search',
    IMPORT: '/catalog/gutenberg/import',
    BY_ID: (gutenbergId: string) => `/catalog/gutenberg/${gutenbergId}`,
    HEALTH: '/catalog/gutenberg/health'
  },
  HEALTH: '/health',
  SERVICES: '/services'
});

const createDirectEndpoints = (): Endpoints => ({
  AUTH: {
    LOGIN: '/api/v1/auth/login',
    REGISTER: '/api/v1/auth/register',
    REFRESH_TOKEN: '/api/v1/auth/refresh',
    REVOKE_TOKEN: '/api/v1/auth/revoke',
    PROFILE: '/api/v1/auth/profile',
    CHANGE_PASSWORD: '/api/v1/auth/change-password',
    FORGOT_PASSWORD: '/api/v1/auth/forgot-password',
    CONFIRM_EMAIL: '/api/v1/auth/confirm-email'
  },
  BOOKS: {
    BASE: '/api/v1/books',
    BY_ID: (id: string) => `/api/v1/books/${id}`,
    SEARCH: '/api/v1/books/search',
    BY_GENRE: '/api/v1/books/by-genre',
    FEATURED: '/api/v1/books/featured',
    POPULAR: '/api/v1/books/popular'
  },
  AUTHORS: {
    BASE: '/api/v1/authors',
    BY_ID: (id: string) => `/api/v1/authors/${id}`,
    BOOKS: (authorId: string) => `/api/v1/authors/${authorId}/books`,
    VERIFIED: '/api/v1/authors/verified'
  },
  CHAPTERS: {
    BASE: (bookId: string) => `/api/v1/books/${bookId}/chapters`,
    BY_ID: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}`,
    ADD: (bookId: string) => `/api/v1/books/${bookId}/chapters`
  },
  PAGES: {
    BASE: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages`,
    BY_ID: (bookId: string, chapterId: string, pageId: string) => 
      `/api/v1/books/${bookId}/chapters/${chapterId}/pages/${pageId}`,
    ADD: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages`
  },
  READING: {
    BOOK: (bookId: string) => `/api/v1/reading/books/${bookId}`,
    CHAPTER: (bookId: string, chapterNumber: number) => `/api/v1/reading/books/${bookId}/chapters/${chapterNumber}`,
    PROGRESS: (bookId: string) => `/api/v1/reading/books/${bookId}/progress`
  },
  VISUALIZATION: {
    JOBS: '/api/v1/jobs',
    JOB_BY_ID: (jobId: string) => `/api/v1/jobs/${jobId}`,
    GENERATE_PAGE: '/api/v1/visualizations/page',
    GENERATE_TEXT: '/api/v1/visualizations/text-selection',
    GENERATE_CHAPTER: '/api/v1/visualizations/chapter',
    PROVIDERS: '/api/v1/providers',
    STYLES: '/api/v1/styles'
  },
  GUTENBERG: {
    SEARCH: '/api/v1/gutenberg/search',
    IMPORT: '/api/v1/gutenberg/import',
    BY_ID: (gutenbergId: string) => `/api/v1/gutenberg/${gutenbergId}`,
    HEALTH: '/api/v1/gutenberg/health'
  },
  HEALTH: '/health',
  SERVICES: '/services'
});

export const API_CONFIG: ApiConfig = {
  GATEWAY_URL: process.env.REACT_APP_GATEWAY_URL || 'http://localhost:5000',
  CATALOG_API_URL: process.env.REACT_APP_CATALOG_API_URL || 'https://localhost:7295',
  VISUALIZATION_API_URL: process.env.REACT_APP_VISUALIZATION_API_URL || 'https://localhost:7130',
  PROMPTGEN_API_URL: process.env.REACT_APP_PROMPTGEN_API_URL || 'http://localhost:8000',
  USE_GATEWAY: process.env.REACT_APP_USE_GATEWAY === 'true',
  GATEWAY_ENDPOINTS: createGatewayEndpoints(),
  DIRECT_ENDPOINTS: createDirectEndpoints(),
  get ENDPOINTS() {
    return this.USE_GATEWAY ? this.GATEWAY_ENDPOINTS : this.DIRECT_ENDPOINTS;
  }
};

export const getBaseUrl = (): string => 
  API_CONFIG.USE_GATEWAY ? API_CONFIG.GATEWAY_URL : API_CONFIG.CATALOG_API_URL;

export const getVisualizationUrl = (): string => 
  API_CONFIG.USE_GATEWAY ? API_CONFIG.GATEWAY_URL : API_CONFIG.VISUALIZATION_API_URL;

export const getPromptGenUrl = (): string =>
  API_CONFIG.USE_GATEWAY ? API_CONFIG.GATEWAY_URL : API_CONFIG.PROMPTGEN_API_URL;

export default API_CONFIG;