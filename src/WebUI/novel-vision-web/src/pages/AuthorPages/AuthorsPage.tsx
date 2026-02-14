// src/pages/AuthorsPage/AuthorsPage.tsx
// Authors listing page with search, filters, and pagination

import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { Avatar } from '../../shared/ui/Avatar';
import { ROUTES } from '../../shared/constants/routes';
import { catalogService } from '../../services/api/catalog.service';
import type { Author, PaginatedResponse } from '../../types';
import styles from './AuthorsPage.module.css';

// ==================== AUTHOR CARD COMPONENT ====================

interface AuthorCardProps {
  author: Author;
  onClick: () => void;
}

const AuthorCard: React.FC<AuthorCardProps> = ({ author, onClick }) => {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      whileHover={{ y: -8 }}
      transition={{ duration: 0.3 }}
    >
      <Card
        variant="glass"
        padding="md"
        hover
        clickable
        onClick={onClick}
        className={styles.authorCard}
      >
        <div className={styles.authorAvatar}>
          <Avatar 
            src={author.avatarUrl} 
            name={author.displayName || 'Author'} 
            size="xl"
          />
          {author.isVerified && (
            <span className={styles.verifiedBadge} title="Verified Author">✓</span>
          )}
        </div>
        
        <div className={styles.authorInfo}>
          <h3 className={styles.authorName}>
            {author.displayName}
          </h3>
          
          {author.biography && (
            <p className={styles.authorBio}>{author.biography}</p>
          )}
          
          <div className={styles.authorStats}>
            <span className={styles.stat}>
              📚 {author.bookCount || 0} {author.bookCount === 1 ? 'book' : 'books'}
            </span>
            {author.followerCount !== undefined && author.followerCount > 0 && (
              <span className={styles.stat}>
                👥 {author.followerCount} followers
              </span>
            )}
          </div>
        </div>
      </Card>
    </motion.div>
  );
};

// ==================== FILTERS COMPONENT ====================

interface AuthorFilters {
  search?: string;
  verified?: boolean;
  pageNumber: number;
  pageSize: number;
}

interface FiltersProps {
  filters: AuthorFilters;
  onFilterChange: (filters: AuthorFilters) => void;
}

const AuthorFiltersSection: React.FC<FiltersProps> = ({ filters, onFilterChange }) => {
  return (
    <div className={styles.filtersSection}>
      <label className={styles.checkboxLabel}>
        <input
          type="checkbox"
          checked={filters.verified || false}
          onChange={(e) =>
            onFilterChange({
              ...filters,
              verified: e.target.checked || undefined,
              pageNumber: 1,
            })
          }
        />
        <span className={styles.checkboxCustom} />
        <span>✓ Verified Authors Only</span>
      </label>
    </div>
  );
};

// ==================== MAIN AUTHORS PAGE ====================

const AuthorsPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  // State
  const [authors, setAuthors] = useState<Author[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  // Get filters from URL
  const getFiltersFromUrl = useCallback((): AuthorFilters => {
    return {
      search: searchParams.get('search') || undefined,
      verified: searchParams.get('verified') === 'true' || undefined,
      pageNumber: searchParams.get('page') ? parseInt(searchParams.get('page')!) : 1,
      pageSize: 20,
    };
  }, [searchParams]);

  const [filters, setFilters] = useState<AuthorFilters>(getFiltersFromUrl());

  // Update URL when filters change
  const updateUrl = useCallback((newFilters: AuthorFilters) => {
    const params = new URLSearchParams();
    if (newFilters.search) params.set('search', newFilters.search);
    if (newFilters.verified) params.set('verified', 'true');
    if (newFilters.pageNumber > 1) params.set('page', newFilters.pageNumber.toString());
    setSearchParams(params);
  }, [setSearchParams]);

  // Fetch authors
  const fetchAuthors = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await catalogService.getAuthors({
        search: filters.search,
        verified: filters.verified,
        pageNumber: filters.pageNumber,
        pageSize: filters.pageSize,
      });

      setAuthors(response.items || []);
      setTotalCount(response.totalCount || 0);
      setTotalPages(response.totalPages || 0);
    } catch (err) {
      console.error('Failed to fetch authors:', err);
      setError('Failed to load authors. Please try again.');
      setAuthors([]);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  // Fetch authors when filters change
  useEffect(() => {
    fetchAuthors();
  }, [fetchAuthors]);

  // Handle filter changes
  const handleFilterChange = (newFilters: AuthorFilters) => {
    setFilters(newFilters);
    updateUrl(newFilters);
  };

  // Handle search
  const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const search = formData.get('search') as string;
    handleFilterChange({ ...filters, search: search || undefined, pageNumber: 1 });
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    const newFilters = { ...filters, pageNumber: page };
    setFilters(newFilters);
    updateUrl(newFilters);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Navigate to author profile
  const handleAuthorClick = (authorId: string) => {
    navigate(ROUTES.AUTHOR_BY_ID(authorId));
  };

  // Clear all filters
  const clearFilters = () => {
    const defaultFilters: AuthorFilters = { pageNumber: 1, pageSize: 20 };
    setFilters(defaultFilters);
    setSearchParams({});
  };

  const hasActiveFilters = filters.search || filters.verified;

  return (
    <div className={styles.page}>
      {/* Hero Section */}
      <section className={styles.hero}>
        <div className={styles.heroContent}>
          <motion.h1
            className={styles.title}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
          >
            ✍️ Discover Authors
          </motion.h1>
          <motion.p
            className={styles.subtitle}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
          >
            Connect with talented writers and explore their works
          </motion.p>

          {/* Search Form */}
          <motion.form
            className={styles.searchForm}
            onSubmit={handleSearch}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
          >
            <Input
              name="search"
              type="text"
              placeholder="Search authors by name..."
              defaultValue={filters.search || ''}
              className={styles.searchInput}
            />
            <Button type="submit" variant="primary">
              🔍 Search
            </Button>
          </motion.form>
        </div>
      </section>

      {/* Main Content */}
      <section className={styles.content}>
        <div className={styles.contentHeader}>
          <div className={styles.resultsInfo}>
            <h2>
              {loading ? 'Loading...' : `${totalCount} Author${totalCount !== 1 ? 's' : ''} Found`}
            </h2>
            {hasActiveFilters && (
              <button className={styles.clearFilters} onClick={clearFilters}>
                ✕ Clear Filters
              </button>
            )}
          </div>

          <AuthorFiltersSection filters={filters} onFilterChange={handleFilterChange} />
        </div>

        {/* Authors Grid */}
        {loading ? (
          <div className={styles.loadingState}>
            <Spinner size="lg" label="Loading authors..." />
          </div>
        ) : error ? (
          <div className={styles.errorState}>
            <span className={styles.errorIcon}>⚠️</span>
            <p>{error}</p>
            <Button variant="outline" onClick={fetchAuthors}>
              Try Again
            </Button>
          </div>
        ) : authors.length === 0 ? (
          <div className={styles.emptyState}>
            <span className={styles.emptyIcon}>✍️</span>
            <h3>No Authors Found</h3>
            <p>
              {hasActiveFilters
                ? 'Try adjusting your search criteria'
                : 'No authors have joined yet'}
            </p>
            {hasActiveFilters && (
              <Button variant="outline" onClick={clearFilters}>
                Clear Filters
              </Button>
            )}
          </div>
        ) : (
          <>
            <motion.div 
              className={styles.authorsGrid}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
            >
              <AnimatePresence mode="popLayout">
                {authors.map((author) => (
                  <AuthorCard
                    key={author.id}
                    author={author}
                    onClick={() => handleAuthorClick(author.id)}
                  />
                ))}
              </AnimatePresence>
            </motion.div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className={styles.pagination}>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={filters.pageNumber <= 1}
                  onClick={() => handlePageChange(filters.pageNumber - 1)}
                >
                  ← Previous
                </Button>
                
                <div className={styles.pageNumbers}>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum: number;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (filters.pageNumber <= 3) {
                      pageNum = i + 1;
                    } else if (filters.pageNumber >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = filters.pageNumber - 2 + i;
                    }
                    return (
                      <button
                        key={pageNum}
                        className={`${styles.pageNumber} ${
                          pageNum === filters.pageNumber ? styles.active : ''
                        }`}
                        onClick={() => handlePageChange(pageNum)}
                      >
                        {pageNum}
                      </button>
                    );
                  })}
                </div>

                <Button
                  variant="outline"
                  size="sm"
                  disabled={filters.pageNumber >= totalPages}
                  onClick={() => handlePageChange(filters.pageNumber + 1)}
                >
                  Next →
                </Button>
              </div>
            )}
          </>
        )}
      </section>
    </div>
  );
};

export default AuthorsPage;