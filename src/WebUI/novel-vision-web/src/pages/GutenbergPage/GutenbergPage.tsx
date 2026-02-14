// src/pages/GutenbergPage/GutenbergPage.tsx
// Gutenberg search and import page

import React, { useState, useEffect, useCallback, forwardRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { gutenbergService, type GutenbergSearchParams } from '../../services/api/gutenberg.service';
import { useAuthStore } from '../../store';
import type { GutenbergBook } from '../../types';
import styles from './GutenbergPage.module.css';

// ==================== GUTENBERG BOOK CARD ====================

interface GutenbergBookCardProps {
  book: GutenbergBook;
  onImport: (book: GutenbergBook) => void;
  importing: boolean;
  imported: boolean;
}

const GutenbergBookCard = forwardRef<HTMLDivElement, GutenbergBookCardProps>(({
  book,
  onImport,
  importing,
  imported,
}, ref) => {
  const coverUrl = gutenbergService.getCoverUrl(book);
  const [imageError, setImageError] = useState(false);
  const primaryAuthor = book.authors?.[0];
  
  // Format author name - pass object with expected properties

// Format author name - map camelCase to snake_case
const authorName = primaryAuthor
  ? gutenbergService.formatAuthorName({
      name: primaryAuthor.name,
      birth_year: primaryAuthor.birthYear,
      death_year: primaryAuthor.deathYear
    })
  : 'Unknown Author';

// Get life years - map camelCase to snake_case
const lifeYears = primaryAuthor
  ? gutenbergService.getAuthorLifeYears({
      birth_year: primaryAuthor.birthYear,
      death_year: primaryAuthor.deathYear
    })
  : null;

  return (
    <motion.div
      ref={ref}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      whileHover={{ y: -4 }}
    >
      <Card variant="glass" padding="none" className={styles.bookCard}>
        <div className={styles.bookCover}>
          {coverUrl && !imageError ? (
            <img
              src={coverUrl}
              alt={book.title}
              onError={() => setImageError(true)}
            />
          ) : (
            <div className={styles.placeholderCover}>
              <span className={styles.placeholderIcon}>📖</span>
            </div>
          )}
          <span className={styles.gutenbergBadge}>
            #{book.id}
          </span>
          {!book.copyright && (
            <span className={styles.publicDomainBadge}>
              Public Domain
            </span>
          )}
        </div>

        <div className={styles.bookInfo}>
          <h3 className={styles.bookTitle}>{book.title}</h3>
          
          <div className={styles.authorInfo}>
            <span className={styles.authorName}>{authorName}</span>
            {lifeYears && (
              <span className={styles.authorYears}>({lifeYears})</span>
            )}
          </div>

          <div className={styles.bookMeta}>
            {book.languages?.length > 0 && (
              <span className={styles.language}>
                🌍 {book.languages[0].toUpperCase()}
              </span>
            )}
            <span className={styles.downloads}>
              ⬇️ {(book.downloadCount ?? 0).toLocaleString()}
            </span>
          </div>

          {book.subjects?.length > 0 && (
            <div className={styles.subjects}>
              {book.subjects.slice(0, 2).map((subject, i) => (
                <span key={i} className={styles.subjectTag}>
                  {subject.length > 30 ? subject.substring(0, 30) + '...' : subject}
                </span>
              ))}
            </div>
          )}

          <div className={styles.cardActions}>
            <a
              href={gutenbergService.getGutenbergUrl(book.id)}
              target="_blank"
              rel="noopener noreferrer"
              className={styles.viewLink}
            >
              View on Gutenberg ↗
            </a>
            <Button
              variant={imported ? 'success' : 'primary'}
              size="sm"
              loading={importing}
              disabled={importing || imported}
              onClick={() => onImport(book)}
            >
              {imported ? '✓ Imported' : importing ? 'Importing...' : '📥 Import'}
            </Button>
          </div>
        </div>
      </Card>
    </motion.div>
  );
});

GutenbergBookCard.displayName = 'GutenbergBookCard';

// ==================== SEARCH FILTERS ====================

interface SearchFiltersProps {
  filters: GutenbergSearchParams;
  onFilterChange: (filters: GutenbergSearchParams) => void;
}

const SearchFilters: React.FC<SearchFiltersProps> = ({ filters, onFilterChange }) => {
  const languages = [
    { code: '', name: 'All Languages' },
    { code: 'en', name: 'English' },
    { code: 'de', name: 'German' },
    { code: 'fr', name: 'French' },
    { code: 'es', name: 'Spanish' },
    { code: 'it', name: 'Italian' },
    { code: 'pt', name: 'Portuguese' },
    { code: 'nl', name: 'Dutch' },
    { code: 'fi', name: 'Finnish' },
  ];

  return (
    <div className={styles.filters}>
      <select
        value={filters.language || ''}
        onChange={(e) =>
          onFilterChange({ ...filters, language: e.target.value || undefined })
        }
        className={styles.filterSelect}
      >
        {languages.map((lang) => (
          <option key={lang.code} value={lang.code}>
            {lang.name}
          </option>
        ))}
      </select>

      <Input
        placeholder="Author name..."
        value={filters.author || ''}
        onChange={(e) =>
          onFilterChange({ ...filters, author: e.target.value || undefined })
        }
        size="sm"
      />

      <Input
        placeholder="Topic/Subject..."
        value={filters.topic || ''}
        onChange={(e) =>
          onFilterChange({ ...filters, topic: e.target.value || undefined })
        }
        size="sm"
      />
    </div>
  );
};

// ==================== MAIN GUTENBERG PAGE ====================

const GutenbergPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const { isAuthenticated } = useAuthStore();

  // State
  const [books, setBooks] = useState<GutenbergBook[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [importingIds, setImportingIds] = useState<Set<number>>(new Set());
  const [importedIds, setImportedIds] = useState<Set<number>>(new Set());
  const [showFilters, setShowFilters] = useState(false);

  // Get filters from URL
  const getFiltersFromUrl = useCallback((): GutenbergSearchParams => {
    return {
      query: searchParams.get('q') || undefined,
      author: searchParams.get('author') || undefined,
      topic: searchParams.get('topic') || undefined,
      language: searchParams.get('lang') || undefined,
      page: searchParams.get('page') ? parseInt(searchParams.get('page')!) : 1,
    };
  }, [searchParams]);

  const [filters, setFilters] = useState<GutenbergSearchParams>(getFiltersFromUrl);
  const [searchQuery, setSearchQuery] = useState(filters.query || '');

  // Update URL
  const updateUrl = useCallback((newFilters: GutenbergSearchParams) => {
    const params = new URLSearchParams();
    if (newFilters.query) params.set('q', newFilters.query);
    if (newFilters.author) params.set('author', newFilters.author);
    if (newFilters.topic) params.set('topic', newFilters.topic);
    if (newFilters.language) params.set('lang', newFilters.language);
    if (newFilters.page && newFilters.page > 1) params.set('page', newFilters.page.toString());
    setSearchParams(params);
  }, [setSearchParams]);

  // Search books
  const searchBooks = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await gutenbergService.search(filters);
      setBooks(result.results || []);
      setTotalCount(result.count || 0);
    } catch (err) {
      console.error('Error searching Gutenberg:', err);
      setError('Failed to search books. Please try again.');
      setBooks([]);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  // Load popular books on initial load
  useEffect(() => {
    if (!filters.query && !filters.author && !filters.topic) {
      loadPopularBooks();
    } else {
      searchBooks();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filters]);

  const loadPopularBooks = async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await gutenbergService.getPopular(currentPage, filters.language);
      setBooks(result.results || []);
      setTotalCount(result.count || 0);
    } catch (err) {
      console.error('Error loading popular books:', err);
      setError('Failed to load books. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  // Handle search
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const newFilters = { ...filters, query: searchQuery, page: 1 };
    setFilters(newFilters);
    updateUrl(newFilters);
  };

  // Handle filter change
  const handleFilterChange = (newFilters: GutenbergSearchParams) => {
    const updatedFilters = { ...newFilters, page: 1 };
    setFilters(updatedFilters);
    updateUrl(updatedFilters);
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    const newFilters = { ...filters, page };
    setFilters(newFilters);
    setCurrentPage(page);
    updateUrl(newFilters);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Handle import
  const handleImport = async (book: GutenbergBook) => {
    if (!isAuthenticated) {
      // TODO: Show login modal
      alert('Please log in to import books');
      return;
    }

    setImportingIds((prev) => new Set(prev).add(book.id));

    try {
      const result = await gutenbergService.importBook({
        gutenbergId: book.id,
        authorId: '', // Will be auto-created
      });

      if (result.success) {
        setImportedIds((prev) => new Set(prev).add(book.id));
      } else {
        alert(result.error || 'Failed to import book');
      }
    } catch (err) {
      console.error('Error importing book:', err);
      alert('Failed to import book. Please try again.');
    } finally {
      setImportingIds((prev) => {
        const next = new Set(prev);
        next.delete(book.id);
        return next;
      });
    }
  };

  const hasActiveFilters = filters.query || filters.author || filters.topic || filters.language;
  const totalPages = Math.ceil(totalCount / 32);

  return (
    <div className={styles.page}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <motion.div
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
          >
            <span className={styles.headerIcon}>📚</span>
            <h1 className={styles.title}>Project Gutenberg</h1>
            <p className={styles.subtitle}>
              Browse and import over 70,000 free public domain books
            </p>
          </motion.div>

          {/* Search Form */}
          <form className={styles.searchForm} onSubmit={handleSearch}>
            <Input
              placeholder="Search books, authors, topics..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              icon={<span>🔍</span>}
              size="lg"
              fullWidth
            />
            <Button type="submit" variant="primary" size="lg">
              Search
            </Button>
            <Button
              type="button"
              variant="ghost"
              size="lg"
              onClick={() => setShowFilters(!showFilters)}
            >
              🎛️ Filters
            </Button>
          </form>

          {/* Filters */}
          <AnimatePresence>
            {showFilters && (
              <motion.div
                initial={{ height: 0, opacity: 0 }}
                animate={{ height: 'auto', opacity: 1 }}
                exit={{ height: 0, opacity: 0 }}
              >
                <SearchFilters filters={filters} onFilterChange={handleFilterChange} />
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </header>

      {/* Content */}
      <main className={styles.content}>
        {/* Results Header */}
        <div className={styles.resultsHeader}>
          <h2 className={styles.resultsTitle}>
            {hasActiveFilters ? 'Search Results' : 'Popular Books'}
          </h2>
          <span className={styles.resultCount}>
            {loading ? 'Searching...' : `${totalCount.toLocaleString()} books found`}
          </span>
        </div>

        {/* Loading State */}
        {loading && (
          <div className={styles.loadingState}>
            <Spinner size="lg" label="Searching Gutenberg..." />
          </div>
        )}

        {/* Error State */}
        {error && !loading && (
          <div className={styles.errorState}>
            <span className={styles.errorIcon}>⚠️</span>
            <p>{error}</p>
            <Button variant="primary" onClick={searchBooks}>
              Try Again
            </Button>
          </div>
        )}

        {/* Empty State */}
        {!loading && !error && books.length === 0 && (
          <div className={styles.emptyState}>
            <span className={styles.emptyIcon}>📭</span>
            <h3>No books found</h3>
            <p>Try a different search term or adjust your filters</p>
          </div>
        )}

        {/* Books Grid */}
        {!loading && !error && books.length > 0 && (
          <>
            <div className={styles.booksGrid}>
              <AnimatePresence mode="popLayout">
                {books.map((book) => (
                  <GutenbergBookCard
                    key={book.id}
                    book={book}
                    onImport={handleImport}
                    importing={importingIds.has(book.id)}
                    imported={importedIds.has(book.id)}
                  />
                ))}
              </AnimatePresence>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className={styles.pagination}>
                <Button
                  variant="ghost"
                  disabled={currentPage === 1}
                  onClick={() => handlePageChange(currentPage - 1)}
                >
                  ← Previous
                </Button>

                <span className={styles.pageInfo}>
                  Page {currentPage} of {totalPages}
                </span>

                <Button
                  variant="ghost"
                  disabled={currentPage === totalPages}
                  onClick={() => handlePageChange(currentPage + 1)}
                >
                  Next →
                </Button>
              </div>
            )}
          </>
        )}
      </main>

      {/* Info Section */}
      <section className={styles.infoSection}>
        <Card variant="glass" padding="lg" className={styles.infoCard}>
          <h3>About Project Gutenberg</h3>
          <p>
            Project Gutenberg is a library of over 70,000 free eBooks. These are
            primarily older works for which U.S. copyright has expired. Import
            classic literature and enable AI visualizations to experience these
            timeless stories in a whole new way.
          </p>
          <div className={styles.infoFeatures}>
            <div className={styles.infoFeature}>
              <span>📖</span>
              <span>70,000+ Free Books</span>
            </div>
            <div className={styles.infoFeature}>
              <span>🎨</span>
              <span>AI Visualizations</span>
            </div>
            <div className={styles.infoFeature}>
              <span>🌍</span>
              <span>Multiple Languages</span>
            </div>
            <div className={styles.infoFeature}>
              <span>✨</span>
              <span>Public Domain</span>
            </div>
          </div>
        </Card>
      </section>
    </div>
  );
};

export default GutenbergPage;