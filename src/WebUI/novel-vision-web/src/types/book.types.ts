// src/types/book.types.ts
// Типы данных соответствующие структуре Catalog.API

export interface Author {
  id: string;
  displayName: string;
  email: string;
  biography?: string;
  isVerified: boolean;
  socialLinks?: Record<string, string>;
  createdAt: string;
  updatedAt: string;
}

export interface BookMetadata {
  title: string;
  description: string;
  coverImageUrl?: string;
  language: string;
  pageCount: number;
  wordCount: number;
}

export interface PublicationInfo {
  publisher?: string;
  publicationDate?: string;
  edition?: string;
}

export interface Book {
  id: string;
  metadata: BookMetadata;
  authorId: string;
  author?: Author;
  isbn?: string;
  publicationInfo?: PublicationInfo;
  genres: string[];
  tags: string[];
  rating: number;
  reviewCount: number;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Chapter {
  id: string;
  bookId: string;
  chapterNumber: number;
  title: string;
  content?: string;
  pageCount: number;
  createdAt: string;
}

export interface Page {
  id: string;
  chapterId: string;
  pageNumber: number;
  content: string;
  wordCount: number;
}

// Request types
export interface CreateBookRequest {
  title: string;
  description: string;
  authorId: string;
  coverImageUrl?: string;
  language: string;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  edition?: string;
  genres: string[];
  tags: string[];
}

export interface UpdateBookRequest {
  title?: string;
  description?: string;
  coverImageUrl?: string;
  language?: string;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  edition?: string;
  genres?: string[];
  tags?: string[];
}

export interface CreateAuthorRequest {
  displayName: string;
  email: string;
  biography?: string;
  socialLinks?: Record<string, string>;
}

// Response types
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}