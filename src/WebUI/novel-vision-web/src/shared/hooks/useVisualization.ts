// src/shared/hooks/useVisualization.ts
// Custom hook for managing visualizations in the Reader
// Integrates with Visualization.API backend

import { useState, useCallback, useRef, useEffect } from 'react';
import {
  visualizationService,
  type VisualizationJob,
  type GeneratedImage,
  type JobStatus,
  type AIProvider,
  type GenerationParameters,
  type TextSelection,
  isJobInProgress,
  isJobTerminal,
  getDefaultParameters,
} from '../../services/api/visualization.service';

// ==================== TYPES ====================

export interface Visualization {
  id: string;
  jobId: string;
  imageUrl: string;
  thumbnailUrl?: string;
  prompt?: string;
  selectedText?: string;
  position: 'left' | 'right' | 'center' | 'alternate';
  status: 'pending' | 'generating' | 'completed' | 'failed';
  error?: string;
}

export interface UseVisualizationOptions {
  bookId: string;
  preferredProvider?: AIProvider;
  defaultParameters?: GenerationParameters;
  defaultPosition?: 'left' | 'right' | 'center' | 'alternate';
  autoSavePreferences?: boolean;
  pollInterval?: number;
}

export interface UseVisualizationReturn {
  // State
  visualizations: Map<string, Visualization[]>;
  activeJobs: Map<string, VisualizationJob>;
  isGenerating: boolean;
  error: string | null;
  
  // Actions
  generateFromSelection: (
    text: string,
    pageId: string,
    options?: {
      position?: 'left' | 'right' | 'center';
      contextBefore?: string;
      contextAfter?: string;
    }
  ) => Promise<Visualization | null>;
  
  generateForPage: (pageId: string) => Promise<Visualization | null>;
  generateForChapter: (chapterId: string) => Promise<Visualization | null>;
  
  removeVisualization: (pageId: string, vizId: string) => void;
  regenerateVisualization: (pageId: string, vizId: string) => Promise<void>;
  
  cancelJob: (jobId: string) => Promise<void>;
  retryJob: (jobId: string) => Promise<void>;
  
  // Utilities
  getVisualizationsForPage: (pageId: string) => Visualization[];
  loadExistingVisualizations: () => Promise<void>;
  clearError: () => void;
}

// ==================== HOOK ====================

export function useVisualization(options: UseVisualizationOptions): UseVisualizationReturn {
  const {
    bookId,
    preferredProvider = 'DallE3',
    defaultParameters,
    defaultPosition = 'right',
    pollInterval = 2000,
  } = options;

  // State
  const [visualizations, setVisualizations] = useState<Map<string, Visualization[]>>(new Map());
  const [activeJobs, setActiveJobs] = useState<Map<string, VisualizationJob>>(new Map());
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Refs for cleanup
  const pollingIntervals = useRef<Map<string, NodeJS.Timeout>>(new Map());
  const vizCounter = useRef(0);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      pollingIntervals.current.forEach((interval) => clearInterval(interval));
      pollingIntervals.current.clear();
    };
  }, []);

  // ==================== POLLING ====================

  const startPolling = useCallback((jobId: string, pageId: string) => {
    // Clear existing polling for this job
    if (pollingIntervals.current.has(jobId)) {
      clearInterval(pollingIntervals.current.get(jobId)!);
    }

    const poll = async () => {
      try {
        const job = await visualizationService.getJob(jobId);
        
        // Update active jobs
        setActiveJobs(prev => {
          const newMap = new Map(prev);
          if (isJobTerminal(job.status)) {
            newMap.delete(jobId);
          } else {
            newMap.set(jobId, job);
          }
          return newMap;
        });

        // Update visualization status
        setVisualizations(prev => {
          const newMap = new Map(prev);
          const pageVizs = [...(newMap.get(pageId) || [])];
          const vizIndex = pageVizs.findIndex(v => v.jobId === jobId);
          
          if (vizIndex !== -1) {
            if (job.status === 'Completed' && job.selectedImage) {
              pageVizs[vizIndex] = {
                ...pageVizs[vizIndex],
                imageUrl: job.selectedImage.imageUrl,
                thumbnailUrl: job.selectedImage.thumbnailUrl,
                status: 'completed',
              };
            } else if (job.status === 'Failed') {
              pageVizs[vizIndex] = {
                ...pageVizs[vizIndex],
                status: 'failed',
                error: job.errorMessage || 'Generation failed',
              };
            } else if (isJobInProgress(job.status)) {
              pageVizs[vizIndex] = {
                ...pageVizs[vizIndex],
                status: 'generating',
              };
            }
            newMap.set(pageId, pageVizs);
          }
          
          return newMap;
        });

        // Stop polling if job is complete
        if (isJobTerminal(job.status)) {
          clearInterval(pollingIntervals.current.get(jobId)!);
          pollingIntervals.current.delete(jobId);
          
          if (!pollingIntervals.current.size) {
            setIsGenerating(false);
          }
        }
      } catch (err) {
        console.error('Polling error:', err);
      }
    };

    // Start polling
    const interval = setInterval(poll, pollInterval);
    pollingIntervals.current.set(jobId, interval);
    
    // Initial poll
    poll();
  }, [pollInterval]);

  // ==================== GENERATE FROM SELECTION ====================

  const generateFromSelection = useCallback(async (
    text: string,
    pageId: string,
    opts?: {
      position?: 'left' | 'right' | 'center';
      contextBefore?: string;
      contextAfter?: string;
    }
  ): Promise<Visualization | null> => {
    if (!text.trim() || text.length < 10) {
      setError('Selected text is too short');
      return null;
    }

    setIsGenerating(true);
    setError(null);

    try {
      // Create text selection object
      const textSelection: TextSelection = {
        selectedText: text,
        pageId,
        contextBefore: opts?.contextBefore,
        contextAfter: opts?.contextAfter,
      };

      // Create job
      const job = await visualizationService.createSelectionVisualization({
        bookId,
        textSelection,
        preferredProvider,
        parameters: defaultParameters || getDefaultParameters(preferredProvider),
      });

      // Calculate position
      const position = opts?.position || 
        (defaultPosition === 'alternate' 
          ? (vizCounter.current % 2 === 0 ? 'right' : 'left') 
          : defaultPosition);
      
      vizCounter.current++;

      // Create placeholder visualization
      const viz: Visualization = {
        id: `viz-${job.id}`,
        jobId: job.id,
        imageUrl: '',
        selectedText: text,
        position,
        status: 'generating',
      };

      // Add to state
      setVisualizations(prev => {
        const newMap = new Map(prev);
        const pageVizs = [...(newMap.get(pageId) || []), viz];
        newMap.set(pageId, pageVizs);
        return newMap;
      });

      setActiveJobs(prev => new Map(prev).set(job.id, job));

      // Start polling for updates
      startPolling(job.id, pageId);

      return viz;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to create visualization';
      setError(message);
      setIsGenerating(false);
      return null;
    }
  }, [bookId, preferredProvider, defaultParameters, defaultPosition, startPolling]);

  // ==================== GENERATE FOR PAGE ====================

  const generateForPage = useCallback(async (pageId: string): Promise<Visualization | null> => {
    setIsGenerating(true);
    setError(null);

    try {
      const job = await visualizationService.createPageVisualization({
        bookId,
        pageId,
        preferredProvider,
        parameters: defaultParameters || getDefaultParameters(preferredProvider),
      });

      const viz: Visualization = {
        id: `viz-${job.id}`,
        jobId: job.id,
        imageUrl: '',
        position: defaultPosition === 'alternate' 
          ? (vizCounter.current++ % 2 === 0 ? 'right' : 'left') 
          : defaultPosition,
        status: 'generating',
      };

      setVisualizations(prev => {
        const newMap = new Map(prev);
        const pageVizs = [...(newMap.get(pageId) || []), viz];
        newMap.set(pageId, pageVizs);
        return newMap;
      });

      setActiveJobs(prev => new Map(prev).set(job.id, job));
      startPolling(job.id, pageId);

      return viz;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to create visualization';
      setError(message);
      setIsGenerating(false);
      return null;
    }
  }, [bookId, preferredProvider, defaultParameters, defaultPosition, startPolling]);

  // ==================== GENERATE FOR CHAPTER ====================

  const generateForChapter = useCallback(async (chapterId: string): Promise<Visualization | null> => {
    setIsGenerating(true);
    setError(null);

    try {
      const job = await visualizationService.createChapterVisualization({
        bookId,
        chapterId,
        preferredProvider,
        parameters: defaultParameters || getDefaultParameters(preferredProvider),
      });

      const viz: Visualization = {
        id: `viz-${job.id}`,
        jobId: job.id,
        imageUrl: '',
        position: defaultPosition === 'alternate' ? 'right' : defaultPosition,
        status: 'generating',
      };

      // Store under chapter ID
      setVisualizations(prev => {
        const newMap = new Map(prev);
        const chapterVizs = [...(newMap.get(chapterId) || []), viz];
        newMap.set(chapterId, chapterVizs);
        return newMap;
      });

      setActiveJobs(prev => new Map(prev).set(job.id, job));
      startPolling(job.id, chapterId);

      return viz;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to create visualization';
      setError(message);
      setIsGenerating(false);
      return null;
    }
  }, [bookId, preferredProvider, defaultParameters, defaultPosition, startPolling]);

  // ==================== REMOVE VISUALIZATION ====================

  const removeVisualization = useCallback((pageId: string, vizId: string) => {
    setVisualizations(prev => {
      const newMap = new Map(prev);
      const pageVizs = (newMap.get(pageId) || []).filter(v => v.id !== vizId);
      
      if (pageVizs.length === 0) {
        newMap.delete(pageId);
      } else {
        newMap.set(pageId, pageVizs);
      }
      
      return newMap;
    });

    // Also cancel the job if still in progress
    const viz = visualizations.get(pageId)?.find(v => v.id === vizId);
    if (viz?.jobId && activeJobs.has(viz.jobId)) {
      visualizationService.cancelJob(viz.jobId).catch(console.error);
      
      if (pollingIntervals.current.has(viz.jobId)) {
        clearInterval(pollingIntervals.current.get(viz.jobId)!);
        pollingIntervals.current.delete(viz.jobId);
      }
      
      setActiveJobs(prev => {
        const newMap = new Map(prev);
        newMap.delete(viz.jobId);
        return newMap;
      });
    }
  }, [visualizations, activeJobs]);

  // ==================== REGENERATE VISUALIZATION ====================

  const regenerateVisualization = useCallback(async (pageId: string, vizId: string) => {
    const pageVizs = visualizations.get(pageId);
    const viz = pageVizs?.find(v => v.id === vizId);
    
    if (!viz) return;

    // Mark as generating
    setVisualizations(prev => {
      const newMap = new Map(prev);
      const vizs = [...(newMap.get(pageId) || [])];
      const index = vizs.findIndex(v => v.id === vizId);
      
      if (index !== -1) {
        vizs[index] = { ...vizs[index], status: 'generating', error: undefined };
        newMap.set(pageId, vizs);
      }
      
      return newMap;
    });

    try {
      // If we have original text, create new selection visualization
      if (viz.selectedText) {
        await generateFromSelection(viz.selectedText, pageId, { position: viz.position as any });
        
        // Remove old visualization
        setVisualizations(prev => {
          const newMap = new Map(prev);
          const vizs = (newMap.get(pageId) || []).filter(v => v.id !== vizId);
          newMap.set(pageId, vizs);
          return newMap;
        });
      } else if (viz.jobId) {
        // Retry the existing job
        const job = await visualizationService.retryJob(viz.jobId);
        setActiveJobs(prev => new Map(prev).set(job.id, job));
        startPolling(job.id, pageId);
      }
    } catch (err) {
      setVisualizations(prev => {
        const newMap = new Map(prev);
        const vizs = [...(newMap.get(pageId) || [])];
        const index = vizs.findIndex(v => v.id === vizId);
        
        if (index !== -1) {
          vizs[index] = { 
            ...vizs[index], 
            status: 'failed', 
            error: err instanceof Error ? err.message : 'Regeneration failed' 
          };
          newMap.set(pageId, vizs);
        }
        
        return newMap;
      });
    }
  }, [visualizations, generateFromSelection, startPolling]);

  // ==================== CANCEL/RETRY JOBS ====================

  const cancelJob = useCallback(async (jobId: string) => {
    try {
      await visualizationService.cancelJob(jobId);
      
      if (pollingIntervals.current.has(jobId)) {
        clearInterval(pollingIntervals.current.get(jobId)!);
        pollingIntervals.current.delete(jobId);
      }
      
      setActiveJobs(prev => {
        const newMap = new Map(prev);
        newMap.delete(jobId);
        return newMap;
      });
    } catch (err) {
      console.error('Failed to cancel job:', err);
    }
  }, []);

  const retryJob = useCallback(async (jobId: string) => {
    try {
      const job = await visualizationService.retryJob(jobId);
      setActiveJobs(prev => new Map(prev).set(job.id, job));
      
      // Find the page ID for this job and start polling
      for (const [pageId, vizs] of visualizations.entries()) {
        const viz = vizs.find(v => v.jobId === jobId);
        if (viz) {
          startPolling(job.id, pageId);
          break;
        }
      }
    } catch (err) {
      console.error('Failed to retry job:', err);
      setError(err instanceof Error ? err.message : 'Failed to retry job');
    }
  }, [visualizations, startPolling]);

  // ==================== UTILITIES ====================

  const getVisualizationsForPage = useCallback((pageId: string): Visualization[] => {
    return visualizations.get(pageId) || [];
  }, [visualizations]);

  const loadExistingVisualizations = useCallback(async () => {
    try {
      const existingVizs = await visualizationService.getBookVisualizations(bookId);
      
      const newMap = new Map<string, Visualization[]>();
      
      existingVizs.forEach((image, pageId) => {
        const viz: Visualization = {
          id: `existing-${image.id}`,
          jobId: image.jobId,
          imageUrl: image.imageUrl,
          thumbnailUrl: image.thumbnailUrl,
          position: defaultPosition === 'alternate' ? 'right' : defaultPosition,
          status: 'completed',
        };
        
        const pageVizs = newMap.get(pageId) || [];
        pageVizs.push(viz);
        newMap.set(pageId, pageVizs);
      });
      
      setVisualizations(newMap);
    } catch (err) {
      console.error('Failed to load existing visualizations:', err);
    }
  }, [bookId, defaultPosition]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    visualizations,
    activeJobs,
    isGenerating,
    error,
    generateFromSelection,
    generateForPage,
    generateForChapter,
    removeVisualization,
    regenerateVisualization,
    cancelJob,
    retryJob,
    getVisualizationsForPage,
    loadExistingVisualizations,
    clearError,
  };
}

export default useVisualization;