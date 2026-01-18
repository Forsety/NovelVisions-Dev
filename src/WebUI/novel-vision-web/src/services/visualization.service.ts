// src/services/visualization.service.ts
import apiService from './api.service';
import { API_CONFIG, getVisualizationUrl } from '../config/api.config';
import { 
  VisualizationJob, 
  AIProvider, 
  ArtStyle, 
  TextSelection,
  PaginatedResult,
  JobStatus
} from '../types';

interface GeneratePageParams {
  bookId: string;
  pageId: string;
  provider?: AIProvider;
  style?: ArtStyle;
  customPrompt?: string;
}

interface GenerateTextParams {
  bookId: string;
  selection: TextSelection;
  provider?: AIProvider;
  style?: ArtStyle;
}

interface GenerateChapterParams {
  bookId: string;
  chapterId: string;
  provider?: AIProvider;
  style?: ArtStyle;
}

interface GetJobsParams {
  page?: number;
  pageSize?: number;
  status?: JobStatus;
  bookId?: string;
}

class VisualizationService {
  // ==================== GENERATION ====================
  async generateForPage(params: GeneratePageParams): Promise<VisualizationJob> {
    return apiService.post<VisualizationJob>(
      API_CONFIG.ENDPOINTS.VISUALIZATION.GENERATE_PAGE,
      {
        bookId: params.bookId,
        pageId: params.pageId,
        preferredProvider: params.provider || 'dalle3',
        style: params.style || 'realistic',
        customPrompt: params.customPrompt
      }
    );
  }

  async generateForText(params: GenerateTextParams): Promise<VisualizationJob> {
    return apiService.post<VisualizationJob>(
      API_CONFIG.ENDPOINTS.VISUALIZATION.GENERATE_TEXT,
      {
        bookId: params.bookId,
        ...params.selection,
        preferredProvider: params.provider || 'dalle3',
        style: params.style || 'realistic'
      }
    );
  }

  async generateForChapter(params: GenerateChapterParams): Promise<VisualizationJob> {
    return apiService.post<VisualizationJob>(
      API_CONFIG.ENDPOINTS.VISUALIZATION.GENERATE_CHAPTER,
      {
        bookId: params.bookId,
        chapterId: params.chapterId,
        preferredProvider: params.provider || 'dalle3',
        style: params.style || 'realistic'
      }
    );
  }

  // ==================== JOBS ====================
  async getJobs(params: GetJobsParams = {}): Promise<PaginatedResult<VisualizationJob>> {
    const { page = 1, pageSize = 10, ...rest } = params;
    return apiService.get<PaginatedResult<VisualizationJob>>(
      API_CONFIG.ENDPOINTS.VISUALIZATION.JOBS,
      { page, pageSize, ...rest }
    );
  }

  async getJob(jobId: string): Promise<VisualizationJob> {
    return apiService.get<VisualizationJob>(
      API_CONFIG.ENDPOINTS.VISUALIZATION.JOB_BY_ID(jobId)
    );
  }

  async cancelJob(jobId: string): Promise<void> {
    await apiService.post(`${API_CONFIG.ENDPOINTS.VISUALIZATION.JOB_BY_ID(jobId)}/cancel`);
  }

  async retryJob(jobId: string): Promise<VisualizationJob> {
    return apiService.post<VisualizationJob>(
      `${API_CONFIG.ENDPOINTS.VISUALIZATION.JOB_BY_ID(jobId)}/retry`
    );
  }

  // ==================== PROVIDERS & STYLES ====================
  async getProviders(): Promise<any[]> {
    return apiService.get<any[]>(API_CONFIG.ENDPOINTS.VISUALIZATION.PROVIDERS);
  }

  async getStyles(): Promise<any[]> {
    return apiService.get<any[]>(API_CONFIG.ENDPOINTS.VISUALIZATION.STYLES);
  }

  // ==================== REAL-TIME UPDATES ====================
  subscribeToJob(
    jobId: string,
    onUpdate: (job: VisualizationJob) => void,
    onError?: (error: Error) => void
  ): () => void {
    const token = localStorage.getItem('token');
    const baseUrl = getVisualizationUrl();
    
    const eventSource = new EventSource(
      `${baseUrl}/api/v1/visualizations/jobs/${jobId}/stream?token=${token}`
    );

    eventSource.onmessage = (event) => {
      try {
        const job = JSON.parse(event.data);
        onUpdate(job);
      } catch (e) {
        console.error('Failed to parse job update:', e);
      }
    };

    eventSource.onerror = () => {
      if (onError) onError(new Error('Connection lost'));
      eventSource.close();
    };

    return () => eventSource.close();
  }

  // ==================== POLLING FALLBACK ====================
  async pollJobStatus(
    jobId: string,
    onUpdate: (job: VisualizationJob) => void,
    intervalMs = 2000,
    maxAttempts = 60
  ): Promise<VisualizationJob> {
    let attempts = 0;

    return new Promise((resolve, reject) => {
      const poll = async () => {
        try {
          const job = await this.getJob(jobId);
          onUpdate(job);

          if (job.status === 'Completed') {
            resolve(job);
            return;
          }

          if (job.status === 'Failed' || job.status === 'Cancelled') {
            reject(new Error(job.errorMessage || `Job ${job.status.toLowerCase()}`));
            return;
          }

          attempts++;
          if (attempts >= maxAttempts) {
            reject(new Error('Polling timeout'));
            return;
          }

          setTimeout(poll, intervalMs);
        } catch (error) {
          reject(error);
        }
      };

      poll();
    });
  }
}

export const visualizationService = new VisualizationService();
export default visualizationService;