// src/components/AIVisualizationPanel.tsx
import React, { useState } from 'react';
import { GeneratedImage, VisualizationMode } from '../types/visualization.types';
import './AIVisualizationPanel.css';

interface AIVisualizationPanelProps {
  visualization: GeneratedImage | null;
  isGenerating: boolean;
  progress: number;
  onRegenerate: () => void;
  onClose: () => void;
  mode: VisualizationMode;
}

const AIVisualizationPanel: React.FC<AIVisualizationPanelProps> = ({
  visualization,
  isGenerating,
  progress,
  onRegenerate,
  onClose,
  mode
}) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const [isLiked, setIsLiked] = useState(false);
  const [showActions, setShowActions] = useState(false);

  // Placeholder for when no visualization is available
  if (!visualization && !isGenerating) {
    return (
      <div className="visualization-panel placeholder">
        <div className="placeholder-content">
          <div className="placeholder-icon">
            <span className="icon">🎨</span>
            <div className="icon-glow"></div>
          </div>
          <h3>No Visualization Yet</h3>
          <p>
            {mode === 'UserSelected' 
              ? 'Select text or click "Visualize Page" to create an AI illustration'
              : 'Click "Visualize Page" to generate an illustration for this scene'}
          </p>
          <button className="btn-generate" onClick={onRegenerate}>
            <span className="btn-icon">✨</span>
            Generate Visualization
          </button>
        </div>
        <div className="placeholder-particles">
          {[...Array(10)].map((_, i) => (
            <div key={i} className="particle" style={{
              left: `${Math.random() * 100}%`,
              animationDelay: `${Math.random() * 2}s`
            }} />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className={`visualization-panel ${isExpanded ? 'expanded' : ''}`}>
      {/* Generation Progress */}
      {isGenerating && (
        <div className="generation-overlay">
          <div className="generation-content">
            <div className="generation-animation">
              <div className="magic-circle">
                <div className="circle-ring ring-1"></div>
                <div className="circle-ring ring-2"></div>
                <div className="circle-ring ring-3"></div>
                <div className="circle-center">
                  <span className="magic-icon">✨</span>
                </div>
              </div>
            </div>
            
            <div className="generation-info">
              <h3>Creating Your Visualization</h3>
              <p className="generation-status">
                {progress < 30 && 'Analyzing text...'}
                {progress >= 30 && progress < 60 && 'Generating prompt...'}
                {progress >= 60 && progress < 90 && 'AI is painting...'}
                {progress >= 90 && 'Finalizing...'}
              </p>
              
              <div className="progress-container">
                <div className="progress-bar">
                  <div 
                    className="progress-fill"
                    style={{ width: `${progress}%` }}
                  >
                    <div className="progress-shimmer"></div>
                  </div>
                </div>
                <span className="progress-text">{progress}%</span>
              </div>

              <div className="generation-tips">
                <span className="tip-icon">💡</span>
                <span className="tip-text">
                  AI analyzes the text, extracts key elements, and creates a unique illustration
                </span>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Visualization Display */}
      {visualization && !isGenerating && (
        <>
          {/* Image Container */}
          <div 
            className="image-container"
            onClick={() => setIsExpanded(!isExpanded)}
            onMouseEnter={() => setShowActions(true)}
            onMouseLeave={() => setShowActions(false)}
          >
            <img 
              src={visualization.imageUrl} 
              alt="AI Generated Visualization"
              className="visualization-image"
            />
            
            {/* Overlay Gradient */}
            <div className="image-overlay">
              <div className="overlay-gradient"></div>
            </div>

            {/* Expand Indicator */}
            <div className="expand-indicator">
              <span>{isExpanded ? '↙️' : '↗️'}</span>
            </div>

            {/* AI Badge */}
            <div className="ai-badge">
              <span className="badge-icon">🤖</span>
              <span className="badge-text">AI Generated</span>
            </div>

            {/* Action Buttons */}
            <div className={`image-actions ${showActions ? 'visible' : ''}`}>
              <button 
                className={`action-btn ${isLiked ? 'liked' : ''}`}
                onClick={(e) => {
                  e.stopPropagation();
                  setIsLiked(!isLiked);
                }}
                title="Like"
              >
                <span>{isLiked ? '❤️' : '🤍'}</span>
              </button>
              
              <button 
                className="action-btn"
                onClick={(e) => {
                  e.stopPropagation();
                  onRegenerate();
                }}
                title="Regenerate"
              >
                <span>🔄</span>
              </button>
              
              <button 
                className="action-btn"
                onClick={(e) => {
                  e.stopPropagation();
                  // Download image
                  const link = document.createElement('a');
                  link.href = visualization.imageUrl;
                  link.download = 'novelvision-art.png';
                  link.click();
                }}
                title="Download"
              >
                <span>⬇️</span>
              </button>
              
              <button 
                className="action-btn"
                onClick={(e) => {
                  e.stopPropagation();
                  navigator.share?.({
                    title: 'NovelVision AI Art',
                    url: visualization.imageUrl
                  });
                }}
                title="Share"
              >
                <span>📤</span>
              </button>
            </div>
          </div>

          {/* Image Info (shown when expanded) */}
          {isExpanded && (
            <div className="image-info">
              <div className="info-row">
                <span className="info-label">Resolution</span>
                <span className="info-value">{visualization.width} × {visualization.height}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Created</span>
                <span className="info-value">
                  {new Date(visualization.createdAt).toLocaleString()}
                </span>
              </div>
            </div>
          )}

          {/* Quick Actions Bar */}
          <div className="quick-actions">
            <button 
              className="quick-btn"
              onClick={onRegenerate}
            >
              <span className="btn-icon">✨</span>
              <span className="btn-text">New Version</span>
            </button>
            <button 
              className="quick-btn"
              onClick={onClose}
            >
              <span className="btn-icon">📖</span>
              <span className="btn-text">Hide</span>
            </button>
          </div>
        </>
      )}

      {/* Decorative Elements */}
      <div className="panel-decorations">
        <div className="corner corner-tl"></div>
        <div className="corner corner-tr"></div>
        <div className="corner corner-bl"></div>
        <div className="corner corner-br"></div>
      </div>
    </div>
  );
};

export default AIVisualizationPanel;