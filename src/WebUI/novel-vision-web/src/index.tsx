// src/index.tsx
// NovelVision Application Entry Point

import React from 'react';
import ReactDOM from 'react-dom/client';

// Styles (order matters!)
import './styles/variables.css';
import './styles/globals.css';
import './styles/animations.css';

// App
import App from './app/App';

// Create root and render
const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

export {};