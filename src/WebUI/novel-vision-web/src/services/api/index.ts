// src/services/api/index.ts
// Export all API services

export { apiClient, default as api } from './apiClient';
export { catalogService } from './catalog.service';
export { gutenbergService } from './gutenberg.service';
export { visualizationService } from './visualization.service';
export { readingService } from './reading.service';

// Re-export types from catalog
export type { 
  BookFilters, 
  Genre,
  BookForReading, 
  ChapterForReading, 
  PageForReading,
  VisualizationPoint,
  BookReview,
} from './catalog.service';

// Re-export types from gutenberg
export type { 
  GutenbergSearchParams,
  GutenbergBookPreview,
  GutenbergAuthor,
  ImportStatus,
  ImportProgress,
  BulkImportResult,
} from './gutenberg.service';

// Re-export types from visualization
export type { 
  VisualizationJob, 
  VisualizationResult, 
  AIProviderInfo, 
  VisualizationStyle,
  JobFilters,
  RegenerateOptions,
  EnhancePromptRequest,
  EnhancedPrompt,
  PromptTemplate,
  GeneratePageVisualizationRequest,
  GenerateTextVisualizationRequest,
  GenerateChapterVisualizationRequest,
} from './visualization.service';

// Re-export types from reading
export type { 
  ReadingStats,
  MonthlyStats,
  WeeklyStats,
  ReadingActivity,
  ReadingSession,
  ReadingGoal,
  BookmarkWithDetails,
  ReadingPreferences,
} from './reading.service';