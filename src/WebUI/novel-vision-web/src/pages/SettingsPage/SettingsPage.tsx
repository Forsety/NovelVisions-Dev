// src/pages/SettingsPage/SettingsPage.tsx

import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { useToast } from '../../shared/ui/Toast';
import { ROUTES } from '../../shared/constants/routes';
import { VISUALIZATION_STYLES, AI_PROVIDERS } from '../../shared/constants/theme';
import type { ThemeMode } from '../../shared/constants/theme';
import { readingService, type ReadingPreferences } from '../../services/api/reading.service';
import { useAuthStore } from '../../store';
import { useUIStore } from '../../store/useUIStore';
import styles from './SettingsPage.module.css';

// ==================== LOCAL CONSTANTS ====================

const THEME_OPTIONS: { id: ThemeMode; name: string; icon: string }[] = [
  { id: 'dark', name: 'Dark', icon: '🌙' },
  { id: 'light', name: 'Light', icon: '☀️' },
  { id: 'sepia', name: 'Sepia', icon: '📜' },
];

const FONTS = [
  { value: 'Georgia', label: 'Georgia (Serif)' },
  { value: 'Merriweather', label: 'Merriweather' },
  { value: 'Lora', label: 'Lora' },
  { value: 'Inter', label: 'Inter (Sans)' },
  { value: 'Open Sans', label: 'Open Sans' },
  { value: 'Roboto Mono', label: 'Roboto Mono' },
];

// ==================== TYPES ====================

interface ExtendedPreferences {
  defaultFontSize: number;
  defaultLineHeight: number;
  defaultTheme: ThemeMode;
  autoSaveProgress: boolean;
  showReadingTime: boolean;
  enableAnimations: boolean;
  defaultVisualizationProvider?: string;
  defaultVisualizationStyle?: string;
  fontFamily?: string;
  pageWidth?: 'narrow' | 'medium' | 'wide';
}

interface NotificationSettings {
  emailNotifications: boolean;
  pushNotifications: boolean;
  newChapterAlerts: boolean;
  authorUpdates: boolean;
  weeklyDigest: boolean;
}

const defaultPreferences: ExtendedPreferences = {
  defaultFontSize: 18,
  defaultLineHeight: 1.8,
  defaultTheme: 'dark',
  autoSaveProgress: true,
  showReadingTime: true,
  enableAnimations: true,
  defaultVisualizationProvider: 'dalle3',
  defaultVisualizationStyle: 'realistic',
  fontFamily: 'Georgia',
  pageWidth: 'medium',
};

const defaultNotifications: NotificationSettings = {
  emailNotifications: true,
  pushNotifications: false,
  newChapterAlerts: true,
  authorUpdates: true,
  weeklyDigest: false,
};

// ==================== READER SETTINGS SECTION ====================

interface ReaderSettingsProps {
  preferences: ExtendedPreferences;
  onChange: (preferences: ExtendedPreferences) => void;
}

const ReaderSettingsSection: React.FC<ReaderSettingsProps> = ({ preferences, onChange }) => {
  const widths = [
    { value: 'narrow', label: 'Narrow (600px)' },
    { value: 'medium', label: 'Medium (800px)' },
    { value: 'wide', label: 'Wide (1000px)' },
  ];

  return (
    <div className={styles.settingsSection}>
      <h2 className={styles.sectionTitle}>📖 Reader Settings</h2>
      <p className={styles.sectionDescription}>Customize your reading experience</p>

      <div className={styles.settingsGrid}>
        {/* Font Family */}
        <div className={styles.settingItem}>
          <label className={styles.settingLabel}>Font Family</label>
          <select
            value={preferences.fontFamily || 'Georgia'}
            onChange={(e) => onChange({ ...preferences, fontFamily: e.target.value })}
            className={styles.select}
          >
            {FONTS.map((font) => (
              <option key={font.value} value={font.value}>{font.label}</option>
            ))}
          </select>
        </div>

        {/* Font Size */}
        <div className={styles.settingItem}>
          <label className={styles.settingLabel}>Font Size: {preferences.defaultFontSize}px</label>
          <input
            type="range"
            min="14"
            max="28"
            value={preferences.defaultFontSize}
            onChange={(e) => onChange({ ...preferences, defaultFontSize: parseInt(e.target.value) })}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>Small</span>
            <span>Large</span>
          </div>
        </div>

        {/* Line Height */}
        <div className={styles.settingItem}>
          <label className={styles.settingLabel}>Line Height: {preferences.defaultLineHeight.toFixed(1)}</label>
          <input
            type="range"
            min="1.4"
            max="2.2"
            step="0.1"
            value={preferences.defaultLineHeight}
            onChange={(e) => onChange({ ...preferences, defaultLineHeight: parseFloat(e.target.value) })}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>Compact</span>
            <span>Spacious</span>
          </div>
        </div>

        {/* Theme */}
        <div className={styles.settingItem}>
          <label className={styles.settingLabel}>Reading Theme</label>
          <div className={styles.themeOptions}>
            {THEME_OPTIONS.map((theme) => (
              <button
                key={theme.id}
                className={`${styles.themeButton} ${preferences.defaultTheme === theme.id ? styles.active : ''} ${styles[theme.id]}`}
                onClick={() => onChange({ ...preferences, defaultTheme: theme.id })}
              >
                {theme.icon} {theme.name}
              </button>
            ))}
          </div>
        </div>

        {/* Page Width */}
        <div className={styles.settingItem}>
          <label className={styles.settingLabel}>Page Width</label>
          <select
            value={preferences.pageWidth || 'medium'}
            onChange={(e) => onChange({ ...preferences, pageWidth: e.target.value as ExtendedPreferences['pageWidth'] })}
            className={styles.select}
          >
            {widths.map((width) => (
              <option key={width.value} value={width.value}>{width.label}</option>
            ))}
          </select>
        </div>

        {/* Toggle Options */}
        <div className={styles.settingItem}>
          <label className={styles.toggleLabel}>
            <input
              type="checkbox"
              checked={preferences.autoSaveProgress}
              onChange={(e) => onChange({ ...preferences, autoSaveProgress: e.target.checked })}
            />
            <span className={styles.toggleSwitch} />
            <span>Auto-save reading progress</span>
          </label>
        </div>

        <div className={styles.settingItem}>
          <label className={styles.toggleLabel}>
            <input
              type="checkbox"
              checked={preferences.showReadingTime}
              onChange={(e) => onChange({ ...preferences, showReadingTime: e.target.checked })}
            />
            <span className={styles.toggleSwitch} />
            <span>Show estimated reading time</span>
          </label>
        </div>

        <div className={styles.settingItem}>
          <label className={styles.toggleLabel}>
            <input
              type="checkbox"
              checked={preferences.enableAnimations}
              onChange={(e) => onChange({ ...preferences, enableAnimations: e.target.checked })}
            />
            <span className={styles.toggleSwitch} />
            <span>Enable page animations</span>
          </label>
        </div>
      </div>

      {/* Preview */}
      <div
        className={styles.preview}
        style={{
          fontFamily: preferences.fontFamily,
          fontSize: `${preferences.defaultFontSize}px`,
          lineHeight: preferences.defaultLineHeight,
        }}
      >
        <p>
          The quick brown fox jumps over the lazy dog. This is a preview of your reading settings.
          Adjust the options above to customize your experience.
        </p>
      </div>
    </div>
  );
};

// ==================== VISUALIZATION SETTINGS SECTION ====================

interface VisualizationSettingsProps {
  preferences: ExtendedPreferences;
  onChange: (preferences: ExtendedPreferences) => void;
}

const VisualizationSettingsSection: React.FC<VisualizationSettingsProps> = ({ preferences, onChange }) => {
  return (
    <div className={styles.settingsSection}>
      <h2 className={styles.sectionTitle}>🎨 Visualization Settings</h2>
      <p className={styles.sectionDescription}>Configure AI image generation preferences</p>

      <div className={styles.settingsGrid}>
        {/* Default Provider */}
        <div className={styles.settingItem}>
          <label className={styles.settingLabel}>Default AI Provider</label>
          <div className={styles.providerOptions}>
            {AI_PROVIDERS.map((provider) => (
              <button
                key={provider.value}
                className={`${styles.providerButton} ${preferences.defaultVisualizationProvider === provider.value ? styles.active : ''}`}
                onClick={() => onChange({ ...preferences, defaultVisualizationProvider: provider.value })}
              >
                <span className={styles.providerName}>{provider.label}</span>
                <span className={styles.providerDesc}>{provider.description}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Default Style */}
        <div className={styles.settingItem}>
          <label className={styles.settingLabel}>Default Art Style</label>
          <select
            value={preferences.defaultVisualizationStyle || 'realistic'}
            onChange={(e) => onChange({ ...preferences, defaultVisualizationStyle: e.target.value })}
            className={styles.select}
          >
            {VISUALIZATION_STYLES.map((style) => (
              <option key={style.value} value={style.value}>
                {style.icon} {style.label}
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
};

// ==================== NOTIFICATION SETTINGS SECTION ====================

interface NotificationSettingsProps {
  settings: NotificationSettings;
  onChange: (settings: NotificationSettings) => void;
}

const NotificationSettingsSection: React.FC<NotificationSettingsProps> = ({ settings, onChange }) => {
  return (
    <div className={styles.settingsSection}>
      <h2 className={styles.sectionTitle}>🔔 Notifications</h2>
      <p className={styles.sectionDescription}>Manage your notification preferences</p>

      <div className={styles.notificationsList}>
        <label className={styles.toggleLabel}>
          <input
            type="checkbox"
            checked={settings.emailNotifications}
            onChange={(e) => onChange({ ...settings, emailNotifications: e.target.checked })}
          />
          <span className={styles.toggleSwitch} />
          <div className={styles.toggleInfo}>
            <span className={styles.toggleTitle}>Email Notifications</span>
            <span className={styles.toggleDesc}>Receive updates via email</span>
          </div>
        </label>

        <label className={styles.toggleLabel}>
          <input
            type="checkbox"
            checked={settings.pushNotifications}
            onChange={(e) => onChange({ ...settings, pushNotifications: e.target.checked })}
          />
          <span className={styles.toggleSwitch} />
          <div className={styles.toggleInfo}>
            <span className={styles.toggleTitle}>Push Notifications</span>
            <span className={styles.toggleDesc}>Browser push notifications</span>
          </div>
        </label>

        <label className={styles.toggleLabel}>
          <input
            type="checkbox"
            checked={settings.newChapterAlerts}
            onChange={(e) => onChange({ ...settings, newChapterAlerts: e.target.checked })}
          />
          <span className={styles.toggleSwitch} />
          <div className={styles.toggleInfo}>
            <span className={styles.toggleTitle}>New Chapter Alerts</span>
            <span className={styles.toggleDesc}>Get notified when followed books update</span>
          </div>
        </label>

        <label className={styles.toggleLabel}>
          <input
            type="checkbox"
            checked={settings.authorUpdates}
            onChange={(e) => onChange({ ...settings, authorUpdates: e.target.checked })}
          />
          <span className={styles.toggleSwitch} />
          <div className={styles.toggleInfo}>
            <span className={styles.toggleTitle}>Author Updates</span>
            <span className={styles.toggleDesc}>News from authors you follow</span>
          </div>
        </label>

        <label className={styles.toggleLabel}>
          <input
            type="checkbox"
            checked={settings.weeklyDigest}
            onChange={(e) => onChange({ ...settings, weeklyDigest: e.target.checked })}
          />
          <span className={styles.toggleSwitch} />
          <div className={styles.toggleInfo}>
            <span className={styles.toggleTitle}>Weekly Digest</span>
            <span className={styles.toggleDesc}>Summary of your reading activity</span>
          </div>
        </label>
      </div>
    </div>
  );
};

// ==================== MAIN SETTINGS PAGE ====================

const SettingsPage: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuthStore();
  const { setTheme } = useUIStore();
  const toast = useToast();

  const [preferences, setPreferences] = useState<ExtendedPreferences>(defaultPreferences);
  const [notifications, setNotifications] = useState<NotificationSettings>(defaultNotifications);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate(ROUTES.LOGIN);
    }
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    const fetchPreferences = async () => {
      try {
        const prefs = await readingService.getPreferences();
        if (prefs) {
          setPreferences({ ...defaultPreferences, ...prefs });
        }
      } catch (err) {
        console.error('Failed to fetch preferences:', err);
      } finally {
        setLoading(false);
      }
    };

    if (isAuthenticated) {
      fetchPreferences();
    }
  }, [isAuthenticated]);

  const handlePreferencesChange = (newPrefs: ExtendedPreferences) => {
    setPreferences(newPrefs);
    setHasChanges(true);
    setTheme(newPrefs.defaultTheme);
  };

  const handleNotificationChange = (newNotifications: NotificationSettings) => {
    setNotifications(newNotifications);
    setHasChanges(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await readingService.updatePreferences({
        defaultFontSize: preferences.defaultFontSize,
        defaultLineHeight: preferences.defaultLineHeight,
        defaultTheme: preferences.defaultTheme,
        autoSaveProgress: preferences.autoSaveProgress,
        showReadingTime: preferences.showReadingTime,
        enableAnimations: preferences.enableAnimations,
        defaultVisualizationProvider: preferences.defaultVisualizationProvider,
        defaultVisualizationStyle: preferences.defaultVisualizationStyle,
      });
      setHasChanges(false);
      toast.success('Settings saved successfully!');
    } catch (err) {
      console.error('Failed to save preferences:', err);
      toast.error('Failed to save settings. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    setPreferences(defaultPreferences);
    setNotifications(defaultNotifications);
    setHasChanges(true);
    setTheme(defaultPreferences.defaultTheme);
  };

  if (loading) {
    return (
      <div className={styles.loadingPage}>
        <Spinner size="lg" label="Loading settings..." />
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <div className={styles.container}>
        <motion.div className={styles.header} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}>
          <h1 className={styles.title}>⚙️ Settings</h1>
          <p className={styles.subtitle}>Customize your NovelVision experience</p>
        </motion.div>

        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}>
          <Card variant="glass" padding="lg" className={styles.card}>
            <ReaderSettingsSection preferences={preferences} onChange={handlePreferencesChange} />
          </Card>
        </motion.div>

        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
          <Card variant="glass" padding="lg" className={styles.card}>
            <VisualizationSettingsSection preferences={preferences} onChange={handlePreferencesChange} />
          </Card>
        </motion.div>

        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}>
          <Card variant="glass" padding="lg" className={styles.card}>
            <NotificationSettingsSection settings={notifications} onChange={handleNotificationChange} />
          </Card>
        </motion.div>

        <motion.div className={styles.actions} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }}>
          <Button variant="ghost" onClick={handleReset}>Reset to Defaults</Button>
          <Button variant="primary" onClick={handleSave} disabled={!hasChanges || saving}>
            {saving ? 'Saving...' : 'Save Changes'}
          </Button>
        </motion.div>
      </div>
    </div>
  );
};

export default SettingsPage;