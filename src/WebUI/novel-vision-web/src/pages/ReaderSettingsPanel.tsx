// src/components/ReaderSettingsPanel.tsx
import React from 'react';
import { 
  ReaderSettings, 
  FONT_OPTIONS, 
  PAGE_WIDTH_OPTIONS 
} from '../types/reader.types';
import { 
  AI_PROVIDER_OPTIONS, 
  ART_STYLE_OPTIONS 
} from '../types/visualization.types';
import './ReaderSettingsPanel.css';

interface ReaderSettingsPanelProps {
  isOpen: boolean;
  onClose: () => void;
  settings: ReaderSettings;
  onSettingsChange: (settings: ReaderSettings) => void;
  onModeChange: () => void;
}

const ReaderSettingsPanel: React.FC<ReaderSettingsPanelProps> = ({
  isOpen,
  onClose,
  settings,
  onSettingsChange,
  onModeChange
}) => {
  if (!isOpen) return null;

  const updateSettings = (updates: Partial<ReaderSettings>) => {
    onSettingsChange({ ...settings, ...updates });
  };

  const updateVisualization = (updates: Partial<typeof settings.visualization>) => {
    onSettingsChange({
      ...settings,
      visualization: { ...settings.visualization, ...updates }
    });
  };

  return (
    <div className="settings-overlay" onClick={onClose}>
      <div className="settings-panel" onClick={e => e.stopPropagation()}>
        {/* Header */}
        <div className="settings-header">
          <h2>
            <span className="header-icon">⚙️</span>
            Reader Settings
          </h2>
          <button className="btn-close" onClick={onClose}>×</button>
        </div>

        {/* Content */}
        <div className="settings-content">
          {/* Appearance Section */}
          <section className="settings-section">
            <h3 className="section-title">
              <span className="section-icon">🎨</span>
              Appearance
            </h3>

            {/* Theme */}
            <div className="setting-group">
              <label className="setting-label">Theme</label>
              <div className="theme-options">
                {(['light', 'dark', 'sepia'] as const).map(theme => (
                  <button
                    key={theme}
                    className={`theme-btn ${settings.theme === theme ? 'active' : ''} theme-${theme}`}
                    onClick={() => updateSettings({ theme })}
                  >
                    <span className="theme-preview"></span>
                    <span className="theme-name">{theme.charAt(0).toUpperCase() + theme.slice(1)}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Font Size */}
            <div className="setting-group">
              <label className="setting-label">
                Font Size
                <span className="setting-value">{settings.fontSize}px</span>
              </label>
              <div className="slider-container">
                <span className="slider-label">A</span>
                <input
                  type="range"
                  min="12"
                  max="32"
                  value={settings.fontSize}
                  onChange={(e) => updateSettings({ fontSize: parseInt(e.target.value) })}
                  className="slider"
                />
                <span className="slider-label slider-large">A</span>
              </div>
            </div>

            {/* Font Family */}
            <div className="setting-group">
              <label className="setting-label">Font</label>
              <select
                value={settings.fontFamily}
                onChange={(e) => updateSettings({ fontFamily: e.target.value })}
                className="select-input"
              >
                {FONT_OPTIONS.map(font => (
                  <option key={font.value} value={font.value} style={{ fontFamily: font.value }}>
                    {font.label}
                  </option>
                ))}
              </select>
            </div>

            {/* Line Height */}
            <div className="setting-group">
              <label className="setting-label">
                Line Spacing
                <span className="setting-value">{settings.lineHeight}</span>
              </label>
              <input
                type="range"
                min="1.2"
                max="2.4"
                step="0.1"
                value={settings.lineHeight}
                onChange={(e) => updateSettings({ lineHeight: parseFloat(e.target.value) })}
                className="slider"
              />
            </div>

            {/* Page Width */}
            <div className="setting-group">
              <label className="setting-label">Page Width</label>
              <div className="width-options">
                {(['narrow', 'medium', 'wide'] as const).map(width => (
                  <button
                    key={width}
                    className={`width-btn ${settings.pageWidth === width ? 'active' : ''}`}
                    onClick={() => updateSettings({ pageWidth: width })}
                  >
                    <span className={`width-icon width-${width}`}></span>
                    <span>{width.charAt(0).toUpperCase() + width.slice(1)}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Text Align */}
            <div className="setting-group">
              <label className="setting-label">Text Alignment</label>
              <div className="align-options">
                <button
                  className={`align-btn ${settings.textAlign === 'left' ? 'active' : ''}`}
                  onClick={() => updateSettings({ textAlign: 'left' })}
                >
                  <span className="align-icon">≡</span>
                  Left
                </button>
                <button
                  className={`align-btn ${settings.textAlign === 'justify' ? 'active' : ''}`}
                  onClick={() => updateSettings({ textAlign: 'justify' })}
                >
                  <span className="align-icon">☰</span>
                  Justify
                </button>
              </div>
            </div>
          </section>

          {/* AI Visualization Section */}
          <section className="settings-section">
            <h3 className="section-title">
              <span className="section-icon">✨</span>
              AI Visualization
            </h3>

            {/* Visualization Mode */}
            <div className="setting-group">
              <label className="setting-label">Visualization Mode</label>
              <button className="mode-change-btn" onClick={onModeChange}>
                <span className="mode-icon">
                  {settings.visualization.mode === 'PerPage' && '🖼️'}
                  {settings.visualization.mode === 'PerChapter' && '📑'}
                  {settings.visualization.mode === 'UserSelected' && '✋'}
                  {settings.visualization.mode === 'None' && '📖'}
                </span>
                <span className="mode-name">
                  {settings.visualization.mode === 'PerPage' && 'Every Page'}
                  {settings.visualization.mode === 'PerChapter' && 'Every Chapter'}
                  {settings.visualization.mode === 'UserSelected' && 'On Demand'}
                  {settings.visualization.mode === 'None' && 'Disabled'}
                </span>
                <span className="change-icon">→</span>
              </button>
            </div>

            {/* AI Provider */}
            <div className="setting-group">
              <label className="setting-label">AI Model</label>
              <div className="provider-options">
                {AI_PROVIDER_OPTIONS.map(provider => (
                  <button
                    key={provider.value}
                    className={`provider-btn ${settings.visualization.preferredProvider === provider.value ? 'active' : ''}`}
                    onClick={() => updateVisualization({ preferredProvider: provider.value as any })}
                  >
                    <span className="provider-name">{provider.label}</span>
                    <span className="provider-desc">{provider.description}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Art Style */}
            <div className="setting-group">
              <label className="setting-label">Art Style</label>
              <div className="style-grid">
                {ART_STYLE_OPTIONS.map(style => (
                  <button
                    key={style.value}
                    className={`style-btn ${settings.visualization.artStyle === style.value ? 'active' : ''}`}
                    onClick={() => updateVisualization({ artStyle: style.value as any })}
                  >
                    <span className="style-icon">{style.icon}</span>
                    <span className="style-name">{style.label}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Auto Visualize Toggle */}
            <div className="setting-group toggle-group">
              <label className="setting-label">Auto-generate visualizations</label>
              <label className="toggle">
                <input
                  type="checkbox"
                  checked={settings.visualization.autoVisualize}
                  onChange={(e) => updateVisualization({ autoVisualize: e.target.checked })}
                />
                <span className="toggle-slider"></span>
              </label>
            </div>

            {/* Show Progress Toggle */}
            <div className="setting-group toggle-group">
              <label className="setting-label">Show generation progress</label>
              <label className="toggle">
                <input
                  type="checkbox"
                  checked={settings.visualization.showGenerationProgress}
                  onChange={(e) => updateVisualization({ showGenerationProgress: e.target.checked })}
                />
                <span className="toggle-slider"></span>
              </label>
            </div>
          </section>

          {/* Reading Section */}
          <section className="settings-section">
            <h3 className="section-title">
              <span className="section-icon">📖</span>
              Reading
            </h3>

            {/* Show Progress */}
            <div className="setting-group toggle-group">
              <label className="setting-label">Show reading progress</label>
              <label className="toggle">
                <input
                  type="checkbox"
                  checked={settings.showProgress}
                  onChange={(e) => updateSettings({ showProgress: e.target.checked })}
                />
                <span className="toggle-slider"></span>
              </label>
            </div>

            {/* Auto Scroll */}
            <div className="setting-group toggle-group">
              <label className="setting-label">Auto-scroll</label>
              <label className="toggle">
                <input
                  type="checkbox"
                  checked={settings.autoScroll}
                  onChange={(e) => updateSettings({ autoScroll: e.target.checked })}
                />
                <span className="toggle-slider"></span>
              </label>
            </div>

            {settings.autoScroll && (
              <div className="setting-group">
                <label className="setting-label">
                  Scroll Speed
                  <span className="setting-value">{settings.scrollSpeed}</span>
                </label>
                <input
                  type="range"
                  min="10"
                  max="100"
                  value={settings.scrollSpeed}
                  onChange={(e) => updateSettings({ scrollSpeed: parseInt(e.target.value) })}
                  className="slider"
                />
              </div>
            )}
          </section>
        </div>

        {/* Footer */}
        <div className="settings-footer">
          <button className="btn-reset" onClick={() => onSettingsChange({
            ...settings,
            fontSize: 18,
            lineHeight: 1.8,
            theme: 'dark'
          })}>
            Reset to Defaults
          </button>
          <button className="btn-done" onClick={onClose}>
            Done
          </button>
        </div>
      </div>
    </div>
  );
};

export default ReaderSettingsPanel;