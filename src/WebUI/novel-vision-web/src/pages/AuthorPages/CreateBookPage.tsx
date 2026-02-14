// src/pages/AuthorPages/CreateBookPage.tsx
// Premium Create/Edit Book Page — NovelVision Library Theme
// FIXED: Auto-upgrade to Author role on 403 + token refresh

import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { ROUTES } from '../../shared/constants/routes';
import { useAuthStore } from '../../store';
import { catalogService } from '../../services/api/catalog.service';
import { authService } from '../../features/auth/services/authService';
import type { Book, CreateBookRequest, UpdateBookRequest, VisualizationMode } from '../../types';
import styles from './CreateBookPage.module.css';

// ==================== CONSTANTS ====================

const MAX_TITLE = 200;
const MAX_DESC = 5000;
const MAX_GENRES = 3;
const MAX_TAGS = 10;

const GENRE_LIST: { name: string; icon: string }[] = [
  { name: 'Fantasy', icon: '🐉' },
  { name: 'Romance', icon: '💕' },
  { name: 'Mystery', icon: '🔍' },
  { name: 'Science Fiction', icon: '🚀' },
  { name: 'Thriller', icon: '🔪' },
  { name: 'Horror', icon: '👻' },
  { name: 'Historical Fiction', icon: '🏰' },
  { name: 'Literary Fiction', icon: '📜' },
  { name: 'Adventure', icon: '🗺️' },
  { name: 'Drama', icon: '🎭' },
  { name: 'Comedy', icon: '😂' },
  { name: 'Poetry', icon: '🌸' },
  { name: 'Biography', icon: '👤' },
  { name: 'Philosophy', icon: '🧠' },
  { name: 'Self-Help', icon: '✨' },
  { name: 'Children', icon: '🧸' },
];

const LANG_LIST = [
  { code: 'en', label: 'English', flag: '🇬🇧' },
  { code: 'es', label: 'Spanish', flag: '🇪🇸' },
  { code: 'fr', label: 'French', flag: '🇫🇷' },
  { code: 'de', label: 'German', flag: '🇩🇪' },
  { code: 'it', label: 'Italian', flag: '🇮🇹' },
  { code: 'pt', label: 'Portuguese', flag: '🇵🇹' },
  { code: 'ru', label: 'Russian', flag: '🇷🇺' },
  { code: 'uk', label: 'Ukrainian', flag: '🇺🇦' },
  { code: 'pl', label: 'Polish', flag: '🇵🇱' },
  { code: 'nl', label: 'Dutch', flag: '🇳🇱' },
  { code: 'ja', label: 'Japanese', flag: '🇯🇵' },
  { code: 'zh', label: 'Chinese', flag: '🇨🇳' },
];

const VIZ_MODES: { id: VisualizationMode; label: string; icon: string; desc: string; badge?: string }[] = [
  { id: 'None', label: 'Text Only', icon: '📖', desc: 'Classic reading experience — no AI images' },
  { id: 'PerPage', label: 'Per Page', icon: '🖼️', desc: 'Auto-generate an illustration for every page', badge: 'Popular' },
  { id: 'PerChapter', label: 'Per Chapter', icon: '📑', desc: 'One key scene illustration per chapter' },
  { id: 'UserSelected', label: "Reader's Choice", icon: '✨', desc: 'Readers select text passages to visualize', badge: 'Recommended' },
  { id: 'AuthorDefined', label: 'Author Defined', icon: '🎨', desc: 'You mark specific visualization points' },
];

// ==================== ANIMATIONS ====================

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.45, ease: [0.22, 1, 0.36, 1] as [number, number, number, number] } },
};

const stagger = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: { staggerChildren: 0.07, delayChildren: 0.08 } },
};

// ==================== FORM DATA ====================

interface BookFormData {
  title: string;
  description: string;
  coverImageUrl: string;
  language: string;
  isbn: string;
  publisher: string;
  genres: string[];
  tags: string[];
  visualizationMode: VisualizationMode;
  isPublished: boolean;
}

const defaultForm: BookFormData = {
  title: '',
  description: '',
  coverImageUrl: '',
  language: 'en',
  isbn: '',
  publisher: '',
  genres: [],
  tags: [],
  visualizationMode: 'UserSelected',
  isPublished: false,
};

interface FieldErrors {
  title?: string;
  description?: string;
  coverImageUrl?: string;
}

// ==================== SUB-COMPONENTS ====================

/* ── Breadcrumbs ── */
const Breadcrumbs: React.FC<{ isEdit: boolean; title?: string }> = ({ isEdit, title }) => (
  <nav className={styles.breadcrumbs} aria-label="Breadcrumb">
    <Link to={ROUTES.HOME} className={styles.crumb}>Home</Link>
    <span className={styles.crumbSep}>›</span>
    <Link to={ROUTES.AUTHOR_DASHBOARD} className={styles.crumb}>Dashboard</Link>
    <span className={styles.crumbSep}>›</span>
    <span className={styles.crumbCurrent}>{isEdit ? (title || 'Edit Book') : 'Create Book'}</span>
  </nav>
);

/* ── Section wrapper ── */
const Section: React.FC<{
  icon: string;
  title: string;
  desc?: string;
  step?: number;
  children: React.ReactNode;
}> = ({ icon, title, desc, step, children }) => (
  <motion.section className={styles.section} initial="hidden" animate="visible" variants={fadeUp}>
    <div className={styles.sectionHead}>
      {step !== undefined && <span className={styles.stepBadge}>{step}</span>}
      <span className={styles.sectionIcon}>{icon}</span>
      <div>
        <h2 className={styles.sectionTitle}>{title}</h2>
        {desc && <p className={styles.sectionDesc}>{desc}</p>}
      </div>
    </div>
    <div className={styles.sectionBody}>{children}</div>
  </motion.section>
);

/* ── Character counter bar ── */
const CharBar: React.FC<{ cur: number; max: number }> = ({ cur, max }) => {
  const pct = (cur / max) * 100;
  const over = cur > max;
  return (
    <div className={`${styles.charBar} ${over ? styles.charOver : pct > 80 ? styles.charWarn : ''}`}>
      <div className={styles.charTrack}>
        <div className={styles.charFill} style={{ width: `${Math.min(pct, 100)}%` }} />
      </div>
      <span>{cur.toLocaleString()} / {max.toLocaleString()}</span>
    </div>
  );
};

/* ── 3D Book Cover Preview ── */
const BookPreview3D: React.FC<{
  url: string;
  title: string;
  author: string;
  genres: string[];
}> = ({ url, title, author, genres }) => {
  const [imgErr, setImgErr] = useState(false);
  useEffect(() => { setImgErr(false); }, [url]);

  return (
    <div className={styles.previewBlock}>
      <div className={styles.bookPerspective}>
        <div className={styles.book3d}>
          <div className={styles.bookFront}>
            {url && !imgErr ? (
              <img src={url} alt="Cover" onError={() => setImgErr(true)} />
            ) : (
              <div className={styles.bookPlaceholder}>
                <span className={styles.phEmoji}>📖</span>
                <span className={styles.phTitle}>{title || 'Your Book'}</span>
                <span className={styles.phAuthor}>{author}</span>
              </div>
            )}
          </div>
          <div className={styles.bookSpine} />
          <div className={styles.bookEdge} />
        </div>
      </div>
      <div className={styles.previewMeta}>
        <h4 className={styles.pmTitle}>{title || 'Book Title'}</h4>
        <p className={styles.pmAuthor}>by {author || 'Author'}</p>
        {genres.length > 0 && (
          <div className={styles.pmTags}>
            {genres.map((g) => <span key={g} className={styles.pmTag}>{g}</span>)}
          </div>
        )}
        {!url && <p className={styles.pmHint}>Add a cover URL to see it here</p>}
      </div>
    </div>
  );
};

/* ── Genre Selector ── */
const GenreSelector: React.FC<{
  selected: string[];
  onChange: (g: string[]) => void;
}> = ({ selected, onChange }) => {
  const toggle = (name: string) => {
    if (selected.includes(name)) onChange(selected.filter((g) => g !== name));
    else if (selected.length < MAX_GENRES) onChange([...selected, name]);
  };

  return (
    <div className={styles.genreWrap}>
      <div className={styles.genreGrid}>
        {GENRE_LIST.map(({ name, icon }) => {
          const on = selected.includes(name);
          return (
            <button
              key={name}
              type="button"
              className={`${styles.genreChip} ${on ? styles.genreOn : ''}`}
              onClick={() => toggle(name)}
              disabled={!on && selected.length >= MAX_GENRES}
            >
              <span className={styles.genreIcon}>{icon}</span>
              <span>{name}</span>
            </button>
          );
        })}
      </div>
      <span className={styles.genreCount}>{selected.length}/{MAX_GENRES} selected</span>
    </div>
  );
};

/* ── Tag Input ── */
const TagInput: React.FC<{
  tags: string[];
  onChange: (t: string[]) => void;
}> = ({ tags, onChange }) => {
  const [val, setVal] = useState('');
  const ref = useRef<HTMLInputElement>(null);

  const add = () => {
    const t = val.trim().toLowerCase();
    if (t && !tags.includes(t) && tags.length < MAX_TAGS) { onChange([...tags, t]); setVal(''); }
  };

  const remove = (t: string) => onChange(tags.filter((x) => x !== t));

  const onKey = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' || e.key === ',') { e.preventDefault(); add(); }
    if (e.key === 'Backspace' && !val && tags.length) remove(tags[tags.length - 1]);
  };

  return (
    <div className={styles.tagWrap} onClick={() => ref.current?.focus()}>
      <div className={styles.tagList}>
        <AnimatePresence>
          {tags.map((t) => (
            <motion.span
              key={t}
              className={styles.tagChip}
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.8 }}
              transition={{ duration: 0.15 }}
            >
              {t}
              <button type="button" onClick={() => remove(t)} className={styles.tagX}>×</button>
            </motion.span>
          ))}
        </AnimatePresence>
      </div>
      <input
        ref={ref}
        type="text"
        value={val}
        onChange={(e) => setVal(e.target.value)}
        onKeyDown={onKey}
        onBlur={add}
        placeholder={tags.length < MAX_TAGS ? 'Type and press Enter…' : 'Max reached'}
        className={styles.tagField}
        disabled={tags.length >= MAX_TAGS}
      />
      <span className={styles.tagCount}>{tags.length}/{MAX_TAGS}</span>
    </div>
  );
};

/* ── Visualization Selector ── */
const VizSelector: React.FC<{
  value: VisualizationMode;
  onChange: (m: VisualizationMode) => void;
}> = ({ value, onChange }) => (
  <div className={styles.vizGrid}>
    {VIZ_MODES.map((m) => (
      <button
        key={m.id}
        type="button"
        className={`${styles.vizCard} ${value === m.id ? styles.vizOn : ''}`}
        onClick={() => onChange(m.id)}
      >
        {m.badge && <span className={styles.vizBadge}>{m.badge}</span>}
        <span className={styles.vizIcon}>{m.icon}</span>
        <span className={styles.vizLabel}>{m.label}</span>
        <span className={styles.vizDesc}>{m.desc}</span>
        {value === m.id && (
          <motion.span
            className={styles.vizCheck}
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ type: 'spring', stiffness: 500, damping: 25 }}
          >✓</motion.span>
        )}
      </button>
    ))}
  </div>
);

/* ── Publish toggle ── */
const PublishToggle: React.FC<{
  on: boolean;
  onChange: (v: boolean) => void;
}> = ({ on, onChange }) => (
  <div className={styles.pubRow}>
    <div className={styles.pubInfo}>
      <span className={styles.pubIcon}>{on ? '🌐' : '🔒'}</span>
      <div>
        <h4 className={styles.pubTitle}>{on ? 'Published — Visible to readers' : 'Draft — Only you can see'}</h4>
        <p className={styles.pubDesc}>
          {on
            ? 'Your book is live in the library. Readers can find and read it.'
            : "Keep working on your book. Publish when you're ready."}
        </p>
      </div>
    </div>
    <button type="button" className={`${styles.toggle} ${on ? styles.toggleOn : ''}`} onClick={() => onChange(!on)}>
      <span className={styles.toggleTrack}><span className={styles.toggleThumb} /></span>
    </button>
  </div>
);

/* ── Skeleton loader ── */
const EditSkeleton: React.FC = () => (
  <div className={styles.skeleton}>
    <div className={styles.skelBar}><div className={styles.shimmer} /></div>
    <div className={styles.skelLayout}>
      <div className={styles.skelMain}>
        <div className={styles.skelCard}><div className={styles.shimmer} /></div>
        <div className={styles.skelCard}><div className={styles.shimmer} /></div>
      </div>
      <div className={styles.skelSide}><div className={styles.shimmer} /></div>
    </div>
  </div>
);

// ==================== MAIN PAGE ====================

const CreateBookPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const isEditMode = Boolean(id);
  const initialSnap = useRef<string>(JSON.stringify(defaultForm));

  // State
  const [form, setForm] = useState<BookFormData>(defaultForm);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [saveErr, setSaveErr] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  // Dirty flag
  const dirty = useMemo(() => JSON.stringify(form) !== initialSnap.current, [form]);

  // Unsaved-changes guard
  useEffect(() => {
    const h = (e: BeforeUnloadEvent) => {
      if (dirty && !saving) { e.preventDefault(); e.returnValue = ''; }
    };
    window.addEventListener('beforeunload', h);
    return () => window.removeEventListener('beforeunload', h);
  }, [dirty, saving]);

  // ============================================================
  // NO REDIRECT useEffect HERE! ProtectedRoute already handles it
  // ============================================================

  // Load existing book (edit mode)
  useEffect(() => {
    if (!id) return;
    let cancelled = false;

    (async () => {
      setLoading(true);
      setSaveErr(null);
      try {
        const book = await catalogService.getBookById(id);
        if (cancelled) return;
        const data: BookFormData = {
          title: book.title || '',
          description: book.description || '',
          coverImageUrl: book.coverImageUrl || '',
          language: book.language || 'en',
          isbn: book.isbn || '',
          publisher: book.publisher || '',
          genres: book.genres || [],
          tags: book.tags || [],
          visualizationMode: (book.visualizationMode as VisualizationMode) || 'UserSelected',
          isPublished: book.isPublished ?? false,
        };
        setForm(data);
        initialSnap.current = JSON.stringify(data);
      } catch (err) {
        console.error('Error fetching book:', err);
        if (!cancelled) setSaveErr('Failed to load book details');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => { cancelled = true; };
  }, [id]);

  // ── Handlers ──
  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
      const { name, value, type } = e.target;
      setForm((p) => ({
        ...p,
        [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value,
      }));
      if (errors[name as keyof FieldErrors]) setErrors((p) => ({ ...p, [name]: undefined }));
      setSaved(false);
    },
    [errors],
  );

  const validate = useCallback((): boolean => {
    const e: FieldErrors = {};
    if (!form.title.trim()) e.title = 'Title is required';
    else if (form.title.length > MAX_TITLE) e.title = `Must be under ${MAX_TITLE} characters`;
    if (form.description.length > MAX_DESC) e.description = `Must be under ${MAX_DESC.toLocaleString()} characters`;
    if (form.coverImageUrl && !/^https?:\/\//.test(form.coverImageUrl)) e.coverImageUrl = 'Must be a valid URL (https://…)';
    setErrors(e);
    return Object.keys(e).length === 0;
  }, [form]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    setSaving(true);
    setSaveErr(null);

    /** Builds the create-book payload */
    const buildCreatePayload = (): CreateBookRequest => ({
      title: form.title.trim(),
      description: form.description.trim(),
      authorId: user!.id,
      coverImageUrl: form.coverImageUrl || undefined,
      language: form.language,
      isbn: form.isbn || undefined,
      publisher: form.publisher || undefined,
      genres: form.genres,
      tags: form.tags,
      visualizationMode: form.visualizationMode,
    });

    /** Builds the update-book payload */
    const buildUpdatePayload = (): UpdateBookRequest => ({
      title: form.title.trim(),
      description: form.description.trim(),
      coverImageUrl: form.coverImageUrl || undefined,
      language: form.language,
      genres: form.genres,
      tags: form.tags,
      visualizationMode: form.visualizationMode,
      isPublished: form.isPublished,
    });

    /** Execute the actual save (create or update) */
    const executeSave = async () => {
      if (isEditMode && id) {
        await catalogService.updateBook(id, buildUpdatePayload());
        initialSnap.current = JSON.stringify(form);
        setSaved(true);
        setTimeout(() => navigate(ROUTES.BOOK_BY_ID(id)), 1200);
      } else {
        const book = await catalogService.createBook(buildCreatePayload());
        initialSnap.current = JSON.stringify(form);
        setSaved(true);
        setTimeout(() => navigate(ROUTES.BOOK_BY_ID(book.id)), 1200);
      }
    };

    /**
     * Auto-upgrade Reader → Author on 403.
     * Calls POST /auth/become-author, then refreshes the JWT so
     * the new token contains the Author role claim, then retries.
     */
    const upgradeToAuthorAndRetry = async () => {
      console.log('[CreateBook] 403 detected — upgrading to Author role…');
      try {
        await authService.becomeAuthor();
        console.log('[CreateBook] become-author succeeded, refreshing token…');

        // Refresh the JWT so the new Author role claim is embedded
        const currentRefresh = localStorage.getItem('refreshToken');
        const currentAccess = localStorage.getItem('accessToken');
        if (currentRefresh) {
          const refreshResp = await authService.refreshToken(currentRefresh, currentAccess || undefined);
          const newAccess = refreshResp.accessToken;
          const newRefresh = refreshResp.refreshToken;
          if (newAccess && newRefresh) {
            // Persist new tokens via store + localStorage
            useAuthStore.getState().setTokens(newAccess, newRefresh);
            if (refreshResp.user) {
              useAuthStore.getState().setUser(refreshResp.user);
            }
            console.log('[CreateBook] Token refreshed with Author role — retrying save…');
          }
        }

        // Retry the original save
        await executeSave();
      } catch (upgradeErr) {
        console.error('[CreateBook] Author upgrade failed:', upgradeErr);
        setSaveErr(
          'Your account needs the Author role to create books. ' +
          'The automatic upgrade failed — please log out, log back in, and try again.'
        );
      }
    };

    try {
      await executeSave();
    } catch (err: any) {
      const status = err?.status ?? err?.response?.status ?? 0;
      if (status === 403) {
        // Forbidden — likely missing Author role; attempt silent upgrade
        await upgradeToAuthorAndRetry();
      } else {
        console.error('Save error:', err);
        setSaveErr('Failed to save book. Please try again.');
      }
    } finally {
      setSaving(false);
    }
  };

  const authorName =
    user?.displayName ||
    `${user?.firstName || ''} ${user?.lastName || ''}`.trim() ||
    'Author';

  const langObj = LANG_LIST.find((l) => l.code === form.language);
  const vizObj = VIZ_MODES.find((m) => m.id === form.visualizationMode);

  // ==================== LOADING ====================
  if (loading) {
    return (
      <div className={styles.page}>
        <div className={styles.container}>
          <Breadcrumbs isEdit={isEditMode} />
          <EditSkeleton />
        </div>
      </div>
    );
  }

  // ==================== RENDER ====================
  return (
    <div className={styles.page}>
      <div className={styles.container}>
        <Breadcrumbs isEdit={isEditMode} title={isEditMode ? form.title : undefined} />

        {/* Header */}
        <motion.header className={styles.header} initial="hidden" animate="visible" variants={stagger}>
          <motion.div variants={fadeUp}>
            <button className={styles.backBtn} type="button" onClick={() => navigate(ROUTES.AUTHOR_DASHBOARD)}>
              ← Back to Dashboard
            </button>
          </motion.div>
          <motion.h1 className={styles.pageTitle} variants={fadeUp}>
            {isEditMode ? '✏️ Edit Book' : '📚 Create New Book'}
          </motion.h1>
          <motion.p className={styles.pageSub} variants={fadeUp}>
            {isEditMode ? 'Update your book details and settings' : 'Fill in the details to bring your story to life'}
          </motion.p>
        </motion.header>

        {/* Status toast */}
        <AnimatePresence>
          {(dirty || saved || saveErr) && (
            <motion.div
              className={`${styles.toast} ${saved ? styles.toastOk : saveErr ? styles.toastErr : styles.toastDirty}`}
              initial={{ opacity: 0, y: -10, height: 0, marginBottom: 0 }}
              animate={{ opacity: 1, y: 0, height: 'auto', marginBottom: 20 }}
              exit={{ opacity: 0, y: -10, height: 0, marginBottom: 0 }}
            >
              {saved && '✅ Saved successfully! Redirecting…'}
              {saveErr && `⚠️ ${saveErr}`}
              {!saved && !saveErr && dirty && '📝 You have unsaved changes'}
            </motion.div>
          )}
        </AnimatePresence>

        {/* Form */}
        <form onSubmit={handleSubmit} className={styles.form} noValidate>
          <div className={styles.layout}>

            {/* ═══ LEFT: form sections ═══ */}
            <div className={styles.main}>

              {/* 1 ── Basic Info ── */}
              <Section icon="📖" title="Basic Information" desc="Title, description, and cover image" step={1}>
                <div className={styles.field}>
                  <label className={styles.label}>Book Title <span className={styles.req}>*</span></label>
                  <input
                    type="text"
                    name="title"
                    value={form.title}
                    onChange={handleChange}
                    placeholder="Enter your book title…"
                    className={`${styles.input} ${errors.title ? styles.inputErr : ''}`}
                    maxLength={MAX_TITLE}
                    autoFocus={!isEditMode}
                  />
                  {errors.title && <span className={styles.errMsg}>{errors.title}</span>}
                  <CharBar cur={form.title.length} max={MAX_TITLE} />
                </div>

                <div className={styles.field}>
                  <label className={styles.label}>Description</label>
                  <textarea
                    name="description"
                    value={form.description}
                    onChange={handleChange}
                    placeholder="Write a compelling description that will draw readers in…"
                    className={`${styles.textarea} ${errors.description ? styles.inputErr : ''}`}
                    rows={6}
                  />
                  {errors.description && <span className={styles.errMsg}>{errors.description}</span>}
                  <CharBar cur={form.description.length} max={MAX_DESC} />
                </div>

                <div className={styles.field}>
                  <label className={styles.label}>Cover Image URL</label>
                  <input
                    type="url"
                    name="coverImageUrl"
                    value={form.coverImageUrl}
                    onChange={handleChange}
                    placeholder="https://example.com/cover.jpg"
                    className={`${styles.input} ${errors.coverImageUrl ? styles.inputErr : ''}`}
                  />
                  {errors.coverImageUrl && <span className={styles.errMsg}>{errors.coverImageUrl}</span>}
                </div>

                <div className={styles.row2}>
                  <div className={styles.field}>
                    <label className={styles.label}>Language</label>
                    <select name="language" value={form.language} onChange={handleChange} className={styles.select}>
                      {LANG_LIST.map((l) => (
                        <option key={l.code} value={l.code}>{l.flag} {l.label}</option>
                      ))}
                    </select>
                  </div>
                  <div className={styles.field}>
                    <label className={styles.label}>ISBN <span className={styles.opt}>(optional)</span></label>
                    <input
                      type="text"
                      name="isbn"
                      value={form.isbn}
                      onChange={handleChange}
                      placeholder="978-0-000-00000-0"
                      className={styles.input}
                    />
                  </div>
                </div>

                <div className={styles.field}>
                  <label className={styles.label}>Publisher <span className={styles.opt}>(optional)</span></label>
                  <input
                    type="text"
                    name="publisher"
                    value={form.publisher}
                    onChange={handleChange}
                    placeholder="Publisher name"
                    className={styles.input}
                  />
                </div>
              </Section>

              {/* 2 ── Genres & Tags ── */}
              <Section icon="🏷️" title="Genres & Tags" desc="Help readers discover your book" step={2}>
                <div className={styles.field}>
                  <label className={styles.label}>Genres <span className={styles.hint}>(up to {MAX_GENRES})</span></label>
                  <GenreSelector
                    selected={form.genres}
                    onChange={(genres) => { setForm((p) => ({ ...p, genres })); setSaved(false); }}
                  />
                </div>
                <div className={styles.field}>
                  <label className={styles.label}>Tags <span className={styles.hint}>(up to {MAX_TAGS})</span></label>
                  <TagInput
                    tags={form.tags}
                    onChange={(tags) => { setForm((p) => ({ ...p, tags })); setSaved(false); }}
                  />
                </div>
              </Section>

              {/* 3 ── AI Visualization ── */}
              <Section icon="🎨" title="AI Visualization" desc="Choose how AI-generated illustrations enhance your book" step={3}>
                <VizSelector
                  value={form.visualizationMode}
                  onChange={(v) => { setForm((p) => ({ ...p, visualizationMode: v })); setSaved(false); }}
                />
              </Section>

              {/* 4 ── Publishing (edit only) ── */}
              {isEditMode && (
                <Section icon="🚀" title="Publishing" desc="Control your book's visibility" step={4}>
                  <PublishToggle
                    on={form.isPublished}
                    onChange={(v) => { setForm((p) => ({ ...p, isPublished: v })); setSaved(false); }}
                  />
                </Section>
              )}
            </div>

            {/* ═══ RIGHT: sticky preview ═══ */}
            <aside className={styles.aside}>
              <div className={styles.sticky}>
                <h3 className={styles.asideTitle}>📐 Live Preview</h3>
                <BookPreview3D
                  url={form.coverImageUrl}
                  title={form.title}
                  author={authorName}
                  genres={form.genres}
                />
                <div className={styles.sumTable}>
                  <div className={styles.sumRow}><span>Language</span><span>{langObj?.flag} {langObj?.label ?? form.language}</span></div>
                  <div className={styles.sumRow}><span>Visualization</span><span>{vizObj?.icon} {vizObj?.label}</span></div>
                  <div className={styles.sumRow}><span>Genres</span><span>{form.genres.length > 0 ? form.genres.join(', ') : '—'}</span></div>
                  <div className={styles.sumRow}><span>Tags</span><span>{form.tags.length || '—'}</span></div>
                  {isEditMode && (
                    <div className={styles.sumRow}>
                      <span>Status</span>
                      <span className={form.isPublished ? styles.live : styles.draft}>
                        {form.isPublished ? '🌐 Published' : '🔒 Draft'}
                      </span>
                    </div>
                  )}
                </div>
              </div>
            </aside>
          </div>

          {/* ═══ Bottom Bar ═══ */}
          <div className={styles.bottomBar}>
            <div className={styles.barLeft}>
              {dirty && !saved && <span className={styles.dirtyDot}>●</span>}
            </div>
            <div className={styles.barRight}>
              <button type="button" className={styles.cancelBtn} onClick={() => navigate(ROUTES.AUTHOR_DASHBOARD)}>
                Cancel
              </button>
              <button
                type="submit"
                className={`${styles.saveBtn} ${saving ? styles.saveBtnSpin : ''} ${saved ? styles.saveBtnDone : ''}`}
                disabled={saving || saved}
              >
                {saving ? (<><span className={styles.miniSpin} />Saving…</>) :
                  saved ? '✓ Saved!' :
                  isEditMode ? '💾 Save Changes' : '📚 Create Book'}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateBookPage;