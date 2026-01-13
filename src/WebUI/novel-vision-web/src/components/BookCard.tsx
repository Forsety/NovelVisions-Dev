import React from 'react';
import { Book } from '../types/book.types';
import './BookCard.css';

// Интерфейс для пропсов компонента
interface BookCardProps {
  book: Book;
  onClick?: (book: Book) => void;
}

// Компонент карточки книги
const BookCard: React.FC<BookCardProps> = ({ book, onClick }) => {
  // Обработчик клика по карточке
  const handleClick = () => {
    if (onClick) {
      onClick(book);
    }
  };

  // Форматируем дату публикации
  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Не указано';
    return new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  // Обрезаем описание если оно слишком длинное
  const truncateDescription = (text: string, maxLength: number = 150) => {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
  };

  return (
    <div className="book-card" onClick={handleClick}>
      {/* Обложка книги */}
      <div className="book-card__cover">
        {book.metadata.coverImageUrl ? (
          <img 
            src={book.metadata.coverImageUrl} 
            alt={book.metadata.title}
            className="book-card__image"
          />
        ) : (
          <div className="book-card__placeholder">
            <span>{book.metadata.title[0]}</span>
          </div>
        )}
        
        {/* Бейдж если книга новая (добавлена менее 7 дней назад) */}
        {isNewBook(book.createdAt) && (
          <span className="book-card__badge">Новинка</span>
        )}
      </div>

      {/* Информация о книге */}
      <div className="book-card__info">
        <h3 className="book-card__title">{book.metadata.title}</h3>
        
        {/* Автор - пока показываем только ID, потом добавим имя */}
        <p className="book-card__author">ID автора: {book.authorId}</p>
        
        {/* Описание */}
        <p className="book-card__description">
          {truncateDescription(book.metadata.description)}
        </p>
        
        {/* Жанры */}
        {book.genres.length > 0 && (
          <div className="book-card__genres">
            {book.genres.slice(0, 3).map((genre, index) => (
              <span key={index} className="book-card__genre">
                {genre}
              </span>
            ))}
            {book.genres.length > 3 && (
              <span className="book-card__genre">+{book.genres.length - 3}</span>
            )}
          </div>
        )}
        
        {/* Дополнительная информация */}
        <div className="book-card__meta">
          <span className="book-card__pages">
            📖 {book.metadata.pageCount} стр.
          </span>
          {book.rating > 0 && (
            <span className="book-card__rating">
              ⭐ {book.rating.toFixed(1)} ({book.reviewCount})
            </span>
          )}
        </div>
        
        {/* Дата публикации */}
        {book.publicationInfo?.publicationDate && (
          <p className="book-card__date">
            Опубликовано: {formatDate(book.publicationInfo.publicationDate)}
          </p>
        )}
      </div>
    </div>
  );
};

// Вспомогательная функция для проверки, является ли книга новой
function isNewBook(createdAt: string): boolean {
  const createdDate = new Date(createdAt);
  const now = new Date();
  const diffInDays = (now.getTime() - createdDate.getTime()) / (1000 * 3600 * 24);
  return diffInDays <= 7;
}

export default BookCard;