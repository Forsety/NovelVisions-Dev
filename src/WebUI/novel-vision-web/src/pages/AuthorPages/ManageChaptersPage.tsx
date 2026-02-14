// src/pages/AuthorPages/ManageChaptersPage.tsx
// NovelVision Book Content Editor — Chapters, Pages, Summaries
// Professional diploma-quality full-featured editor

import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Spinner } from '../../shared/ui/Spinner';
import { Button } from '../../shared/ui/Button';
import { ROUTES } from '../../shared/constants/routes';
import { useAuthStore } from '../../store';
import { catalogService } from '../../services/api/catalog.service';
import type { Book, Chapter, Page } from '../../types';
import styles from './ManageChaptersPage.module.css';

// ==================== TYPES ====================

type EditorTab = 'summary' | 'pages';
type ToastType = 'success' | 'error' | 'info';
type SaveStatus = 'saved' | 'saving' | 'unsaved';

interface ToastMessage {
  id: number;
  text: string;
  type: ToastType;
}

interface ChapterDraft {
  title: string;
  summary: string;
}

interface PageDraft {
  content: string;
}

// ==================== HELPER: Word count ====================

const countWords = (text: string): number => {
  if (!text || !text.trim()) return 0;
  return text.trim().split(/\s+/).length;
};

// ==================== HELPER: Reading time estimate ====================

const readingTime = (wordCount: number): string => {
  const minutes = Math.ceil(wordCount / 230);
  if (minutes < 1) return '< 1 мин';
  return `~${minutes} мин`;
};

// ==================== ADD CHAPTER MODAL ====================

interface AddChapterModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (title: string, summary: string) => void;
  loading: boolean;
}

const AddChapterModal: React.FC<AddChapterModalProps> = ({ isOpen, onClose, onSubmit, loading }) => {
  const [title, setTitle] = useState('');
  const [summary, setSummary] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isOpen) {
      setTitle('');
      setSummary('');
      setTimeout(() => inputRef.current?.focus(), 100);
    }
  }, [isOpen]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;
    onSubmit(title.trim(), summary.trim());
  };

  if (!isOpen) return null;

  return (
    <motion.div
      className={styles.modalOverlay}
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      onClick={onClose}
    >
      <motion.div
        className={styles.modal}
        initial={{ opacity: 0, scale: 0.95, y: 20 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.95, y: 20 }}
        onClick={(e) => e.stopPropagation()}
      >
        <h3 className={styles.modalTitle}>📝 Новая глава</h3>
        <form onSubmit={handleSubmit}>
          <div className={styles.modalField}>
            <label className={styles.modalLabel}>Название главы *</label>
            <input
              ref={inputRef}
              type="text"
              className={styles.modalInput}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Например: Глава 1 — Начало пути"
              maxLength={200}
            />
          </div>
          <div className={styles.modalField}>
            <label className={styles.modalLabel}>Краткое описание</label>
            <textarea
              className={styles.modalTextarea}
              value={summary}
              onChange={(e) => setSummary(e.target.value)}
              placeholder="О чём эта глава? (необязательно)"
              maxLength={500}
            />
          </div>
          <div className={styles.modalActions}>
            <button type="button" className={styles.modalBtnCancel} onClick={onClose}>
              Отмена
            </button>
            <button
              type="submit"
              className={styles.modalBtnSubmit}
              disabled={!title.trim() || loading}
            >
              {loading ? 'Создание...' : 'Создать главу'}
            </button>
          </div>
        </form>
      </motion.div>
    </motion.div>
  );
};

// ==================== DELETE CONFIRM MODAL ====================

interface DeleteModalProps {
  isOpen: boolean;
  entityName: string;
  entityType: 'chapter' | 'page';
  onClose: () => void;
  onConfirm: () => void;
  loading: boolean;
}

const DeleteModal: React.FC<DeleteModalProps> = ({
  isOpen, entityName, entityType, onClose, onConfirm, loading
}) => {
  if (!isOpen) return null;

  const typeLabel = entityType === 'chapter' ? 'главу' : 'страницу';

  return (
    <motion.div
      className={styles.modalOverlay}
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      onClick={onClose}
    >
      <motion.div
        className={`${styles.modal} ${styles.deleteModal}`}
        initial={{ opacity: 0, scale: 0.95, y: 20 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.95, y: 20 }}
        onClick={(e) => e.stopPropagation()}
      >
        <div className={styles.deleteModalIcon}>🗑️</div>
        <h3 className={styles.modalTitle}>Удалить {typeLabel}?</h3>
        <p className={styles.deleteModalText}>
          Вы уверены, что хотите удалить{' '}
          <span className={styles.deleteModalName}>{entityName}</span>?
          Это действие нельзя отменить.
        </p>
        <div className={styles.modalActions}>
          <button type="button" className={styles.modalBtnCancel} onClick={onClose}>
            Отмена
          </button>
          <button
            type="button"
            className={styles.modalBtnDelete}
            onClick={onConfirm}
            disabled={loading}
          >
            {loading ? 'Удаление...' : 'Удалить'}
          </button>
        </div>
      </motion.div>
    </motion.div>
  );
};

// ==================== TOAST COMPONENT ====================

const Toast: React.FC<{ message: ToastMessage }> = ({ message }) => {
  const typeClass =
    message.type === 'success' ? styles.toastSuccess :
    message.type === 'error' ? styles.toastError : styles.toastInfo;

  return (
    <motion.div
      className={`${styles.toast} ${typeClass}`}
      initial={{ opacity: 0, y: 20, x: 20 }}
      animate={{ opacity: 1, y: 0, x: 0 }}
      exit={{ opacity: 0, y: 20, x: 20 }}
    >
      {message.type === 'success' && '✓ '}
      {message.type === 'error' && '✗ '}
      {message.type === 'info' && 'ℹ '}
      {message.text}
    </motion.div>
  );
};

// ==================== MAIN COMPONENT ====================

const ManageChaptersPage: React.FC = () => {
  const { id: bookId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();

  // ---- Data state ----
  const [book, setBook] = useState<Book | null>(null);
  const [chapters, setChapters] = useState<Chapter[]>([]);
  const [pages, setPages] = useState<Page[]>([]);

  // ---- UI state ----
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedChapterId, setSelectedChapterId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<EditorTab>('pages');
  const [currentPageIndex, setCurrentPageIndex] = useState(0);
  const [pagesLoading, setPagesLoading] = useState(false);

  // ---- Edit state ----
  const [chapterDraft, setChapterDraft] = useState<ChapterDraft>({ title: '', summary: '' });
  const [pageDraft, setPageDraft] = useState<PageDraft>({ content: '' });
  const [saveStatus, setSaveStatus] = useState<SaveStatus>('saved');

  // ---- Modal state ----
  const [showAddChapter, setShowAddChapter] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<{ type: 'chapter' | 'page'; id: string; name: string } | null>(null);
  const [modalLoading, setModalLoading] = useState(false);

  // ---- Toast state ----
  const [toasts, setToasts] = useState<ToastMessage[]>([]);
  const toastIdRef = useRef(0);

  // ---- Auto-save timer ----
  const saveTimerRef = useRef<NodeJS.Timeout | null>(null);
  const lastSavedContentRef = useRef<string>('');
  const lastSavedTitleRef = useRef<string>('');
  const lastSavedSummaryRef = useRef<string>('');

  // ==================== TOAST HELPER ====================

  const showToast = useCallback((text: string, type: ToastType = 'info') => {
    const id = ++toastIdRef.current;
    setToasts((prev) => [...prev.slice(-2), { id, text, type }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 3500);
  }, []);

  // ==================== SELECTED CHAPTER ====================

  const selectedChapter = chapters.find((c) => c.id === selectedChapterId) || null;
  const currentPage = pages[currentPageIndex] || null;

  // ==================== LOAD BOOK + CHAPTERS ====================

  const loadBookAndChapters = useCallback(async () => {
    if (!bookId) return;

    setLoading(true);
    setError(null);

    try {
      const [bookData, chaptersData] = await Promise.all([
        catalogService.getBookById(bookId),
        catalogService.getChapters(bookId),
      ]);

      setBook(bookData);

      const sorted = [...chaptersData].sort(
        (a, b) => (a.chapterNumber || 0) - (b.chapterNumber || 0)
      );
      setChapters(sorted);

      // Авто-выбрать первую главу
      if (sorted.length > 0 && !selectedChapterId) {
        setSelectedChapterId(sorted[0].id);
      }
    } catch (err: any) {
      console.error('Error loading book:', err);
      setError(err.message || 'Не удалось загрузить книгу');
    } finally {
      setLoading(false);
    }
  }, [bookId, selectedChapterId]);

  useEffect(() => {
    loadBookAndChapters();
  }, [loadBookAndChapters]);

  // ==================== LOAD PAGES FOR SELECTED CHAPTER ====================

  const loadPages = useCallback(async () => {
    if (!bookId || !selectedChapterId) {
      setPages([]);
      return;
    }

    setPagesLoading(true);

    try {
      const pagesData = await catalogService.getPages(bookId, selectedChapterId);
      const sorted = [...pagesData].sort((a, b) => (a.pageNumber || 0) - (b.pageNumber || 0));
      setPages(sorted);
      setCurrentPageIndex(0);

      // Инициализировать draft первой страницы
      if (sorted.length > 0) {
        setPageDraft({ content: sorted[0].content || '' });
        lastSavedContentRef.current = sorted[0].content || '';
      } else {
        setPageDraft({ content: '' });
        lastSavedContentRef.current = '';
      }
    } catch (err: any) {
      console.error('Error loading pages:', err);
      setPages([]);
      showToast('Не удалось загрузить страницы', 'error');
    } finally {
      setPagesLoading(false);
    }
  }, [bookId, selectedChapterId, showToast]);

  // При смене главы — инициализировать черновик
  useEffect(() => {
    if (selectedChapter) {
      setChapterDraft({
        title: selectedChapter.title || '',
        summary: selectedChapter.summary || '',
      });
      lastSavedTitleRef.current = selectedChapter.title || '';
      lastSavedSummaryRef.current = selectedChapter.summary || '';
      setSaveStatus('saved');
    }
  }, [selectedChapter]);

  // Загрузить страницы при смене главы
  useEffect(() => {
    loadPages();
  }, [loadPages]);

  // При смене страницы — обновить draft
  useEffect(() => {
    if (currentPage) {
      setPageDraft({ content: currentPage.content || '' });
      lastSavedContentRef.current = currentPage.content || '';
    }
  }, [currentPage]);

  // ==================== AUTO-SAVE LOGIC ====================

  const saveChapter = useCallback(async () => {
    if (!bookId || !selectedChapterId) return;

    const titleChanged = chapterDraft.title !== lastSavedTitleRef.current;
    const summaryChanged = chapterDraft.summary !== lastSavedSummaryRef.current;

    if (!titleChanged && !summaryChanged) return;

    setSaveStatus('saving');
    try {
      await catalogService.updateChapter(bookId, selectedChapterId, {
        title: chapterDraft.title || undefined,
        content: chapterDraft.summary || undefined,
      });

      lastSavedTitleRef.current = chapterDraft.title;
      lastSavedSummaryRef.current = chapterDraft.summary;

      // Обновить в списке глав
      setChapters((prev) =>
        prev.map((ch) =>
          ch.id === selectedChapterId
            ? { ...ch, title: chapterDraft.title, summary: chapterDraft.summary }
            : ch
        )
      );

      setSaveStatus('saved');
    } catch (err) {
      console.error('Error saving chapter:', err);
      setSaveStatus('unsaved');
      showToast('Ошибка сохранения главы', 'error');
    }
  }, [bookId, selectedChapterId, chapterDraft, showToast]);

  const savePage = useCallback(async () => {
    if (!bookId || !selectedChapterId || !currentPage) return;

    if (pageDraft.content === lastSavedContentRef.current) return;

    setSaveStatus('saving');
    try {
      await catalogService.updatePage(bookId, selectedChapterId, currentPage.id, {
        content: pageDraft.content,
      });

      lastSavedContentRef.current = pageDraft.content;

      // Обновить в массиве pages
      setPages((prev) =>
        prev.map((p) =>
          p.id === currentPage.id
            ? { ...p, content: pageDraft.content, wordCount: countWords(pageDraft.content) }
            : p
        )
      );

      setSaveStatus('saved');
    } catch (err) {
      console.error('Error saving page:', err);
      setSaveStatus('unsaved');
      showToast('Ошибка сохранения страницы', 'error');
    }
  }, [bookId, selectedChapterId, currentPage, pageDraft, showToast]);

  // Debounced auto-save
  const triggerAutoSave = useCallback(() => {
    setSaveStatus('unsaved');
    if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    saveTimerRef.current = setTimeout(async () => {
      await saveChapter();
      await savePage();
    }, 1500);
  }, [saveChapter, savePage]);

  // Ctrl+S ручное сохранение
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
        saveChapter();
        savePage();
        showToast('Сохранено', 'success');
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [saveChapter, savePage, showToast]);

  // Cleanup timer
  useEffect(() => {
    return () => {
      if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    };
  }, []);

  // ==================== CHAPTER ACTIONS ====================

  const handleAddChapter = async (title: string, summary: string) => {
    if (!bookId) return;

    setModalLoading(true);
    try {
      const newChapter = await catalogService.createChapter(bookId, {
        title,
        content: summary,
        chapterNumber: chapters.length + 1,
      });

      setChapters((prev) => [...prev, newChapter]);
      setSelectedChapterId(newChapter.id);
      setShowAddChapter(false);
      showToast(`Глава «${title}» создана`, 'success');
    } catch (err: any) {
      console.error('Error creating chapter:', err);
      showToast('Ошибка создания главы', 'error');
    } finally {
      setModalLoading(false);
    }
  };

  const handleDeleteChapter = async () => {
    if (!bookId || !deleteTarget || deleteTarget.type !== 'chapter') return;

    setModalLoading(true);
    try {
      await catalogService.deleteChapter(bookId, deleteTarget.id);

      setChapters((prev) => prev.filter((ch) => ch.id !== deleteTarget.id));

      if (selectedChapterId === deleteTarget.id) {
        const remaining = chapters.filter((ch) => ch.id !== deleteTarget.id);
        setSelectedChapterId(remaining.length > 0 ? remaining[0].id : null);
      }

      setDeleteTarget(null);
      showToast('Глава удалена', 'success');
    } catch (err) {
      console.error('Error deleting chapter:', err);
      showToast('Ошибка удаления главы', 'error');
    } finally {
      setModalLoading(false);
    }
  };

  const handleMoveChapter = async (chapterId: string, direction: 'up' | 'down') => {
    const idx = chapters.findIndex((c) => c.id === chapterId);
    if (idx < 0) return;
    const newIdx = direction === 'up' ? idx - 1 : idx + 1;
    if (newIdx < 0 || newIdx >= chapters.length) return;

    const newOrder = [...chapters];
    [newOrder[idx], newOrder[newIdx]] = [newOrder[newIdx], newOrder[idx]];
    setChapters(newOrder);

    try {
      await catalogService.reorderChapters(
        bookId!,
        newOrder.map((c) => c.id)
      );
      showToast('Порядок глав обновлён', 'info');
    } catch (err) {
      console.error('Error reordering:', err);
      setChapters(chapters); // Revert
      showToast('Ошибка изменения порядка', 'error');
    }
  };

  // ==================== PAGE ACTIONS ====================

  const handleAddPage = async () => {
    if (!bookId || !selectedChapterId) return;

    try {
      const newPage = await catalogService.createPage(bookId, selectedChapterId, {
        content: '',
        pageNumber: pages.length + 1,
      });

      setPages((prev) => [...prev, newPage]);
      setCurrentPageIndex(pages.length); // перейти на новую
      setPageDraft({ content: '' });
      lastSavedContentRef.current = '';
      showToast('Страница добавлена', 'success');
    } catch (err) {
      console.error('Error creating page:', err);
      showToast('Ошибка добавления страницы', 'error');
    }
  };

  const handleDeletePage = async () => {
    if (!bookId || !selectedChapterId || !deleteTarget || deleteTarget.type !== 'page') return;

    setModalLoading(true);
    try {
      await catalogService.deletePage(bookId, selectedChapterId, deleteTarget.id);

      const newPages = pages.filter((p) => p.id !== deleteTarget.id);
      setPages(newPages);

      if (currentPageIndex >= newPages.length) {
        setCurrentPageIndex(Math.max(0, newPages.length - 1));
      }

      setDeleteTarget(null);
      showToast('Страница удалена', 'success');
    } catch (err) {
      console.error('Error deleting page:', err);
      showToast('Ошибка удаления страницы', 'error');
    } finally {
      setModalLoading(false);
    }
  };

  const goToPage = (index: number) => {
    // Сохранить текущую перед переходом
    savePage();
    setCurrentPageIndex(index);
  };

  // ==================== SELECT CHAPTER ====================

  const handleSelectChapter = (chapterId: string) => {
    if (chapterId === selectedChapterId) return;

    // Сохранить текущее
    if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    saveChapter();
    savePage();

    setSelectedChapterId(chapterId);
    setActiveTab('pages');
  };

  // ==================== COMPUTED ====================

  const totalWords = chapters.reduce((sum, ch) => sum + (ch.wordCount || 0), 0);
  const totalPages = chapters.reduce((sum, ch) => sum + (ch.pageCount || 0), 0);
  const currentPageWordCount = countWords(pageDraft.content);

  // ==================== RENDER ====================

  if (loading) {
    return (
      <div className={styles.page}>
        <div className={styles.loadingState}>
          <Spinner size="lg" />
          <span className={styles.loadingText}>Загрузка редактора...</span>
        </div>
      </div>
    );
  }

  if (error || !book) {
    return (
      <div className={styles.page}>
        <div className={styles.errorState}>
          <span className={styles.errorIcon}>⚠️</span>
          <p className={styles.errorText}>{error || 'Книга не найдена'}</p>
          <Button variant="outline" onClick={() => navigate(ROUTES.AUTHOR_DASHBOARD)}>
            ← Назад к дашборду
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <div className={styles.container}>
        {/* ============ HEADER ============ */}
        <header className={styles.header}>
          <div className={styles.headerLeft}>
            <nav className={styles.breadcrumb}>
              <Link to={ROUTES.AUTHOR_DASHBOARD}>Дашборд</Link>
              <span className={styles.breadcrumbSep}>›</span>
              <Link to={ROUTES.EDIT_BOOK_BY_ID(bookId!)}>Книга</Link>
              <span className={styles.breadcrumbSep}>›</span>
              <span>Редактор</span>
            </nav>
            <h1 className={styles.title}>
              <span className={styles.titleIcon}>✏️</span>
              Редактор контента
            </h1>
            <p className={styles.bookTitle}>{book.title}</p>
          </div>
          <div className={styles.headerActions}>
            <button
              className={`${styles.headerBtn} ${styles.headerBtnBack}`}
              onClick={() => navigate(ROUTES.AUTHOR_DASHBOARD)}
            >
              ← Назад
            </button>
            <button
              className={`${styles.headerBtn} ${styles.headerBtnPreview}`}
              onClick={() => navigate(ROUTES.READER_BY_ID(bookId!))}
            >
              👁️ Предпросмотр
            </button>
            <button
              className={`${styles.headerBtn} ${styles.headerBtnPrimary}`}
              onClick={() => {
                saveChapter();
                savePage();
                showToast('Все изменения сохранены', 'success');
              }}
            >
              💾 Сохранить всё
            </button>
          </div>
        </header>

        {/* ============ STATS BAR ============ */}
        <div className={styles.statsBar}>
          <div className={styles.statPill}>
            <span className={styles.statPillIcon}>📑</span>
            <span>Глав:</span>
            <span className={styles.statPillValue}>{chapters.length}</span>
          </div>
          <div className={styles.statPill}>
            <span className={styles.statPillIcon}>📄</span>
            <span>Страниц:</span>
            <span className={styles.statPillValue}>{totalPages}</span>
          </div>
          <div className={styles.statPill}>
            <span className={styles.statPillIcon}>📝</span>
            <span>Слов:</span>
            <span className={styles.statPillValue}>{totalWords.toLocaleString()}</span>
          </div>
          <div className={styles.statPill}>
            <span className={styles.statPillIcon}>⏱️</span>
            <span>Чтение:</span>
            <span className={styles.statPillValue}>{readingTime(totalWords)}</span>
          </div>
          <div className={styles.statPill}>
            <span className={styles.statPillIcon}>🎨</span>
            <span>Визуализация:</span>
            <span className={styles.statPillValue}>{book.visualizationMode || 'UserSelected'}</span>
          </div>
        </div>

        {/* ============ MAIN LAYOUT ============ */}
        <div className={styles.layout}>
          {/* ======== LEFT: CHAPTER LIST ======== */}
          <aside className={styles.sidebar}>
            <div className={styles.sidebarHeader}>
              <h3 className={styles.sidebarTitle}>Оглавление</h3>
              <button className={styles.addChapterBtn} onClick={() => setShowAddChapter(true)}>
                + Глава
              </button>
            </div>

            <div className={styles.chapterList}>
              {chapters.length === 0 ? (
                <div className={styles.emptyChapters}>
                  <span className={styles.emptyIcon}>📖</span>
                  <h4 className={styles.emptyTitle}>Нет глав</h4>
                  <p className={styles.emptyText}>
                    Добавьте первую главу, чтобы начать работу над книгой
                  </p>
                  <button className={styles.addChapterBtn} onClick={() => setShowAddChapter(true)}>
                    + Добавить главу
                  </button>
                </div>
              ) : (
                chapters.map((chapter, idx) => (
                  <div
                    key={chapter.id}
                    className={`${styles.chapterItem} ${
                      selectedChapterId === chapter.id ? styles.active : ''
                    }`}
                    onClick={() => handleSelectChapter(chapter.id)}
                  >
                    {/* Drag handle */}
                    <div className={styles.dragHandle} title="Перетащить">
                      <span className={styles.dragDot} />
                      <span className={styles.dragDot} />
                      <span className={styles.dragDot} />
                      <span className={styles.dragDot} />
                      <span className={styles.dragDot} />
                      <span className={styles.dragDot} />
                    </div>

                    <span className={styles.chapterNum}>{idx + 1}</span>

                    <div className={styles.chapterItemInfo}>
                      <p className={styles.chapterItemTitle}>
                        {chapter.title || 'Без названия'}
                      </p>
                      <span className={styles.chapterItemMeta}>
                        {chapter.pageCount || 0} стр.
                        {chapter.wordCount ? ` • ${chapter.wordCount.toLocaleString()} сл.` : ''}
                      </span>
                    </div>

                    <div className={styles.chapterItemActions}>
                      <button
                        className={styles.chItemBtn}
                        title="Переместить вверх"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleMoveChapter(chapter.id, 'up');
                        }}
                        disabled={idx === 0}
                      >
                        ▲
                      </button>
                      <button
                        className={styles.chItemBtn}
                        title="Переместить вниз"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleMoveChapter(chapter.id, 'down');
                        }}
                        disabled={idx === chapters.length - 1}
                      >
                        ▼
                      </button>
                      <button
                        className={`${styles.chItemBtn} ${styles.chItemBtnDanger}`}
                        title="Удалить главу"
                        onClick={(e) => {
                          e.stopPropagation();
                          setDeleteTarget({
                            type: 'chapter',
                            id: chapter.id,
                            name: chapter.title || `Глава ${idx + 1}`,
                          });
                        }}
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </aside>

          {/* ======== RIGHT: EDITOR ======== */}
          <main className={styles.editorArea}>
            {!selectedChapter ? (
              <div className={styles.noChapterSelected}>
                <span className={styles.noChapterIcon}>📝</span>
                <h3 className={styles.noChapterTitle}>Выберите главу</h3>
                <p className={styles.noChapterText}>
                  Выберите главу из оглавления слева или создайте новую, чтобы начать редактирование
                </p>
              </div>
            ) : (
              <>
                {/* ---- Editor Toolbar ---- */}
                <div className={styles.editorToolbar}>
                  <div className={styles.editorToolbarLeft}>
                    <input
                      type="text"
                      className={styles.chapterTitleInput}
                      value={chapterDraft.title}
                      onChange={(e) => {
                        setChapterDraft((d) => ({ ...d, title: e.target.value }));
                        triggerAutoSave();
                      }}
                      placeholder="Название главы..."
                    />
                  </div>
                  <div className={styles.editorToolbarRight}>
                    <span
                      className={`${styles.saveIndicator} ${
                        saveStatus === 'saved' ? styles.saved :
                        saveStatus === 'saving' ? styles.saving : styles.unsaved
                      }`}
                    >
                      {saveStatus === 'saved' && '✓ Сохранено'}
                      {saveStatus === 'saving' && '⟳ Сохранение...'}
                      {saveStatus === 'unsaved' && '● Несохранено'}
                    </span>
                  </div>
                </div>

                {/* ---- Tabs ---- */}
                <div className={styles.editorTabs}>
                  <button
                    className={`${styles.editorTab} ${activeTab === 'pages' ? styles.editorTabActive : ''}`}
                    onClick={() => setActiveTab('pages')}
                  >
                    📄 Страницы
                    <span className={styles.editorTabBadge}>{pages.length}</span>
                  </button>
                  <button
                    className={`${styles.editorTab} ${activeTab === 'summary' ? styles.editorTabActive : ''}`}
                    onClick={() => setActiveTab('summary')}
                  >
                    📋 Описание
                  </button>
                </div>

                {/* ---- Summary Tab Content ---- */}
                {activeTab === 'summary' && (
                  <motion.div
                    className={styles.summaryPanel}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ duration: 0.15 }}
                  >
                    <label className={styles.summaryLabel}>Краткое содержание главы</label>
                    <textarea
                      className={styles.summaryTextarea}
                      value={chapterDraft.summary}
                      onChange={(e) => {
                        setChapterDraft((d) => ({ ...d, summary: e.target.value }));
                        triggerAutoSave();
                      }}
                      placeholder="Опишите, о чём эта глава. Это поможет читателям в оглавлении и может быть использовано AI для генерации визуализаций..."
                    />
                  </motion.div>
                )}

                {/* ---- Pages Tab Content ---- */}
                {activeTab === 'pages' && (
                  <div className={styles.pagesPanel}>
                    {/* Pages Toolbar */}
                    <div className={styles.pagesToolbar}>
                      <div className={styles.pagesToolbarLeft}>
                        {pages.length > 0 && (
                          <div className={styles.pageNavigation}>
                            <button
                              className={styles.pageNavBtn}
                              onClick={() => goToPage(currentPageIndex - 1)}
                              disabled={currentPageIndex <= 0}
                              title="Предыдущая страница"
                            >
                              ‹
                            </button>
                            <span className={styles.pageIndicator}>
                              <span className={styles.pageIndicatorCurrent}>
                                {currentPageIndex + 1}
                              </span>
                              {' / '}
                              {pages.length}
                            </span>
                            <button
                              className={styles.pageNavBtn}
                              onClick={() => goToPage(currentPageIndex + 1)}
                              disabled={currentPageIndex >= pages.length - 1}
                              title="Следующая страница"
                            >
                              ›
                            </button>
                          </div>
                        )}
                      </div>
                      <div className={styles.pagesToolbarRight}>
                        {currentPage && (
                          <button
                            className={styles.deletePageBtn}
                            onClick={() =>
                              setDeleteTarget({
                                type: 'page',
                                id: currentPage.id,
                                name: `Страница ${currentPageIndex + 1}`,
                              })
                            }
                            title="Удалить текущую страницу"
                          >
                            🗑️ Удалить
                          </button>
                        )}
                        <button className={styles.addPageBtn} onClick={handleAddPage}>
                          + Страница
                        </button>
                      </div>
                    </div>

                    {/* Text Editor */}
                    <div className={styles.textEditor}>
                      {pagesLoading ? (
                        <div className={styles.loadingState}>
                          <Spinner size="md" />
                          <span className={styles.loadingText}>Загрузка страниц...</span>
                        </div>
                      ) : pages.length === 0 ? (
                        <div className={styles.noChapterSelected}>
                          <span className={styles.noChapterIcon}>📄</span>
                          <h3 className={styles.noChapterTitle}>Нет страниц</h3>
                          <p className={styles.noChapterText}>
                            Добавьте первую страницу, чтобы начать писать содержимое главы
                          </p>
                          <button
                            className={`${styles.headerBtn} ${styles.headerBtnPrimary}`}
                            onClick={handleAddPage}
                            style={{ marginTop: '16px' }}
                          >
                            + Добавить страницу
                          </button>
                        </div>
                      ) : (
                        <div className={styles.textEditorContent}>
                          <textarea
                            className={styles.pageTextarea}
                            value={pageDraft.content}
                            onChange={(e) => {
                              setPageDraft({ content: e.target.value });
                              triggerAutoSave();
                            }}
                            placeholder="Начните писать содержимое страницы здесь...

Совет: используйте Ctrl+S для быстрого сохранения."
                          />
                        </div>
                      )}
                    </div>

                    {/* Editor Footer */}
                    {pages.length > 0 && currentPage && (
                      <div className={styles.editorFooter}>
                        <div className={styles.editorFooterLeft}>
                          <span className={styles.footerStat}>
                            <span className={styles.footerStatLabel}>Слов:</span>
                            <span className={styles.footerStatValue}>
                              {currentPageWordCount.toLocaleString()}
                            </span>
                          </span>
                          <span className={styles.footerStat}>
                            <span className={styles.footerStatLabel}>Символов:</span>
                            <span className={styles.footerStatValue}>
                              {pageDraft.content.length.toLocaleString()}
                            </span>
                          </span>
                          <span className={styles.footerStat}>
                            <span className={styles.footerStatLabel}>Чтение:</span>
                            <span className={styles.footerStatValue}>
                              {readingTime(currentPageWordCount)}
                            </span>
                          </span>
                        </div>
                        <div className={styles.editorFooterRight}>
                          <span className={styles.footerStat}>
                            <span className={styles.footerStatLabel}>Страница</span>
                            <span className={styles.footerStatValue}>
                              {currentPageIndex + 1} из {pages.length}
                            </span>
                          </span>
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </>
            )}
          </main>
        </div>
      </div>

      {/* ============ MODALS ============ */}
      <AnimatePresence>
        {showAddChapter && (
          <AddChapterModal
            isOpen={showAddChapter}
            onClose={() => setShowAddChapter(false)}
            onSubmit={handleAddChapter}
            loading={modalLoading}
          />
        )}
      </AnimatePresence>

      <AnimatePresence>
        {deleteTarget && (
          <DeleteModal
            isOpen={!!deleteTarget}
            entityName={deleteTarget.name}
            entityType={deleteTarget.type}
            onClose={() => setDeleteTarget(null)}
            onConfirm={
              deleteTarget.type === 'chapter' ? handleDeleteChapter : handleDeletePage
            }
            loading={modalLoading}
          />
        )}
      </AnimatePresence>

      {/* ============ TOASTS ============ */}
      <AnimatePresence>
        {toasts.map((t) => (
          <Toast key={t.id} message={t} />
        ))}
      </AnimatePresence>
    </div>
  );
};

export default ManageChaptersPage;
