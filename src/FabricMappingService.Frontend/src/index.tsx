/**
 * Einstiegspunkt für das Fabric Mapping Service Frontend
 */

import React from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';

// Hole Root-Element
const container = document.getElementById('root');

if (!container) {
  throw new Error('Root-Element nicht gefunden');
}

// Erstelle React Root und rendere App
const root = createRoot(container);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
