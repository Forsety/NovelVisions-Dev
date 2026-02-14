// src/pages/BookmarksPage/BookmarksPage.tsx

import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { useToast } from '../../shared/ui/Toast';
import { ROUTES } from '../../shared/constants/routes';
import { readingService, type BookmarkWithDetails } from '../../services/api/reading.service';
import { useAuthStore } from '../../store';
import styles from './BookmarksPage.module.css';

// ==================== BOOKMARK CARD ====================

interface BookmarkCardProps {
  bookmark: BookmarkWithDetails;
  onNavigate: () => void;
  onEdit: () => void;
  onDelete: () => void;
  isDeleting: boolean;
  onCancelDelete: () => void;
}

const BookmarkCard: React.FC<BookmarkCardProps> = ({ 
  bookmark, 
  onNavigate, 
  onEdit, 
  onDelete,
  isDeleting,
  onCancelDelete,
}) => {
  const [showActions, setShowActions] = useState(false);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, x: -20 }}
      className={`${styles.bookmarkCard} ${isDeleting ? styles.deleting : ''}`}
      onMouseEnter={() => setShowActions(true)}
      onMouseLeave={() => { setShowActions(false); onCancelDelete(); }}
    >
      <div className={styles.bookmarkColor} style={{ backgroundColor: bookmark.color || '#8b5cf6' }} />
      
      <div className={styles.bookmarkContent} onClick={onNavigate}>
        <div className={styles.bookmarkHeader}>
          <span className={styles.pageNumber}>Page {bookmark.pageNumber}</span>
          {bookmark.chapterTitle && (
            <span className={styles.chapterTitle}>{bookmark.chapterTitle}</span>
          )}
          <span className={styles.bookmarkDate}>{formatDate(bookmark.createdAt)}</span>
        </div>
        
        {bookmark.note && (
          <p className={styles.bookmarkNote}>{bookmark.note}</p>
        )}
        
        {bookmark.pageContent && (
          <p className={styles.bookmarkPreview}>"{bookmark.pageContent.slice(0, 150)}..."</p>
        )}
      </div>

      <AnimatePresence>
        {showActions && (
          <motion.div
            className={styles.bookmarkActions}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            {isDeleting ? (
              <>
                <button 
                  className={`${styles.actionBtn} ${styles.confirmDeleteBtn}`} 
                  onClick={onDelete} 
                  title="Click to confirm delete"
                >
                  ✓ Confirm
                </button>
                <button 
                  className={styles.actionBtn} 
                  onClick={onCancelDelete} 
                  title="Cancel"
                >
                  ✕
                </button>
              </>
            ) : (
              <>
                <button className={styles.actionBtn} onClick={onEdit} title="Edit">✏️</button>
                <button className={styles.actionBtn} onClick={onDelete} title="Delete">🗑️</button>
              </>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </motion.div>
  );
};

// ==================== BOOK GROUP ====================

interface BookGroupProps {
  bookId: string;
  bookTitle: string;
  bookCoverUrl?: string;
  bookmarks: BookmarkWithDetails[];
  onNavigate: (bookmark: BookmarkWithDetails) => void;
  onEdit: (bookmark: BookmarkWithDetails) => void;
  onDelete: (bookmark: BookmarkWithDetails) => void;
  deletingId: string | null;
  onSetDeleting: (id: string | null) => void;
}

const BookGroup: React.FC<BookGroupProps> = ({
  bookId,
  bookTitle,
  bookCoverUrl,
  bookmarks,
  onNavigate,
  onEdit,
  onDelete,
  deletingId,
  onSetDeleting,
}) => {
  const [expanded, setExpanded] = useState(true);
  const navigate = useNavigate();

  const handleDeleteClick = (bookmark: BookmarkWithDetails) => {
    if (deletingId === bookmark.id) {
      // Second click - confirm delete
      onDelete(bookmark);
      onSetDeleting(null);
    } else {
      // First click - show confirmation
      onSetDeleting(bookmark.id);
    }
  };

  return (
    <Card variant="glass" padding="md" className={styles.bookGroup}>
      <div className={styles.bookGroupHeader} onClick={() => setExpanded(!expanded)}>
        <div className={styles.bookInfo}>
          {bookCoverUrl ? (
            <img src={bookCoverUrl} alt={bookTitle} className={styles.bookCover} />
          ) : (
            <div className={styles.bookCoverPlaceholder}>📖</div>
          )}
          <div>
            <h3 className={styles.bookTitle}>{bookTitle}</h3>
            <span className={styles.bookmarkCount}>
              {bookmarks.length} bookmark{bookmarks.length !== 1 ? 's' : ''}
            </span>
          </div>
        </div>
        <div className={styles.bookGroupActions}>
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              navigate(ROUTES.BOOK_BY_ID(bookId));
            }}
          >
            View Book
          </Button>
          <span className={`${styles.expandIcon} ${expanded ? styles.expanded : ''}`}>▼</span>
        </div>
      </div>

      <AnimatePresence>
        {expanded && (
          <motion.div
            className={styles.bookmarksList}
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
          >
            {bookmarks.map((bookmark) => (
              <BookmarkCard
                key={bookmark.id}
                bookmark={bookmark}
                onNavigate={() => onNavigate(bookmark)}
                onEdit={() => onEdit(bookmark)}
                onDelete={() => handleDeleteClick(bookmark)}
                isDeleting={deletingId === bookmark.id}
                onCancelDelete={() => onSetDeleting(null)}
              />
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </Card>
  );
};

// ==================== EDIT MODAL ====================

interface EditModalProps {
  bookmark: BookmarkWithDetails;
  onSave: (note: string, color: string) => void;
  onClose: () => void;
}

const EditModal: React.FC<EditModalProps> = ({ bookmark, onSave, onClose }) => {
  const [note, setNote] = useState(bookmark.note || '');
  const [color, setColor] = useState(bookmark.color || '#8b5cf6');

  const colors = ['#8b5cf6', '#ef4444', '#f59e0b', '#10b981', '#3b82f6', '#ec4899'];

  return (
    <div className={styles.modalOverlay} onClick={onClose}>
      <motion.div
        className={styles.modal}
        onClick={(e) => e.stopPropagation()}
        initial={{ scale: 0.9, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
      >
        <h3 className={styles.modalTitle}>Edit Bookmark</h3>
        
        <div className={styles.modalField}>
          <label>Note</label>
          <textarea
            value={note}
            onChange={(e) => setNote(e.target.value)}
            placeholder="Add a note..."
            rows={3}
          />
        </div>

        <div className={styles.modalField}>
          <label>Color</label>
          <div className={styles.colorPicker}>
            {colors.map((c) => (
              <button
                key={c}
                className={`${styles.colorOption} ${color === c ? styles.selected : ''}`}
                style={{ backgroundColor: c }}
                onClick={() => setColor(c)}
              />
            ))}
          </div>
        </div>

        <div className={styles.modalActions}>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button variant="primary" onClick={() => onSave(note, color)}>Save</Button>
        </div>
      </motion.div>
    </div>
  );
};

// ==================== MAIN PAGE ====================

const BookmarksPage: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuthStore();
  const toast = useToast();

  const [bookmarks, setBookmarks] = useState<BookmarkWithDetails[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [editingBookmark, setEditingBookmark] = useState<BookmarkWithDetails | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate(ROUTES.LOGIN);
    }
  }, [isAuthenticated, navigate]);

  const fetchBookmarks = useCallback(async () => {
    setLoading(true);
    try {
      const response = await readingService.getAllBookmarks(1, 100);
      setBookmarks(response.items || []);
    } catch (err) {
      console.error('Failed to fetch bookmarks:', err);
      toast.error('Failed to load bookmarks');
    } finally {
      setLoading(false);
    }
  }, [toast]);

  useEffect(() => {
    if (isAuthenticated) {
      fetchBookmarks();
    }
  }, [isAuthenticated, fetchBookmarks]);

  const handleSearch = async () => {
    if (!searchQuery.trim()) {
      fetchBookmarks();
      return;
    }
    setLoading(true);
    try {
      const results = await readingService.searchBookmarks(searchQuery);
      setBookmarks(results);
    } catch (err) {
      console.error('Failed to search bookmarks:', err);
      toast.error('Search failed');
    } finally {
      setLoading(false);
    }
  };

  const handleNavigate = (bookmark: BookmarkWithDetails) => {
    navigate(`${ROUTES.READER_BY_ID(bookmark.bookId)}?page=${bookmark.pageNumber}`);
  };

  const handleEdit = (bookmark: BookmarkWithDetails) => {
    setEditingBookmark(bookmark);
  };

  const handleSaveEdit = async (note: string, color: string) => {
    if (!editingBookmark) return;
    try {
      await readingService.updateBookmark(editingBookmark.bookId, editingBookmark.id, { note, color });
      setBookmarks((prev) =>
        prev.map((b) => (b.id === editingBookmark.id ? { ...b, note, color } : b))
      );
      toast.success('Bookmark updated');
      setEditingBookmark(null);
    } catch (err) {
      console.error('Failed to update bookmark:', err);
      toast.error('Failed to update bookmark');
    }
  };

  const handleDelete = async (bookmark: BookmarkWithDetails) => {
    try {
      await readingService.deleteBookmark(bookmark.bookId, bookmark.id);
      setBookmarks((prev) => prev.filter((b) => b.id !== bookmark.id));
      toast.success('Bookmark deleted');
    } catch (err) {
      console.error('Failed to delete bookmark:', err);
      toast.error('Failed to delete bookmark');
    }
  };

  // Group bookmarks by book
  const groupedBookmarks = bookmarks.reduce((groups, bookmark) => {
    const key = bookmark.bookId;
    if (!groups[key]) {
      groups[key] = {
        bookId: bookmark.bookId,
        bookTitle: bookmark.bookTitle || 'Unknown Book',
        bookCoverUrl: bookmark.bookCoverUrl,
        bookmarks: [],
      };
    }
    groups[key].bookmarks.push(bookmark);
    return groups;
  }, {} as Record<string, { bookId: string; bookTitle: string; bookCoverUrl?: string; bookmarks: BookmarkWithDetails[] }>);

  const bookGroups = Object.values(groupedBookmarks);

  if (loading) {
    return (
      <div className={styles.loadingPage}>
        <Spinner size="lg" label="Loading bookmarks..." />
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <div className={styles.container}>
        {/* Header */}
        <motion.div className={styles.header} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}>
          <h1 className={styles.title}>🔖 My Bookmarks</h1>
          <p className={styles.subtitle}>
            {bookmarks.length} bookmark{bookmarks.length !== 1 ? 's' : ''} across {bookGroups.length} book{bookGroups.length !== 1 ? 's' : ''}
          </p>
        </motion.div>

        {/* Search */}
        <motion.div className={styles.searchBar} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}>
          <Input
            type="text"
            placeholder="Search bookmarks..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            className={styles.searchInput}
          />
          <Button variant="primary" onClick={handleSearch}>🔍 Search</Button>
          {searchQuery && (
            <Button variant="ghost" onClick={() => { setSearchQuery(''); fetchBookmarks(); }}>Clear</Button>
          )}
        </motion.div>

        {/* Content */}
        {bookmarks.length === 0 ? (
          <motion.div className={styles.emptyState} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
            <span className={styles.emptyIcon}>🔖</span>
            <h3>No Bookmarks Yet</h3>
            <p>Start reading and add bookmarks to save your favorite passages</p>
            <Button variant="primary" onClick={() => navigate(ROUTES.CATALOG)}>
              Browse Library
            </Button>
          </motion.div>
        ) : (
          <motion.div className={styles.bookGroups} initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 0.2 }}>
            {bookGroups.map((group) => (
              <BookGroup
                key={group.bookId}
                bookId={group.bookId}
                bookTitle={group.bookTitle}
                bookCoverUrl={group.bookCoverUrl}
                bookmarks={group.bookmarks}
                onNavigate={handleNavigate}
                onEdit={handleEdit}
                onDelete={handleDelete}
                deletingId={deletingId}
                onSetDeleting={setDeletingId}
              />
            ))}
          </motion.div>
        )}
      </div>

      {/* Edit Modal */}
      {editingBookmark && (
        <EditModal
          bookmark={editingBookmark}
          onSave={handleSaveEdit}
          onClose={() => setEditingBookmark(null)}
        />
      )}
    </div>
  );
};

export default BookmarksPage;