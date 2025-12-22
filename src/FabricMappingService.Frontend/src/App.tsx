/**
 * Haupt-App-Komponente für das Fabric Mapping Service Frontend
 * 
 * Orchestriert den Konfigurationsbereich und Bearbeitungsbereich
 * und verwaltet den globalen Anwendungs-State.
 */

import React, { useState, useEffect } from 'react';
import {
  Stack,
  Spinner,
  SpinnerSize,
  MessageBar,
  MessageBarType,
  mergeStyles,
  initializeIcons,
} from '@fluentui/react';
import { ConfigurationPanel } from './components/ConfigurationPanel';
import { EditingArea } from './components/EditingArea';
import { apiClient } from './services/apiClient';
import { AppState, ReferenceTableRow, Language, EditMode } from './types';

// Initialisiere Fluent UI Icons
initializeIcons();

// Styling
const appContainerStyle = mergeStyles({
  height: '100vh',
  overflow: 'hidden',
  backgroundColor: '#faf9f8',
  fontFamily: "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif",
});

const headerStyle = mergeStyles({
  padding: '16px 20px',
  backgroundColor: '#0078d4',
  color: '#ffffff',
  fontSize: '24px',
  fontWeight: 600,
  borderBottom: '2px solid #106ebe',
});

const contentStyle = mergeStyles({
  padding: '20px',
  height: 'calc(100vh - 200px)',
  overflow: 'auto',
});

const loadingContainerStyle = mergeStyles({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100%',
});

/**
 * Haupt-App-Komponente
 */
export const App: React.FC = () => {
  // Anwendungs-State
  const [state, setState] = useState<AppState>({
    selectedTable: null,
    tables: [],
    currentTableData: null,
    searchFilter: '',
    language: 'de-DE',
    showActiveOnly: false,
    editMode: 'basic',
    isLoading: false,
    error: null,
  });

  // Lade Tabellenliste beim Start
  useEffect(() => {
    loadTables();
  }, []);

  // Lade Tabellendaten, wenn eine Tabelle ausgewählt wird
  useEffect(() => {
    if (state.selectedTable) {
      loadTableData(state.selectedTable);
    }
  }, [state.selectedTable]);

  /**
   * Lädt die Liste aller verfügbaren Referenztabellen
   */
  const loadTables = async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const tables = await apiClient.listReferenceTables();
      setState((prev) => ({ ...prev, tables, isLoading: false }));
    } catch (error) {
      setState((prev) => ({
        ...prev,
        error: `Fehler beim Laden der Tabellen: ${error}`,
        isLoading: false,
      }));
    }
  };

  /**
   * Lädt die Daten einer bestimmten Referenztabelle
   */
  const loadTableData = async (tableName: string) => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const tableData = await apiClient.getReferenceTable(tableName);
      setState((prev) => ({ ...prev, currentTableData: tableData, isLoading: false }));
    } catch (error) {
      setState((prev) => ({
        ...prev,
        error: `Fehler beim Laden der Tabellendaten: ${error}`,
        isLoading: false,
        currentTableData: null,
      }));
    }
  };

  /**
   * Handler für Tabellen-Auswahl
   */
  const handleTableSelect = (tableName: string | null) => {
    setState((prev) => ({ ...prev, selectedTable: tableName }));
  };

  /**
   * Handler für Such-Filter
   */
  const handleSearchChange = (filter: string) => {
    setState((prev) => ({ ...prev, searchFilter: filter }));
  };

  /**
   * Handler für Sprach-Wechsel
   */
  const handleLanguageChange = (language: Language) => {
    setState((prev) => ({ ...prev, language }));
  };

  /**
   * Handler für "Nur aktive Werte anzeigen" Toggle
   */
  const handleShowActiveOnlyChange = (checked: boolean) => {
    setState((prev) => ({ ...prev, showActiveOnly: checked }));
  };

  /**
   * Handler für Bearbeitungsmodus-Wechsel
   */
  const handleEditModeChange = (mode: EditMode) => {
    setState((prev) => ({ ...prev, editMode: mode }));
  };

  /**
   * Handler für Speichern von Änderungen
   */
  const handleSave = async (rows: ReferenceTableRow[]) => {
    if (!state.selectedTable) return;

    setState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      // Speichere jede Zeile einzeln
      for (const row of rows) {
        await apiClient.updateRow(state.selectedTable!, row);
      }

      // Lade die Tabellendaten neu
      await loadTableData(state.selectedTable);
      
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: null,
      }));
    } catch (error) {
      setState((prev) => ({
        ...prev,
        error: `Fehler beim Speichern: ${error}`,
        isLoading: false,
      }));
      throw error;
    }
  };

  /**
   * Handler für Abbrechen
   */
  const handleCancel = () => {
    if (state.selectedTable) {
      loadTableData(state.selectedTable);
    }
  };

  // Filtere Tabellen basierend auf Suchfilter
  const filteredTables = state.tables.filter((table) =>
    table.toLowerCase().includes(state.searchFilter.toLowerCase())
  );

  return (
    <div className={appContainerStyle}>
      {/* Header */}
      <div className={headerStyle}>
        Fabric Mapping Service - Referenztabellen Editor
      </div>

      {/* Fehleranzeige */}
      {state.error && (
        <MessageBar
          messageBarType={MessageBarType.error}
          onDismiss={() => setState((prev) => ({ ...prev, error: null }))}
        >
          {state.error}
        </MessageBar>
      )}

      {/* Konfigurationsbereich */}
      <ConfigurationPanel
        tables={filteredTables}
        selectedTable={state.selectedTable}
        searchFilter={state.searchFilter}
        language={state.language}
        showActiveOnly={state.showActiveOnly}
        isLoading={state.isLoading}
        onTableSelect={handleTableSelect}
        onSearchChange={handleSearchChange}
        onLanguageChange={handleLanguageChange}
        onShowActiveOnlyChange={handleShowActiveOnlyChange}
      />

      {/* Hauptinhalt */}
      <div className={contentStyle}>
        {state.isLoading ? (
          <div className={loadingContainerStyle}>
            <Spinner size={SpinnerSize.large} label="Lade Daten..." />
          </div>
        ) : state.selectedTable ? (
          <EditingArea
            tableData={state.currentTableData}
            editMode={state.editMode}
            onEditModeChange={handleEditModeChange}
            onSave={handleSave}
            onCancel={handleCancel}
          />
        ) : (
          <MessageBar messageBarType={MessageBarType.info}>
            Bitte wählen Sie eine Referenztabelle aus, um mit der Bearbeitung zu beginnen.
          </MessageBar>
        )}
      </div>
    </div>
  );
};
