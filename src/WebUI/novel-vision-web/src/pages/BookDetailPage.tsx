// src/pages/BookDetailPage.tsx
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import CatalogApiService from '../services/catalog-api.service';
import { useAuth } from '../contexts/AuthContext';
import './BookDetailPage.css';

interface BookDetail {
  id: string;
  title: string;
  description: string;
  authorId: string;
  authorName?: string;
  coverImageUrl?: string;
  language: string;
  pageCount: number;
  wordCount: number;
  isbn?: string;
  publisher?: string;
  publicationDate?: string;
  edition?: string;
  genres: string[];
  tags: string[];
  rating: number;
  reviewCount: number;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
}

interface Chapter {
  id: string;
  bookId: string;
  chapterNumber: number;
  title: string;
  content?: string;
  pageCount: number;
  createdAt: string;
}

const BookDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();
  
  const [book, setBook] = useState<BookDetail | null>(null);
  const [chapters, setChapters] = useState<Chapter[]>([]);
  const [author, setAuthor] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAddChapter, setShowAddChapter] = useState(false);
  const [newChapterTitle, setNewChapterTitle] = useState('');

  useEffect(() => {
    if (id) {
      loadBookDetails();
    }
  }, [id]);

  const loadBookDetails = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Загружаем информацию о книге
      const bookData = await CatalogApiService.getBookById(id!);
      setBook(bookData);
      
      // Загружаем информацию об авторе
      try {
        const authorData = await CatalogApiService.getAuthorById(bookData.authorId);
        setAuthor(authorData);
      } catch (err) {
        console.error('Ошибка загрузки автора:', err);
      }
      
      // Загружаем главы книги
      try {
        const chaptersData = await CatalogApiService.getChapters(id!);
        setChapters(chaptersData);
      } catch (err) {
        console.error('Ошибка загрузки глав:', err);
      }
    } catch (err: any) {
      console.error('Ошибка загрузки книги:', err);
      setError(err.message || 'Не удалось загрузить книгу');
    } finally {
      setLoading(false);
    }
  };

  const handleAddChapter = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newChapterTitle.trim()) {
      alert('Введите название главы');
      return;
    }

    try {
      const newChapter = await CatalogApiService.createChapter(id!, {
        title: newChapterTitle.trim()
      });
      
      setChapters([...chapters, newChapter]);
      setNewChapterTitle('');
      setShowAddChapter(false);
      alert('Глава успешно добавлена!');
    } catch (err: any) {
      alert(err.message || 'Ошибка при добавлении главы');
    }
  };

  const canEditBook = () => {
    if (!isAuthenticated || !user || !book) return false;
    // Может редактировать админ или автор книги
    return user.role === 'Admin' || 
           (user.role === 'Author' && author?.email === user.email);
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Не указано';
    return new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const getReadingTime = (wordCount: number) => {
    const wordsPerMinute = 200;
    const minutes = Math.ceil(wordCount / wordsPerMinute);
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    
    if (hours > 0) {
      return `${hours} ч ${mins} мин`;
    }
    return `${mins} мин`;
  };

  if (loading) {
    return (
      <div className="book-detail-page">
        <div className="loading-state">
          <div className="loading-spinner"></div>
          <p>Загружаем книгу...</p>
        </div>
      </div>
    );
  }

  if (error || !book) {
    return (
      <div className="book-detail-page">
        <div className="error-state">
          <p>⚠️ {error || 'Книга не найдена'}</p>
          <button onClick={() => navigate('/')}>Вернуться к каталогу</button>
        </div>
      </div>
    );
  }

  return (
    <div className="book-detail-page">
      <div className="book-header">
        <div className="book-header-content">
          <div className="book-cover-section">
            {book.coverImageUrl ? (
              <img 
                src={book.coverImageUrl} 
                alt={book.title}
                className="book-cover-large"
                onError={(e) => {
                  (e.target as HTMLImageElement).src = 'https://via.placeholder.com/300x420?text=Нет+обложки';
                }}
              />
            ) : (
              <div className="book-cover-placeholder">
                <span>{book.title[0]}</span>
              </div>
            )}
            
            {book.isPublished && (
              <span className="published-badge">✅ Опубликовано</span>
            )}
          </div>

          <div className="book-info-section">
            <h1 className="book-title">{book.title}</h1>
            
            {author && (
              <Link to={`/authors/${author.id}/books`} className="book-author">
                {author.displayName}
                {author.isVerified && <span className="verified">✓</span>}
              </Link>
            )}

            <div className="book-meta">
              {book.rating > 0 && (
                <div className="rating">
                  ⭐ {book.rating.toFixed(1)} 
                  <span className="review-count">({book.reviewCount} отзывов)</span>
                </div>
              )}
              
              <div className="stats">
                📖 {book.pageCount || 0} страниц
                <span className="separator">•</span>
                ⏱️ {getReadingTime(book.wordCount || 0)} чтения
                <span className="separator">•</span>
                🌐 {book.language.toUpperCase()}
              </div>
            </div>

            <div className="book-description">
              <h3>Описание</h3>
              <p>{book.description}</p>
            </div>

            <div className="book-genres">
              {book.genres.map(genre => (
                <span key={genre} className="genre-tag">
                  {genre}
                </span>
              ))}
            </div>

            {book.tags.length > 0 && (
              <div className="book-tags">
                {book.tags.map(tag => (
                  <span key={tag} className="tag">
                    #{tag}
                  </span>
                ))}
              </div>
            )}

            <div className="book-actions">
              <button className="btn-read">
                📖 Начать чтение
              </button>
              
              {canEditBook() && (
                <button 
                  className="btn-edit"
                  onClick={() => alert('Редактирование будет доступно позже')}
                >
                  ✏️ Редактировать
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="book-content">
        <div className="book-details-grid">
          <div className="details-section">
            <h3>📚 Информация о публикации</h3>
            <dl>
              {book.isbn && (
                <>
                  <dt>ISBN:</dt>
                  <dd>{book.isbn}</dd>
                </>
              )}
              {book.publisher && (
                <>
                  <dt>Издательство:</dt>
                  <dd>{book.publisher}</dd>
                </>
              )}
              {book.publicationDate && (
                <>
                  <dt>Дата публикации:</dt>
                  <dd>{formatDate(book.publicationDate)}</dd>
                </>
              )}
              {book.edition && (
                <>
                  <dt>Издание:</dt>
                  <dd>{book.edition}</dd>
                </>
              )}
              <dt>Добавлено:</dt>
              <dd>{formatDate(book.createdAt)}</dd>
              <dt>Обновлено:</dt>
              <dd>{formatDate(book.updatedAt)}</dd>
            </dl>
          </div>

          <div className="chapters-section">
            <div className="chapters-header">
              <h3>📑 Главы ({chapters.length})</h3>
              {canEditBook() && (
                <button 
                  className="btn-add-chapter"
                  onClick={() => setShowAddChapter(!showAddChapter)}
                >
                  ➕ Добавить главу
                </button>
              )}
            </div>

            {showAddChapter && (
              <form onSubmit={handleAddChapter} className="add-chapter-form">
                <input
                  type="text"
                  value={newChapterTitle}
                  onChange={(e) => setNewChapterTitle(e.target.value)}
                  placeholder="Название главы"
                  required
                />
                <div className="form-actions">
                  <button type="submit">Добавить</button>
                  <button 
                    type="button" 
                    onClick={() => {
                      setShowAddChapter(false);
                      setNewChapterTitle('');
                    }}
                  >
                    Отмена
                  </button>
                </div>
              </form>
            )}

            {chapters.length === 0 ? (
              <p className="no-chapters">Пока нет глав</p>
            ) : (
              <div className="chapters-list">
                {chapters.map((chapter, index) => (
                  <div key={chapter.id} className="chapter-item">
                    <span className="chapter-number">Глава {chapter.chapterNumber || index + 1}</span>
                    <span className="chapter-title">{chapter.title}</span>
                    <span className="chapter-pages">{chapter.pageCount || 0} стр.</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {author && author.biography && (
          <div className="author-section">
            <h3>✍️ Об авторе</h3>
            <p>{author.biography}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default BookDetailPage;