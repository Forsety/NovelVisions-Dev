// src/pages/ReaderPage.tsx
import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import CatalogApiService from '../services/catalog-api.service';
import VisualizationApiService from '../services/visualization-api.service';
import { 
  ReaderSettings, 
  PageContent, 
  ChapterInfo, 
  BookReaderData,
  DEFAULT_READER_SETTINGS,
  READER_THEMES
} from '../types/reader.types';
import { 
  VisualizationMode, 
  GeneratedImage, 
  VisualizationJob,
  READER_VISUALIZATION_OPTIONS 
} from '../types/visualization.types';
import VisualizationModeModal from '../components/VisualizationModeModal';
import ReaderSettingsPanel from '../components/ReaderSettingsPanel';
import ChapterListSidebar from '../components/ChapterListSidebar';
import AIVisualizationPanel from '../components/AIVisualizationPanel';
import './ReaderPage.css';

const ReaderPage: React.FC = () => {
  const { bookId } = useParams<{ bookId: string }>();
  const navigate = useNavigate();
  const contentRef = useRef<HTMLDivElement>(null);

  // Core state
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [book, setBook] = useState<BookReaderData | null>(null);
  const [chapters, setChapters] = useState<ChapterInfo[]>([]);
  const [currentChapter, setCurrentChapter] = useState<ChapterInfo | null>(null);
  const [currentPage, setCurrentPage] = useState<PageContent | null>(null);
  const [pages, setPages] = useState<PageContent[]>([]);
  const [currentPageIndex, setCurrentPageIndex] = useState(0);

  // UI state
  const [showModeModal, setShowModeModal] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [showChapterList, setShowChapterList] = useState(false);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [settings, setSettings] = useState<ReaderSettings>(DEFAULT_READER_SETTINGS);

  // Visualization state
  const [visualizationMode, setVisualizationMode] = useState<VisualizationMode>('UserSelected');
  const [currentVisualization, setCurrentVisualization] = useState<GeneratedImage | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [generationProgress, setGenerationProgress] = useState(0);
  const [selectedText, setSelectedText] = useState<string>('');

  // Load book data
  useEffect(() => {
    if (bookId) {
      loadBookData();
    }
  }, [bookId]);

  const loadBookData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Load book details
      const bookData = await CatalogApiService.getBookById(bookId!);
      
      // Load chapters
      const chaptersData = await CatalogApiService.getChapters(bookId!);
      
      const readerData: BookReaderData = {
        book: {
          id: bookData.id,
          title: bookData.title,
          authorName: bookData.authorName || 'Unknown Author',
          coverImageUrl: bookData.coverImageUrl,
          visualizationMode: 'UserSelected', // Default
          totalPages: bookData.pageCount,
          totalChapters: chaptersData.length
        },
        chapters: chaptersData.map(ch => ({
          id: ch.id,
          bookId: ch.bookId,
          chapterNumber: ch.chapterNumber,
          title: ch.title,
          pageCount: ch.pageCount,
          wordCount: 0,
          estimatedReadTime: Math.ceil(ch.pageCount * 2),
          isCompleted: false
        }))
      };

      setBook(readerData);
      setChapters(readerData.chapters);

      // Show mode selection modal on first load
      const savedMode = localStorage.getItem(`reader_mode_${bookId}`);
      if (savedMode) {
        setVisualizationMode(savedMode as VisualizationMode);
        if (readerData.chapters.length > 0) {
          await loadChapter(readerData.chapters[0].id);
        }
      } else {
        setShowModeModal(true);
      }

    } catch (err: any) {
      console.error('Failed to load book:', err);
      setError(err.message || 'Failed to load book');
    } finally {
      setLoading(false);
    }
  };

  const loadChapter = async (chapterId: string) => {
    try {
      const chapter = chapters.find(ch => ch.id === chapterId);
      if (!chapter) return;

      setCurrentChapter(chapter);

      // Load pages for chapter
      const pagesData = await CatalogApiService.getPages(bookId!, chapterId);
      const pageContents: PageContent[] = pagesData.map(p => ({
        id: p.id,
        chapterId: p.chapterId,
        bookId: bookId!,
        pageNumber: p.pageNumber,
        content: p.content,
        wordCount: p.wordCount,
        hasVisualization: false
      }));

      setPages(pageContents);
      setCurrentPageIndex(0);
      if (pageContents.length > 0) {
        setCurrentPage(pageContents[0]);
        
        // Load visualization if exists
        await loadPageVisualization(pageContents[0].id);
      }

    } catch (err) {
      console.error('Failed to load chapter:', err);
    }
  };

  const loadPageVisualization = async (pageId: string) => {
    try {
      const visualizations = await VisualizationApiService.getPageVisualizations(bookId!, pageId);
      if (visualizations.length > 0) {
        const selected = visualizations.find(v => v.isSelected) || visualizations[0];
        setCurrentVisualization(selected);
      } else {
        setCurrentVisualization(null);
      }
    } catch (err) {
      // No visualization available
      setCurrentVisualization(null);
    }
  };

  // Navigation
  const goToPage = async (index: number) => {
    if (index >= 0 && index < pages.length) {
      setCurrentPageIndex(index);
      setCurrentPage(pages[index]);
      await loadPageVisualization(pages[index].id);

      // Auto-generate visualization if mode is PerPage
      if (visualizationMode === 'PerPage' && !pages[index].hasVisualization) {
        handleVisualize();
      }
    }
  };

  const goToNextPage = () => goToPage(currentPageIndex + 1);
  const goToPrevPage = () => goToPage(currentPageIndex - 1);

  const goToNextChapter = () => {
    const currentIndex = chapters.findIndex(ch => ch.id === currentChapter?.id);
    if (currentIndex < chapters.length - 1) {
      loadChapter(chapters[currentIndex + 1].id);
    }
  };

  const goToPrevChapter = () => {
    const currentIndex = chapters.findIndex(ch => ch.id === currentChapter?.id);
    if (currentIndex > 0) {
      loadChapter(chapters[currentIndex - 1].id);
    }
  };

  // Visualization
  const handleVisualize = async () => {
    if (!currentPage || isGenerating) return;

    try {
      setIsGenerating(true);
      setGenerationProgress(0);

      const job = await VisualizationApiService.createPageVisualization(
        bookId!,
        currentPage.id,
        settings.visualization.preferredProvider,
        {
          style: settings.visualization.artStyle
        }
      );

      // Subscribe to updates
      const unsubscribe = VisualizationApiService.subscribeToJobUpdates(
        job.id,
        (updatedJob) => {
          setGenerationProgress(updatedJob.progress || 0);
          
          if (updatedJob.status === 'Completed' && updatedJob.images.length > 0) {
            const selectedImage = updatedJob.images.find(i => i.isSelected) || updatedJob.images[0];
            setCurrentVisualization(selectedImage);
            setIsGenerating(false);
            unsubscribe();
          } else if (updatedJob.status === 'Failed') {
            setIsGenerating(false);
            unsubscribe();
          }
        },
        (error) => {
          console.error('Visualization error:', error);
          setIsGenerating(false);
        }
      );

    } catch (err) {
      console.error('Failed to start visualization:', err);
      setIsGenerating(false);
    }
  };

  const handleTextSelection = () => {
    const selection = window.getSelection();
    if (selection && selection.toString().trim().length > 10) {
      setSelectedText(selection.toString().trim());
    }
  };

  const handleVisualizeSelection = async () => {
    if (!selectedText || !currentPage || isGenerating) return;

    try {
      setIsGenerating(true);
      
      const job = await VisualizationApiService.createTextSelectionVisualization(
        bookId!,
        {
          selectedText,
          startPosition: 0,
          endPosition: selectedText.length,
          pageId: currentPage.id,
          chapterId: currentChapter?.id
        },
        settings.visualization.preferredProvider
      );

      // Handle job similar to page visualization
      // ... (subscribe to updates)

    } catch (err) {
      console.error('Failed to visualize selection:', err);
      setIsGenerating(false);
    }
  };

  // Mode selection
  const handleModeSelect = async (mode: VisualizationMode) => {
    setVisualizationMode(mode);
    localStorage.setItem(`reader_mode_${bookId}`, mode);
    setShowModeModal(false);

    // Load first chapter if not loaded
    if (chapters.length > 0 && !currentChapter) {
      await loadChapter(chapters[0].id);
    }
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'ArrowRight' || e.key === ' ') {
        goToNextPage();
      } else if (e.key === 'ArrowLeft') {
        goToPrevPage();
      } else if (e.key === 'Escape') {
        setShowSettings(false);
        setShowChapterList(false);
      } else if (e.key === 'f' || e.key === 'F') {
        toggleFullscreen();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [currentPageIndex, pages.length]);

  // Fullscreen
  const toggleFullscreen = () => {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
      setIsFullscreen(true);
    } else {
      document.exitFullscreen();
      setIsFullscreen(false);
    }
  };

  // Get theme colors
  const theme = READER_THEMES[settings.theme];

  if (loading) {
    return (
      <div className="reader-loading" style={{ background: theme.background }}>
        <div className="loading-content">
          <div className="loading-book-animation">
            <div className="book-spine"></div>
            <div className="book-page page-1"></div>
            <div className="book-page page-2"></div>
            <div className="book-page page-3"></div>
          </div>
          <h2>Loading your reading experience...</h2>
          <div className="loading-progress">
            <div className="progress-bar"></div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="reader-error">
        <div className="error-content">
          <span className="error-icon">📚</span>
          <h2>Oops! Something went wrong</h2>
          <p>{error}</p>
          <button onClick={() => navigate(-1)} className="btn-back">
            ← Go Back
          </button>
        </div>
      </div>
    );
  }

  return (
    <div 
      className={`reader-page ${isFullscreen ? 'fullscreen' : ''}`}
      style={{ 
        '--bg-color': theme.background,
        '--text-color': theme.text,
        '--secondary-color': theme.secondary,
        '--accent-color': theme.accent,
        '--surface-color': theme.surface,
        '--border-color': theme.border,
        '--font-size': `${settings.fontSize}px`,
        '--font-family': settings.fontFamily,
        '--line-height': settings.lineHeight
      } as React.CSSProperties}
    >
      {/* Mode Selection Modal */}
      <VisualizationModeModal
        isOpen={showModeModal}
        onClose={() => setShowModeModal(false)}
        onSelect={handleModeSelect}
        bookTitle={book?.book.title || ''}
      />

      {/* Header */}
      <header className="reader-header">
        <div className="header-left">
          <button className="btn-icon" onClick={() => navigate(-1)} title="Back">
            <span>←</span>
          </button>
          <div className="book-info">
            <h1 className="book-title">{book?.book.title}</h1>
            <span className="chapter-title">
              {currentChapter ? `Chapter ${currentChapter.chapterNumber}: ${currentChapter.title}` : ''}
            </span>
          </div>
        </div>

        <div className="header-center">
          <div className="page-indicator">
            <span className="current-page">{currentPageIndex + 1}</span>
            <span className="separator">/</span>
            <span className="total-pages">{pages.length}</span>
          </div>
        </div>

        <div className="header-right">
          <button 
            className="btn-icon" 
            onClick={() => setShowChapterList(true)}
            title="Chapters"
          >
            <span>📑</span>
          </button>
          <button 
            className="btn-icon" 
            onClick={() => setShowSettings(true)}
            title="Settings"
          >
            <span>⚙️</span>
          </button>
          <button 
            className="btn-icon" 
            onClick={toggleFullscreen}
            title="Fullscreen"
          >
            <span>{isFullscreen ? '⊙' : '⛶'}</span>
          </button>
          <button 
            className={`btn-icon ${settings.theme === 'dark' ? 'active' : ''}`}
            onClick={() => setSettings(s => ({
              ...s, 
              theme: s.theme === 'dark' ? 'light' : 'dark'
            }))}
            title="Toggle Theme"
          >
            <span>{settings.theme === 'dark' ? '🌙' : '☀️'}</span>
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="reader-main">
        {/* Visualization Panel */}
        {(currentVisualization || isGenerating || visualizationMode !== 'None') && (
          <AIVisualizationPanel
            visualization={currentVisualization}
            isGenerating={isGenerating}
            progress={generationProgress}
            onRegenerate={handleVisualize}
            onClose={() => setCurrentVisualization(null)}
            mode={visualizationMode}
          />
        )}

        {/* Text Content */}
        <div 
          className="reader-content"
          ref={contentRef}
          onMouseUp={handleTextSelection}
        >
          <article 
            className="page-content"
            style={{ maxWidth: settings.pageWidth === 'narrow' ? '600px' : 
                              settings.pageWidth === 'wide' ? '1000px' : '800px' }}
          >
            {currentPage ? (
              <div 
                className="page-text"
                style={{ textAlign: settings.textAlign }}
                dangerouslySetInnerHTML={{ __html: formatContent(currentPage.content) }}
              />
            ) : (
              <div className="no-content">
                <span>📖</span>
                <p>Select a chapter to start reading</p>
              </div>
            )}
          </article>
        </div>

        {/* Navigation */}
        <nav className="reader-navigation">
          <button 
            className="nav-btn nav-prev"
            onClick={goToPrevPage}
            disabled={currentPageIndex === 0}
          >
            <span className="nav-icon">‹</span>
            <span className="nav-text">Previous</span>
          </button>

          <div className="nav-center">
            {/* Progress Bar */}
            <div className="reading-progress">
              <div 
                className="progress-fill"
                style={{ width: `${((currentPageIndex + 1) / pages.length) * 100}%` }}
              />
            </div>

            {/* Visualization Button */}
            {visualizationMode !== 'None' && (
              <button 
                className={`btn-visualize ${isGenerating ? 'generating' : ''}`}
                onClick={handleVisualize}
                disabled={isGenerating}
              >
                {isGenerating ? (
                  <>
                    <span className="spinner"></span>
                    <span>Generating... {generationProgress}%</span>
                  </>
                ) : (
                  <>
                    <span className="icon">🎨</span>
                    <span>Visualize Page</span>
                  </>
                )}
              </button>
            )}

            {/* Selection Visualization */}
            {selectedText && visualizationMode === 'UserSelected' && (
              <button 
                className="btn-visualize-selection"
                onClick={handleVisualizeSelection}
                disabled={isGenerating}
              >
                <span className="icon">✨</span>
                <span>Visualize Selection</span>
              </button>
            )}
          </div>

          <button 
            className="nav-btn nav-next"
            onClick={goToNextPage}
            disabled={currentPageIndex === pages.length - 1}
          >
            <span className="nav-text">Next</span>
            <span className="nav-icon">›</span>
          </button>
        </nav>
      </main>

      {/* Chapter List Sidebar */}
      <ChapterListSidebar
        isOpen={showChapterList}
        onClose={() => setShowChapterList(false)}
        chapters={chapters}
        currentChapterId={currentChapter?.id}
        onSelectChapter={(id) => {
          loadChapter(id);
          setShowChapterList(false);
        }}
      />

      {/* Settings Panel */}
      <ReaderSettingsPanel
        isOpen={showSettings}
        onClose={() => setShowSettings(false)}
        settings={settings}
        onSettingsChange={setSettings}
        onModeChange={() => setShowModeModal(true)}
      />
    </div>
  );
};

// Helper function to format content
const formatContent = (content: string): string => {
  // Add paragraph tags and format
  const paragraphs = content.split('\n\n').filter(p => p.trim());
  return paragraphs.map(p => `<p>${p.trim()}</p>`).join('');
};

export default ReaderPage;