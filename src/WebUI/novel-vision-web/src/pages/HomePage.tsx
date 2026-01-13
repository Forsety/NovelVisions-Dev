// src/pages/HomePage.tsx
import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Book } from '../types/book.types';
import CatalogApiService from '../services/catalog-api.service';
import BookCard from '../components/BookCard';
import './HomePage.css';

const HomePage: React.FC = () => {
  const navigate = useNavigate();
  const [featuredBooks, setFeaturedBooks] = useState<Book[]>([]);
  const [recentBooks, setRecentBooks] = useState<Book[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeGenre, setActiveGenre] = useState('all');

  const genres = [
    { id: 'all', name: 'All', icon: '📚' },
    { id: 'fantasy', name: 'Fantasy', icon: '🐉' },
    { id: 'romance', name: 'Romance', icon: '💕' },
    { id: 'mystery', name: 'Mystery', icon: '🔍' },
    { id: 'scifi', name: 'Sci-Fi', icon: '🚀' },
    { id: 'thriller', name: 'Thriller', icon: '😱' },
    { id: 'classics', name: 'Classics', icon: '📜' }
  ];

  const stats = [
    { value: '10K+', label: 'Books', icon: '📖' },
    { value: '5K+', label: 'Authors', icon: '✍️' },
    { value: '1M+', label: 'Visualizations', icon: '🎨' },
    { value: '50K+', label: 'Readers', icon: '👥' }
  ];

  useEffect(() => {
    loadBooks();
  }, []);

  const loadBooks = async () => {
    try {
      setLoading(true);
      const response = await CatalogApiService.getBooks(1, 12);
      const books = response.items.map(dto => ({
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
        genres: dto.genres || [],
        tags: dto.tags || [],
        rating: dto.rating || 4.5,
        reviewCount: dto.reviewCount || 0,
        isPublished: dto.isPublished,
        createdAt: dto.createdAt,
        updatedAt: dto.updatedAt
      })) as Book[];

      setFeaturedBooks(books.slice(0, 6));
      setRecentBooks(books.slice(6, 12));
    } catch (error) {
      console.error('Failed to load books:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleBookClick = (book: Book) => {
    navigate(`/books/${book.id}`);
  };

  return (
    <div className="home-page">
      {/* Hero Section */}
      <section className="hero">
        <div className="hero-particles">
          {[...Array(50)].map((_, i) => (
            <div key={i} className="particle" style={{
              left: `${Math.random() * 100}%`,
              animationDelay: `${Math.random() * 5}s`,
              animationDuration: `${3 + Math.random() * 4}s`
            }} />
          ))}
        </div>
        
        <div className="hero-content">
          <div className="hero-badge">
            <span className="badge-icon">✨</span>
            <span>AI-Powered Reading Experience</span>
          </div>
          
          <h1 className="hero-title">
            <span className="gradient-text">Dive into Stories.</span>
            <br />
            <span className="highlight-text">See Worlds.</span>
          </h1>
          
          <p className="hero-subtitle">
            Experience books like never before. Every page comes alive with 
            AI-generated illustrations tailored to your imagination.
          </p>
          
          <div className="hero-actions">
            <Link to="/catalog" className="btn-primary">
              <span className="btn-icon">🚀</span>
              Start Exploring
            </Link>
            <Link to="/about" className="btn-secondary">
              <span className="btn-icon">▶️</span>
              Watch Demo
            </Link>
          </div>

          <div className="hero-stats">
            {stats.map((stat, index) => (
              <div key={index} className="stat-item">
                <span className="stat-icon">{stat.icon}</span>
                <span className="stat-value">{stat.value}</span>
                <span className="stat-label">{stat.label}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="hero-visual">
          <div className="floating-books">
            <div className="book-3d book-1">
              <div className="book-cover">📖</div>
            </div>
            <div className="book-3d book-2">
              <div className="book-cover">📚</div>
            </div>
            <div className="book-3d book-3">
              <div className="book-cover">📕</div>
            </div>
          </div>
          <div className="magic-circle"></div>
          <div className="glow-orb"></div>
        </div>
      </section>

      {/* Features Section */}
      <section className="features-section">
        <div className="container">
          <div className="section-header">
            <h2 className="section-title">
              <span className="title-accent">🔮</span>
              The Future of Reading
            </h2>
            <p className="section-subtitle">
              Powered by cutting-edge AI to transform your reading experience
            </p>
          </div>

          <div className="features-grid">
            <div className="feature-card feature-ai">
              <div className="feature-icon-wrapper">
                <div className="feature-icon">🎨</div>
                <div className="feature-glow"></div>
              </div>
              <h3>AI Visualization</h3>
              <p>Every scene, every character brought to life with stunning AI-generated artwork</p>
              <div className="feature-tags">
                <span>DALL-E 3</span>
                <span>Midjourney</span>
                <span>Stable Diffusion</span>
              </div>
            </div>

            <div className="feature-card feature-reader">
              <div className="feature-icon-wrapper">
                <div className="feature-icon">📱</div>
                <div className="feature-glow"></div>
              </div>
              <h3>Immersive Reader</h3>
              <p>Customizable reading experience with dark mode, fonts, and visual preferences</p>
              <div className="feature-tags">
                <span>Dark Mode</span>
                <span>Custom Fonts</span>
                <span>Sync</span>
              </div>
            </div>

            <div className="feature-card feature-author">
              <div className="feature-icon-wrapper">
                <div className="feature-icon">✍️</div>
                <div className="feature-glow"></div>
              </div>
              <h3>Author Tools</h3>
              <p>Publish your work with powerful visualization controls and analytics</p>
              <div className="feature-tags">
                <span>Publishing</span>
                <span>Analytics</span>
                <span>Monetization</span>
              </div>
            </div>

            <div className="feature-card feature-community">
              <div className="feature-icon-wrapper">
                <div className="feature-icon">👥</div>
                <div className="feature-glow"></div>
              </div>
              <h3>Community</h3>
              <p>Connect with readers and authors, share visualizations, discuss books</p>
              <div className="feature-tags">
                <span>Reviews</span>
                <span>Sharing</span>
                <span>Collections</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Genre Filter */}
      <section className="genres-section">
        <div className="container">
          <div className="genre-filter">
            {genres.map(genre => (
              <button
                key={genre.id}
                className={`genre-btn ${activeGenre === genre.id ? 'active' : ''}`}
                onClick={() => setActiveGenre(genre.id)}
              >
                <span className="genre-icon">{genre.icon}</span>
                <span className="genre-name">{genre.name}</span>
              </button>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Books */}
      <section className="books-section">
        <div className="container">
          <div className="section-header">
            <h2 className="section-title">
              <span className="title-accent">⭐</span>
              Featured Books
            </h2>
            <Link to="/catalog" className="view-all-link">
              View All
              <span className="arrow">→</span>
            </Link>
          </div>

          {loading ? (
            <div className="loading-container">
              <div className="loading-spinner"></div>
              <p>Loading amazing books...</p>
            </div>
          ) : (
            <div className="books-grid">
              {featuredBooks.map(book => (
                <BookCard 
                  key={book.id} 
                  book={book} 
                  onClick={handleBookClick}
                />
              ))}
            </div>
          )}
        </div>
      </section>

      {/* How It Works */}
      <section className="how-it-works">
        <div className="container">
          <div className="section-header centered">
            <h2 className="section-title">
              <span className="title-accent">🪄</span>
              How It Works
            </h2>
            <p className="section-subtitle">
              Three simple steps to transform your reading experience
            </p>
          </div>

          <div className="steps-container">
            <div className="step-card">
              <div className="step-number">01</div>
              <div className="step-icon">📖</div>
              <h3>Choose a Book</h3>
              <p>Browse our extensive library or upload your own favorite stories</p>
            </div>

            <div className="step-connector">
              <div className="connector-line"></div>
              <div className="connector-dot"></div>
            </div>

            <div className="step-card">
              <div className="step-number">02</div>
              <div className="step-icon">⚙️</div>
              <h3>Select Mode</h3>
              <p>Choose how you want AI to visualize: every page, chapter, or on-demand</p>
            </div>

            <div className="step-connector">
              <div className="connector-line"></div>
              <div className="connector-dot"></div>
            </div>

            <div className="step-card">
              <div className="step-number">03</div>
              <div className="step-icon">✨</div>
              <h3>Experience Magic</h3>
              <p>Watch as AI brings every scene to life with stunning visualizations</p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="cta-section">
        <div className="cta-background">
          <div className="cta-gradient"></div>
          <div className="cta-particles">
            {[...Array(30)].map((_, i) => (
              <div key={i} className="cta-particle" style={{
                left: `${Math.random() * 100}%`,
                top: `${Math.random() * 100}%`,
                animationDelay: `${Math.random() * 3}s`
              }} />
            ))}
          </div>
        </div>
        
        <div className="cta-content">
          <h2>Ready to See Your Stories Come Alive?</h2>
          <p>Join thousands of readers experiencing the future of literature</p>
          <div className="cta-actions">
            <Link to="/register" className="btn-cta-primary">
              Get Started Free
            </Link>
            <Link to="/catalog" className="btn-cta-secondary">
              Browse Library
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;