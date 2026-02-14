// src/pages/ProfilePage/ProfilePage.tsx
// User profile page with settings and reading history

import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { Avatar } from '../../shared/ui/Avatar';
import { Spinner } from '../../shared/ui/Spinner';
import { ROUTES } from '../../shared/constants/routes';
import { useAuthStore } from '../../store';
import { authService } from '../../features/auth/services/authService';
import { readingService, type ReadingStats } from '../../services/api';
import type { ReadingProgress, UpdateProfileRequest } from '../../types';
import styles from './ProfilePage.module.css';

// ==================== PROFILE HEADER ====================

interface ProfileHeaderProps {
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    displayName: string;
    avatarUrl?: string;
    role: string;
    createdAt?: string;
  };
  onEdit: () => void;
}

const ProfileHeader: React.FC<ProfileHeaderProps> = ({ user, onEdit }) => {
  const memberSince = user.createdAt
    ? new Date(user.createdAt).toLocaleDateString('en-US', {
        month: 'long',
        year: 'numeric',
      })
    : 'Unknown';

  return (
    <div className={styles.profileHeader}>
      <div className={styles.avatarSection}>
        <Avatar
          src={user.avatarUrl}
          name={user.displayName || `${user.firstName} ${user.lastName}`}
          size="xl"
        />
        <div className={styles.userInfo}>
          <h1 className={styles.userName}>
            {user.displayName || `${user.firstName} ${user.lastName}`}
          </h1>
          <p className={styles.userEmail}>{user.email}</p>
          <div className={styles.userMeta}>
            <span className={styles.roleBadge}>{user.role}</span>
            <span className={styles.memberSince}>Member since {memberSince}</span>
          </div>
        </div>
      </div>
      <Button variant="outline" onClick={onEdit}>
        ✏️ Edit Profile
      </Button>
    </div>
  );
};

// ==================== STATS CARD ====================

interface StatsCardProps {
  stats: ReadingStats | null;
  loading: boolean;
}

const StatsCard: React.FC<StatsCardProps> = ({ stats, loading }) => {
  if (loading) {
    return (
      <Card variant="glass" padding="lg" className={styles.statsCard}>
        <Spinner size="sm" />
      </Card>
    );
  }

  if (!stats) {
    return (
      <Card variant="glass" padding="lg" className={styles.statsCard}>
        <p className={styles.noStats}>No reading stats yet. Start reading to track your progress!</p>
      </Card>
    );
  }

  return (
    <Card variant="gradient" padding="lg" className={styles.statsCard}>
      <h3 className={styles.cardTitle}>📊 Reading Stats</h3>
      <div className={styles.statsGrid}>
        <div className={styles.statItem}>
          <span className={styles.statValue}>{stats.totalBooksRead}</span>
          <span className={styles.statLabel}>Books Read</span>
        </div>
        <div className={styles.statItem}>
          <span className={styles.statValue}>{stats.totalPagesRead.toLocaleString()}</span>
          <span className={styles.statLabel}>Pages Read</span>
        </div>
        <div className={styles.statItem}>
          <span className={styles.statValue}>{stats.totalReadingTime}</span>
          <span className={styles.statLabel}>Reading Time</span>
        </div>
        <div className={styles.statItem}>
          <span className={styles.statValue}>{stats.currentStreak}</span>
          <span className={styles.statLabel}>Day Streak 🔥</span>
        </div>
      </div>
    </Card>
  );
};

// ==================== READING HISTORY ====================

interface ReadingHistoryProps {
  progress: ReadingProgress[];
  loading: boolean;
  onContinue: (bookId: string) => void;
}

const ReadingHistory: React.FC<ReadingHistoryProps> = ({ progress, loading, onContinue }) => {
  if (loading) {
    return (
      <Card variant="glass" padding="lg">
        <Spinner size="sm" label="Loading reading history..." />
      </Card>
    );
  }

  if (progress.length === 0) {
    return (
      <Card variant="glass" padding="lg" className={styles.emptyHistory}>
        <span className={styles.emptyIcon}>📚</span>
        <h3>No Reading History</h3>
        <p>Start reading books to see your progress here</p>
      </Card>
    );
  }

  return (
    <div className={styles.historyList}>
      {progress.map((item) => (
        <motion.div
          key={item.id}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
        >
          <Card variant="glass" padding="md" className={styles.historyItem}>
            <div className={styles.historyInfo}>
              <h4 className={styles.historyTitle}>Book ID: {item.bookId.substring(0, 8)}...</h4>
              <div className={styles.historyMeta}>
                <span>Page {item.currentPage} of {item.totalPages}</span>
                <span>•</span>
                <span>{item.progressPercent}% complete</span>
              </div>
              <div className={styles.progressBar}>
                <div
                  className={styles.progressFill}
                  style={{ width: `${item.progressPercent}%` }}
                />
              </div>
            </div>
            <Button
              variant="primary"
              size="sm"
              onClick={() => onContinue(item.bookId)}
            >
              Continue →
            </Button>
          </Card>
        </motion.div>
      ))}
    </div>
  );
};

// ==================== EDIT PROFILE MODAL ====================

interface EditProfileModalProps {
  user: {
    firstName: string;
    lastName: string;
    displayName: string;
  };
  onSave: (data: UpdateProfileRequest) => Promise<void>;
  onClose: () => void;
  saving: boolean;
}

const EditProfileModal: React.FC<EditProfileModalProps> = ({
  user,
  onSave,
  onClose,
  saving,
}) => {
  const [formData, setFormData] = useState({
    firstName: user.firstName,
    lastName: user.lastName,
    displayName: user.displayName,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await onSave(formData);
  };

  return (
    <div className={styles.modalOverlay} onClick={onClose}>
      <motion.div
        className={styles.modal}
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
      >
        <div className={styles.modalHeader}>
          <h2>Edit Profile</h2>
          <button className={styles.closeBtn} onClick={onClose}>
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className={styles.editForm}>
          <div className={styles.formRow}>
            <Input
              label="First Name"
              value={formData.firstName}
              onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
              required
              fullWidth
            />
            <Input
              label="Last Name"
              value={formData.lastName}
              onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
              required
              fullWidth
            />
          </div>

          <Input
            label="Display Name"
            value={formData.displayName}
            onChange={(e) => setFormData({ ...formData, displayName: e.target.value })}
            fullWidth
          />

          <div className={styles.modalActions}>
            <Button variant="ghost" type="button" onClick={onClose}>
              Cancel
            </Button>
            <Button variant="primary" type="submit" loading={saving}>
              Save Changes
            </Button>
          </div>
        </form>
      </motion.div>
    </div>
  );
};

// ==================== MAIN PROFILE PAGE ====================

const ProfilePage: React.FC = () => {
  const navigate = useNavigate();
const { user, isAuthenticated, logout, updateUser, setUser } = useAuthStore();
  const [stats, setStats] = useState<ReadingStats | null>(null);
  const [progress, setProgress] = useState<ReadingProgress[]>([]);
  const [loadingStats, setLoadingStats] = useState(true);
  const [loadingProgress, setLoadingProgress] = useState(true);
  const [showEditModal, setShowEditModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [activeTab, setActiveTab] = useState<'overview' | 'history' | 'settings'>('overview');

  // Redirect if not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      navigate(ROUTES.LOGIN);
    }
  }, [isAuthenticated, navigate]);

  // Fetch stats and progress
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoadingStats(true);
        const statsData = await readingService.getReadingStats();
        setStats(statsData);
      } catch (err) {
        console.error('Error fetching stats:', err);
      } finally {
        setLoadingStats(false);
      }

      try {
        setLoadingProgress(true);
        const progressData = await readingService.getAllProgress();
        setProgress(progressData);
      } catch (err) {
        console.error('Error fetching progress:', err);
      } finally {
        setLoadingProgress(false);
      }
    };

    if (isAuthenticated) {
      fetchData();
    }
  }, [isAuthenticated]);

  const handleEditProfile = async (data: UpdateProfileRequest) => {
    setSaving(true);
    try {
      const updatedUser = await authService.updateProfile(data);
        setUser(updatedUser);
      setShowEditModal(false);
    } catch (err) {
      console.error('Error updating profile:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleLogout = async () => {
    await logout();
    navigate(ROUTES.HOME);
  };

  const handleContinueReading = (bookId: string) => {
    navigate(ROUTES.READER.replace(':id', bookId));
  };

  if (!user) {
    return (
      <div className={styles.loadingPage}>
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <div className={styles.container}>
        {/* Profile Header */}
        <ProfileHeader user={user} onEdit={() => setShowEditModal(true)} />

        {/* Tabs */}
        <div className={styles.tabs}>
          <button
            className={`${styles.tab} ${activeTab === 'overview' ? styles.active : ''}`}
            onClick={() => setActiveTab('overview')}
          >
            📊 Overview
          </button>
          <button
            className={`${styles.tab} ${activeTab === 'history' ? styles.active : ''}`}
            onClick={() => setActiveTab('history')}
          >
            📚 Reading History
          </button>
          <button
            className={`${styles.tab} ${activeTab === 'settings' ? styles.active : ''}`}
            onClick={() => setActiveTab('settings')}
          >
            ⚙️ Settings
          </button>
        </div>

        {/* Tab Content */}
        <div className={styles.tabContent}>
          {activeTab === 'overview' && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className={styles.overview}
            >
              <StatsCard stats={stats} loading={loadingStats} />

              <Card variant="glass" padding="lg">
                <h3 className={styles.cardTitle}>📖 Continue Reading</h3>
                <ReadingHistory
                  progress={progress.slice(0, 3)}
                  loading={loadingProgress}
                  onContinue={handleContinueReading}
                />
                {progress.length > 3 && (
                  <Button
                    variant="ghost"
                    fullWidth
                    onClick={() => setActiveTab('history')}
                  >
                    View All History →
                  </Button>
                )}
              </Card>
            </motion.div>
          )}

          {activeTab === 'history' && (
            <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
              <Card variant="glass" padding="lg">
                <h3 className={styles.cardTitle}>📚 Reading History</h3>
                <ReadingHistory
                  progress={progress}
                  loading={loadingProgress}
                  onContinue={handleContinueReading}
                />
              </Card>
            </motion.div>
          )}

          {activeTab === 'settings' && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className={styles.settings}
            >
              <Card variant="glass" padding="lg">
                <h3 className={styles.cardTitle}>🔐 Account</h3>
                <div className={styles.settingsList}>
                  <div className={styles.settingItem}>
                    <div>
                      <span className={styles.settingLabel}>Email</span>
                      <span className={styles.settingValue}>{user.email}</span>
                    </div>
                  </div>
                  <div className={styles.settingItem}>
                    <div>
                      <span className={styles.settingLabel}>Password</span>
                      <span className={styles.settingValue}>••••••••</span>
                    </div>
                    <Button variant="ghost" size="sm">
                      Change
                    </Button>
                  </div>
                </div>
              </Card>

              <Card variant="glass" padding="lg">
                <h3 className={styles.cardTitle}>🎨 Preferences</h3>
                <div className={styles.settingsList}>
                  <div className={styles.settingItem}>
                    <div>
                      <span className={styles.settingLabel}>Theme</span>
                      <span className={styles.settingValue}>Dark Mode</span>
                    </div>
                    <Button variant="ghost" size="sm">
                      Change
                    </Button>
                  </div>
                  <div className={styles.settingItem}>
                    <div>
                      <span className={styles.settingLabel}>Email Notifications</span>
                      <span className={styles.settingValue}>Enabled</span>
                    </div>
                    <Button variant="ghost" size="sm">
                      Manage
                    </Button>
                  </div>
                </div>
              </Card>

<Card variant="glass" padding="lg" className={styles.dangerZone}>
                <h3 className={styles.cardTitle}>⚠️ Danger Zone</h3>
                <div className={styles.dangerActions}>
                  <Button variant="danger" onClick={handleLogout}>
                    Sign Out
                  </Button>
                </div>
              </Card>
            </motion.div>
          )}
        </div>
      </div>

      {/* Edit Profile Modal */}
      {showEditModal && (
        <EditProfileModal
          user={user}
          onSave={handleEditProfile}
          onClose={() => setShowEditModal(false)}
          saving={saving}
        />
      )}
    </div>
  );
};

export default ProfilePage;