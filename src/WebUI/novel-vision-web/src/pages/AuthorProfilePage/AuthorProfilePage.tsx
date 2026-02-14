// src/pages/AuthorProfilePage/AuthorProfilePage.tsx

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { Avatar } from '../../shared/ui/Avatar';
import { ROUTES } from '../../shared/constants/routes';
import { catalogService } from '../../services/api/catalog.service';
import { useAuthStore } from '../../store';
import type { Author, Book } from '../../types';
import styles from './AuthorProfilePage.module.css';

interface BookCardProps {
  book: Book;
  onClick: () => void;
}

const BookCard: React.FC<BookCardProps> = ({ book, onClick }) => {
  const [imageError, setImageError] = useState(false);

  return (
    <motion.div whileHover={{ y: -4 }} transition={{ duration: 0.2 }}>
      <Card variant="glass" padding="none" hover clickable onClick={onClick} className={styles.bookCard}>
        <div className={styles.bookCover}>
          {book.coverImageUrl && !imageError ? (
            <img src={book.coverImageUrl} alt={book.title} onError={() => setImageError(true)} />
          ) : (
            <div className={styles.placeholderCover}>
              <span>📖</span>
            </div>
          )}
          {book.hasVisualization && <span className={styles.aiTag}>🎨 AI</span>}
        </div>
        <div className={styles.bookInfo}>
          <h4 className={styles.bookTitle}>{book.title}</h4>
          <div className={styles.bookMeta}>
            {book.genres && book.genres.length > 0 && <span className={styles.genre}>{book.genres[0]}</span>}
            {book.pageCount && <span className={styles.pages}>{book.pageCount} pages</span>}
          </div>
        </div>
      </Card>
    </motion.div>
  );
};

interface SocialLinksProps {
  links?: Record<string, string>;
}

const SocialLinks: React.FC<SocialLinksProps> = ({ links }) => {
  if (!links) return null;

  const socialIcons: Record<string, string> = {
    website: '🌐',
    twitter: '🐦',
    instagram: '📷',
    facebook: '📘',
    goodreads: '📚',
    linkedin: '💼',
    youtube: '📺',
    tiktok: '🎵',
  };

  const entries = Object.entries(links).filter((item) => item[1]);
  if (entries.length === 0) return null;

  return (
    <div className={styles.socialLinks}>
      {entries.map((item) => (<a
        
          key={item[0]}
          href={item[1]}
          target="_blank"
          rel="noopener noreferrer"
          className={styles.socialLink}
          title={item[0].charAt(0).toUpperCase() + item[0].slice(1)}
        >
          {socialIcons[item[0]] || '🔗'}
        </a>
      ))}
    </div>
  );

};

interface StatsProps {
  author: Author;
  bookCount: number;
}

const AuthorStats: React.FC<StatsProps> = ({ author, bookCount }) => {
  const stats = [
    { label: 'Books', value: bookCount || author.bookCount || 0, icon: '📚' },
    { label: 'Followers', value: author.followerCount || 0, icon: '👥' },
  ];

  return (
    <div className={styles.statsGrid}>
      {stats.map((stat) => (
        <div key={stat.label} className={styles.statCard}>
          <span className={styles.statIcon}>{stat.icon}</span>
          <span className={styles.statValue}>{stat.value}</span>
          <span className={styles.statLabel}>{stat.label}</span>
        </div>
      ))}
    </div>
  );
};

const AuthorProfilePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuthStore();

  const [author, setAuthor] = useState<Author | null>(null);
  const [books, setBooks] = useState<Book[]>([]);
  const [loading, setLoading] = useState(true);
  const [booksLoading, setBooksLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isFollowing, setIsFollowing] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [hasMoreBooks, setHasMoreBooks] = useState(false);

  useEffect(() => {
    const fetchAuthor = async () => {
      if (!id) return;
      setLoading(true);
      setError(null);
      try {
        const authorData = await catalogService.getAuthorById(id);
        setAuthor(authorData);
        const booksResponse = await catalogService.getAuthorBooks(id, 1);
        setBooks(booksResponse.items || []);
        setHasMoreBooks((booksResponse.totalPages || 1) > 1);
      } catch (err) {
        console.error('Failed to fetch author:', err);
        setError('Author not found or an error occurred.');
      } finally {
        setLoading(false);
      }
    };
    fetchAuthor();
  }, [id]);

  const loadMoreBooks = async () => {
    if (!id || booksLoading) return;
    setBooksLoading(true);
    try {
      const nextPage = currentPage + 1;
      const response = await catalogService.getAuthorBooks(id, nextPage);
      setBooks((prev) => [...prev, ...(response.items || [])]);
      setCurrentPage(nextPage);
      setHasMoreBooks(nextPage < (response.totalPages || 1));
    } catch (err) {
      console.error('Failed to load more books:', err);
    } finally {
      setBooksLoading(false);
    }
  };

  const handleFollowToggle = () => {
    if (!isAuthenticated) {
      navigate(ROUTES.LOGIN);
      return;
    }
    setIsFollowing(!isFollowing);
  };

  const handleBookClick = (bookId: string) => {
    navigate(ROUTES.BOOK_BY_ID(bookId));
  };

  const isOwnProfile = isAuthenticated && user?.id === author?.userId;

  if (loading) {
    return (
      <div className={styles.loadingPage}>
        <Spinner size="lg" label="Loading author profile..." />
      </div>
    );
  }

  if (error || !author) {
    return (
      <div className={styles.errorPage}>
        <span className={styles.errorIcon}>✍️</span>
        <h2>Author Not Found</h2>
        <p>{error || 'The author you are looking for does not exist.'}</p>
        <Button variant="primary" onClick={() => navigate(ROUTES.AUTHORS)}>
          Browse Authors
        </Button>
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <section className={styles.hero}>
        <div className={styles.heroBackground}>
          <div className={styles.heroOverlay} />
        </div>
        <div className={styles.heroContent}>
          <motion.div className={styles.profileHeader} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}>
            <div className={styles.avatarWrapper}>
              <Avatar src={author.avatarUrl} name={author.displayName || 'Author'} size="xl" className={styles.avatar} />
              {author.isVerified && <span className={styles.verifiedBadge} title="Verified Author">✓</span>}
            </div>
            <div className={styles.profileInfo}>
              <h1 className={styles.authorName}>{author.displayName}</h1>
              {author.email && <p className={styles.authorEmail}>✉️ {author.email}</p>}
              <SocialLinks links={author.socialLinks} />
              {!isOwnProfile && (
                <div className={styles.actions}>
                  <Button variant={isFollowing ? 'outline' : 'primary'} onClick={handleFollowToggle}>
                    {isFollowing ? '✓ Following' : '+ Follow'}
                  </Button>
                </div>
              )}
              {isOwnProfile && (
                <div className={styles.actions}>
                  <Button variant="outline" onClick={() => navigate(ROUTES.AUTHOR_DASHBOARD)}>
                    ✏️ Edit Profile
                  </Button>
                </div>
              )}
            </div>
          </motion.div>
        </div>
      </section>

      <section className={styles.content}>
        <div className={styles.mainColumn}>
          {author.biography && (
            <motion.div className={styles.bioSection} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}>
              <h2 className={styles.sectionTitle}>About</h2>
              <p className={styles.bioText}>{author.biography}</p>
            </motion.div>
          )}

          <motion.div className={styles.booksSection} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
            <h2 className={styles.sectionTitle}>📚 Books ({author.bookCount || books.length})</h2>
            {books.length === 0 ? (
              <div className={styles.noBooksState}>
                <p>No published books yet.</p>
              </div>
            ) : (
              <React.Fragment>
                <div className={styles.booksGrid}>
                  {books.map((book) => (
                    <BookCard key={book.id} book={book} onClick={() => handleBookClick(book.id)} />
                  ))}
                </div>
                {hasMoreBooks && (
                  <div className={styles.loadMore}>
                    <Button variant="outline" onClick={loadMoreBooks} disabled={booksLoading}>
                      {booksLoading ? 'Loading...' : 'Load More Books'}
                    </Button>
                  </div>
                )}
              </React.Fragment>
            )}
          </motion.div>
        </div>

        <aside className={styles.sidebar}>
          <AuthorStats author={author} bookCount={books.length} />
          {author.createdAt && (
            <div className={styles.joinedInfo}>
              <span>📅 Joined {new Date(author.createdAt).toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}</span>
            </div>
          )}
        </aside>
      </section>
    </div>
  );
};

export default AuthorProfilePage;