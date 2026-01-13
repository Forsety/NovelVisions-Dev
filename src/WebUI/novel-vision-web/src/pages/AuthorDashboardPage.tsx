// src/pages/AuthorDashboardPage.tsx
import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import CatalogApiService from '../services/catalog-api.service';
import './AuthorDashboardPage.css';

interface BookStats {
  id: string;
  title: string;
  coverUrl?: string;
  views: number;
  likes: number;
  visualizations: number;
  chapters: number;
  pages: number;
  status: 'draft' | 'published' | 'archived';
  lastUpdated: string;
}

const AuthorDashboardPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [books, setBooks] = useState<BookStats[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'all' | 'published' | 'drafts'>('all');

  // Stats
  const [stats, setStats] = useState({
    totalBooks: 0,
    totalViews: 0,
    totalLikes: 0,
    totalVisualizations: 0
  });

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      // In real app, this would fetch from API
      // For now, using mock data
      const mockBooks: BookStats[] = [
        {
          id: '1',
          title: 'The Crystal Kingdom',
          coverUrl: '',
          views: 12450,
          likes: 892,
          visualizations: 3421,
          chapters: 24,
          pages: 312,
          status: 'published',
          lastUpdated: new Date().toISOString()
        },
        {
          id: '2',
          title: 'Shadows of Tomorrow',
          coverUrl: '',
          views: 8230,
          likes: 645,
          visualizations: 2156,
          chapters: 18,
          pages: 245,
          status: 'published',
          lastUpdated: new Date(Date.now() - 86400000).toISOString()
        },
        {
          id: '3',
          title: 'New Story (Draft)',
          coverUrl: '',
          views: 0,
          likes: 0,
          visualizations: 0,
          chapters: 5,
          pages: 42,
          status: 'draft',
          lastUpdated: new Date(Date.now() - 172800000).toISOString()
        }
      ];

      setBooks(mockBooks);
      setStats({
        totalBooks: mockBooks.length,
        totalViews: mockBooks.reduce((sum, b) => sum + b.views, 0),
        totalLikes: mockBooks.reduce((sum, b) => sum + b.likes, 0),
        totalVisualizations: mockBooks.reduce((sum, b) => sum + b.visualizations, 0)
      });
    } catch (error) {
      console.error('Failed to load dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredBooks = books.filter(book => {
    if (activeTab === 'all') return true;
    if (activeTab === 'published') return book.status === 'published';
    if (activeTab === 'drafts') return book.status === 'draft';
    return true;
  });

  const formatNumber = (num: number): string => {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
  };

  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / 86400000);
    
    if (days === 0) return 'Today';
    if (days === 1) return 'Yesterday';
    if (days < 7) return `${days} days ago`;
    return date.toLocaleDateString();
  };

  return (
    <div className="dashboard-page">
      {/* Header */}
      <header className="dashboard-header">
        <div className="header-content">
          <div className="header-left">
            <h1>Author Dashboard</h1>
            <p className="welcome-text">
              Welcome back, <strong>{user?.firstName || 'Author'}</strong>! 
              Here's how your stories are performing.
            </p>
          </div>
          <div className="header-actions">
            <Link to="/author/books/new" className="btn-create">
              <span className="btn-icon">✨</span>
              Create New Book
            </Link>
          </div>
        </div>
      </header>

      {/* Stats Cards */}
      <section className="stats-section">
        <div className="stats-grid">
          <div className="stat-card stat-books">
            <div className="stat-icon">📚</div>
            <div className="stat-content">
              <span className="stat-value">{stats.totalBooks}</span>
              <span className="stat-label">Total Books</span>
            </div>
            <div className="stat-trend up">+2 this month</div>
          </div>

          <div className="stat-card stat-views">
            <div className="stat-icon">👁️</div>
            <div className="stat-content">
              <span className="stat-value">{formatNumber(stats.totalViews)}</span>
              <span className="stat-label">Total Views</span>
            </div>
            <div className="stat-trend up">+12% this week</div>
          </div>

          <div className="stat-card stat-likes">
            <div className="stat-icon">❤️</div>
            <div className="stat-content">
              <span className="stat-value">{formatNumber(stats.totalLikes)}</span>
              <span className="stat-label">Total Likes</span>
            </div>
            <div className="stat-trend up">+8% this week</div>
          </div>

          <div className="stat-card stat-viz">
            <div className="stat-icon">🎨</div>
            <div className="stat-content">
              <span className="stat-value">{formatNumber(stats.totalVisualizations)}</span>
              <span className="stat-label">Visualizations</span>
            </div>
            <div className="stat-trend up">+24% this week</div>
          </div>
        </div>
      </section>

      {/* Books Section */}
      <section className="books-section">
        <div className="section-header">
          <h2>Your Books</h2>
          <div className="tab-filter">
            <button 
              className={`tab-btn ${activeTab === 'all' ? 'active' : ''}`}
              onClick={() => setActiveTab('all')}
            >
              All ({books.length})
            </button>
            <button 
              className={`tab-btn ${activeTab === 'published' ? 'active' : ''}`}
              onClick={() => setActiveTab('published')}
            >
              Published ({books.filter(b => b.status === 'published').length})
            </button>
            <button 
              className={`tab-btn ${activeTab === 'drafts' ? 'active' : ''}`}
              onClick={() => setActiveTab('drafts')}
            >
              Drafts ({books.filter(b => b.status === 'draft').length})
            </button>
          </div>
        </div>

        {loading ? (
          <div className="loading-state">
            <div className="loading-spinner"></div>
            <p>Loading your books...</p>
          </div>
        ) : filteredBooks.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">📝</div>
            <h3>No books yet</h3>
            <p>Start your journey as an author by creating your first book</p>
            <Link to="/author/books/new" className="btn-create">
              <span>✨</span> Create Your First Book
            </Link>
          </div>
        ) : (
          <div className="books-table">
            <div className="table-header">
              <div className="col-book">Book</div>
              <div className="col-stats">Views</div>
              <div className="col-stats">Likes</div>
              <div className="col-stats">AI Viz</div>
              <div className="col-content">Content</div>
              <div className="col-status">Status</div>
              <div className="col-actions">Actions</div>
            </div>

            {filteredBooks.map(book => (
              <div key={book.id} className="table-row">
                <div className="col-book">
                  <div className="book-cover">
                    {book.coverUrl ? (
                      <img src={book.coverUrl} alt={book.title} />
                    ) : (
                      <div className="cover-placeholder">📖</div>
                    )}
                  </div>
                  <div className="book-info">
                    <h4 className="book-title">{book.title}</h4>
                    <span className="book-updated">Updated {formatDate(book.lastUpdated)}</span>
                  </div>
                </div>

                <div className="col-stats">
                  <span className="stat-number">{formatNumber(book.views)}</span>
                </div>

                <div className="col-stats">
                  <span className="stat-number">{formatNumber(book.likes)}</span>
                </div>

                <div className="col-stats">
                  <span className="stat-number">{formatNumber(book.visualizations)}</span>
                </div>

                <div className="col-content">
                  <span className="content-stat">{book.chapters} chapters</span>
                  <span className="content-stat">{book.pages} pages</span>
                </div>

                <div className="col-status">
                  <span className={`status-badge status-${book.status}`}>
                    {book.status === 'published' && '🟢'}
                    {book.status === 'draft' && '🟡'}
                    {book.status === 'archived' && '⚫'}
                    {book.status.charAt(0).toUpperCase() + book.status.slice(1)}
                  </span>
                </div>

                <div className="col-actions">
                  <button 
                    className="action-btn" 
                    title="Edit"
                    onClick={() => navigate(`/author/books/${book.id}/edit`)}
                  >
                    ✏️
                  </button>
                  <button 
                    className="action-btn" 
                    title="View"
                    onClick={() => navigate(`/books/${book.id}`)}
                  >
                    👁️
                  </button>
                  <button 
                    className="action-btn" 
                    title="Analytics"
                    onClick={() => navigate(`/author/books/${book.id}/analytics`)}
                  >
                    📊
                  </button>
                  <button className="action-btn action-more" title="More">
                    ⋯
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Quick Actions */}
      <section className="quick-actions-section">
        <h2>Quick Actions</h2>
        <div className="quick-actions-grid">
          <Link to="/author/books/new" className="quick-action-card">
            <span className="action-icon">📝</span>
            <span className="action-title">Write New Chapter</span>
          </Link>
          <Link to="/author/visualizations" className="quick-action-card">
            <span className="action-icon">🎨</span>
            <span className="action-title">Manage Visualizations</span>
          </Link>
          <Link to="/author/analytics" className="quick-action-card">
            <span className="action-icon">📈</span>
            <span className="action-title">View Analytics</span>
          </Link>
          <Link to="/author/settings" className="quick-action-card">
            <span className="action-icon">⚙️</span>
            <span className="action-title">Author Settings</span>
          </Link>
        </div>
      </section>

      {/* Recent Activity */}
      <section className="activity-section">
        <h2>Recent Activity</h2>
        <div className="activity-list">
          <div className="activity-item">
            <div className="activity-icon">❤️</div>
            <div className="activity-content">
              <p><strong>John D.</strong> liked <strong>The Crystal Kingdom</strong></p>
              <span className="activity-time">2 minutes ago</span>
            </div>
          </div>
          <div className="activity-item">
            <div className="activity-icon">🎨</div>
            <div className="activity-content">
              <p><strong>Sarah M.</strong> created 3 visualizations in <strong>Shadows of Tomorrow</strong></p>
              <span className="activity-time">15 minutes ago</span>
            </div>
          </div>
          <div className="activity-item">
            <div className="activity-icon">💬</div>
            <div className="activity-content">
              <p><strong>Mike R.</strong> commented on <strong>The Crystal Kingdom</strong></p>
              <span className="activity-time">1 hour ago</span>
            </div>
          </div>
          <div className="activity-item">
            <div className="activity-icon">👁️</div>
            <div className="activity-content">
              <p><strong>150 readers</strong> viewed your books today</p>
              <span className="activity-time">Today</span>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default AuthorDashboardPage;