// src/config/api.config.ts
// Конфигурация для подключения к NovelVision API

export const API_CONFIG = {
  // API Gateway URL 
  GATEWAY_URL: process.env.REACT_APP_GATEWAY_URL || 'http://localhost:5000',
  
  // Catalog API URL (HTTPS)
  CATALOG_API_URL: process.env.REACT_APP_CATALOG_API_URL || 'https://localhost:7295',
  
  // Используем ли API Gateway или прямое подключение
  USE_GATEWAY: false, // Установите true, если хотите использовать API Gateway
  
  // Endpoints для прямого подключения к Catalog API
  ENDPOINTS: {
    // Auth endpoints (Identity встроен в Catalog.API)
    AUTH: {
      LOGIN: '/api/v1/auth/login',
      REGISTER: '/api/v1/auth/register',
      REFRESH_TOKEN: '/api/v1/auth/refresh',
      REVOKE_TOKEN: '/api/v1/auth/revoke',
      PROFILE: '/api/v1/auth/profile',
      CHANGE_PASSWORD: '/api/v1/auth/change-password',
      FORGOT_PASSWORD: '/api/v1/auth/forgot-password',
      RESET_PASSWORD: '/api/v1/auth/reset-password',
      CONFIRM_EMAIL: '/api/v1/auth/confirm-email'
    },
    
    // Books endpoints
    BOOKS: {
      BASE: '/api/v1/books',
      BY_ID: (id: string) => `/api/v1/books/${id}`,
      SEARCH: '/api/v1/books/search',
      BY_GENRE: '/api/v1/books/by-genre',
      BY_AUTHOR: (authorId: string) => `/api/v1/books/author/${authorId}`
    },
    
    // Authors endpoints  
    AUTHORS: {
      BASE: '/api/v1/authors',
      BY_ID: (id: string) => `/api/v1/authors/${id}`,
      BY_EMAIL: '/api/v1/authors/by-email',
      VERIFIED: '/api/v1/authors/verified',
      BOOKS: (authorId: string) => `/api/v1/authors/${authorId}/books`
    },
    
    // Chapters endpoints
    CHAPTERS: {
      BASE: (bookId: string) => `/api/v1/books/${bookId}/chapters`,
      BY_ID: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}`,
      ADD: (bookId: string) => `/api/v1/books/${bookId}/chapters`
    },
    
    // Pages endpoints
    PAGES: {
      BASE: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages`,
      BY_ID: (bookId: string, chapterId: string, pageNumber: number) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages/${pageNumber}`,
      ADD: (bookId: string, chapterId: string) => `/api/v1/books/${bookId}/chapters/${chapterId}/pages`
    }
  },
  
  // Endpoints при использовании API Gateway
  GATEWAY_ENDPOINTS: {
    AUTH: {
      LOGIN: '/auth/login',
      REGISTER: '/auth/register',
      REFRESH_TOKEN: '/auth/refresh',
      PROFILE: '/auth/profile'
    },
    BOOKS: {
      BASE: '/catalog/books',
      BY_ID: (id: string) => `/catalog/books/${id}`
    },
    AUTHORS: {
      BASE: '/catalog/authors',
      BY_ID: (id: string) => `/catalog/authors/${id}`
    }
  }
};