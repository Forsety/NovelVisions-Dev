// src/services/visualization-api.service.ts
import axios, { AxiosInstance, AxiosError } from 'axios';
import { API_CONFIG } from '../config/api.config';
import {
  VisualizationJob,
  VisualizationRequest,
  GeneratedImage,
  TextSelection,
  AIProvider,
  GenerationParameters
} from '../types/visualization.types';

class VisualizationApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_CONFIG.VISUALIZATION_API_URL || 'https://localhost:7297',
      timeout: 60000, // Longer timeout for AI generation
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    });

    // Request interceptor - add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor
    this.api.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        console.error('Visualization API Error:', error.response?.data || error.message);
        return Promise.reject(this.handleError(error));
      }
    );
  }

  // ==================== VISUALIZATION JOBS ====================

  /**
   * Create visualization for a page (button click)
   */
  async createPageVisualization(
    bookId: string,
    pageId: string,
    preferredProvider?: AIProvider,
    parameters?: GenerationParameters
  ): Promise<VisualizationJob> {
    const response = await this.api.post('/api/v1/visualizations/page', {
      bookId,
      pageId,
      preferredProvider: preferredProvider || 'dalle3',
      parameters
    });
    return response.data;
  }

  /**
   * Create visualization for selected text
   */
  async createTextSelectionVisualization(
    bookId: string,
    textSelection: TextSelection,
    preferredProvider?: AIProvider,
    parameters?: GenerationParameters
  ): Promise<VisualizationJob> {
    const response = await this.api.post('/api/v1/visualizations/text-selection', {
      bookId,
      ...textSelection,
      preferredProvider: preferredProvider || 'dalle3',
      parameters
    });
    return response.data;
  }

  /**
   * Start auto-novel generation for entire book
   */
  async startAutoNovelGeneration(
    bookId: string,
    preferredProvider?: AIProvider,
    skipExisting: boolean = true
  ): Promise<VisualizationJob[]> {
    const response = await this.api.post('/api/v1/visualizations/auto-novel', {
      bookId,
      preferredProvider: preferredProvider || 'dalle3',
      skipExistingVisualizations: skipExisting
    });
    return response.data;
  }

  /**
   * Get visualization job status
   */
  async getJobStatus(jobId: string): Promise<VisualizationJob> {
    const response = await this.api.get(`/api/v1/visualizations/jobs/${jobId}`);
    return response.data;
  }

  /**
   * Get all jobs for a user
   */
  async getUserJobs(
    page: number = 1,
    pageSize: number = 20,
    status?: string
  ): Promise<{ items: VisualizationJob[]; totalCount: number }> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString()
    });
    if (status) params.append('status', status);

    const response = await this.api.get(`/api/v1/visualizations/jobs?${params}`);
    return response.data;
  }

  /**
   * Get visualizations for a page
   */
  async getPageVisualizations(
    bookId: string,
    pageId: string
  ): Promise<GeneratedImage[]> {
    const response = await this.api.get(
      `/api/v1/visualizations/books/${bookId}/pages/${pageId}`
    );
    return response.data;
  }

  /**
   * Get visualizations for a chapter
   */
  async getChapterVisualizations(
    bookId: string,
    chapterId: string
  ): Promise<GeneratedImage[]> {
    const response = await this.api.get(
      `/api/v1/visualizations/books/${bookId}/chapters/${chapterId}`
    );
    return response.data;
  }

  // ==================== IMAGE MANAGEMENT ====================

  /**
   * Select an image as the primary one
   */
  async selectImage(jobId: string, imageId: string): Promise<void> {
    await this.api.post(`/api/v1/visualizations/jobs/${jobId}/images/${imageId}/select`);
  }

  /**
   * Delete a generated image
   */
  async deleteImage(jobId: string, imageId: string): Promise<void> {
    await this.api.delete(`/api/v1/visualizations/jobs/${jobId}/images/${imageId}`);
  }

  /**
   * Regenerate visualization (create new variation)
   */
  async regenerateVisualization(jobId: string): Promise<VisualizationJob> {
    const response = await this.api.post(`/api/v1/visualizations/jobs/${jobId}/regenerate`);
    return response.data;
  }

  // ==================== JOB CONTROL ====================

  /**
   * Cancel a pending job
   */
  async cancelJob(jobId: string): Promise<void> {
    await this.api.post(`/api/v1/visualizations/jobs/${jobId}/cancel`);
  }

  /**
   * Retry a failed job
   */
  async retryJob(jobId: string): Promise<VisualizationJob> {
    const response = await this.api.post(`/api/v1/visualizations/jobs/${jobId}/retry`);
    return response.data;
  }

  // ==================== REAL-TIME UPDATES ====================

  /**
   * Subscribe to job updates via Server-Sent Events
   */
  subscribeToJobUpdates(
    jobId: string,
    onUpdate: (job: VisualizationJob) => void,
    onError?: (error: Error) => void
  ): () => void {
    const token = localStorage.getItem('token');
    const eventSource = new EventSource(
      `${API_CONFIG.VISUALIZATION_API_URL}/api/v1/visualizations/jobs/${jobId}/stream?token=${token}`
    );

    eventSource.onmessage = (event) => {
      try {
        const job = JSON.parse(event.data);
        onUpdate(job);
      } catch (e) {
        console.error('Failed to parse job update:', e);
      }
    };

    eventSource.onerror = (error) => {
      console.error('EventSource error:', error);
      if (onError) onError(new Error('Connection lost'));
      eventSource.close();
    };

    // Return cleanup function
    return () => {
      eventSource.close();
    };
  }

  // ==================== HELPERS ====================

  private handleError(error: AxiosError): Error {
    if (error.response) {
      const data = error.response.data as any;
      const message = data?.error || data?.message || `Server error: ${error.response.status}`;
      return new Error(message);
    } else if (error.request) {
      return new Error('Visualization service unavailable. Please try again.');
    }
    return new Error('An unexpected error occurred');
  }
}

// Export singleton instance
export default new VisualizationApiService();