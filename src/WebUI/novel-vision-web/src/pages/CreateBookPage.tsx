// src/pages/CreateBookPage.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import CatalogApiService from '../services/catalog-api.service';
import { Author } from '../types/book.types';
import { useAuth } from '../contexts/AuthContext';
import './CreateBookPage.css';

const CreateBookPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [authors, setAuthors] = useState<Author[]>([]);
  const [loading, setLoading] = useState(false);
  
  const [bookData, setBookData] = useState({
    title: '',
    description: '',
    authorId: '',
    coverImageUrl: '',
    language: 'ru',
    isbn: '',
    publisher: '',
    publicationDate: '',
    edition: '',
    genres: [] as string[],
    tags: [] as string[]
  });

  const [genreInput, setGenreInput] = useState('');
  const [tagInput, setTagInput] = useState('');

  const predefinedGenres = [
    'Fantasy', 'Science Fiction', 'Romance', 'Thriller', 
    'Mystery', 'Horror', 'Poetry', 'Drama', 'Adventure', 
    'Historical', 'Biography', 'Self-Help'
  ];

  useEffect(() => {
    loadAuthors();
    // Если пользователь - автор, автоматически выбираем его
    checkUserAuthor();
  }, []);

  const loadAuthors = async () => {
    try {
      const data = await CatalogApiService.getAuthors();
      setAuthors(data);
    } catch (err) {
      console.error('Ошибка загрузки авторов:', err);
      // Если не удалось загрузить, оставляем пустой список
      setAuthors([]);
    }
  };

  const checkUserAuthor = async () => {
    // Если пользователь - автор, пытаемся найти его в списке авторов
    if (user && user.role === 'Author') {
      try {
        const authors = await CatalogApiService.getAuthors();
        const userAuthor = authors.find(a => a.email === user.email);
        if (userAuthor) {
          setBookData(prev => ({ ...prev, authorId: userAuthor.id }));
        }
      } catch (err) {
        console.error('Ошибка поиска автора пользователя:', err);
      }
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!bookData.title || !bookData.description || !bookData.authorId) {
      alert('Заполните обязательные поля: название, описание и автор');
      return;
    }

    if (bookData.genres.length === 0) {
      alert('Выберите хотя бы один жанр');
      return;
    }

    try {
      setLoading(true);
      
      const createdBook = await CatalogApiService.createBook({
        title: bookData.title,
        description: bookData.description,
        authorId: bookData.authorId,
        coverImageUrl: bookData.coverImageUrl || undefined,
        language: bookData.language,
        isbn: bookData.isbn || undefined,
        publisher: bookData.publisher || undefined,
        publicationDate: bookData.publicationDate || undefined,
        edition: bookData.edition || undefined,
        genres: bookData.genres,
        tags: bookData.tags
      });
      
      alert('Книга успешно создана!');
      navigate(`/books/${createdBook.id}`);
    } catch (err: any) {
      console.error('Ошибка создания книги:', err);
      alert(err.message || 'Ошибка при создании книги');
    } finally {
      setLoading(false);
    }
  };

  const addGenre = (genre: string) => {
    if (genre && !bookData.genres.includes(genre)) {
      setBookData({
        ...bookData,
        genres: [...bookData.genres, genre]
      });
    }
  };

  const removeGenre = (genre: string) => {
    setBookData({
      ...bookData,
      genres: bookData.genres.filter(g => g !== genre)
    });
  };

  const addTag = () => {
    if (tagInput.trim() && !bookData.tags.includes(tagInput.trim())) {
      setBookData({
        ...bookData,
        tags: [...bookData.tags, tagInput.trim()]
      });
      setTagInput('');
    }
  };

  const removeTag = (tag: string) => {
    setBookData({
      ...bookData,
      tags: bookData.tags.filter(t => t !== tag)
    });
  };

  return (
    <div className="create-book-page">
      <div className="create-book-header">
        <h1>✍️ Создать новую книгу</h1>
        <p>Поделитесь своей историей с миром</p>
      </div>

      <div className="create-book-content">
        <form onSubmit={handleSubmit} className="book-form">
          <div className="form-section">
            <h2>📖 Основная информация</h2>
            
            <div className="form-group">
              <label>Название книги *</label>
              <input
                type="text"
                value={bookData.title}
                onChange={(e) => setBookData({...bookData, title: e.target.value})}
                placeholder="Введите название вашей книги"
                required
              />
            </div>

            <div className="form-group">
              <label>Описание *</label>
              <textarea
                value={bookData.description}
                onChange={(e) => setBookData({...bookData, description: e.target.value})}
                placeholder="О чем ваша книга? Заинтригуйте читателей..."
                rows={5}
                required
              />
            </div>

            <div className="form-group">
              <label>Автор *</label>
              <select
                value={bookData.authorId}
                onChange={(e) => setBookData({...bookData, authorId: e.target.value})}
                required
              >
                <option value="">Выберите автора</option>
                {authors.map(author => (
                  <option key={author.id} value={author.id}>
                    {author.displayName} {author.isVerified && '✓'}
                  </option>
                ))}
              </select>
              {authors.length === 0 && (
                <small style={{ color: '#e74c3c' }}>
                  Нет доступных авторов. <a href="/authors">Добавьте автора сначала</a>
                </small>
              )}
            </div>

            <div className="form-group">
              <label>URL обложки</label>
              <input
                type="url"
                value={bookData.coverImageUrl}
                onChange={(e) => setBookData({...bookData, coverImageUrl: e.target.value})}
                placeholder="https://example.com/cover.jpg"
              />
              {bookData.coverImageUrl && (
                <div className="cover-preview">
                  <img 
                    src={bookData.coverImageUrl} 
                    alt="Предпросмотр обложки"
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = 'https://via.placeholder.com/200x280?text=Ошибка+загрузки';
                    }}
                  />
                </div>
              )}
            </div>

            <div className="form-group">
              <label>Язык книги</label>
              <select
                value={bookData.language}
                onChange={(e) => setBookData({...bookData, language: e.target.value})}
              >
                <option value="ru">Русский</option>
                <option value="en">English</option>
                <option value="es">Español</option>
                <option value="fr">Français</option>
                <option value="de">Deutsch</option>
              </select>
            </div>
          </div>

          <div className="form-section">
            <h2>📚 Информация о публикации</h2>
            
            <div className="form-row">
              <div className="form-group">
                <label>ISBN</label>
                <input
                  type="text"
                  value={bookData.isbn}
                  onChange={(e) => setBookData({...bookData, isbn: e.target.value})}
                  placeholder="978-3-16-148410-0"
                />
              </div>

              <div className="form-group">
                <label>Издательство</label>
                <input
                  type="text"
                  value={bookData.publisher}
                  onChange={(e) => setBookData({...bookData, publisher: e.target.value})}
                  placeholder="Название издательства"
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Дата публикации</label>
                <input
                  type="date"
                  value={bookData.publicationDate}
                  onChange={(e) => setBookData({...bookData, publicationDate: e.target.value})}
                />
              </div>

              <div className="form-group">
                <label>Издание</label>
                <input
                  type="text"
                  value={bookData.edition}
                  onChange={(e) => setBookData({...bookData, edition: e.target.value})}
                  placeholder="Первое издание"
                />
              </div>
            </div>
          </div>

          <div className="form-section">
            <h2>🏷️ Жанры и теги</h2>
            
            <div className="form-group">
              <label>Жанры * (выберите хотя бы один)</label>
              <div className="genre-selector">
                {predefinedGenres.map(genre => (
                  <button
                    key={genre}
                    type="button"
                    className={`genre-chip ${bookData.genres.includes(genre) ? 'selected' : ''}`}
                    onClick={() => {
                      if (bookData.genres.includes(genre)) {
                        removeGenre(genre);
                      } else {
                        addGenre(genre);
                      }
                    }}
                  >
                    {genre}
                    {bookData.genres.includes(genre) && ' ✓'}
                  </button>
                ))}
              </div>
              
              <div className="custom-genre">
                <input
                  type="text"
                  value={genreInput}
                  onChange={(e) => setGenreInput(e.target.value)}
                  placeholder="Добавить свой жанр"
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault();
                      addGenre(genreInput);
                      setGenreInput('');
                    }
                  }}
                />
                <button
                  type="button"
                  onClick={() => {
                    addGenre(genreInput);
                    setGenreInput('');
                  }}
                >
                  Добавить
                </button>
              </div>

              {bookData.genres.length > 0 && (
                <div className="selected-items">
                  <strong>Выбрано:</strong>
                  {bookData.genres.map(genre => (
                    <span key={genre} className="selected-chip">
                      {genre}
                      <button
                        type="button"
                        onClick={() => removeGenre(genre)}
                        className="remove-btn"
                      >
                        ✖
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>

            <div className="form-group">
              <label>Теги (ключевые слова)</label>
              <div className="tag-input">
                <input
                  type="text"
                  value={tagInput}
                  onChange={(e) => setTagInput(e.target.value)}
                  placeholder="Добавьте теги для поиска"
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault();
                      addTag();
                    }
                  }}
                />
                <button type="button" onClick={addTag}>
                  Добавить тег
                </button>
              </div>

              {bookData.tags.length > 0 && (
                <div className="selected-items">
                  {bookData.tags.map(tag => (
                    <span key={tag} className="selected-chip">
                      #{tag}
                      <button
                        type="button"
                        onClick={() => removeTag(tag)}
                        className="remove-btn"
                      >
                        ✖
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>
          </div>

          <div className="form-actions">
            <button
              type="submit"
              className="btn-primary"
              disabled={loading}
            >
              {loading ? 'Создание...' : '📚 Создать книгу'}
            </button>
            <button
              type="button"
              className="btn-secondary"
              onClick={() => navigate('/')}
            >
              Отмена
            </button>
          </div>
        </form>

        <div className="tips-sidebar">
          <h3>💡 Советы</h3>
          <ul>
            <li>Название должно быть запоминающимся</li>
            <li>Описание - ваш шанс заинтересовать читателей</li>
            <li>Выберите 2-3 основных жанра</li>
            <li>Теги помогут найти вашу книгу</li>
            <li>Качественная обложка привлекает внимание</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default CreateBookPage;