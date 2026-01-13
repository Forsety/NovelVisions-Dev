import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import ApiService from '../services/catalog-api.service';
import { Author } from '../types/book.types';
import './AuthorsPage.css';

const AuthorsPage: React.FC = () => {
  const [authors, setAuthors] = useState<Author[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const navigate = useNavigate();

  // Данные для нового автора
  const [newAuthor, setNewAuthor] = useState({
    displayName: '',
    email: '',
    biography: ''
  });

  // Загружаем авторов при монтировании
  useEffect(() => {
    loadAuthors();
  }, []);

  const loadAuthors = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ApiService.getAuthors();
      setAuthors(data);
    } catch (err) {
      console.error('Ошибка загрузки авторов:', err);
      setError('Не удалось загрузить авторов');
      // Создаем фейковых авторов для демонстрации
      setAuthors(createMockAuthors());
    } finally {
      setLoading(false);
    }
  };

  // Добавление нового автора
  const handleAddAuthor = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Проверка заполнения полей
    if (!newAuthor.displayName || !newAuthor.email) {
      alert('Заполните имя и email автора');
      return;
    }

    try {
      // Здесь должен быть вызов API для создания автора
      // const created = await ApiService.createAuthor(newAuthor);
      
      // Пока добавляем локально для демонстрации
      const fakeNewAuthor: Author = {
        id: `author-${Date.now()}`,
        displayName: newAuthor.displayName,
        email: newAuthor.email,
        biography: newAuthor.biography,
        isVerified: false,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };
      
      setAuthors([fakeNewAuthor, ...authors]);
      setShowAddForm(false);
      setNewAuthor({ displayName: '', email: '', biography: '' });
      alert('Автор успешно добавлен!');
    } catch (err) {
      alert('Ошибка при добавлении автора');
    }
  };

  // Переход к книгам автора
  const handleAuthorClick = (author: Author) => {
    navigate(`/authors/${author.id}/books`);
  };

  return (
    <div className="authors-page">
      {/* Заголовок страницы */}
      <div className="authors-header">
        <h1>📚 Наши авторы</h1>
        <p>Талантливые писатели, создающие удивительные миры</p>
      </div>

      {/* Панель управления */}
      <div className="authors-controls">
        <button 
          className="btn-add-author"
          onClick={() => setShowAddForm(!showAddForm)}
        >
          {showAddForm ? '✖ Закрыть' : '➕ Добавить автора'}
        </button>
      </div>

      {/* Форма добавления автора */}
      {showAddForm && (
        <div className="add-author-form">
          <h3>Новый автор</h3>
          <form onSubmit={handleAddAuthor}>
            <div className="form-group">
              <label>Имя автора *</label>
              <input
                type="text"
                value={newAuthor.displayName}
                onChange={(e) => setNewAuthor({...newAuthor, displayName: e.target.value})}
                placeholder="Например: Иван Иванов"
                required
              />
            </div>
            
            <div className="form-group">
              <label>Email *</label>
              <input
                type="email"
                value={newAuthor.email}
                onChange={(e) => setNewAuthor({...newAuthor, email: e.target.value})}
                placeholder="author@example.com"
                required
              />
            </div>
            
            <div className="form-group">
              <label>Биография</label>
              <textarea
                value={newAuthor.biography}
                onChange={(e) => setNewAuthor({...newAuthor, biography: e.target.value})}
                placeholder="Расскажите об авторе..."
                rows={4}
              />
            </div>
            
            <div className="form-actions">
              <button type="submit" className="btn-submit">
                💾 Сохранить автора
              </button>
              <button 
                type="button" 
                className="btn-cancel"
                onClick={() => {
                  setShowAddForm(false);
                  setNewAuthor({ displayName: '', email: '', biography: '' });
                }}
              >
                Отмена
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Список авторов */}
      <div className="authors-content">
        {loading && (
          <div className="loading-state">
            <div className="spinner"></div>
            <p>Загружаем авторов...</p>
          </div>
        )}

        {error && (
          <div className="error-state">
            <p>⚠️ {error}</p>
            <button onClick={loadAuthors}>Попробовать снова</button>
          </div>
        )}

        {!loading && !error && authors.length === 0 && (
          <div className="empty-state">
            <p>📝 Пока нет авторов</p>
            <p>Добавьте первого автора!</p>
          </div>
        )}

        {!loading && !error && authors.length > 0 && (
          <div className="authors-grid">
            {authors.map(author => (
              <div 
                key={author.id} 
                className="author-card"
                onClick={() => handleAuthorClick(author)}
              >
                {/* Аватар автора */}
                <div className="author-avatar">
                  {author.displayName.charAt(0).toUpperCase()}
                </div>
                
                {/* Информация об авторе */}
                <div className="author-info">
                  <h3>{author.displayName}</h3>
                  {author.isVerified && (
                    <span className="verified-badge">✓ Проверен</span>
                  )}
                  <p className="author-email">{author.email}</p>
                  {author.biography && (
                    <p className="author-bio">{author.biography}</p>
                  )}
                  <p className="author-date">
                    Зарегистрирован: {new Date(author.createdAt).toLocaleDateString('ru-RU')}
                  </p>
                </div>
                
                {/* Действия */}
                <div className="author-actions">
                  <button className="btn-view-books">
                    📚 Книги автора
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

// Создаем фейковых авторов для демонстрации
function createMockAuthors(): Author[] {
  return [
    {
      id: 'author-1',
      displayName: 'Александр Пушкин',
      email: 'pushkin@example.com',
      biography: 'Великий русский поэт и писатель',
      isVerified: true,
      createdAt: '2024-01-15T10:00:00Z',
      updatedAt: '2024-01-15T10:00:00Z'
    },
    {
      id: 'author-2',
      displayName: 'Лев Толстой',
      email: 'tolstoy@example.com',
      biography: 'Автор романов "Война и мир" и "Анна Каренина"',
      isVerified: true,
      createdAt: '2024-01-20T10:00:00Z',
      updatedAt: '2024-01-20T10:00:00Z'
    },
    {
      id: 'author-3',
      displayName: 'Новый Автор',
      email: 'newauthor@example.com',
      biography: 'Начинающий писатель фантастики',
      isVerified: false,
      createdAt: '2024-10-01T10:00:00Z',
      updatedAt: '2024-10-01T10:00:00Z'
    }
  ];
}

export default AuthorsPage;