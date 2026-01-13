// src/pages/CatalogPage.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import BookCard from '../components/BookCard';
import CatalogApiService from '../services/catalog-api.service';
import { Book } from '../types/book.types';
import './СatalogPage.css';

const CatalogPage: React.FC = () => {
  const [books, setBooks] = useState<Book[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  
  // Пагинация
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 20;
  
  // Поиск и фильтры
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [selectedGenre, setSelectedGenre] = useState<string>('all');
  const [availableGenres, setAvailableGenres] = useState<string[]>([]);

  useEffect(() => {
    loadBooks();
  }, [currentPage, selectedGenre]);

  const loadBooks = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const genre = selectedGenre !== 'all' ? selectedGenre : undefined;
      const response = await CatalogApiService.getBooks(
        currentPage, 
        pageSize, 
        genre
      );
      
      // Преобразуем BookDto в Book формат
      const transformedBooks: Book[] = response.items.map(dto => ({
        id: dto.id,
        metadata: {
          title: dto.title,
          description: dto.description,
          coverImageUrl: dto.coverImageUrl,
          language: dto.language,
          pageCount: dto.pageCount,
          wordCount: dto.wordCount
        },
        authorId: dto.authorId,
        isbn: dto.isbn,
        publicationInfo: {
          publisher: dto.publisher,
          publicationDate: dto.publicationDate,
          edition: dto.edition
        },
        genres: dto.genres || [],
        tags: dto.tags || [],
        rating: dto.rating || 0,
        reviewCount: dto.reviewCount || 0,
        isPublished: dto.isPublished,
        createdAt: dto.createdAt,
        updatedAt: dto.updatedAt
      }));
      
      setBooks(transformedBooks);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
      
      // Извлекаем уникальные жанры
      if (availableGenres.length === 0 && transformedBooks.length > 0) {
        const genres = new Set<string>();
        transformedBooks.forEach(book => {
          book.genres.forEach(genre => genres.add(genre));
        });
        if (genres.size > 0) {
          setAvailableGenres(Array.from(genres).sort());
        } else {
          // Если жанры не загрузились, используем предустановленные
          setAvailableGenres(['Fantasy', 'Science Fiction', 'Romance', 'Thriller', 'Mystery', 'Horror']);
        }
      }
    } catch (err: any) {
      console.error('Ошибка при загрузке книг:', err);
      setError(err.message || 'Не удалось загрузить книги');
      
      // Если сервер недоступен, показываем заглушку
      if (err.message.includes('недоступен')) {
        setBooks([]);
        setAvailableGenres(['Fantasy', 'Science Fiction', 'Romance', 'Thriller', 'Mystery']);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async () => {
    if (!searchQuery.trim()) {
      loadBooks();
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const results = await CatalogApiService.searchBooks(searchQuery);
      
      // Преобразуем результаты поиска
      const transformedBooks: Book[] = results.map(dto => ({
        id: dto.id,
        metadata: {
          title: dto.title,
          description: dto.description,
          coverImageUrl: dto.coverImageUrl,
          language: dto.language,
          pageCount: dto.pageCount,
          wordCount: dto.wordCount
        },
        authorId: dto.authorId,
        isbn: dto.isbn,
        publicationInfo: {
          publisher: dto.publisher,
          publicationDate: dto.publicationDate,
          edition: dto.edition
        },
        genres: dto.genres || [],
        tags: dto.tags || [],
        rating: dto.rating || 0,
        reviewCount: dto.reviewCount || 0,
        isPublished: dto.isPublished,
        createdAt: dto.createdAt,
        updatedAt: dto.updatedAt
      }));
      
      setBooks(transformedBooks);
      setTotalPages(1);
      setCurrentPage(1);
    } catch (err: any) {
      console.error('Ошибка поиска:', err);
      setError(err.message || 'Ошибка при поиске книг');
    } finally {
      setLoading(false);
    }
  };

  const handleGenreFilter = (genre: string) => {
    setSelectedGenre(genre);
    setCurrentPage(1);
  };

  const handleBookClick = (book: Book) => {
    navigate(`/books/${book.id}`);
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const handlePageChange = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  return (
    <div className="catalog-page">
      <div className="catalog-header">
        <div className="catalog-banner">
          <h1>Dive into Stories.</h1>
          <h2>See Worlds.</h2>
        </div>
      </div>

      <div className="catalog-controls">
        <div className="search-section">
          <input
            type="text"
            className="search-input"
            placeholder="Поиск книг по названию..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyPress={handleKeyPress}
          />
          <button className="search-button" onClick={handleSearch}>
            🔍 Поиск
          </button>
        </div>

        <div className="genre-filter">
          <h3>Жанры:</h3>
          <div className="genre-buttons">
            <button
              className={`genre-button ${selectedGenre === 'all' ? 'active' : ''}`}
              onClick={() => handleGenreFilter('all')}
            >
              Все
            </button>
            {availableGenres.map(genre => (
              <button
                key={genre}
                className={`genre-button ${selectedGenre === genre ? 'active' : ''}`}
                onClick={() => handleGenreFilter(genre)}
              >
                {genre}
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="catalog-content">
        {loading && (
          <div className="loading-state">
            <div className="loading-spinner"></div>
            <p>Загружаем книги...</p>
          </div>
        )}

        {error && (
          <div className="error-state">
            <p>⚠️ {error}</p>
            <button onClick={loadBooks}>Попробовать снова</button>
            <p style={{ marginTop: '10px', fontSize: '14px', color: '#7f8c8d' }}>
              Убедитесь, что Catalog.API запущен на порту 5001
            </p>
          </div>
        )}

        {!loading && !error && books.length === 0 && (
          <div className="empty-state">
            <p>📚 Книги не найдены</p>
            <p>Добавьте первую книгу в каталог!</p>
          </div>
        )}

        {!loading && !error && books.length > 0 && (
          <>
            <h2 className="section-title">
              {selectedGenre === 'all' ? 'Все книги' : `Жанр: ${selectedGenre}`}
              <span className="book-count">({books.length} книг)</span>
            </h2>
            
            <div className="books-grid">
              {books.map(book => (
                <BookCard
                  key={book.id}
                  book={book}
                  onClick={handleBookClick}
                />
              ))}
            </div>

            {totalPages > 1 && (
              <div className="pagination" style={{ 
                display: 'flex', 
                justifyContent: 'center', 
                gap: '10px', 
                marginTop: '30px' 
              }}>
                <button
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                  style={{
                    padding: '8px 16px',
                    background: currentPage === 1 ? '#ecf0f1' : '#667eea',
                    color: currentPage === 1 ? '#95a5a6' : 'white',
                    border: 'none',
                    borderRadius: '20px',
                    cursor: currentPage === 1 ? 'not-allowed' : 'pointer'
                  }}
                >
                  ← Предыдущая
                </button>
                
                <span style={{ 
                  padding: '8px 16px', 
                  background: '#ecf0f1', 
                  borderRadius: '20px' 
                }}>
                  Страница {currentPage} из {totalPages}
                </span>
                
                <button
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage === totalPages}
                  style={{
                    padding: '8px 16px',
                    background: currentPage === totalPages ? '#ecf0f1' : '#667eea',
                    color: currentPage === totalPages ? '#95a5a6' : 'white',
                    border: 'none',
                    borderRadius: '20px',
                    cursor: currentPage === totalPages ? 'not-allowed' : 'pointer'
                  }}
                >
                  Следующая →
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default CatalogPage;