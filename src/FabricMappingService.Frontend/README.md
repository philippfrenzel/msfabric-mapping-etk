# Fabric Mapping Service - React Frontend

React-basiertes Frontend für den Fabric Mapping Service, entwickelt gemäß den Microsoft Fabric Extension Toolkit Richtlinien.

## 🎯 Übersicht

Dieses Frontend bietet eine moderne Benutzeroberfläche zur Verwaltung und Bearbeitung von Referenztabellen (KeyMapping-Tabellen) im Microsoft Fabric Ökosystem. Es verwendet **Fluent UI React** Komponenten für eine native Fabric-Erfahrung.

## 🏗️ Architektur

### Haupt-Bereiche

1. **Konfigurationsbereich** (`ConfigurationPanel.tsx`)
   - Dropdown zur Auswahl von Referenztabellen
   - Such-/Filterfeld für schnelle Navigation
   - Spracheinstellungen (Deutsch/Englisch)
   - Toggle für "Nur aktive Werte anzeigen"

2. **Bearbeitungsbereich** (`EditingArea.tsx`)
   - **Basis-Modus** (`BasicModeEditor.tsx`): Inline-editierbare Datentabelle
     - CRUD-Operationen (Create, Read, Update, Delete)
     - Inline-Bearbeitung von Zellen
     - Hinzufügen und Löschen von Zeilen
   - **Experten-Modus** (`ExpertModeEditor.tsx`): JSON-Editor
     - Monaco Editor mit Syntax-Highlighting
     - JSON-Formatierung und Validierung
     - Direktes Bearbeiten der gesamten Tabelle

### Komponenten-Struktur

```
src/
├── components/
│   ├── ConfigurationPanel.tsx    # Konfigurationsbereich
│   ├── EditingArea.tsx            # Container für beide Bearbeitungsmodi
│   ├── BasicModeEditor.tsx        # Tabellen-basierter Editor
│   └── ExpertModeEditor.tsx       # JSON-basierter Editor
├── services/
│   └── apiClient.ts               # API-Client für Backend-Kommunikation
├── types/
│   └── index.ts                   # TypeScript-Typdefinitionen
├── App.tsx                        # Haupt-App-Komponente
└── index.tsx                      # Einstiegspunkt
```

## 🚀 Verwendete Technologien

### UI-Framework
- **React 19.2** - Moderne React-Version mit allen neuesten Features
- **TypeScript 5.9** - Statische Typisierung für bessere Code-Qualität
- **Fluent UI React 8.x** (`@fluentui/react`) - Microsoft's offizielle UI-Komponenten-Bibliothek

### UI-Komponenten (aus Fluent UI)
- `Dropdown` - Auswahl von Referenztabellen
- `SearchBox` - Filterung von Tabellen
- `Toggle` - Einstellungen (Sprache, aktive Werte)
- `DetailsList` - Darstellung von Tabellendaten (Basis-Modus)
- `CommandBar` - Aktionsleiste mit Buttons
- `Pivot/PivotItem` - Tab-Navigation zwischen Modi
- `TextField` - Eingabefelder für Inline-Bearbeitung
- `MessageBar` - Feedback und Fehlermeldungen
- `Dialog` - Bestätigungsdialoge
- `Spinner` - Ladeanzeigen

### Zusätzliche Bibliotheken
- **Monaco Editor** (`@monaco-editor/react`) - VS Code Editor für JSON-Bearbeitung
- **Webpack 5** - Module Bundler
- **webpack-dev-server** - Entwicklungsserver mit Hot Reload

## 📦 Installation

### Using Setup Script (Recommended)

```powershell
# Navigate to repository root
cd /path/to/msfabric-mapping-etk

# Run frontend setup script
.\scripts\Setup\SetupFrontend.ps1

# Force reinstall all packages
.\scripts\Setup\SetupFrontend.ps1 -Force

# Run security audit
.\scripts\Setup\SetupFrontend.ps1 -Audit
```

### Manual Installation

```bash
# Abhängigkeiten installieren
npm install
```

## 🛠️ Entwicklung

### Using Development Scripts (Recommended)

```powershell
# Navigate to repository root
cd /path/to/msfabric-mapping-etk

# Start frontend development server
.\scripts\Run\StartFrontendDevServer.ps1

# Start on custom port
.\scripts\Run\StartFrontendDevServer.ps1 -Port 3001

# Open browser automatically
.\scripts\Run\StartFrontendDevServer.ps1 -Open

# Start full stack (backend + frontend)
.\scripts\Run\StartFullStack.ps1

# Customize ports for full stack
.\scripts\Run\StartFullStack.ps1 -ApiPort 5500 -FrontendPort 3001
```

### Manual Start

```bash
# Entwicklungsserver starten (Port 3000)
npm start
```

Der Entwicklungsserver startet automatisch und öffnet die Anwendung im Browser unter `http://localhost:3000`.

### Hot Reload

Der Entwicklungsserver unterstützt Hot Module Replacement (HMR) - Änderungen am Code werden automatisch im Browser aktualisiert.

## 🏗️ Build

### Using Build Scripts (Recommended)

```powershell
# Navigate to repository root
cd /path/to/msfabric-mapping-etk

# Build frontend for production
.\scripts\Build\BuildFrontend.ps1

# Build for development
.\scripts\Build\BuildFrontend.ps1 -Mode development

# Clean and build
.\scripts\Build\BuildFrontend.ps1 -Clean

# Force reinstall and build
.\scripts\Build\BuildFrontend.ps1 -Clean -Install

# Build both backend and frontend
.\scripts\Build\BuildAll.ps1

# Build with custom configuration
.\scripts\Build\BuildAll.ps1 -Configuration Debug -FrontendMode development

# Skip tests during build
.\scripts\Build\BuildAll.ps1 -SkipTests

# Build only frontend (skip backend)
.\scripts\Build\BuildAll.ps1 -SkipBackend
```

### Manual Build

```bash
# Produktions-Build erstellen
npm run build

# Entwicklungs-Build erstellen
npm run build:dev
```

Der Build wird im `dist/` Verzeichnis erstellt.

## 🔧 Konfiguration

### API-Endpunkt

Die Backend-URL kann über eine Umgebungsvariable konfiguriert werden:

```bash
# Standard: https://localhost:5001/api
export API_BASE_URL=https://your-api-url.com/api
```

Oder in `src/services/apiClient.ts` anpassen.

### TypeScript-Konfiguration

Die TypeScript-Konfiguration befindet sich in `tsconfig.json`:
- Target: ES2020
- Module: ESNext
- JSX: React
- Strict Mode: Aktiviert

### Webpack-Konfiguration

Die Webpack-Konfiguration (`webpack.config.js`) enthält:
- TypeScript-Loader (ts-loader)
- CSS-Loader mit Style-Loader
- HTML-Plugin für automatische Index-HTML-Generierung
- Dev-Server-Konfiguration

## 🎨 Styling

Das Frontend verwendet das **Fluent UI Design System**:
- Fluent UI Design-Tokens
- Microsoft Fabric Designkonventionen
- Responsive Layouts mit Fluent UI `Stack`-Komponenten
- Konsistente Farben, Typografie und Abstände

### Custom Styles

Custom Styles werden mit `mergeStyles` von Fluent UI erstellt:

```typescript
const customStyle = mergeStyles({
  padding: '20px',
  backgroundColor: '#faf9f8',
  // ...
});
```

## 📚 Komponenten-Dokumentation

### ConfigurationPanel

**Zweck**: Ermöglicht die Konfiguration der Ansicht und Auswahl von Referenztabellen.

**Props**:
- `tables`: Liste verfügbarer Tabellen
- `selectedTable`: Aktuell ausgewählte Tabelle
- `searchFilter`: Aktueller Suchfilter
- `language`: Aktuell ausgewählte Sprache
- `showActiveOnly`: Status des "Nur aktive Werte" Toggle
- `isLoading`: Ladezustand
- `onTableSelect`: Callback für Tabellen-Auswahl
- `onSearchChange`: Callback für Such-Änderungen
- `onLanguageChange`: Callback für Sprach-Wechsel
- `onShowActiveOnlyChange`: Callback für Toggle-Änderung

### BasicModeEditor

**Zweck**: Bietet eine tabellarische Ansicht mit Inline-Bearbeitung für Referenztabellen-Daten.

**Features**:
- Inline-Bearbeitung von Zellen
- Hinzufügen neuer Zeilen
- Löschen von Zeilen mit Bestätigungsdialog
- CRUD-Operationen
- Validierung vor dem Speichern

**Props**:
- `tableData`: Referenztabellen-Daten
- `onSave`: Callback zum Speichern von Änderungen
- `onCancel`: Callback zum Abbrechen

### ExpertModeEditor

**Zweck**: Ermöglicht die direkte Bearbeitung der Tabellendaten als JSON.

**Features**:
- Monaco Editor mit Syntax-Highlighting
- JSON-Formatierung
- JSON-Validierung
- Änderungs-Tracking
- Zurücksetzen-Funktion

**Props**:
- `tableData`: Referenztabellen-Daten
- `onSave`: Callback zum Speichern von Änderungen
- `onCancel`: Callback zum Abbrechen

### EditingArea

**Zweck**: Container-Komponente für Basis- und Experten-Modus mit Tab-Navigation.

**Props**:
- `tableData`: Referenztabellen-Daten
- `editMode`: Aktueller Bearbeitungsmodus ('basic' | 'expert')
- `onEditModeChange`: Callback für Modus-Wechsel
- `onSave`: Callback zum Speichern
- `onCancel`: Callback zum Abbrechen

## 🔌 API-Integration

Der `ApiClient` (`src/services/apiClient.ts`) bietet folgende Methoden:

```typescript
// Tabellenliste abrufen
await apiClient.listReferenceTables(): Promise<string[]>

// Tabellendaten laden
await apiClient.getReferenceTable(tableName: string): Promise<ReferenceTableData>

// Tabelle erstellen
await apiClient.createReferenceTable(metadata: ReferenceTableMetadata): Promise<void>

// Zeile aktualisieren
await apiClient.updateRow(tableName: string, row: ReferenceTableRow): Promise<void>

// Tabelle löschen
await apiClient.deleteReferenceTable(tableName: string): Promise<void>

// Daten synchronisieren
await apiClient.syncReferenceTable(tableName: string, keyAttributeName: string, data: any[]): Promise<{...}>
```

## 🌍 Internationalisierung

Das Frontend unterstützt aktuell:
- **Deutsch (de-DE)** - Standard
- **Englisch (en-US)**

Die Sprachauswahl kann über das Dropdown im Konfigurationsbereich geändert werden.

## 🎯 Microsoft Fabric Integration

Dieses Frontend wurde gemäß den [Microsoft Fabric Extensibility Toolkit Richtlinien](https://learn.microsoft.com/en-us/fabric/extensibility-toolkit/overview-story) entwickelt:

1. **Fluent UI Komponenten**: Alle UI-Komponenten stammen aus der offiziellen Fluent UI Bibliothek
2. **Design-Konsistenz**: Strikte Einhaltung der Microsoft Fabric Designkonventionen
3. **Modulare Struktur**: Komponenten sind wiederverwendbar und gut strukturiert
4. **TypeScript**: Vollständige Typsicherheit für bessere Wartbarkeit
5. **Responsive Design**: Optimiert für verschiedene Bildschirmgrößen

## 🔒 Best Practices

### Code-Qualität
- TypeScript strict mode aktiviert
- ESLint-konforme Code-Struktur
- Ausführliche Code-Kommentare (Deutsch)
- Konsistente Namenskonventionen

### React Best Practices
- Funktionale Komponenten mit Hooks
- Props-Validierung mit TypeScript
- Memoization wo sinnvoll
- Fehlerbehandlung mit try-catch

### Performance
- Lazy Loading von Komponenten möglich
- Optimierte Re-Renders
- Effiziente State-Verwaltung

## 📝 Weitere Hinweise

### Custom Components

Alle Komponenten basieren auf Standard-Fluent-UI-Komponenten. Es wurden keine vollständig benutzerdefinierten Komponenten implementiert, sondern nur Erweiterungen und Zusammenstellungen der vorhandenen Fluent UI Elemente.

### Anpassungen

Falls spezifische Anpassungen benötigt werden:
1. Fluent UI Theming kann über `ThemeProvider` angepasst werden
2. Custom Styles mit `mergeStyles` hinzufügen
3. Komponenten-Verhalten über Props konfigurieren

## 🤝 Beitragen

Dieses Projekt ist Teil des Microsoft Fabric Extensibility Toolkit Contest. Feedback und Verbesserungsvorschläge sind willkommen.

## 📄 Lizenz

Teil des Fabric Mapping Service Projekts von Philipp Frenzel.

## 🔗 Ressourcen

- [Microsoft Fabric Extensibility Toolkit](https://learn.microsoft.com/en-us/fabric/extensibility-toolkit/)
- [Fluent UI React](https://developer.microsoft.com/en-us/fluentui)
- [Monaco Editor](https://microsoft.github.io/monaco-editor/)
- [React Documentation](https://react.dev/)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)

## 📜 Scripts Documentation

For detailed documentation on all available PowerShell scripts for setup, building, and running the frontend and full stack:

- **[Scripts Documentation](../../scripts/README.md)**: Complete reference for all automation scripts
- **[Project Setup Guide](../../docs/PROJECT_SETUP.md)**: Comprehensive environment setup instructions
- **[Main README](../../README.md)**: Repository overview and quick start guide
