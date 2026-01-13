// src/pages/ProfilePage.tsx
import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './ProfilePage.css';

interface ReadingHistory {
  id: string;
  bookId: string;
  bookTitle: string;
  coverUrl?: string;
  progress: number;
  lastRead: string;
  totalPages: number;
  currentPage: number;
}

interface Bookmark {
  id: string;
  bookId: string;
  bookTitle: string;
  chapterTitle: string;
  pageNumber: number;
  note?: string;
  createdAt: string;
}

const ProfilePage: React.FC = () => {
  const { user, logout } = useAuth();
  const [activeTab, setActiveTab] = useState<'reading' | 'bookmarks' | 'visualizations' | 'settings'>('reading');
  const [readingHistory, setReadingHistory] = useState<ReadingHistory[]>([]);
  const [bookmarks, setBookmarks] = useState<Bookmark[]>([]);
  const [loading, setLoading] = useState(true);

  // Settings state
  const [settings, setSettings] = useState({
    emailNotifications: true,
    weeklyDigest: true,
    darkMode: true,
    autoVisualize: false,
    preferredArtStyle: 'realistic',
    language: 'en'
  });

  useEffect(() => {
    loadProfileData();
  }, []);

  const loadProfileData = async () => {
    try {
      setLoading(true);
      // Mock data - in real app would fetch from API
      setReadingHistory([
        {
          id: '1',
          bookId: 'book1',
          bookTitle: 'The Crystal Kingdom',
          progress: 68,
          lastRead: new Date().toISOString(),
          totalPages: 312,
          currentPage: 212
        },
        {
          id: '2',
          bookId: 'book2',
          bookTitle: 'Shadows of Tomorrow',
          progress: 25,
          lastRead: new Date(Date.now() - 86400000).toISOString(),
          totalPages: 245,
          currentPage: 61
        }
      ]);

      setBookmarks([
        {
          id: '1',
          bookId: 'book1',
          bookTitle: 'The Crystal Kingdom',
          chapterTitle: 'Chapter 12: The Revelation',
          pageNumber: 156,
          note: 'Amazing plot twist!',
          createdAt: new Date().toISOString()
        }
      ]);
    } catch (error) {
      console.error('Failed to load profile:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateStr: string) => {
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
    <div className="profile-page">
      {/* Profile Header */}
      <header className="profile-header">
        <div className="profile-cover">
          <div className="cover-gradient"></div>
        </div>
        
        <div className="profile-info">
          <div className="avatar-container">
            <div className="avatar">
              <span className="avatar-letter">
                {user?.firstName?.charAt(0).toUpperCase() || '?'}
              </span>
            </div>
            <button className="avatar-edit" title="Change avatar">
              📷
            </button>
          </div>

          <div className="user-details">
            <h1 className="user-name">
              {user?.firstName} {user?.lastName}
            </h1>
            <p className="user-email">{user?.email}</p>
            <span className="user-role-badge">
              {user?.role === 'Author' ? '✍️ Author' : '📖 Reader'}
            </span>
          </div>

          <div className="profile-stats">
            <div className="stat-item">
              <span className="stat-value">{readingHistory.length}</span>
              <span className="stat-label">Books Read</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{bookmarks.length}</span>
              <span className="stat-label">Bookmarks</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">142</span>
              <span className="stat-label">Visualizations</span>
            </div>
          </div>
        </div>
      </header>

      {/* Tab Navigation */}
      <nav className="profile-tabs">
        <button 
          className={`tab-btn ${activeTab === 'reading' ? 'active' : ''}`}
          onClick={() => setActiveTab('reading')}
        >
          <span className="tab-icon">📚</span>
          Reading History
        </button>
        <button 
          className={`tab-btn ${activeTab === 'bookmarks' ? 'active' : ''}`}
          onClick={() => setActiveTab('bookmarks')}
        >
          <span className="tab-icon">🔖</span>
          Bookmarks
        </button>
        <button 
          className={`tab-btn ${activeTab === 'visualizations' ? 'active' : ''}`}
          onClick={() => setActiveTab('visualizations')}
        >
          <span className="tab-icon">🎨</span>
          My Visualizations
        </button>
        <button 
          className={`tab-btn ${activeTab === 'settings' ? 'active' : ''}`}
          onClick={() => setActiveTab('settings')}
        >
          <span className="tab-icon">⚙️</span>
          Settings
        </button>
      </nav>

      {/* Tab Content */}
      <main className="profile-content">
        {/* Reading History Tab */}
        {activeTab === 'reading' && (
          <section className="tab-content">
            <div className="content-header">
              <h2>Continue Reading</h2>
              <Link to="/catalog" className="btn-browse">
                Browse More Books →
              </Link>
            </div>

            {loading ? (
              <div className="loading-state">
                <div className="loading-spinner"></div>
              </div>
            ) : readingHistory.length === 0 ? (
              <div className="empty-state">
                <span className="empty-icon">📚</span>
                <h3>No reading history yet</h3>
                <p>Start reading a book to track your progress</p>
                <Link to="/catalog" className="btn-primary">Browse Library</Link>
              </div>
            ) : (
              <div className="reading-list">
                {readingHistory.map(item => (
                  <Link 
                    key={item.id} 
                    to={`/read/${item.bookId}`}
                    className="reading-card"
                  >
                    <div className="book-cover">
                      {item.coverUrl ? (
                        <img src={item.coverUrl} alt={item.bookTitle} />
                      ) : (
                        <div className="cover-placeholder">📖</div>
                      )}
                    </div>
                    
                    <div className="reading-info">
                      <h3 className="book-title">{item.bookTitle}</h3>
                      <div className="progress-info">
                        <div className="progress-bar">
                          <div 
                            className="progress-fill"
                            style={{ width: `${item.progress}%` }}
                          />
                        </div>
                        <span className="progress-text">
                          {item.progress}% • Page {item.currentPage} of {item.totalPages}
                        </span>
                      </div>
                      <span className="last-read">Last read {formatDate(item.lastRead)}</span>
                    </div>

                    <button className="btn-continue">
                      Continue →
                    </button>
                  </Link>
                ))}
              </div>
            )}
          </section>
        )}

        {/* Bookmarks Tab */}
        {activeTab === 'bookmarks' && (
          <section className="tab-content">
            <div className="content-header">
              <h2>Your Bookmarks</h2>
              <span className="bookmark-count">{bookmarks.length} saved</span>
            </div>

            {bookmarks.length === 0 ? (
              <div className="empty-state">
                <span className="empty-icon">🔖</span>
                <h3>No bookmarks yet</h3>
                <p>Add bookmarks while reading to save your favorite moments</p>
              </div>
            ) : (
              <div className="bookmarks-list">
                {bookmarks.map(bookmark => (
                  <div key={bookmark.id} className="bookmark-card">
                    <div className="bookmark-icon">🔖</div>
                    <div className="bookmark-content">
                      <h4 className="bookmark-book">{bookmark.bookTitle}</h4>
                      <p className="bookmark-chapter">{bookmark.chapterTitle}</p>
                      <span className="bookmark-page">Page {bookmark.pageNumber}</span>
                      {bookmark.note && (
                        <p className="bookmark-note">"{bookmark.note}"</p>
                      )}
                      <span className="bookmark-date">{formatDate(bookmark.createdAt)}</span>
                    </div>
                    <div className="bookmark-actions">
                      <Link to={`/read/${bookmark.bookId}?page=${bookmark.pageNumber}`} className="btn-go">
                        Go to page →
                      </Link>
                      <button className="btn-delete" title="Remove bookmark">
                        🗑️
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </section>
        )}

        {/* Visualizations Tab */}
        {activeTab === 'visualizations' && (
          <section className="tab-content">
            <div className="content-header">
              <h2>Your Visualizations</h2>
              <span className="viz-count">142 generated</span>
            </div>

            <div className="visualizations-grid">
              {[1, 2, 3, 4, 5, 6].map(i => (
                <div key={i} className="viz-card">
                  <div className="viz-image">
                    <div className="viz-placeholder">🎨</div>
                  </div>
                  <div className="viz-info">
                    <span className="viz-book">The Crystal Kingdom</span>
                    <span className="viz-page">Page {i * 20}</span>
                  </div>
                </div>
              ))}
            </div>

            <button className="btn-load-more">
              Load More Visualizations
            </button>
          </section>
        )}

        {/* Settings Tab */}
        {activeTab === 'settings' && (
          <section className="tab-content settings-content">
            <div className="settings-section">
              <h3>Notifications</h3>
              
              <div className="setting-item">
                <div className="setting-info">
                  <span className="setting-label">Email Notifications</span>
                  <span className="setting-desc">Receive updates about new chapters and books</span>
                </div>
                <label className="toggle">
                  <input
                    type="checkbox"
                    checked={settings.emailNotifications}
                    onChange={(e) => setSettings({...settings, emailNotifications: e.target.checked})}
                  />
                  <span className="toggle-slider"></span>
                </label>
              </div>

              <div className="setting-item">
                <div className="setting-info">
                  <span className="setting-label">Weekly Digest</span>
                  <span className="setting-desc">Get a summary of your reading activity</span>
                </div>
                <label className="toggle">
                  <input
                    type="checkbox"
                    checked={settings.weeklyDigest}
                    onChange={(e) => setSettings({...settings, weeklyDigest: e.target.checked})}
                  />
                  <span className="toggle-slider"></span>
                </label>
              </div>
            </div>

            <div className="settings-section">
              <h3>Appearance</h3>
              
              <div className="setting-item">
                <div className="setting-info">
                  <span className="setting-label">Dark Mode</span>
                  <span className="setting-desc">Use dark theme across the app</span>
                </div>
                <label className="toggle">
                  <input
                    type="checkbox"
                    checked={settings.darkMode}
                    onChange={(e) => setSettings({...settings, darkMode: e.target.checked})}
                  />
                  <span className="toggle-slider"></span>
                </label>
              </div>

              <div className="setting-item">
                <div className="setting-info">
                  <span className="setting-label">Language</span>
                  <span className="setting-desc">Select your preferred language</span>
                </div>
                <select 
                  className="setting-select"
                  value={settings.language}
                  onChange={(e) => setSettings({...settings, language: e.target.value})}
                >
                  <option value="en">English</option>
                  <option value="ru">Русский</option>
                  <option value="es">Español</option>
                  <option value="de">Deutsch</option>
                </select>
              </div>
            </div>

            <div className="settings-section">
              <h3>AI Visualization</h3>
              
              <div className="setting-item">
                <div className="setting-info">
                  <span className="setting-label">Auto-Visualize</span>
                  <span className="setting-desc">Automatically generate visualizations while reading</span>
                </div>
                <label className="toggle">
                  <input
                    type="checkbox"
                    checked={settings.autoVisualize}
                    onChange={(e) => setSettings({...settings, autoVisualize: e.target.checked})}
                  />
                  <span className="toggle-slider"></span>
                </label>
              </div>

              <div className="setting-item">
                <div className="setting-info">
                  <span className="setting-label">Preferred Art Style</span>
                  <span className="setting-desc">Default style for AI-generated images</span>
                </div>
                <select 
                  className="setting-select"
                  value={settings.preferredArtStyle}
                  onChange={(e) => setSettings({...settings, preferredArtStyle: e.target.value})}
                >
                  <option value="realistic">Realistic</option>
                  <option value="anime">Anime</option>
                  <option value="oil_painting">Oil Painting</option>
                  <option value="watercolor">Watercolor</option>
                  <option value="fantasy">Fantasy Art</option>
                </select>
              </div>
            </div>

            <div className="settings-section danger-zone">
              <h3>Account</h3>
              
              <button className="btn-danger" onClick={logout}>
                <span>🚪</span> Sign Out
              </button>
              
              <button className="btn-delete-account">
                <span>⚠️</span> Delete Account
              </button>
            </div>
          </section>
        )}
      </main>
    </div>
  );
};

export default ProfilePage;