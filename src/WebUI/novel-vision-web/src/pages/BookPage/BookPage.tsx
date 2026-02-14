// src/pages/BookPage/BookPage.tsx
// Premium Library Book Detail Page with Visualization Mode Selection
// Real API integration — Gutenberg books allow readers to choose their visualization preference

import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { catalogService } from '../../services/api/catalog.service';
import { ROUTES } from '../../shared/constants/routes';
import type { Book as ApiBook, Chapter as ApiChapter, Author as ApiAuthor } from '../../types/api.types';
import styles from './BookPage.module.css';

// ==================== TYPES ====================

type VisualizationMode = 'None' | 'PerPage' | 'PerChapter' | 'UserSelected';

interface LocalAuthor {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  birthYear?: number;
  deathYear?: number;
  biography?: string;
  bookCount?: number;
}

interface LocalChapter {
  id: string;
  bookId: string;
  title: string;
  number: number;
  pageCount: number;
  wordCount?: number;
  summary?: string;
}

interface LocalBook {
  id: string;
  title: string;
  description?: string;
  author: LocalAuthor;
  coverImageUrl?: string;
  language: string;
  visualizationMode: VisualizationMode;
  gutenbergId?: number;
  chapters: LocalChapter[];
  chapterCount: number;
  pageCount: number;
  wordCount?: number;
  subjects?: string[];
  genres?: string[];
  rating?: number;
  reviewCount?: number;
  downloadCount?: number;
  viewCount?: number;
  isPublished: boolean;
  source?: string;
  isbn?: string;
  publisher?: string;
  publishedAt?: string;
  createdAt: string;
}

interface UserVisualizationPreference {
  mode: VisualizationMode;
  autoGenerate: boolean;
  imagePosition: 'left' | 'right' | 'center' | 'alternate';
  imageSize: 'small' | 'medium' | 'large';
}

type ContentTab = 'chapters' | 'about' | 'details';

// ==================== ANIMATION VARIANTS ====================

const fadeInUp = {
  hidden: { opacity: 0, y: 30 },
  visible: {
    opacity: 1,
    y: 0,
    transition: { duration: 0.6, ease: [0.22, 1, 0.36, 1] as [number, number, number, number] },
  },
};

const staggerContainer = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.08, delayChildren: 0.15 },
  },
};

const scaleIn = {
  hidden: { opacity: 0, scale: 0.95 },
  visible: {
    opacity: 1,
    scale: 1,
    transition: { duration: 0.5, ease: 'easeOut' as const },
  },
};

// ==================== HELPERS ====================

const mapApiBookToLocal = (apiBook: ApiBook, chapters: ApiChapter[], authorDetail?: ApiAuthor | null): LocalBook => {
  const authorObj = authorDetail || apiBook.author;
  const authorName = authorObj?.displayName || apiBook.authorName || 'Unknown Author';
  const nameParts = authorName.split(' ');

  return {
    id: apiBook.id,
    title: apiBook.title,
    description: apiBook.description,
    author: {
      id: authorObj?.id || apiBook.authorId || '',
      firstName: nameParts[0] || '',
      lastName: nameParts.slice(1).join(' ') || '',
      fullName: authorName,
      avatarUrl: authorObj?.avatarUrl,
      birthYear: authorObj?.birthYear,
      deathYear: authorObj?.deathYear,
      biography: authorObj?.biography,
      bookCount: authorObj?.bookCount,
    },
    coverImageUrl: apiBook.coverImageUrl,
    language: apiBook.language || 'en',
    visualizationMode: (apiBook.visualizationMode as VisualizationMode) || 'UserSelected',
    gutenbergId: apiBook.externalId ? parseInt(apiBook.externalId, 10) : undefined,
    chapters: chapters.map((ch) => ({
      id: ch.id,
      bookId: apiBook.id,
      title: ch.title,
      number: ch.chapterNumber,
      pageCount: ch.pageCount || 0,
      wordCount: ch.wordCount,
      summary: ch.summary,
    })),
    chapterCount: apiBook.chapterCount || chapters.length,
    pageCount: apiBook.pageCount || 0,
    wordCount: apiBook.wordCount || 0,
    subjects: [...(apiBook.genres || []), ...(apiBook.tags || [])],
    genres: apiBook.genres || [],
    rating: apiBook.rating || apiBook.averageRating || 0,
    reviewCount: apiBook.reviewCount || 0,
    downloadCount: apiBook.downloadCount || 0,
    viewCount: apiBook.viewCount || 0,
    isPublished: apiBook.isPublished,
    source: typeof apiBook.source === 'string' ? apiBook.source : undefined,
    isbn: apiBook.isbn,
    publisher: apiBook.publisher,
    publishedAt: apiBook.publicationDate,
    createdAt: apiBook.createdAt,
  };
};

const formatReadingTime = (wordCount?: number): string => {
  if (!wordCount) return '—';
  const minutes = Math.ceil(wordCount / 200);
  const hours = Math.floor(minutes / 60);
  const rem = minutes % 60;
  return hours > 0 ? `${hours}h ${rem}m` : `${minutes} min`;
};

const getLanguageName = (code: string): string => {
  const langs: Record<string, string> = {
    en: 'English', fr: 'French', de: 'German', es: 'Spanish', it: 'Italian',
    pt: 'Portuguese', ru: 'Russian', pl: 'Polish', nl: 'Dutch', sv: 'Swedish',
    da: 'Danish', fi: 'Finnish', ja: 'Japanese', zh: 'Chinese', ko: 'Korean',
  };
  return langs[code] || code.toUpperCase();
};

const formatDate = (dateStr?: string): string => {
  if (!dateStr) return '—';
  try {
    return new Date(dateStr).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
  } catch {
    return dateStr;
  }
};

const formatNumber = (n?: number): string => {
  if (!n) return '0';
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1)}M`;
  if (n >= 1_000) return `${(n / 1_000).toFixed(1)}K`;
  return n.toLocaleString();
};

// ==================== SKELETON LOADERS ====================

const HeroSkeleton: React.FC = () => (
  <div className={styles.heroSkeleton}>
    <div className={styles.skelCover}><div className={styles.shimmer} /></div>
    <div className={styles.skelDetails}>
      <div className={styles.skelBadges}><div className={styles.shimmer} /></div>
      <div className={styles.skelTitle}><div className={styles.shimmer} /></div>
      <div className={styles.skelAuthor}><div className={styles.shimmer} /></div>
      <div className={styles.skelStats}><div className={styles.shimmer} /></div>
      <div className={styles.skelDesc}><div className={styles.shimmer} /></div>
      <div className={styles.skelDesc} style={{ width: '65%' }}><div className={styles.shimmer} /></div>
      <div className={styles.skelActions}><div className={styles.shimmer} /></div>
    </div>
  </div>
);

const ChapterSkeleton: React.FC = () => (
  <div className={styles.skelChapter}>
    <div className={styles.skelChNum}><div className={styles.shimmer} /></div>
    <div className={styles.skelChInfo}>
      <div className={styles.skelChTitle}><div className={styles.shimmer} /></div>
      <div className={styles.skelChMeta}><div className={styles.shimmer} /></div>
    </div>
  </div>
);

const SidebarSkeleton: React.FC = () => (
  <div className={styles.skelSidebar}>
    {[1, 2, 3].map((i) => (
      <div key={i} className={styles.skelSideCard}><div className={styles.shimmer} /></div>
    ))}
  </div>
);

// ==================== BREADCRUMBS ====================

const Breadcrumbs: React.FC<{ book: LocalBook }> = ({ book }) => (
  <nav className={styles.breadcrumbs} aria-label="Breadcrumb">
    <Link to={ROUTES.HOME} className={styles.crumb}>Home</Link>
    <span className={styles.crumbSep}>›</span>
    <Link to={ROUTES.CATALOG} className={styles.crumb}>Library</Link>
    {book.genres && book.genres[0] && (
      <>
        <span className={styles.crumbSep}>›</span>
        <Link to={`${ROUTES.CATALOG}?genre=${encodeURIComponent(book.genres[0])}`} className={styles.crumb}>
          {book.genres[0]}
        </Link>
      </>
    )}
    <span className={styles.crumbSep}>›</span>
    <span className={styles.crumbCurrent}>{book.title}</span>
  </nav>
);

// ==================== VISUALIZATION MODE SELECTOR ====================

interface VisualizationModeSelectorProps {
  currentMode: VisualizationMode;
  preference: UserVisualizationPreference;
  onPreferenceChange: (pref: UserVisualizationPreference) => void;
  isGutenbergBook: boolean;
}

const VisualizationModeSelector: React.FC<VisualizationModeSelectorProps> = ({
  currentMode,
  preference,
  onPreferenceChange,
  isGutenbergBook,
}) => {
  const modes: { value: VisualizationMode; label: string; icon: string; description: string }[] = [
    { value: 'UserSelected', label: "Reader's Choice", icon: '✨', description: 'Select text passages to visualize as you read' },
    { value: 'PerPage', label: 'Per Page', icon: '📄', description: 'Automatically generate visualization for each page' },
    { value: 'PerChapter', label: 'Per Chapter', icon: '📑', description: 'Generate one visualization per chapter' },
    { value: 'None', label: 'Text Only', icon: '📖', description: 'Read without any visualizations' },
  ];

  const positions: { value: 'left' | 'right' | 'center' | 'alternate'; label: string }[] = [
    { value: 'left', label: 'Left' },
    { value: 'right', label: 'Right' },
    { value: 'center', label: 'Center' },
    { value: 'alternate', label: 'Alternate' },
  ];

  const sizes: { value: 'small' | 'medium' | 'large'; label: string }[] = [
    { value: 'small', label: 'Small' },
    { value: 'medium', label: 'Medium' },
    { value: 'large', label: 'Large' },
  ];

  return (
    <motion.div className={styles.vizSelector} initial="hidden" animate="visible" variants={fadeInUp}>
      <div className={styles.vizSelectorHeader}>
        <span className={styles.vizSelectorIcon}>🎨</span>
        <div>
          <h3>Visualization Settings</h3>
          <p>Choose how you want to experience this book</p>
        </div>
      </div>

      <div className={styles.modeGrid}>
        {modes.map((mode) => (
          <button
            key={mode.value}
            className={`${styles.modeOption} ${preference.mode === mode.value ? styles.modeActive : ''}`}
            onClick={() => onPreferenceChange({ ...preference, mode: mode.value })}
          >
            <span className={styles.modeIcon}>{mode.icon}</span>
            <span className={styles.modeLabel}>{mode.label}</span>
            <span className={styles.modeDesc}>{mode.description}</span>
          </button>
        ))}
      </div>

      <AnimatePresence>
        {preference.mode !== 'None' && (
          <motion.div
            className={styles.advancedSettings}
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            transition={{ duration: 0.3 }}
          >
            <div className={styles.settingRow}>
              <label>Image Position</label>
              <div className={styles.optionButtons}>
                {positions.map((pos) => (
                  <button
                    key={pos.value}
                    className={`${styles.optionBtn} ${preference.imagePosition === pos.value ? styles.optionActive : ''}`}
                    onClick={() => onPreferenceChange({ ...preference, imagePosition: pos.value })}
                  >
                    {pos.label}
                  </button>
                ))}
              </div>
            </div>

            <div className={styles.settingRow}>
              <label>Image Size</label>
              <div className={styles.optionButtons}>
                {sizes.map((size) => (
                  <button
                    key={size.value}
                    className={`${styles.optionBtn} ${preference.imageSize === size.value ? styles.optionActive : ''}`}
                    onClick={() => onPreferenceChange({ ...preference, imageSize: size.value })}
                  >
                    {size.label}
                  </button>
                ))}
              </div>
            </div>

            {(preference.mode === 'PerPage' || preference.mode === 'PerChapter') && (
              <div className={styles.settingRow}>
                <label>Auto-generate images</label>
                <button
                  className={`${styles.toggleBtn} ${preference.autoGenerate ? styles.toggleActive : ''}`}
                  onClick={() => onPreferenceChange({ ...preference, autoGenerate: !preference.autoGenerate })}
                >
                  <span className={styles.toggleTrack}>
                    <span className={styles.toggleThumb} />
                  </span>
                  <span>{preference.autoGenerate ? 'On' : 'Off'}</span>
                </button>
              </div>
            )}
          </motion.div>
        )}
      </AnimatePresence>

      {isGutenbergBook && (
        <div className={styles.gutenbergNote}>
          <span>📚</span>
          <p>This is a Project Gutenberg book. You have full control over visualization settings.</p>
        </div>
      )}
    </motion.div>
  );
};

// ==================== CHAPTER CARD ====================

interface ChapterCardProps {
  chapter: LocalChapter;
  index: number;
  isExpanded: boolean;
  onToggle: () => void;
  onStartReading: () => void;
}

const ChapterCard: React.FC<ChapterCardProps> = ({ chapter, index, isExpanded, onToggle, onStartReading }) => {
  const readTime = useMemo(() => {
    if (!chapter.wordCount) return null;
    const mins = Math.ceil(chapter.wordCount / 200);
    return mins >= 60 ? `${Math.floor(mins / 60)}h ${mins % 60}m` : `${mins} min`;
  }, [chapter.wordCount]);

  return (
    <motion.div
      className={`${styles.chapterCard} ${isExpanded ? styles.expanded : ''}`}
      variants={fadeInUp}
      onClick={onToggle}
    >
      <div className={styles.chapterHeader}>
        <span className={styles.chapterNumber}>{chapter.number}</span>
        <div className={styles.chapterInfo}>
          <h4>{chapter.title}</h4>
          <div className={styles.chapterMeta}>
            <span>{chapter.pageCount} pages</span>
            {chapter.wordCount && (
              <>
                <span className={styles.metaSep}>•</span>
                <span>{chapter.wordCount.toLocaleString()} words</span>
              </>
            )}
            {readTime && (
              <>
                <span className={styles.metaSep}>•</span>
                <span>⏱ {readTime}</span>
              </>
            )}
          </div>
        </div>
        <span className={`${styles.expandIcon} ${isExpanded ? styles.expandOpen : ''}`}>▾</span>
      </div>

      <AnimatePresence>
        {isExpanded && (
          <motion.div
            className={styles.chapterDetails}
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            transition={{ duration: 0.3 }}
          >
            {chapter.summary && <p className={styles.chapterSummary}>{chapter.summary}</p>}
            <button
              className={styles.readChapterBtn}
              onClick={(e) => {
                e.stopPropagation();
                onStartReading();
              }}
            >
              📖 Start Reading Chapter {chapter.number}
            </button>
          </motion.div>
        )}
      </AnimatePresence>
    </motion.div>
  );
};

// ==================== SHARE MODAL ====================

const ShareModal: React.FC<{ book: LocalBook; onClose: () => void }> = ({ book, onClose }) => {
  const [copied, setCopied] = useState(false);
  const shareUrl = window.location.href;

  const copyLink = async () => {
    try {
      await navigator.clipboard.writeText(shareUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2500);
    } catch { /* fallback */ }
  };

  return (
    <motion.div className={styles.modalOverlay} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} onClick={onClose}>
      <motion.div
        className={styles.shareModal}
        initial={{ opacity: 0, scale: 0.92, y: 20 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.92, y: 20 }}
        transition={{ duration: 0.3, ease: [0.22, 1, 0.36, 1] }}
        onClick={(e) => e.stopPropagation()}
      >
        <div className={styles.shareHeader}>
          <h3>Share this book</h3>
          <button className={styles.shareClose} onClick={onClose}>✕</button>
        </div>
        <div className={styles.shareBookPreview}>
          {book.coverImageUrl && <img src={book.coverImageUrl} alt="" className={styles.shareCoverThumb} />}
          <div>
            <p className={styles.shareBookTitle}>{book.title}</p>
            <p className={styles.shareBookAuthor}>by {book.author.fullName}</p>
          </div>
        </div>
        <div className={styles.shareActions}>
          <button className={`${styles.shareBtn} ${copied ? styles.shareCopied : ''}`} onClick={copyLink}>
            {copied ? '✓ Copied!' : '🔗 Copy Link'}
          </button>
          <a
            className={styles.shareBtn}
            href={`https://twitter.com/intent/tweet?text=${encodeURIComponent(`Reading "${book.title}" by ${book.author.fullName} on NovelVision ✨`)}&url=${encodeURIComponent(shareUrl)}`}
            target="_blank"
            rel="noopener noreferrer"
          >
            𝕏 Share on X
          </a>
          <a
            className={styles.shareBtn}
            href={`mailto:?subject=${encodeURIComponent(`Check out "${book.title}" on NovelVision`)}&body=${encodeURIComponent(`I'm reading "${book.title}" by ${book.author.fullName} on NovelVision — an AI-powered reading experience!\n\n${shareUrl}`)}`}
          >
            ✉️ Email
          </a>
        </div>
      </motion.div>
    </motion.div>
  );
};

// ==================== MAIN BOOK PAGE COMPONENT ====================

const BookPage: React.FC = () => {
  const params = useParams<{ bookId?: string; id?: string }>();
  const bookId = params.bookId || params.id;
  const navigate = useNavigate();

  // State
  const [book, setBook] = useState<LocalBook | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedChapter, setExpandedChapter] = useState<string | null>(null);
  const [imageError, setImageError] = useState(false);
  const [activeTab, setActiveTab] = useState<ContentTab>('chapters');
  const [descExpanded, setDescExpanded] = useState(false);
  const [showShare, setShowShare] = useState(false);
  const [bookmarked, setBookmarked] = useState(false);
  const [readingProgress, setReadingProgress] = useState(0);
  const [heroScrolled, setHeroScrolled] = useState(false);

  const [vizPreference, setVizPreference] = useState<UserVisualizationPreference>({
    mode: 'UserSelected',
    autoGenerate: false,
    imagePosition: 'right',
    imageSize: 'medium',
  });

  // Scroll listener for floating CTA
  useEffect(() => {
    const handleScroll = () => setHeroScrolled(window.scrollY > 500);
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Load book data from real API
  useEffect(() => {
    const loadBook = async () => {
      if (!bookId) {
        setError('No book ID provided');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        setImageError(false);

        // Fetch book details
        const apiBook = await catalogService.getBookById(bookId);

        // Fetch chapters
        let chapters: ApiChapter[] = [];
        try {
          chapters = await catalogService.getChapters(bookId);
          if (!Array.isArray(chapters)) chapters = [];
        } catch {
          chapters = [];
        }

        // Optionally fetch author details for richer info
        let authorDetail: ApiAuthor | null = null;
        const authorId = apiBook.author?.id || apiBook.authorId;
        if (authorId) {
          try {
            authorDetail = await catalogService.getAuthorById(authorId);
          } catch {
            authorDetail = null;
          }
        }

        const localBook = mapApiBookToLocal(apiBook, chapters, authorDetail);
        setBook(localBook);

        // Restore viz preferences
        if (localBook.visualizationMode === 'UserSelected') {
          setVizPreference((prev) => ({ ...prev, mode: 'UserSelected' }));
        }
        try {
          const saved = sessionStorage.getItem(`viz-pref-${bookId}`);
          if (saved) setVizPreference(JSON.parse(saved));
        } catch { /* ignore */ }

        // Check bookmark
        try {
          const bm = JSON.parse(localStorage.getItem('nv-bookmarks') || '[]');
          setBookmarked(bm.includes(bookId));
        } catch { /* ignore */ }

        // Check reading progress
        try {
          const p = localStorage.getItem(`nv-progress-${bookId}`);
          if (p) setReadingProgress(parseFloat(p));
        } catch { /* ignore */ }

      } catch (err) {
        console.error('Error loading book:', err);
        setError('Failed to load book. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    loadBook();
  }, [bookId]);

  // Handlers
  const handleStartReading = useCallback(
    (chapterIndex?: number) => {
      if (!book) return;
      sessionStorage.setItem(`viz-pref-${book.id}`, JSON.stringify(vizPreference));
      const url = chapterIndex !== undefined
        ? `${ROUTES.READER_BY_ID(book.id)}?chapter=${chapterIndex + 1}`
        : ROUTES.READER_BY_ID(book.id);
      navigate(url);
    },
    [book, vizPreference, navigate]
  );

  const toggleChapter = useCallback((chapterId: string) => {
    setExpandedChapter((prev) => (prev === chapterId ? null : chapterId));
  }, []);

  const toggleBookmark = useCallback(() => {
    if (!book) return;
    try {
      const bm: string[] = JSON.parse(localStorage.getItem('nv-bookmarks') || '[]');
      const idx = bm.indexOf(book.id);
      if (idx >= 0) { bm.splice(idx, 1); setBookmarked(false); }
      else { bm.push(book.id); setBookmarked(true); }
      localStorage.setItem('nv-bookmarks', JSON.stringify(bm));
    } catch { /* ignore */ }
  }, [book]);

  // Derived
  const isGutenbergBook = !!book?.gutenbergId || book?.source === 'Gutenberg';
  const showVizSelector = book?.visualizationMode === 'UserSelected' || isGutenbergBook;
  const descNeedsTruncate = (book?.description?.length || 0) > 320;

  const tabs: { key: ContentTab; label: string; icon: string; count?: number }[] = [
    { key: 'chapters', label: 'Chapters', icon: '📑', count: book?.chapterCount },
    { key: 'about', label: 'About', icon: '📝' },
    { key: 'details', label: 'Details', icon: '📊' },
  ];

  // ==================== LOADING STATE ====================
  if (loading) {
    return (
      <div className={styles.page}>
        <div className={styles.breadcrumbsSkel}><div className={styles.shimmer} /></div>
        <section className={styles.heroSection}>
          <div className={styles.heroBackground} />
          <div className={styles.heroContent}>
            <HeroSkeleton />
          </div>
        </section>
        <main className={styles.mainContent}>
          <div className={styles.chaptersSection}>
            <div className={styles.skelTabBar}><div className={styles.shimmer} /></div>
            {Array.from({ length: 5 }).map((_, i) => (
              <ChapterSkeleton key={i} />
            ))}
          </div>
          <aside className={styles.sidebar}>
            <SidebarSkeleton />
          </aside>
        </main>
      </div>
    );
  }

  // ==================== ERROR STATE ====================
  if (error || !book) {
    return (
      <div className={styles.errorPage}>
        <div className={styles.errorContent}>
          <span className={styles.errorIcon}>📚</span>
          <h2>Unable to Load Book</h2>
          <p>{error || 'Book not found'}</p>
          <div className={styles.errorActions}>
            <button className={styles.primaryBtn} onClick={() => window.location.reload()}>
              ↻ Try Again
            </button>
            <Link to={ROUTES.CATALOG} className={styles.secondaryBtn}>
              ← Back to Library
            </Link>
          </div>
        </div>
      </div>
    );
  }

  // ==================== RENDER ====================
  return (
    <div className={styles.page}>
      {/* Breadcrumbs */}
      <Breadcrumbs book={book} />

      {/* ==================== HERO SECTION ==================== */}
      <section className={styles.heroSection}>
        <div className={styles.heroBackground} />

        <motion.div className={styles.heroContent} initial="hidden" animate="visible" variants={staggerContainer}>
          {/* Book Cover */}
          <motion.div className={styles.bookCoverSection} variants={scaleIn}>
            <div className={styles.bookCoverWrapper}>
              <div className={styles.bookCover}>
                {book.coverImageUrl && !imageError ? (
                  <img src={book.coverImageUrl} alt={book.title} onError={() => setImageError(true)} />
                ) : (
                  <div className={styles.placeholderCover}>
                    <span className={styles.placeholderIcon}>📖</span>
                    <span className={styles.placeholderTitle}>{book.title}</span>
                  </div>
                )}
                <div className={styles.bookSpine} />
              </div>
              <div className={styles.pageEdges} />

              {/* Badges */}
              <div className={styles.coverBadges}>
                {isGutenbergBook && <span className={styles.vizBadge}>✦ AI Visualized</span>}
                {book.source === 'Gutenberg' && <span className={styles.sourceBadge}>📜 Gutenberg</span>}
              </div>
            </div>

            {/* Reading Progress */}
            {readingProgress > 0 && (
              <div className={styles.progressIndicator}>
                <div className={styles.progressBar}>
                  <div className={styles.progressFill} style={{ width: `${readingProgress}%` }} />
                </div>
                <span className={styles.progressText}>{Math.round(readingProgress)}% complete</span>
              </div>
            )}
          </motion.div>

          {/* Book Details */}
          <div className={styles.bookDetails}>
            <motion.h1 className={styles.bookTitle} variants={fadeInUp}>
              {book.title}
            </motion.h1>

            <motion.div variants={fadeInUp}>
              <Link to={ROUTES.AUTHOR_BY_ID(book.author.id)} className={styles.authorLink}>
                <div className={styles.authorAvatar}>
                  {book.author.avatarUrl ? (
                    <img src={book.author.avatarUrl} alt={book.author.fullName} />
                  ) : (
                    <span>{book.author.fullName.charAt(0)}</span>
                  )}
                </div>
                <div>
                  <span className={styles.authorName}>{book.author.fullName}</span>
                  {book.author.birthYear && (
                    <span className={styles.authorLabel}>
                      {book.author.birthYear}–{book.author.deathYear || 'present'}
                    </span>
                  )}
                </div>
              </Link>
            </motion.div>

            {/* Stats Row */}
            <motion.div className={styles.statsRow} variants={fadeInUp}>
              {(book.rating ?? 0) > 0 && (
                <>
                  <div className={styles.rating}>
                    <span className={styles.stars}>
                      {'★'.repeat(Math.round(book.rating || 0))}
                      {'☆'.repeat(5 - Math.round(book.rating || 0))}
                    </span>
                    <span className={styles.ratingValue}>{(book.rating || 0).toFixed(1)}</span>
                    {(book.reviewCount ?? 0) > 0 && (
                      <span className={styles.ratingCount}>({formatNumber(book.reviewCount)})</span>
                    )}
                  </div>
                  <span className={styles.statDivider} />
                </>
              )}
              <span className={styles.stat}>
                <strong>{book.chapterCount}</strong> Chapters
              </span>
              <span className={styles.statDivider} />
              <span className={styles.stat}>
                <strong>{book.pageCount}</strong> Pages
              </span>
              <span className={styles.statDivider} />
              <span className={styles.stat}>
                <strong>{formatReadingTime(book.wordCount)}</strong> read
              </span>
            </motion.div>

            {/* Genres / Subjects */}
            {book.subjects && book.subjects.length > 0 && (
              <motion.div className={styles.genres} variants={fadeInUp}>
                {book.subjects.slice(0, 6).map((subject, i) => (
                  <Link key={i} to={`${ROUTES.CATALOG}?genre=${encodeURIComponent(subject)}`} className={styles.genreTag}>
                    {subject}
                  </Link>
                ))}
              </motion.div>
            )}

            {/* Description */}
            {book.description && (
              <motion.div className={styles.descriptionBlock} variants={fadeInUp}>
                <p className={`${styles.description} ${!descExpanded && descNeedsTruncate ? styles.descTruncated : ''}`}>
                  {book.description}
                </p>
                {descNeedsTruncate && (
                  <button className={styles.readMoreBtn} onClick={() => setDescExpanded(!descExpanded)}>
                    {descExpanded ? 'Show less ↑' : 'Read more ↓'}
                  </button>
                )}
              </motion.div>
            )}

            {/* Actions */}
            <motion.div className={styles.actions} variants={fadeInUp}>
              <button className={styles.primaryBtn} onClick={() => handleStartReading()}>
                {readingProgress > 0 ? '📖 Continue Reading' : '📖 Start Reading'}
              </button>
              <button
                className={`${styles.secondaryBtn} ${bookmarked ? styles.bookmarked : ''}`}
                onClick={toggleBookmark}
              >
                {bookmarked ? '★ In Library' : '☆ Add to Library'}
              </button>
              <button className={styles.iconBtn} title="Share" onClick={() => setShowShare(true)}>
                📤
              </button>
            </motion.div>
          </div>
        </motion.div>
      </section>

      {/* ==================== MAIN CONTENT ==================== */}
      <main className={styles.mainContent}>
        {/* Left Column */}
        <div className={styles.chaptersSection}>
          {/* Visualization Mode Selector */}
          {showVizSelector && (
            <VisualizationModeSelector
              currentMode={book.visualizationMode}
              preference={vizPreference}
              onPreferenceChange={setVizPreference}
              isGutenbergBook={isGutenbergBook}
            />
          )}

          {/* Tabs */}
          <div className={styles.tabBar}>
            {tabs.map((tab) => (
              <button
                key={tab.key}
                className={`${styles.tab} ${activeTab === tab.key ? styles.tabActive : ''}`}
                onClick={() => setActiveTab(tab.key)}
              >
                <span className={styles.tabIcon}>{tab.icon}</span>
                <span>{tab.label}</span>
                {tab.count !== undefined && tab.count > 0 && (
                  <span className={styles.tabCount}>{tab.count}</span>
                )}
              </button>
            ))}
            <div
              className={styles.tabIndicator}
              style={{
                transform: `translateX(${tabs.findIndex((t) => t.key === activeTab) * 100}%)`,
                width: `${100 / tabs.length}%`,
              }}
            />
          </div>

          {/* Tab Content */}
          <AnimatePresence mode="wait">
            {/* ===== CHAPTERS TAB ===== */}
            {activeTab === 'chapters' && (
              <motion.div
                key="chapters"
                className={styles.chapterList}
                initial="hidden"
                animate="visible"
                exit={{ opacity: 0 }}
                variants={staggerContainer}
              >
                {book.chapters.length > 0 ? (
                  <div className={styles.chapters}>
                    {book.chapters.map((chapter, index) => (
                      <ChapterCard
                        key={chapter.id}
                        chapter={chapter}
                        index={index}
                        isExpanded={expandedChapter === chapter.id}
                        onToggle={() => toggleChapter(chapter.id)}
                        onStartReading={() => handleStartReading(index)}
                      />
                    ))}

                    {book.chapters.length < book.chapterCount && (
                      <div className={styles.moreChapters}>
                        <span className={styles.moreIcon}>📚</span>
                        <span>+ {book.chapterCount - book.chapters.length} more chapters available when reading</span>
                      </div>
                    )}
                  </div>
                ) : (
                  <div className={styles.emptyChapters}>
                    <span className={styles.emptyIcon}>📭</span>
                    <p>No chapters available yet</p>
                    <p className={styles.emptyHint}>Chapters will appear once the book content is processed</p>
                  </div>
                )}
              </motion.div>
            )}

            {/* ===== ABOUT TAB ===== */}
            {activeTab === 'about' && (
              <motion.div
                key="about"
                className={styles.aboutTab}
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.35 }}
              >
                {/* Full Description */}
                {book.description && (
                  <div className={styles.aboutSection}>
                    <h3 className={styles.aboutSectionTitle}>Synopsis</h3>
                    <p className={styles.aboutText}>{book.description}</p>
                  </div>
                )}

                {/* Author Card */}
                <div className={styles.aboutSection}>
                  <h3 className={styles.aboutSectionTitle}>About the Author</h3>
                  <div className={styles.aboutAuthorCard}>
                    <div className={styles.aboutAuthorAvatar}>
                      {book.author.avatarUrl ? (
                        <img src={book.author.avatarUrl} alt={book.author.fullName} />
                      ) : (
                        <span>{book.author.fullName.charAt(0)}</span>
                      )}
                    </div>
                    <div className={styles.aboutAuthorInfo}>
                      <h4>{book.author.fullName}</h4>
                      {book.author.birthYear && (
                        <p className={styles.aboutAuthorDates}>
                          {book.author.birthYear}–{book.author.deathYear || 'present'}
                        </p>
                      )}
                      {book.author.biography && (
                        <p className={styles.aboutAuthorBio}>
                          {book.author.biography.slice(0, 200)}
                          {book.author.biography.length > 200 && '...'}
                        </p>
                      )}
                      {book.author.bookCount && book.author.bookCount > 1 && (
                        <span className={styles.authorBookCount}>{book.author.bookCount} books on NovelVision</span>
                      )}
                      <Link to={ROUTES.AUTHOR_BY_ID(book.author.id)} className={styles.viewAuthorLink}>
                        View author profile →
                      </Link>
                    </div>
                  </div>
                </div>

                {/* Subjects / Tags */}
                {book.subjects && book.subjects.length > 0 && (
                  <div className={styles.aboutSection}>
                    <h3 className={styles.aboutSectionTitle}>Subjects & Tags</h3>
                    <div className={styles.tags}>
                      {book.subjects.map((subject, i) => (
                        <Link key={i} to={`${ROUTES.CATALOG}?genre=${encodeURIComponent(subject)}`} className={styles.tag}>
                          {subject}
                        </Link>
                      ))}
                    </div>
                  </div>
                )}
              </motion.div>
            )}

            {/* ===== DETAILS TAB ===== */}
            {activeTab === 'details' && (
              <motion.div
                key="details"
                className={styles.detailsTab}
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.35 }}
              >
                <div className={styles.detailsGrid}>
                  {[
                    { label: 'Chapters', value: String(book.chapterCount) },
                    { label: 'Pages', value: String(book.pageCount) },
                    { label: 'Words', value: book.wordCount?.toLocaleString() || '—' },
                    { label: 'Reading Time', value: formatReadingTime(book.wordCount) },
                    { label: 'Language', value: getLanguageName(book.language) },
                    ...(book.isbn ? [{ label: 'ISBN', value: book.isbn }] : []),
                    ...(book.publisher ? [{ label: 'Publisher', value: book.publisher }] : []),
                    ...(book.source ? [{ label: 'Source', value: book.source }] : []),
                    ...(book.publishedAt ? [{ label: 'Published', value: formatDate(book.publishedAt) }] : []),
                    { label: 'Added to Library', value: formatDate(book.createdAt) },
                    ...(isGutenbergBook && book.gutenbergId ? [{ label: 'Gutenberg ID', value: `#${book.gutenbergId}` }] : []),
                    ...((book.downloadCount ?? 0) > 0 ? [{ label: 'Downloads', value: formatNumber(book.downloadCount) }] : []),
                    ...((book.viewCount ?? 0) > 0 ? [{ label: 'Views', value: formatNumber(book.viewCount) }] : []),
                  ].map((item, i) => (
                    <div className={styles.detailItem} key={i}>
                      <span className={styles.detailLabel}>{item.label}</span>
                      <span className={styles.detailValue}>{item.value}</span>
                    </div>
                  ))}
                </div>

                {/* Visualization Summary */}
                <div className={styles.vizInfoCard}>
                  <h4>✦ AI Visualization</h4>
                  <p className={styles.vizInfoDesc}>
                    {vizPreference.mode === 'UserSelected'
                      ? 'Select any text while reading to generate AI illustrations of scenes.'
                      : vizPreference.mode === 'PerPage'
                      ? 'Each page will have an automatically generated illustration.'
                      : vizPreference.mode === 'PerChapter'
                      ? 'Each chapter will feature a key scene illustration.'
                      : 'Reading in text-only mode without visualizations.'}
                  </p>
                  <div className={styles.vizInfoGrid}>
                    <div className={styles.vizInfoRow}>
                      <span>Mode</span>
                      <span className={styles.vizInfoValue}>
                        {vizPreference.mode === 'UserSelected' ? "Reader's Choice" : vizPreference.mode === 'PerPage' ? 'Per Page' : vizPreference.mode === 'PerChapter' ? 'Per Chapter' : 'Text Only'}
                      </span>
                    </div>
                    {vizPreference.mode !== 'None' && (
                      <>
                        <div className={styles.vizInfoRow}>
                          <span>Position</span>
                          <span className={styles.vizInfoValue}>{vizPreference.imagePosition.charAt(0).toUpperCase() + vizPreference.imagePosition.slice(1)}</span>
                        </div>
                        <div className={styles.vizInfoRow}>
                          <span>Size</span>
                          <span className={styles.vizInfoValue}>{vizPreference.imageSize.charAt(0).toUpperCase() + vizPreference.imageSize.slice(1)}</span>
                        </div>
                      </>
                    )}
                  </div>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {/* Right Column - Sidebar */}
        <aside className={styles.sidebar}>
          {/* Quick Stats Card */}
          <div className={styles.sidebarCard}>
            <h3 className={styles.cardTitle}>
              <span>📊</span> Quick Stats
            </h3>
            <div className={styles.statsList}>
              <div className={styles.statItem}>
                <span className={styles.statLabel}>Chapters</span>
                <span className={styles.statValue}>{book.chapterCount}</span>
              </div>
              <div className={styles.statItem}>
                <span className={styles.statLabel}>Pages</span>
                <span className={styles.statValue}>{book.pageCount}</span>
              </div>
              <div className={styles.statItem}>
                <span className={styles.statLabel}>Words</span>
                <span className={styles.statValue}>{book.wordCount?.toLocaleString() || '—'}</span>
              </div>
              <div className={styles.statItem}>
                <span className={styles.statLabel}>Reading Time</span>
                <span className={styles.statValue}>{formatReadingTime(book.wordCount)}</span>
              </div>
              <div className={styles.statItem}>
                <span className={styles.statLabel}>Language</span>
                <span className={styles.statValue}>{getLanguageName(book.language)}</span>
              </div>
            </div>

            {isGutenbergBook && (
              <div className={styles.bookMeta}>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Source</span>
                  <span className={styles.metaValue}>Project Gutenberg</span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Gutenberg ID</span>
                  <span className={styles.metaValue}>#{book.gutenbergId}</span>
                </div>
              </div>
            )}
          </div>

          {/* Viz Info Card */}
          <div className={`${styles.sidebarCard} ${styles.vizCard}`}>
            <h3 className={styles.cardTitle}>
              <span>✨</span> AI Visualization
            </h3>
            <p className={styles.vizDescription}>
              {vizPreference.mode === 'UserSelected'
                ? 'Select any text while reading to generate AI illustrations of scenes.'
                : vizPreference.mode === 'PerPage'
                ? 'Each page will have an automatically generated illustration.'
                : vizPreference.mode === 'PerChapter'
                ? 'Each chapter will feature a key scene illustration.'
                : 'Reading in text-only mode without visualizations.'}
            </p>
            <div className={styles.vizSetting}>
              <span>Current Mode</span>
              <span className={styles.vizValue}>
                {vizPreference.mode === 'UserSelected' ? "Reader's Choice" :
                 vizPreference.mode === 'PerPage' ? 'Per Page' :
                 vizPreference.mode === 'PerChapter' ? 'Per Chapter' : 'Text Only'}
              </span>
            </div>
            {vizPreference.mode !== 'None' && (
              <>
                <div className={styles.vizSetting}>
                  <span>Image Position</span>
                  <span className={styles.vizValue}>
                    {vizPreference.imagePosition.charAt(0).toUpperCase() + vizPreference.imagePosition.slice(1)}
                  </span>
                </div>
                <div className={styles.vizSetting}>
                  <span>Image Size</span>
                  <span className={styles.vizValue}>
                    {vizPreference.imageSize.charAt(0).toUpperCase() + vizPreference.imageSize.slice(1)}
                  </span>
                </div>
              </>
            )}
            {showVizSelector && (
              <button
                className={styles.vizEditBtn}
                onClick={() => {
                  setActiveTab('chapters');
                  window.scrollTo({ top: 500, behavior: 'smooth' });
                }}
              >
                🎨 Edit Settings
              </button>
            )}
          </div>

          {/* Tags */}
          {book.subjects && book.subjects.length > 0 && (
            <div className={styles.sidebarCard}>
              <h3 className={styles.cardTitle}>
                <span>🏷️</span> Tags
              </h3>
              <div className={styles.tags}>
                {book.subjects.map((subject, i) => (
                  <Link key={i} to={`${ROUTES.CATALOG}?tag=${encodeURIComponent(subject)}`} className={styles.tag}>
                    {subject}
                  </Link>
                ))}
              </div>
            </div>
          )}
        </aside>
      </main>

      {/* Floating CTA (mobile + scrolled) */}
      <AnimatePresence>
        {heroScrolled && (
          <motion.div
            className={styles.floatingCta}
            initial={{ opacity: 0, y: 60 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 60 }}
            transition={{ duration: 0.3 }}
          >
            <button className={styles.floatingBtn} onClick={() => handleStartReading()}>
              📖 {readingProgress > 0 ? 'Continue Reading' : 'Start Reading'}
            </button>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Share Modal */}
      <AnimatePresence>
        {showShare && <ShareModal book={book} onClose={() => setShowShare(false)} />}
      </AnimatePresence>
    </div>
  );
};

export default BookPage;
