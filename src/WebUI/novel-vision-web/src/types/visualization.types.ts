// src/types/visualization.types.ts
// Types for AI Visualization System

export type VisualizationMode = 
  | 'None' 
  | 'PerPage' 
  | 'PerChapter' 
  | 'UserSelected' 
  | 'AuthorDefined';

export type AIProvider = 
  | 'dalle3' 
  | 'midjourney' 
  | 'stable-diffusion' 
  | 'flux';

export type VisualizationTrigger = 
  | 'Button' 
  | 'TextSelection' 
  | 'AutoNovel' 
  | 'AuthorDefined' 
  | 'PerChapter' 
  | 'PerPage' 
  | 'Regeneration';

export type VisualizationJobStatus = 
  | 'Pending' 
  | 'Queued' 
  | 'GeneratingPrompt' 
  | 'Processing' 
  | 'Uploading' 
  | 'Completed' 
  | 'Failed' 
  | 'Cancelled';

export type ArtStyle = 
  | 'realistic' 
  | 'anime' 
  | 'oil_painting' 
  | 'watercolor' 
  | 'fantasy' 
  | 'sketch' 
  | 'concept_art' 
  | '3d';

export interface VisualizationSettings {
  mode: VisualizationMode;
  preferredProvider: AIProvider;
  artStyle: ArtStyle;
  autoVisualize: boolean;
  showGenerationProgress: boolean;
}

export interface GenerationParameters {
  width?: number;
  height?: number;
  aspectRatio?: string;
  quality?: 'standard' | 'hd';
  style?: ArtStyle;
  negativePrompt?: string;
}

export interface TextSelection {
  selectedText: string;
  startPosition: number;
  endPosition: number;
  pageId: string;
  chapterId?: string;
  contextBefore?: string;
  contextAfter?: string;
}

export interface VisualizationJob {
  id: string;
  bookId: string;
  pageId?: string;
  chapterId?: string;
  userId: string;
  trigger: VisualizationTrigger;
  status: VisualizationJobStatus;
  preferredProvider: AIProvider;
  parameters: GenerationParameters;
  promptData?: PromptData;
  images: GeneratedImage[];
  queuePosition?: number;
  estimatedWaitTime?: string;
  progress?: number;
  errorMessage?: string;
  createdAt: string;
  completedAt?: string;
}

export interface PromptData {
  originalText: string;
  enhancedPrompt: string;
  negativePrompt?: string;
  targetModel: string;
  style?: string;
}

export interface GeneratedImage {
  id: string;
  imageUrl: string;
  thumbnailUrl?: string;
  width: number;
  height: number;
  isSelected: boolean;
  createdAt: string;
}

export interface VisualizationRequest {
  bookId: string;
  pageId?: string;
  chapterId?: string;
  trigger: VisualizationTrigger;
  preferredProvider?: AIProvider;
  textSelection?: TextSelection;
  parameters?: GenerationParameters;
}

export interface VisualizationModeOption {
  mode: VisualizationMode;
  title: string;
  description: string;
  icon: string;
  recommended?: boolean;
  estimatedTime?: string;
}

// Reader visualization mode options
export const READER_VISUALIZATION_OPTIONS: VisualizationModeOption[] = [
  {
    mode: 'PerPage',
    title: 'Every Page',
    description: 'AI creates a unique illustration for each page as you read',
    icon: '🖼️',
    estimatedTime: '~2 sec per page',
    recommended: true
  },
  {
    mode: 'PerChapter',
    title: 'Every Chapter',
    description: 'One key illustration per chapter - balanced experience',
    icon: '📑',
    estimatedTime: 'Faster loading'
  },
  {
    mode: 'UserSelected',
    title: 'On Demand',
    description: 'Select text or click button to visualize specific moments',
    icon: '✋',
    estimatedTime: 'You control'
  },
  {
    mode: 'None',
    title: 'Classic Reading',
    description: 'Traditional text-only reading experience',
    icon: '📖',
    estimatedTime: 'No AI'
  }
];

// Author visualization mode options
export const AUTHOR_VISUALIZATION_OPTIONS: VisualizationModeOption[] = [
  {
    mode: 'None',
    title: 'Disabled',
    description: 'No AI visualization - classic book format',
    icon: '🚫'
  },
  {
    mode: 'PerPage',
    title: 'Per Page',
    description: 'AI automatically generates illustration for every page',
    icon: '🖼️'
  },
  {
    mode: 'PerChapter',
    title: 'Per Chapter',
    description: 'One key illustration at the beginning of each chapter',
    icon: '📑'
  },
  {
    mode: 'UserSelected',
    title: 'Reader\'s Choice',
    description: 'Readers choose what to visualize - most flexible option',
    icon: '✋',
    recommended: true
  },
  {
    mode: 'AuthorDefined',
    title: 'Author Defined',
    description: 'You mark specific visualization points in your book',
    icon: '🎯'
  }
];

// Art style options
export const ART_STYLE_OPTIONS = [
  { value: 'realistic', label: 'Realistic', icon: '📷' },
  { value: 'anime', label: 'Anime', icon: '🎌' },
  { value: 'oil_painting', label: 'Oil Painting', icon: '🖼️' },
  { value: 'watercolor', label: 'Watercolor', icon: '🎨' },
  { value: 'fantasy', label: 'Fantasy Art', icon: '🐉' },
  { value: 'sketch', label: 'Sketch', icon: '✏️' },
  { value: 'concept_art', label: 'Concept Art', icon: '🎭' },
  { value: '3d', label: '3D Render', icon: '💎' }
];

// AI Provider options
export const AI_PROVIDER_OPTIONS = [
  { value: 'dalle3', label: 'DALL-E 3', description: 'OpenAI - Best quality' },
  { value: 'midjourney', label: 'Midjourney', description: 'Artistic style' },
  { value: 'stable-diffusion', label: 'Stable Diffusion', description: 'Fast & flexible' },
  { value: 'flux', label: 'Flux', description: 'Newest model' }
];