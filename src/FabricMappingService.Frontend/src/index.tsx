/**
 * Einstiegspunkt für das Fabric Mapping Service Frontend
 * 
 * Verwendet FluentProvider mit webLightTheme für natives Fabric-Aussehen.
 */

import React from 'react';
import { createRoot } from 'react-dom/client';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';
import { App } from './App';

// Hole Root-Element
const container = document.getElementById('root');

if (!container) {
  throw new Error('Root-Element nicht gefunden');
}

// Erstelle React Root und rendere App mit FluentProvider
const root = createRoot(container);
root.render(
  <React.StrictMode>
    <FluentProvider theme={webLightTheme}>
      <App />
    </FluentProvider>
  </React.StrictMode>
);
