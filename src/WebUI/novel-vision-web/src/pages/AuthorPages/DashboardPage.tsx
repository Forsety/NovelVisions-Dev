// src/pages/AuthorPages/DashboardPage.tsx
// Author Dashboard - FIXED: Removed double role check (ProtectedRoute handles it)

import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { ROUTES } from '../../shared/constants/routes';
import { useAuthStore } from '../../store';
import { catalogService } from '../../services/api';
import type { Book } from '../../types';
import styles from './DashboardPage.module.css';

// ==================== STATS CARD ====================

interface StatCardProps {
  title: string;
  value: number | string;
  icon: string;
  trend?: number;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, trend }) => (
  <Card variant="glass" padding="md" className={styles.statCard}>
    <div className={styles.statIcon}>{icon}</div>
    <div className={styles.statContent}>
      <span className={styles.statValue}>{value}</span>
      <span className={styles.statTitle}>{title}</span>
    </div>
    {trend !== undefined && (
      <span className={`${styles.statTrend} ${trend >= 0 ? styles.positive : styles.negative}`}>
        {trend >= 0 ? '↑' : '↓'} {Math.abs(trend)}%
      </span>
    )}
  </Card>
);

// ==================== BOOKS TABLE ====================

interface BooksTableProps {
  books: Book[];
  onEdit: (book: Book) => void;
  onDelete: (bookId: string) => void;
  onPublish: (book: Book) => void;
}

const BooksTable: React.FC<BooksTableProps> = ({ books, onEdit, onDelete, onPublish }) => {
  const navigate = useNavigate();
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);

  const handleDelete = (bookId: string) => {
    if (deleteConfirm === bookId) {
      onDelete(bookId);
      setDeleteConfirm(null);
    } else {
      setDeleteConfirm(bookId);
      setTimeout(() => setDeleteConfirm(null), 3000);
    }
  };

  if (books.length === 0) {
    return (
      <div className={styles.emptyBooks}>
        <span className={styles.emptyIcon}>📚</span>
        <h3>No Books Yet</h3>
        <p>Create your first book to get started!</p>
      </div>
    );
  }

  return (
    <div className={styles.tableWrapper}>
      <table className={styles.booksTable}>
        <thead>
          <tr>
            <th>Book</th>
            <th>Status</th>
            <th>Chapters</th>
            <th>Views</th>
            <th>Rating</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {books.map((book) => (
            <tr key={book.id}>
              <td>
                <div className={styles.bookCell}>
                  {book.coverImageUrl ? (
                    <img src={book.coverImageUrl} alt={book.title} className={styles.bookThumb} />
                  ) : (
                    <div className={styles.bookThumbPlaceholder}>📖</div>
                  )}
                  <div className={styles.bookInfo}>
                    <span 
                      className={styles.bookTitle} 
                      onClick={() => navigate(ROUTES.BOOK_BY_ID(book.id))}
                    >
                      {book.title}
                    </span>
                    <span className={styles.bookMeta}>
                      {book.genres?.join(', ') || 'No genre'}
                    </span>
                  </div>
                </div>
              </td>
              <td>
                <span className={`${styles.statusBadge} ${book.isPublished ? styles.published : styles.draft}`}>
                  {book.isPublished ? '✓ Published' : '📝 Draft'}
                </span>
              </td>
              <td>{book.chapterCount || 0}</td>
              <td>{book.viewCount || 0}</td>
              <td>
                {book.rating ? (
                  <span className={styles.rating}>⭐ {book.rating.toFixed(1)}</span>
                ) : (
                  <span className={styles.noRating}>—</span>
                )}
              </td>
              <td>
                <div className={styles.actions}>
                  <button 
                    className={styles.actionBtn} 
                    onClick={() => onEdit(book)}
                    title="Edit"
                  >
                    ✏️
                  </button>
                  <button 
                    className={styles.actionBtn} 
                    onClick={() => onPublish(book)}
                    title={book.isPublished ? 'Unpublish' : 'Publish'}
                  >
                    {book.isPublished ? '📤' : '📥'}
                  </button>
                  <button 
                    className={`${styles.actionBtn} ${styles.deleteBtn} ${
                      deleteConfirm === book.id ? styles.confirm : ''
                    }`}
                    onClick={() => handleDelete(book.id)}
                    title={deleteConfirm === book.id ? 'Click again to confirm' : 'Delete'}
                  >
                    {deleteConfirm === book.id ? '✓' : '🗑️'}
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

// ==================== QUICK ACTIONS ====================

const QuickActions: React.FC<{ onCreateBook: () => void }> = ({ onCreateBook }) => {
  const navigate = useNavigate();
  
  return (
    <Card variant="gradient" padding="lg" className={styles.quickActions}>
      <h3 className={styles.quickTitle}>Quick Actions</h3>
      <div className={styles.quickButtons}>
        <Button variant="primary" onClick={onCreateBook}>
          ➕ New Book
        </Button>
        <Button variant="outline" onClick={() => navigate(ROUTES.GUTENBERG)}>
          📥 Import from Gutenberg
        </Button>
        <Button variant="ghost">
          📊 View Analytics
        </Button>
      </div>
    </Card>
  );
};

// ==================== MAIN DASHBOARD ====================

const DashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const [books, setBooks] = useState<Book[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ============================================================
  // NO REDIRECT useEffect HERE! ProtectedRoute already handles it
  // ============================================================

  // Fetch author's books - with protection from multiple calls
  useEffect(() => {
    let isMounted = true;
    
    const fetchBooks = async () => {
      if (!user?.id) return;

      setLoading(true);
      setError(null);

      try {
        const response = await catalogService.getBooks({
          authorId: user.id,
          pageSize: 50,
        });
        if (isMounted) {
          setBooks(response.items || []);
        }
      } catch (err) {
        console.error('Error fetching books:', err);
        if (isMounted) {
          setError('Failed to load your books');
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    fetchBooks();
    
    return () => {
      isMounted = false;
    };
  }, [user?.id]);

  const handleCreateBook = () => {
    navigate(ROUTES.CREATE_BOOK);
  };

  const handleEditBook = (book: Book) => {
    navigate(ROUTES.EDIT_BOOK_BY_ID(book.id));
  };

  const handleDeleteBook = async (bookId: string) => {
    try {
      await catalogService.deleteBook(bookId);
      setBooks((prev) => prev.filter((b) => b.id !== bookId));
    } catch (err) {
      console.error('Error deleting book:', err);
      setError('Failed to delete book');
    }
  };

  const handlePublishBook = async (book: Book) => {
    try {
      if (book.isPublished) {
        await catalogService.unpublishBook(book.id);
      } else {
        await catalogService.publishBook(book.id);
      }
      setBooks((prev) =>
        prev.map((b) =>
          b.id === book.id ? { ...b, isPublished: !book.isPublished } : b
        )
      );
    } catch (err) {
      console.error('Error publishing book:', err);
      setError('Failed to update book status');
    }
  };

  // Computed stats
  const computedStats = {
    totalBooks: books.length,
    totalViews: books.reduce((sum, b) => sum + (b.viewCount || 0), 0),
    totalReaders: books.reduce((sum, b) => sum + (b.downloadCount || 0), 0),
    avgRating: books.length > 0
      ? books.reduce((sum, b) => sum + (b.rating || 0), 0) / books.length
      : 0,
  };

  if (loading) {
    return (
      <div className={styles.loadingPage}>
        <Spinner size="lg" label="Loading dashboard..." />
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <div className={styles.container}>
        {/* Header */}
        <motion.div
          className={styles.header}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
        >
          <div className={styles.headerContent}>
            <h1 className={styles.title}>Author Dashboard</h1>
            <p className={styles.subtitle}>
              Welcome back, {user?.firstName || user?.displayName || 'Author'}! 
              Manage your books and track your performance.
            </p>
          </div>
          <Button variant="primary" size="lg" onClick={handleCreateBook}>
            ➕ Create New Book
          </Button>
        </motion.div>

        {/* Stats */}
        <motion.div
          className={styles.statsGrid}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
        >
          <StatCard title="Total Books" value={computedStats.totalBooks} icon="📚" />
          <StatCard title="Total Views" value={computedStats.totalViews} icon="👁️" trend={12} />
          <StatCard title="Total Readers" value={computedStats.totalReaders} icon="👥" trend={8} />
          <StatCard 
            title="Avg. Rating" 
            value={computedStats.avgRating > 0 ? computedStats.avgRating.toFixed(1) : '—'} 
            icon="⭐" 
          />
        </motion.div>

        {/* Quick Actions */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
        >
          <QuickActions onCreateBook={handleCreateBook} />
        </motion.div>

        {/* Error Message */}
        {error && (
          <div className={styles.errorMessage}>
            <span>⚠️ {error}</span>
            <button onClick={() => setError(null)}>✕</button>
          </div>
        )}

        {/* Books Section */}
        <motion.div
          className={styles.booksSection}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
        >
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>📖 Your Books</h2>
            <span className={styles.bookCount}>{books.length} books</span>
          </div>
          
          <Card variant="glass" padding="none">
            <BooksTable
              books={books}
              onEdit={handleEditBook}
              onDelete={handleDeleteBook}
              onPublish={handlePublishBook}
            />
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default DashboardPage;