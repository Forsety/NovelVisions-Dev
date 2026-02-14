// src/shared/constants/theme.ts
// NovelVision Theme Constants

export type ThemeMode = 'dark' | 'light' | 'sepia';

export const THEMES: Record<ThemeMode, { name: string; icon: string }> = {
  dark: { name: 'Dark', icon: '🌙' },
  light: { name: 'Light', icon: '☀️' },
  sepia: { name: 'Sepia', icon: '📜' },
};

export const READER_FONTS = [
  { value: 'Georgia, serif', label: 'Georgia' },
  { value: '"Merriweather", serif', label: 'Merriweather' },
  { value: '"Lora", serif', label: 'Lora' },
  { value: '"Crimson Text", serif', label: 'Crimson Text' },
  { value: '"Source Sans Pro", sans-serif', label: 'Source Sans Pro' },
  { value: '"Open Sans", sans-serif', label: 'Open Sans' },
  { value: '"Roboto", sans-serif', label: 'Roboto' },
  { value: 'system-ui, sans-serif', label: 'System' },
] as const;

export const READER_FONT_SIZES = {
  min: 12,
  max: 32,
  default: 18,
  step: 2,
} as const;

export const READER_LINE_HEIGHTS = {
  min: 1.2,
  max: 2.4,
  default: 1.8,
  step: 0.1,
} as const;

export const PAGE_WIDTHS = {
  narrow: '600px',
  medium: '800px',
  wide: '1000px',
} as const;

export const VISUALIZATION_STYLES = [
  { value: 'realistic', label: 'Realistic', icon: '📷' },
  { value: 'anime', label: 'Anime', icon: '🎌' },
  { value: 'oil_painting', label: 'Oil Painting', icon: '🖼️' },
  { value: 'watercolor', label: 'Watercolor', icon: '🎨' },
  { value: 'fantasy', label: 'Fantasy Art', icon: '🐉' },
  { value: 'sketch', label: 'Sketch', icon: '✏️' },
  { value: 'concept_art', label: 'Concept Art', icon: '🎭' },
  { value: '3d', label: '3D Render', icon: '💎' },
] as const;

export const AI_PROVIDERS = [
  { value: 'dalle3', label: 'DALL-E 3', description: 'OpenAI - Best quality' },
  { value: 'midjourney', label: 'Midjourney', description: 'Artistic style' },
  { value: 'stable-diffusion', label: 'Stable Diffusion', description: 'Fast & flexible' },
  { value: 'flux', label: 'Flux', description: 'Newest model' },
] as const;

export const VISUALIZATION_MODES = [
  { value: 'None', label: 'Disabled', icon: '🚫', description: 'No AI visualization' },
  { value: 'PerPage', label: 'Per Page', icon: '🖼️', description: 'Illustration for each page' },
  { value: 'PerChapter', label: 'Per Chapter', icon: '📑', description: 'One per chapter' },
  { value: 'UserSelected', label: 'On Demand', icon: '✋', description: 'You choose what to visualize' },
  { value: 'AuthorDefined', label: 'Author Defined', icon: '🎯', description: 'Marked by author' },
] as const;

export const GENRES = [
  'Fantasy',
  'Science Fiction',
  'Romance',
  'Mystery',
  'Thriller',
  'Horror',
  'Historical Fiction',
  'Literary Fiction',
  'Adventure',
  'Drama',
  'Comedy',
  'Poetry',
  'Biography',
  'Self-Help',
  'Philosophy',
  'Classic',
] as const;