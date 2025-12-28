/**
 * Haupt-App-Komponente für das Fabric Mapping Service Frontend
 * 
 * Orchestriert den Konfigurationsbereich und Bearbeitungsbereich
 * und verwaltet den globalen Anwendungs-State.
 * 
 * Verwendet Fluent UI v9 für natives Microsoft Fabric Aussehen.
 */

import React, { useState, useEffect } from 'react';
import {
  makeStyles,
  tokens,
  Spinner,
  MessageBar,
  MessageBarBody,
  MessageBarTitle,
  Title2,
  Body1,
  Divider,
} from '@fluentui/react-components';
import { TableRegular } from '@fluentui/react-icons';
import { ConfigurationPanel } from './components/ConfigurationPanel';
import { EditingArea } from './components/EditingArea';
import { apiClient } from './services/apiClient';
import { AppState, ReferenceTableRow, Language, EditMode } from './types';

// Fabric-native Styles mit Fluent UI v9 tokens
const useStyles = makeStyles({
  appContainer: {
    height: '100vh',
    overflow: 'hidden',
    backgroundColor: tokens.colorNeutralBackground2,
    display: 'flex',
    flexDirection: 'column',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: tokens.spacingHorizontalM,
    padding: `${tokens.spacingVerticalM} ${tokens.spacingHorizontalL}`,
    backgroundColor: tokens.colorNeutralBackground1,
    borderBottom: `1px solid ${tokens.colorNeutralStroke1}`,
  },
  headerIcon: {
    color: tokens.colorBrandForeground1,
    fontSize: '24px',
  },
  headerTitle: {
    color: tokens.colorNeutralForeground1,
  },
  content: {
    flex: 1,
    padding: tokens.spacingHorizontalL,
    overflow: 'auto',
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100%',
  },
  emptyState: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    height: '300px',
    gap: tokens.spacingVerticalM,
    color: tokens.colorNeutralForeground3,
  },
  emptyStateIcon: {
    fontSize: '48px',
    color: tokens.colorNeutralForeground4,
  },
});

/**
 * Haupt-App-Komponente
 */
export const App: React.FC = () => {
  const styles = useStyles();

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
    <div className={styles.appContainer}>
      {/* Header */}
      <div className={styles.header}>
        <TableRegular className={styles.headerIcon} />
        <Title2 className={styles.headerTitle}>Reference Tables</Title2>
      </div>

      {/* Error display */}
      {state.error && (
        <MessageBar intent="error" style={{ margin: '16px' }}>
          <MessageBarBody>
            <MessageBarTitle>Error</MessageBarTitle>
            {state.error}
          </MessageBarBody>
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

      <Divider />

      {/* Hauptinhalt */}
      <div className={styles.content}>
        {state.isLoading ? (
          <div className={styles.loadingContainer}>
            <Spinner size="large" label="Loading data..." />
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
          <div className={styles.emptyState}>
            <TableRegular className={styles.emptyStateIcon} />
            <Body1>
              Please select a reference table to start editing.
            </Body1>
          </div>
        )}
      </div>
    </div>
  );
};
