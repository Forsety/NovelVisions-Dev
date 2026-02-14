// src/services/api/visualization.service.ts
// NovelVision Visualization API Service
// Integrates with Visualization.API backend for AI image generation

import axios from 'axios';
import { API_CONFIG, getEndpoints } from '../../shared/constants/api';

// ==================== TYPES ====================

export type VisualizationMode = 'None' | 'PerPage' | 'PerChapter' | 'UserSelected';
export type ImagePosition = 'left' | 'right' | 'center' | 'alternate';
export type ImageSize = 'small' | 'medium' | 'large';
export type JobStatus = 'Pending' | 'Queued' | 'GeneratingPrompt' | 'Processing' | 'Uploading' | 'Completed' | 'Failed' | 'Cancelled';
export type AIProvider = 'DallE3' | 'Midjourney' | 'StableDiffusion' | 'Leonardo';

// ==================== DTOs ====================

export interface GenerationParameters {
  size?: string;
  quality?: string;
  aspectRatio?: string;
  seed?: number;
  steps?: number;
  cfgScale?: number;
  sampler?: string;
  upscale?: boolean;
}

export interface TextSelection {
  selectedText: string;
  startPosition?: number;
  endPosition?: number;
  pageId?: string;
  chapterId?: string;
  contextBefore?: string;
  contextAfter?: string;
}

export interface GeneratedImage {
  id: string;
  jobId: string;
  imageUrl: string;
  thumbnailUrl?: string;
  isSelected: boolean;
  variationIndex: number;
  width: number;
  height: number;
  createdAt: string;
}

export interface VisualizationJob {
  id: string;
  bookId: string;
  pageId?: string;
  chapterId?: string;
  userId: string;
  status: JobStatus;
  statusDisplayName: string;
  trigger: string;
  preferredProvider: AIProvider;
  parameters: GenerationParameters;
  textSelection?: TextSelection;
  promptData?: {
    originalText?: string;
    enhancedPrompt?: string;
    negativePrompt?: string;
    style?: string;
  };
  images: GeneratedImage[];
  selectedImage?: GeneratedImage;
  errorMessage?: string;
  retryCount: number;
  priority: number;
  createdAt: string;
  updatedAt: string;
  processingStartedAt?: string;
  completedAt?: string;
  processingTimeMs?: number;
  canCancel: boolean;
  canRetry: boolean;
}

export interface VisualizationJobSummary {
  id: string;
  bookId: string;
  pageId?: string;
  status: JobStatus;
  statusDisplayName: string;
  thumbnailUrl?: string;
  createdAt: string;
  completedAt?: string;
}

// ==================== REQUEST DTOs ====================

export interface CreatePageVisualizationRequest {
  bookId: string;
  pageId: string;
  preferredProvider?: AIProvider;
  parameters?: GenerationParameters;
}

export interface CreateChapterVisualizationRequest {
  bookId: string;
  chapterId: string;
  preferredProvider?: AIProvider;
  parameters?: GenerationParameters;
}

export interface CreateSelectionVisualizationRequest {
  bookId: string;
  textSelection: TextSelection;
  preferredProvider?: AIProvider;
  parameters?: GenerationParameters;
}

export interface BatchVisualizationRequest {
  bookId: string;
  pageIds?: string[];
  chapterIds?: string[];
  mode: 'PerPage' | 'PerChapter';
  preferredProvider?: AIProvider;
  parameters?: GenerationParameters;
}

// ==================== RESPONSE TYPES ====================

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  succeeded?: boolean;
  message?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface QueueStatus {
  queueLength: number;
  estimatedWaitTime: string;
  activeJobs: number;
}

// ==================== AXIOS INSTANCE FOR VISUALIZATION API ====================

const getVisualizationBaseUrl = (): string => {
  return API_CONFIG.USE_GATEWAY 
    ? API_CONFIG.GATEWAY_URL 
    : API_CONFIG.VISUALIZATION_API_URL;
};

const vizAxios = axios.create({
  baseURL: getVisualizationBaseUrl(),
  timeout: API_CONFIG.TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth interceptor
vizAxios.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ==================== ENDPOINT HELPERS ====================

const getVizEndpoint = (path: string): string => {
  const endpoints = getEndpoints();
  
  // Если используем Gateway и есть VISUALIZATION endpoints
  if (API_CONFIG.USE_GATEWAY && 'VISUALIZATION' in endpoints) {
    const vizEndpoints = (endpoints as any).VISUALIZATION;
    // Преобразуем path в gateway endpoint
    switch (path) {
      case 'jobs': return vizEndpoints.JOBS || '/visualization/jobs';
      case 'page': return vizEndpoints.GENERATE_PAGE || '/visualization/jobs/page';
      case 'chapter': return vizEndpoints.GENERATE_CHAPTER || '/visualization/jobs/chapter';
      case 'selection': return vizEndpoints.GENERATE_TEXT || '/visualization/jobs/text-selection';
      case 'queue': return vizEndpoints.QUEUE || '/visualization/queue';
      default: return `/visualization/${path}`;
    }
  }
  
  // Direct API
  return `/api/v1/visualization/${path}`;
};

// ==================== VISUALIZATION SERVICE ====================

export const visualizationService = {
  // ==================== JOBS ====================

  async createPageVisualization(request: CreatePageVisualizationRequest): Promise<VisualizationJob> {
    const url = getVizEndpoint('page');
    const response = await vizAxios.post<ApiResponse<VisualizationJob>>(url, request);
    return response.data.data || response.data as any;
  },

  async createChapterVisualization(request: CreateChapterVisualizationRequest): Promise<VisualizationJob> {
    const url = getVizEndpoint('chapter');
    const response = await vizAxios.post<ApiResponse<VisualizationJob>>(url, request);
    return response.data.data || response.data as any;
  },

  async createSelectionVisualization(request: CreateSelectionVisualizationRequest): Promise<VisualizationJob> {
    const url = getVizEndpoint('selection');
    
    // Flatten the request to match backend DTO (CreateTextSelectionVisualizationRequest)
    const payload = {
      bookId: request.bookId,
      pageId: request.textSelection.pageId,
      selectedText: request.textSelection.selectedText,
      startPosition: request.textSelection.startPosition || 0,
      endPosition: request.textSelection.endPosition || (request.textSelection.startPosition || 0) + request.textSelection.selectedText.length,
      contextBefore: request.textSelection.contextBefore,
      contextAfter: request.textSelection.contextAfter,
      preferredProvider: request.preferredProvider,
    };
    
    const response = await vizAxios.post<ApiResponse<VisualizationJob>>(url, payload);
    return response.data.data || response.data as any;
  },

  async createBatchVisualization(request: BatchVisualizationRequest): Promise<VisualizationJob[]> {
    const url = API_CONFIG.USE_GATEWAY ? '/visualization/batch' : '/api/v1/visualization/jobs/batch';
    const response = await vizAxios.post<ApiResponse<VisualizationJob[]>>(url, request);
    return response.data.data || response.data as any;
  },

  async getJob(jobId: string): Promise<VisualizationJob> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/jobs' : '/api/v1/visualization/jobs';
    const response = await vizAxios.get<ApiResponse<VisualizationJob>>(`${base}/${jobId}`);
    return response.data.data || response.data as any;
  },

  async getMyJobs(page = 1, pageSize = 20): Promise<PaginatedResponse<VisualizationJobSummary>> {
    const url = API_CONFIG.USE_GATEWAY ? '/visualization/jobs/my' : '/api/v1/visualization/jobs/my';
    const response = await vizAxios.get<PaginatedResponse<VisualizationJobSummary>>(url, {
      params: { page, pageSize }
    });
    return response.data;
  },

  async cancelJob(jobId: string): Promise<void> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/jobs' : '/api/v1/visualization/jobs';
    await vizAxios.post(`${base}/${jobId}/cancel`);
  },

  async retryJob(jobId: string): Promise<VisualizationJob> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/jobs' : '/api/v1/visualization/jobs';
    const response = await vizAxios.post<ApiResponse<VisualizationJob>>(`${base}/${jobId}/retry`);
    return response.data.data || response.data as any;
  },

  // ==================== IMAGES ====================

  async getJobImages(jobId: string): Promise<GeneratedImage[]> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/jobs' : '/api/v1/visualization/jobs';
    const response = await vizAxios.get<ApiResponse<GeneratedImage[]>>(`${base}/${jobId}/images`);
    return response.data.data || response.data as any;
  },

  async selectImage(jobId: string, imageId: string): Promise<void> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/jobs' : '/api/v1/visualization/jobs';
    await vizAxios.post(`${base}/${jobId}/images/${imageId}/select`);
  },

  async deleteImage(jobId: string, imageId: string): Promise<void> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/jobs' : '/api/v1/visualization/jobs';
    await vizAxios.delete(`${base}/${jobId}/images/${imageId}`);
  },

  // ==================== QUEUE ====================

  async getQueueStatus(): Promise<QueueStatus> {
    const url = getVizEndpoint('queue');
    const response = await vizAxios.get<ApiResponse<QueueStatus>>(`${url}/status`);
    return response.data.data || response.data as any;
  },

  async getQueuePosition(jobId: string): Promise<number> {
    const url = API_CONFIG.USE_GATEWAY ? '/visualization/queue' : '/api/v1/visualization/queue';
    const response = await vizAxios.get<ApiResponse<{ position: number }>>(`${url}/position/${jobId}`);
    const data = response.data.data || response.data as any;
    return data.position || 0;
  },

  // ==================== BOOK VISUALIZATIONS ====================

  async getBookVisualizations(bookId: string): Promise<Map<string, GeneratedImage>> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/books' : '/api/v1/visualization/books';
    try {
      const response = await vizAxios.get<ApiResponse<Record<string, GeneratedImage>>>(`${base}/${bookId}/visualizations`);
      const data = response.data.data || response.data as any;
      return new Map(Object.entries(data));
    } catch {
      return new Map();
    }
  },

  async getPageVisualization(bookId: string, pageId: string): Promise<GeneratedImage | null> {
    const base = API_CONFIG.USE_GATEWAY ? '/visualization/books' : '/api/v1/visualization/books';
    try {
      const response = await vizAxios.get<ApiResponse<GeneratedImage>>(`${base}/${bookId}/pages/${pageId}/visualization`);
      return response.data.data || response.data as any;
    } catch {
      return null;
    }
  },

  // ==================== POLLING ====================

  async pollJobStatus(
    jobId: string,
    onStatusUpdate?: (job: VisualizationJob) => void,
    options?: { intervalMs?: number; timeoutMs?: number }
  ): Promise<VisualizationJob> {
    const { intervalMs = 2000, timeoutMs = 120000 } = options || {};
    const startTime = Date.now();

    return new Promise((resolve, reject) => {
      const poll = async () => {
        try {
          const job = await this.getJob(jobId);
          onStatusUpdate?.(job);

          if (job.status === 'Completed' || job.status === 'Failed' || job.status === 'Cancelled') {
            resolve(job);
            return;
          }

          if (Date.now() - startTime > timeoutMs) {
            reject(new Error('Job polling timeout'));
            return;
          }

          setTimeout(poll, intervalMs);
        } catch (error) {
          reject(error);
        }
      };

      poll();
    });
  },
};

// ==================== UTILITY FUNCTIONS ====================

export const getStatusText = (status: JobStatus): string => {
  const statusMap: Record<JobStatus, string> = {
    Pending: 'Waiting to start...',
    Queued: 'In queue...',
    GeneratingPrompt: 'Creating prompt...',
    Processing: 'Generating image...',
    Uploading: 'Saving result...',
    Completed: 'Complete!',
    Failed: 'Failed',
    Cancelled: 'Cancelled',
  };
  return statusMap[status] || status;
};

export const isJobInProgress = (status: JobStatus): boolean => {
  return ['Pending', 'Queued', 'GeneratingPrompt', 'Processing', 'Uploading'].includes(status);
};

export const isJobTerminal = (status: JobStatus): boolean => {
  return ['Completed', 'Failed', 'Cancelled'].includes(status);
};

export const getDefaultParameters = (provider: AIProvider = 'DallE3'): GenerationParameters => {
  switch (provider) {
    case 'DallE3':
      return { size: '1024x1024', quality: 'standard', upscale: false };
    case 'StableDiffusion':
      return { size: '1024x1024', steps: 30, cfgScale: 7.5, sampler: 'DPM++ 2M Karras', upscale: false };
    case 'Midjourney':
      return { aspectRatio: '1:1', quality: 'standard', upscale: false };
    default:
      return { size: '1024x1024', quality: 'standard' };
  }
};

export default visualizationService;