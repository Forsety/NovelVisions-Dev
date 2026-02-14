// src/pages/CatalogPage/CatalogPage.tsx
// Premium Library Catalog — Immersive Book Discovery Experience

import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { ROUTES } from '../../shared/constants/routes';
import { catalogService, type BookFilters, type Genre } from '../../services/api/catalog.service';
import type { Book, PaginatedResponse } from '../../types';
import styles from './CatalogPage.module.css';

// ==================== ANIMATION VARIANTS ====================

const fadeInUp = {
  hidden: { opacity: 0, y: 24 },
  visible: (i: number = 0) => ({
    opacity: 1,
    y: 0,
    transition: { duration: 0.5, delay: i * 0.06, ease: [0.22, 1, 0.36, 1] as [number, number, number, number] },
  }),
};

const staggerGrid = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.05, delayChildren: 0.1 },
  },
};

const scaleIn = {
  hidden: { opacity: 0, scale: 0.92 },
  visible: { opacity: 1, scale: 1, transition: { duration: 0.4, ease: [0.22, 1, 0.36, 1] as [number, number, number, number] } },
  exit: { opacity: 0, scale: 0.92, transition: { duration: 0.25 } },
};

// ==================== SKELETON LOADER ====================

const BookCardSkeleton: React.FC<{ index: number }> = ({ index }) => (
  <motion.div
    className={styles.skeletonCard}
    initial={{ opacity: 0 }}
    animate={{ opacity: 1 }}
    transition={{ delay: index * 0.05 }}
  >
    <div className={styles.skeletonCover}>
      <div className={styles.shimmer} />
    </div>
    <div className={styles.skeletonInfo}>
      <div className={styles.skeletonTitle}>
        <div className={styles.shimmer} />
      </div>
      <div className={styles.skeletonAuthor}>
        <div className={styles.shimmer} />
      </div>
      <div className={styles.skeletonMeta}>
        <div className={styles.shimmer} />
      </div>
    </div>
  </motion.div>
);

const ListCardSkeleton: React.FC<{ index: number }> = ({ index }) => (
  <motion.div
    className={styles.skeletonListCard}
    initial={{ opacity: 0 }}
    animate={{ opacity: 1 }}
    transition={{ delay: index * 0.05 }}
  >
    <div className={styles.skeletonListCover}>
      <div className={styles.shimmer} />
    </div>
    <div className={styles.skeletonListInfo}>
      <div className={styles.skeletonListTitle}><div className={styles.shimmer} /></div>
      <div className={styles.skeletonListAuthor}><div className={styles.shimmer} /></div>
      <div className={styles.skeletonListDesc}><div className={styles.shimmer} /></div>
      <div className={styles.skeletonListDesc} style={{ width: '60%' }}><div className={styles.shimmer} /></div>
    </div>
  </motion.div>
);

// ==================== BOOK CARD — GRID VIEW ====================

interface BookCardProps {
  book: Book;
  onClick: () => void;
}

const BookCard: React.FC<BookCardProps> = ({ book, onClick }) => {
  const [imageError, setImageError] = useState(false);
  const [isHovered, setIsHovered] = useState(false);

  const readingTime = useMemo(() => {
    if (!book.pageCount) return null;
    const words = book.pageCount * 250;
    const hours = Math.floor(words / 15000);
    const mins = Math.round((words % 15000) / 250);
    return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
  }, [book.pageCount]);

  return (
    <motion.div
      variants={fadeInUp}
      whileHover={{ y: -10 }}
      transition={{ duration: 0.35, ease: [0.22, 1, 0.36, 1] }}
      onHoverStart={() => setIsHovered(true)}
      onHoverEnd={() => setIsHovered(false)}
    >
      <div className={styles.bookCard} onClick={onClick} role="button" tabIndex={0}>
        {/* Cover */}
        <div className={styles.bookCover}>
          {book.coverImageUrl && !imageError ? (
            <img
              src={book.coverImageUrl}
              alt={book.title}
              loading="lazy"
              onError={() => setImageError(true)}
            />
          ) : (
            <div className={styles.placeholderCover}>
              <span className={styles.placeholderEmoji}>📖</span>
              <span className={styles.placeholderTitle}>
                {book.title.length > 30 ? book.title.substring(0, 30) + '…' : book.title}
              </span>
            </div>
          )}

          {/* Badges */}
          <div className={styles.badgeRow}>
            {book.visualizationEnabled && (
              <span className={styles.aiBadge} title="AI Visualization Available">
                <span className={styles.aiBadgeIcon}>✦</span> AI
              </span>
            )}
            {book.isFeatured && (
              <span className={styles.featuredBadge}>★ Featured</span>
            )}
          </div>

          {/* Hover Overlay */}
          <AnimatePresence>
            {isHovered && (
              <motion.div
                className={styles.cardOverlay}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.25 }}
              >
                <div className={styles.overlayContent}>
                  {book.description && (
                    <p className={styles.overlayDesc}>
                      {book.description.length > 120
                        ? book.description.substring(0, 120) + '…'
                        : book.description}
                    </p>
                  )}
                  <span className={styles.overlayAction}>View Details →</span>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {/* Info */}
        <div className={styles.bookInfo}>
          <h3 className={styles.bookTitle}>{book.title}</h3>
          <p className={styles.bookAuthor}>{book.authorName || 'Unknown Author'}</p>

          <div className={styles.bookMeta}>
            {book.rating > 0 && (
              <span className={styles.rating}>
                <span className={styles.ratingStars}>
                  {'★'.repeat(Math.round(book.rating))}
                  {'☆'.repeat(5 - Math.round(book.rating))}
                </span>
                <span className={styles.ratingNum}>{book.rating.toFixed(1)}</span>
              </span>
            )}
            {book.pageCount > 0 && (
              <span className={styles.metaDot}>
                {book.pageCount} pg{readingTime && ` · ${readingTime}`}
              </span>
            )}
          </div>

          {book.genres && book.genres.length > 0 && (
            <div className={styles.cardGenres}>
              {book.genres.slice(0, 2).map((genre) => (
                <span key={genre} className={styles.genreChip}>{genre}</span>
              ))}
              {book.genres.length > 2 && (
                <span className={styles.genreMore}>+{book.genres.length - 2}</span>
              )}
            </div>
          )}
        </div>
      </div>
    </motion.div>
  );
};

// ==================== BOOK CARD — LIST VIEW ====================

const BookListCard: React.FC<BookCardProps> = ({ book, onClick }) => {
  const [imageError, setImageError] = useState(false);

  const readingTime = useMemo(() => {
    if (!book.pageCount) return null;
    const words = book.pageCount * 250;
    const hours = Math.floor(words / 15000);
    const mins = Math.round((words % 15000) / 250);
    return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
  }, [book.pageCount]);

  return (
    <motion.div variants={fadeInUp}>
      <div className={styles.listCard} onClick={onClick} role="button" tabIndex={0}>
        {/* Cover */}
        <div className={styles.listCover}>
          {book.coverImageUrl && !imageError ? (
            <img
              src={book.coverImageUrl}
              alt={book.title}
              loading="lazy"
              onError={() => setImageError(true)}
            />
          ) : (
            <div className={styles.listPlaceholder}>
              <span>📖</span>
            </div>
          )}
        </div>

        {/* Content */}
        <div className={styles.listContent}>
          <div className={styles.listTop}>
            <div>
              <h3 className={styles.listTitle}>{book.title}</h3>
              <p className={styles.listAuthor}>by {book.authorName || 'Unknown Author'}</p>
            </div>
            <div className={styles.listBadges}>
              {book.visualizationEnabled && (
                <span className={styles.aiBadge}><span className={styles.aiBadgeIcon}>✦</span> AI</span>
              )}
              {book.isFeatured && (
                <span className={styles.featuredBadge}>★ Featured</span>
              )}
            </div>
          </div>

          {book.description && (
            <p className={styles.listDesc}>
              {book.description.length > 200
                ? book.description.substring(0, 200) + '…'
                : book.description}
            </p>
          )}

          <div className={styles.listBottom}>
            <div className={styles.listMeta}>
              {book.rating > 0 && (
                <span className={styles.rating}>
                  <span className={styles.ratingStars}>
                    {'★'.repeat(Math.round(book.rating))}
                  </span>
                  <span className={styles.ratingNum}>{book.rating.toFixed(1)}</span>
                </span>
              )}
              {book.pageCount > 0 && (
                <span className={styles.metaDot}>{book.pageCount} pages</span>
              )}
              {readingTime && (
                <span className={styles.metaDot}>⏱ {readingTime}</span>
              )}
              {book.language && (
                <span className={styles.metaDot}>{book.language.toUpperCase()}</span>
              )}
            </div>

            {book.genres && book.genres.length > 0 && (
              <div className={styles.cardGenres}>
                {book.genres.slice(0, 3).map((genre) => (
                  <span key={genre} className={styles.genreChip}>{genre}</span>
                ))}
              </div>
            )}
          </div>
        </div>

        <div className={styles.listArrow}>→</div>
      </div>
    </motion.div>
  );
};

// ==================== ACTIVE FILTER TAGS ====================

interface ActiveFiltersProps {
  filters: BookFilters;
  genres: Genre[];
  onRemove: (key: keyof BookFilters) => void;
  onReset: () => void;
}

const ActiveFilterTags: React.FC<ActiveFiltersProps> = ({ filters, genres, onRemove, onReset }) => {
  const tags: { key: keyof BookFilters; label: string }[] = [];

  if (filters.search) tags.push({ key: 'search', label: `"${filters.search}"` });
  if (filters.genre) {
    const g = genres.find((g) => g.id === filters.genre);
    tags.push({ key: 'genre', label: g ? `${g.icon} ${g.name}` : filters.genre });
  }
  if (filters.language) tags.push({ key: 'language', label: `Language: ${filters.language.toUpperCase()}` });
  if (filters.source) tags.push({ key: 'source', label: `Source: ${filters.source}` });
  if (filters.status) tags.push({ key: 'status', label: '🎨 With Visualizations' });
  if (filters.minPages) tags.push({ key: 'minPages', label: `Min ${filters.minPages} pages` });
  if (filters.maxPages) tags.push({ key: 'maxPages', label: `Max ${filters.maxPages} pages` });

  if (tags.length === 0) return null;

  return (
    <motion.div
      className={styles.activeTags}
      initial={{ opacity: 0, height: 0 }}
      animate={{ opacity: 1, height: 'auto' }}
      exit={{ opacity: 0, height: 0 }}
    >
      <span className={styles.activeLabel}>Active filters:</span>
      <div className={styles.tagsList}>
        {tags.map(({ key, label }) => (
          <motion.button
            key={key}
            className={styles.filterTag}
            onClick={() => onRemove(key)}
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.8, opacity: 0 }}
            whileHover={{ scale: 1.05 }}
          >
            {label}
            <span className={styles.tagRemove}>×</span>
          </motion.button>
        ))}
        <button className={styles.clearAllBtn} onClick={onReset}>
          Clear all
        </button>
      </div>
    </motion.div>
  );
};

// ==================== GENRE QUICK NAV ====================

interface GenreQuickNavProps {
  genres: Genre[];
  activeGenre?: string;
  onSelect: (genreId: string | undefined) => void;
}

const GenreQuickNav: React.FC<GenreQuickNavProps> = ({ genres, activeGenre, onSelect }) => {
  const scrollRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(true);

  const checkScroll = useCallback(() => {
    const el = scrollRef.current;
    if (!el) return;
    setCanScrollLeft(el.scrollLeft > 10);
    setCanScrollRight(el.scrollLeft < el.scrollWidth - el.clientWidth - 10);
  }, []);

  useEffect(() => {
    const el = scrollRef.current;
    if (!el) return;
    el.addEventListener('scroll', checkScroll, { passive: true });
    checkScroll();
    return () => el.removeEventListener('scroll', checkScroll);
  }, [checkScroll, genres]);

  const scroll = (dir: 'left' | 'right') => {
    scrollRef.current?.scrollBy({ left: dir === 'left' ? -200 : 200, behavior: 'smooth' });
  };

  return (
    <div className={styles.genreNav}>
      {canScrollLeft && (
        <button className={`${styles.genreScrollBtn} ${styles.scrollLeft}`} onClick={() => scroll('left')}>
          ‹
        </button>
      )}
      <div className={styles.genreStrip} ref={scrollRef}>
        <button
          className={`${styles.genrePill} ${!activeGenre ? styles.genrePillActive : ''}`}
          onClick={() => onSelect(undefined)}
        >
          📚 All
        </button>
        {genres.map((genre) => (
          <button
            key={genre.id}
            className={`${styles.genrePill} ${activeGenre === genre.id ? styles.genrePillActive : ''}`}
            onClick={() => onSelect(activeGenre === genre.id ? undefined : genre.id)}
          >
            {genre.icon} {genre.name}
          </button>
        ))}
      </div>
      {canScrollRight && (
        <button className={`${styles.genreScrollBtn} ${styles.scrollRight}`} onClick={() => scroll('right')}>
          ›
        </button>
      )}
    </div>
  );
};

// ==================== FILTER SIDEBAR ====================

interface FilterSidebarProps {
  filters: BookFilters;
  genres: Genre[];
  onFilterChange: (filters: BookFilters) => void;
  onReset: () => void;
  onClose?: () => void;
}

const FilterSidebar: React.FC<FilterSidebarProps> = ({
  filters,
  genres,
  onFilterChange,
  onReset,
  onClose,
}) => {
  const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({
    genre: true,
    language: true,
    source: true,
    pages: false,
    visualization: true,
  });

  const toggleSection = (key: string) => {
    setExpandedSections((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  const languages = [
    { code: 'en', name: 'English', flag: '🇬🇧' },
    { code: 'de', name: 'German', flag: '🇩🇪' },
    { code: 'fr', name: 'French', flag: '🇫🇷' },
    { code: 'es', name: 'Spanish', flag: '🇪🇸' },
    { code: 'it', name: 'Italian', flag: '🇮🇹' },
    { code: 'pt', name: 'Portuguese', flag: '🇵🇹' },
    { code: 'ru', name: 'Russian', flag: '🇷🇺' },
    { code: 'pl', name: 'Polish', flag: '🇵🇱' },
  ];

  const sources = [
    { id: 'all', name: 'All Sources', icon: '🌐' },
    { id: 'Gutenberg', name: 'Project Gutenberg', icon: '📜' },
    { id: 'Manual', name: 'Platform Authors', icon: '✍️' },
  ];

  const activeCount = [
    filters.genre, filters.language, filters.source, filters.status,
    filters.minPages, filters.maxPages,
  ].filter(Boolean).length;

  return (
    <aside className={styles.sidebar}>
      {/* Sidebar Header */}
      <div className={styles.sidebarHeader}>
        <div className={styles.sidebarTitleRow}>
          <h3 className={styles.sidebarTitle}>
            <span className={styles.filterIcon}>⚙</span>
            Filters
          </h3>
          {activeCount > 0 && (
            <span className={styles.activeCountBadge}>{activeCount}</span>
          )}
        </div>
        <div className={styles.sidebarActions}>
          {activeCount > 0 && (
            <button className={styles.resetBtn} onClick={onReset}>Reset</button>
          )}
          {onClose && (
            <button className={styles.closeSidebarBtn} onClick={onClose}>✕</button>
          )}
        </div>
      </div>

      {/* Genre Section */}
      <div className={styles.filterSection}>
        <button className={styles.sectionToggle} onClick={() => toggleSection('genre')}>
          <h4 className={styles.sectionLabel}>Genre</h4>
          <span className={`${styles.chevron} ${expandedSections.genre ? styles.chevronOpen : ''}`}>▾</span>
        </button>
        <AnimatePresence>
          {expandedSections.genre && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 'auto', opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.25 }}
              style={{ overflow: 'hidden' }}
            >
              <div className={styles.genreGrid}>
                {genres.map((genre) => (
                  <button
                    key={genre.id}
                    className={`${styles.genreButton} ${filters.genre === genre.id ? styles.genreButtonActive : ''}`}
                    onClick={() =>
                      onFilterChange({
                        ...filters,
                        genre: filters.genre === genre.id ? undefined : genre.id,
                      })
                    }
                    style={{
                      '--genre-color': genre.color,
                    } as React.CSSProperties}
                  >
                    <span className={styles.genreBtnIcon}>{genre.icon}</span>
                    <span className={styles.genreBtnName}>{genre.name}</span>
                  </button>
                ))}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Language Section */}
      <div className={styles.filterSection}>
        <button className={styles.sectionToggle} onClick={() => toggleSection('language')}>
          <h4 className={styles.sectionLabel}>Language</h4>
          <span className={`${styles.chevron} ${expandedSections.language ? styles.chevronOpen : ''}`}>▾</span>
        </button>
        <AnimatePresence>
          {expandedSections.language && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 'auto', opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.25 }}
              style={{ overflow: 'hidden' }}
            >
              <div className={styles.langGrid}>
                {languages.map((lang) => (
                  <button
                    key={lang.code}
                    className={`${styles.langButton} ${filters.language === lang.code ? styles.langButtonActive : ''}`}
                    onClick={() =>
                      onFilterChange({
                        ...filters,
                        language: filters.language === lang.code ? undefined : lang.code,
                      })
                    }
                  >
                    <span className={styles.langFlag}>{lang.flag}</span>
                    <span>{lang.name}</span>
                  </button>
                ))}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Source Section */}
      <div className={styles.filterSection}>
        <button className={styles.sectionToggle} onClick={() => toggleSection('source')}>
          <h4 className={styles.sectionLabel}>Source</h4>
          <span className={`${styles.chevron} ${expandedSections.source ? styles.chevronOpen : ''}`}>▾</span>
        </button>
        <AnimatePresence>
          {expandedSections.source && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 'auto', opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.25 }}
              style={{ overflow: 'hidden' }}
            >
              <div className={styles.sourceList}>
                {sources.map((source) => (
                  <button
                    key={source.id}
                    className={`${styles.sourceButton} ${
                      (source.id === 'all' && !filters.source) || filters.source === source.id
                        ? styles.sourceButtonActive
                        : ''
                    }`}
                    onClick={() =>
                      onFilterChange({
                        ...filters,
                        source: source.id === 'all' ? undefined : source.id,
                      })
                    }
                  >
                    <span>{source.icon}</span>
                    <span>{source.name}</span>
                  </button>
                ))}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Page Count Section */}
      <div className={styles.filterSection}>
        <button className={styles.sectionToggle} onClick={() => toggleSection('pages')}>
          <h4 className={styles.sectionLabel}>Page Count</h4>
          <span className={`${styles.chevron} ${expandedSections.pages ? styles.chevronOpen : ''}`}>▾</span>
        </button>
        <AnimatePresence>
          {expandedSections.pages && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 'auto', opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.25 }}
              style={{ overflow: 'hidden' }}
            >
              <div className={styles.rangeInputs}>
                <input
                  type="number"
                  placeholder="Min"
                  value={filters.minPages || ''}
                  onChange={(e) =>
                    onFilterChange({
                      ...filters,
                      minPages: e.target.value ? parseInt(e.target.value) : undefined,
                    })
                  }
                  className={styles.rangeInput}
                />
                <span className={styles.rangeSep}>—</span>
                <input
                  type="number"
                  placeholder="Max"
                  value={filters.maxPages || ''}
                  onChange={(e) =>
                    onFilterChange({
                      ...filters,
                      maxPages: e.target.value ? parseInt(e.target.value) : undefined,
                    })
                  }
                  className={styles.rangeInput}
                />
              </div>
              {/* Quick page presets */}
              <div className={styles.pagePresets}>
                {[
                  { label: 'Short (< 100)', min: undefined, max: 100 },
                  { label: 'Medium (100-300)', min: 100, max: 300 },
                  { label: 'Long (300+)', min: 300, max: undefined },
                ].map((p) => (
                  <button
                    key={p.label}
                    className={`${styles.presetBtn} ${
                      filters.minPages === p.min && filters.maxPages === p.max ? styles.presetActive : ''
                    }`}
                    onClick={() =>
                      onFilterChange({ ...filters, minPages: p.min, maxPages: p.max })
                    }
                  >
                    {p.label}
                  </button>
                ))}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* AI Visualization */}
      <div className={styles.filterSection}>
        <button className={styles.sectionToggle} onClick={() => toggleSection('visualization')}>
          <h4 className={styles.sectionLabel}>Features</h4>
          <span className={`${styles.chevron} ${expandedSections.visualization ? styles.chevronOpen : ''}`}>▾</span>
        </button>
        <AnimatePresence>
          {expandedSections.visualization && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 'auto', opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.25 }}
              style={{ overflow: 'hidden' }}
            >
              <label className={styles.toggleLabel}>
                <div className={styles.toggleSwitch}>
                  <input
                    type="checkbox"
                    checked={filters.status === 'visualized'}
                    onChange={(e) =>
                      onFilterChange({
                        ...filters,
                        status: e.target.checked ? 'visualized' : undefined,
                      })
                    }
                  />
                  <span className={styles.toggleTrack}>
                    <span className={styles.toggleThumb} />
                  </span>
                </div>
                <span className={styles.toggleText}>✦ AI Visualizations</span>
              </label>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </aside>
  );
};

// ==================== MAIN CATALOG PAGE ====================

const CatalogPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const searchInputRef = useRef<HTMLInputElement>(null);

  // State
  const [books, setBooks] = useState<Book[]>([]);
  const [genres, setGenres] = useState<Genre[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [showMobileFilters, setShowMobileFilters] = useState(false);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>(() => {
    return (localStorage.getItem('nv-catalog-view') as 'grid' | 'list') || 'grid';
  });
  const [searchValue, setSearchValue] = useState('');

  // Parse URL filters
  const getFiltersFromUrl = useCallback((): BookFilters => {
    return {
      search: searchParams.get('search') || undefined,
      genre: searchParams.get('genre') || undefined,
      language: searchParams.get('language') || undefined,
      source: searchParams.get('source') || undefined,
      status: searchParams.get('status') || undefined,
      minPages: searchParams.get('minPages') ? parseInt(searchParams.get('minPages')!) : undefined,
      maxPages: searchParams.get('maxPages') ? parseInt(searchParams.get('maxPages')!) : undefined,
      pageNumber: searchParams.get('page') ? parseInt(searchParams.get('page')!) : 1,
      pageSize: 20,
      sortBy: searchParams.get('sortBy') || undefined,
      sortOrder: (searchParams.get('sortOrder') as 'asc' | 'desc') || 'desc',
    };
  }, [searchParams]);

  const [filters, setFilters] = useState<BookFilters>(getFiltersFromUrl);

  // Sync search input with filter
  useEffect(() => {
    setSearchValue(filters.search || '');
  }, [filters.search]);

  // Persist view mode
  useEffect(() => {
    localStorage.setItem('nv-catalog-view', viewMode);
  }, [viewMode]);

  // URL sync
  const updateUrl = useCallback(
    (newFilters: BookFilters) => {
      const params = new URLSearchParams();
      if (newFilters.search) params.set('search', newFilters.search);
      if (newFilters.genre) params.set('genre', newFilters.genre);
      if (newFilters.language) params.set('language', newFilters.language);
      if (newFilters.source) params.set('source', newFilters.source);
      if (newFilters.status) params.set('status', newFilters.status);
      if (newFilters.minPages) params.set('minPages', newFilters.minPages.toString());
      if (newFilters.maxPages) params.set('maxPages', newFilters.maxPages.toString());
      if (newFilters.pageNumber && newFilters.pageNumber > 1)
        params.set('page', newFilters.pageNumber.toString());
      if (newFilters.sortBy) params.set('sortBy', newFilters.sortBy);
      if (newFilters.sortOrder && newFilters.sortOrder !== 'desc')
        params.set('sortOrder', newFilters.sortOrder);
      setSearchParams(params);
    },
    [setSearchParams]
  );

  // Fetch books
  const fetchBooks = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await catalogService.getBooks(filters);
      setBooks(response.items || []);
      setTotalCount(response.totalCount || 0);
      setTotalPages(response.totalPages || 0);
    } catch (err) {
      console.error('Error fetching books:', err);
      setError('Failed to load books. Please try again.');
      setBooks([]);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  // Fetch genres
  useEffect(() => {
    catalogService.getGenres().then(setGenres);
  }, []);

  // Fetch books on filter change
  useEffect(() => {
    fetchBooks();
  }, [fetchBooks]);

  // Handlers
  const handleFilterChange = useCallback(
    (newFilters: BookFilters) => {
      const updated = { ...newFilters, pageNumber: 1 };
      setFilters(updated);
      updateUrl(updated);
    },
    [updateUrl]
  );

  const handleSearch = useCallback(
    (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      handleFilterChange({ ...filters, search: searchValue.trim() || undefined });
    },
    [filters, searchValue, handleFilterChange]
  );

  const handlePageChange = useCallback(
    (page: number) => {
      const newFilters = { ...filters, pageNumber: page };
      setFilters(newFilters);
      updateUrl(newFilters);
      window.scrollTo({ top: 300, behavior: 'smooth' });
    },
    [filters, updateUrl]
  );

  const handleSortChange = useCallback(
    (sortBy: string) => {
      handleFilterChange({ ...filters, sortBy });
    },
    [filters, handleFilterChange]
  );

  const handleRemoveFilter = useCallback(
    (key: keyof BookFilters) => {
      handleFilterChange({ ...filters, [key]: undefined });
    },
    [filters, handleFilterChange]
  );

  const handleReset = useCallback(() => {
    const resetFilters: BookFilters = { pageNumber: 1, pageSize: 20 };
    setFilters(resetFilters);
    setSearchValue('');
    setSearchParams(new URLSearchParams());
  }, [setSearchParams]);

  const handleGenreQuickSelect = useCallback(
    (genreId: string | undefined) => {
      handleFilterChange({ ...filters, genre: genreId });
    },
    [filters, handleFilterChange]
  );

  // Keyboard shortcut: / focuses search
  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === '/' && !e.ctrlKey && !e.metaKey) {
        const tag = (e.target as HTMLElement).tagName;
        if (tag !== 'INPUT' && tag !== 'TEXTAREA' && tag !== 'SELECT') {
          e.preventDefault();
          searchInputRef.current?.focus();
        }
      }
    };
    window.addEventListener('keydown', handleKey);
    return () => window.removeEventListener('keydown', handleKey);
  }, []);

  // Pagination helper
  const paginationPages = useMemo(() => {
    const current = filters.pageNumber || 1;
    const pages: (number | 'ellipsis')[] = [];

    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current > 3) pages.push('ellipsis');
      const start = Math.max(2, current - 1);
      const end = Math.min(totalPages - 1, current + 1);
      for (let i = start; i <= end; i++) pages.push(i);
      if (current < totalPages - 2) pages.push('ellipsis');
      pages.push(totalPages);
    }
    return pages;
  }, [filters.pageNumber, totalPages]);

  return (
    <div className={styles.page}>
      {/* ==================== HERO HEADER ==================== */}
      <header className={styles.header}>
        <div className={styles.headerBg}>
          <div className={styles.headerOrb1} />
          <div className={styles.headerOrb2} />
          <div className={styles.headerPattern} />
        </div>

        <div className={styles.headerContent}>
          <motion.div
            className={styles.headerText}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <h1 className={styles.title}>
              <span className={styles.titleIcon}>📚</span>
              Library
            </h1>
            <p className={styles.subtitle}>
              Discover{' '}
              <span className={styles.countHighlight}>
                {totalCount > 0 ? totalCount.toLocaleString() : '...'}
              </span>{' '}
              books with AI-powered visualizations
            </p>
          </motion.div>

          {/* Search Bar */}
          <motion.form
            className={styles.searchForm}
            onSubmit={handleSearch}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.15 }}
          >
            <div className={styles.searchWrapper}>
              <span className={styles.searchIcon}>🔍</span>
              <input
                ref={searchInputRef}
                type="text"
                className={styles.searchInput}
                placeholder="Search by title, author, keyword..."
                value={searchValue}
                onChange={(e) => setSearchValue(e.target.value)}
              />
              {searchValue && (
                <button
                  type="button"
                  className={styles.searchClear}
                  onClick={() => {
                    setSearchValue('');
                    handleFilterChange({ ...filters, search: undefined });
                  }}
                >
                  ✕
                </button>
              )}
              <span className={styles.searchHint}>Press /</span>
            </div>
            <button type="submit" className={styles.searchBtn}>
              Search
            </button>
          </motion.form>

          {/* Collection Stats */}
          <motion.div
            className={styles.statsRow}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.35 }}
          >
            <div className={styles.stat}>
              <span className={styles.statIcon}>📖</span>
              <span className={styles.statValue}>{totalCount > 0 ? totalCount.toLocaleString() : '—'}</span>
              <span className={styles.statLabel}>Books</span>
            </div>
            <div className={styles.statDivider} />
            <div className={styles.stat}>
              <span className={styles.statIcon}>🎨</span>
              <span className={styles.statValue}>{genres.length}</span>
              <span className={styles.statLabel}>Genres</span>
            </div>
            <div className={styles.statDivider} />
            <div className={styles.stat}>
              <span className={styles.statIcon}>✦</span>
              <span className={styles.statValue}>AI</span>
              <span className={styles.statLabel}>Visualized</span>
            </div>
            <div className={styles.statDivider} />
            <div className={styles.stat}>
              <span className={styles.statIcon}>📜</span>
              <span className={styles.statValue}>70K+</span>
              <span className={styles.statLabel}>Gutenberg</span>
            </div>
          </motion.div>
        </div>
      </header>

      {/* ==================== GENRE QUICK NAV ==================== */}
      <GenreQuickNav
        genres={genres}
        activeGenre={filters.genre}
        onSelect={handleGenreQuickSelect}
      />

      {/* ==================== MAIN CONTENT ==================== */}
      <div className={styles.content}>
        {/* Mobile Filter Toggle */}
        <button
          className={styles.mobileFilterToggle}
          onClick={() => setShowMobileFilters(true)}
        >
          <span>⚙</span>
          Filters
          {[filters.genre, filters.language, filters.source, filters.status].filter(Boolean).length > 0 && (
            <span className={styles.filterCount}>
              {[filters.genre, filters.language, filters.source, filters.status].filter(Boolean).length}
            </span>
          )}
        </button>

        {/* Desktop Sidebar */}
        <div className={styles.sidebarWrapper}>
          <FilterSidebar
            filters={filters}
            genres={genres}
            onFilterChange={handleFilterChange}
            onReset={handleReset}
          />
        </div>

        {/* Mobile Sidebar Overlay */}
        <AnimatePresence>
          {showMobileFilters && (
            <>
              <motion.div
                className={styles.overlay}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                onClick={() => setShowMobileFilters(false)}
              />
              <motion.div
                className={styles.mobileSidebar}
                initial={{ x: -320 }}
                animate={{ x: 0 }}
                exit={{ x: -320 }}
                transition={{ type: 'spring', damping: 28, stiffness: 300 }}
              >
                <FilterSidebar
                  filters={filters}
                  genres={genres}
                  onFilterChange={(f) => {
                    handleFilterChange(f);
                  }}
                  onReset={() => {
                    handleReset();
                    setShowMobileFilters(false);
                  }}
                  onClose={() => setShowMobileFilters(false)}
                />
              </motion.div>
            </>
          )}
        </AnimatePresence>

        {/* Main Area */}
        <main className={styles.main}>
          {/* Active Filter Tags */}
          <AnimatePresence>
            <ActiveFilterTags
              filters={filters}
              genres={genres}
              onRemove={handleRemoveFilter}
              onReset={handleReset}
            />
          </AnimatePresence>

          {/* Toolbar */}
          <div className={styles.toolbar}>
            <span className={styles.resultCount}>
              {loading ? (
                <span className={styles.resultLoading}>Searching…</span>
              ) : (
                <>
                  <strong>{totalCount.toLocaleString()}</strong> books found
                </>
              )}
            </span>

            <div className={styles.toolbarRight}>
              {/* Sort */}
              <div className={styles.sortGroup}>
                <select
                  value={filters.sortBy || 'createdAt'}
                  onChange={(e) => handleSortChange(e.target.value)}
                  className={styles.sortSelect}
                >
                  <option value="createdAt">Recently Added</option>
                  <option value="title">Title</option>
                  <option value="rating">Rating</option>
                  <option value="downloadCount">Popularity</option>
                  <option value="pageCount">Page Count</option>
                </select>
                <button
                  className={styles.sortOrder}
                  onClick={() =>
                    handleFilterChange({
                      ...filters,
                      sortOrder: filters.sortOrder === 'desc' ? 'asc' : 'desc',
                    })
                  }
                  title={filters.sortOrder === 'desc' ? 'Descending' : 'Ascending'}
                >
                  {filters.sortOrder === 'desc' ? '↓' : '↑'}
                </button>
              </div>

              {/* View Mode Toggle */}
              <div className={styles.viewToggle}>
                <button
                  className={`${styles.viewBtn} ${viewMode === 'grid' ? styles.viewBtnActive : ''}`}
                  onClick={() => setViewMode('grid')}
                  title="Grid view"
                >
                  <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
                    <rect x="1" y="1" width="6" height="6" rx="1" />
                    <rect x="9" y="1" width="6" height="6" rx="1" />
                    <rect x="1" y="9" width="6" height="6" rx="1" />
                    <rect x="9" y="9" width="6" height="6" rx="1" />
                  </svg>
                </button>
                <button
                  className={`${styles.viewBtn} ${viewMode === 'list' ? styles.viewBtnActive : ''}`}
                  onClick={() => setViewMode('list')}
                  title="List view"
                >
                  <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
                    <rect x="1" y="1" width="14" height="3" rx="1" />
                    <rect x="1" y="6" width="14" height="3" rx="1" />
                    <rect x="1" y="11" width="14" height="3" rx="1" />
                  </svg>
                </button>
              </div>
            </div>
          </div>

          {/* Loading Skeletons */}
          {loading && (
            <div className={viewMode === 'grid' ? styles.booksGrid : styles.booksList}>
              {Array.from({ length: 12 }).map((_, i) =>
                viewMode === 'grid' ? (
                  <BookCardSkeleton key={i} index={i} />
                ) : (
                  <ListCardSkeleton key={i} index={i} />
                )
              )}
            </div>
          )}

          {/* Error State */}
          {error && !loading && (
            <motion.div className={styles.errorState} variants={scaleIn} initial="hidden" animate="visible" exit="exit">
              <div className={styles.errorContent}>
                <span className={styles.errorIcon}>⚠️</span>
                <h3>Something went wrong</h3>
                <p>{error}</p>
                <Button variant="primary" onClick={fetchBooks}>
                  Try Again
                </Button>
              </div>
            </motion.div>
          )}

          {/* Empty State */}
          {!loading && !error && books.length === 0 && (
            <motion.div className={styles.emptyState} variants={scaleIn} initial="hidden" animate="visible" exit="exit">
              <div className={styles.emptyContent}>
                <span className={styles.emptyIcon}>📭</span>
                <h3>No books found</h3>
                <p>Try adjusting your filters or search terms</p>
                <Button variant="outline" onClick={handleReset}>
                  Clear All Filters
                </Button>
              </div>
            </motion.div>
          )}

          {/* ==================== BOOKS GRID / LIST ==================== */}
          {!loading && !error && books.length > 0 && (
            <>
              {viewMode === 'grid' ? (
                <motion.div
                  className={styles.booksGrid}
                  key="grid"
                  variants={staggerGrid}
                  initial="hidden"
                  animate="visible"
                >
                  {books.map((book) => (
                    <BookCard
                      key={book.id}
                      book={book}
                      onClick={() => navigate(ROUTES.BOOK.replace(':id', book.id))}
                    />
                  ))}
                </motion.div>
              ) : (
                <motion.div
                  className={styles.booksList}
                  key="list"
                  variants={staggerGrid}
                  initial="hidden"
                  animate="visible"
                >
                  {books.map((book) => (
                    <BookListCard
                      key={book.id}
                      book={book}
                      onClick={() => navigate(ROUTES.BOOK.replace(':id', book.id))}
                    />
                  ))}
                </motion.div>
              )}

              {/* ==================== PAGINATION ==================== */}
              {totalPages > 1 && (
                <div className={styles.pagination}>
                  <button
                    className={styles.pageNavBtn}
                    disabled={(filters.pageNumber || 1) <= 1}
                    onClick={() => handlePageChange((filters.pageNumber || 1) - 1)}
                  >
                    ← Prev
                  </button>

                  <div className={styles.pageNumbers}>
                    {paginationPages.map((p, i) =>
                      p === 'ellipsis' ? (
                        <span key={`e-${i}`} className={styles.pageEllipsis}>
                          …
                        </span>
                      ) : (
                        <button
                          key={p}
                          className={`${styles.pageBtn} ${
                            (filters.pageNumber || 1) === p ? styles.pageBtnActive : ''
                          }`}
                          onClick={() => handlePageChange(p)}
                        >
                          {p}
                        </button>
                      )
                    )}
                  </div>

                  <button
                    className={styles.pageNavBtn}
                    disabled={(filters.pageNumber || 1) >= totalPages}
                    onClick={() => handlePageChange((filters.pageNumber || 1) + 1)}
                  >
                    Next →
                  </button>

                  <span className={styles.pageInfo}>
                    Page {filters.pageNumber || 1} of {totalPages}
                  </span>
                </div>
              )}
            </>
          )}
        </main>
      </div>
    </div>
  );
};

export default CatalogPage;