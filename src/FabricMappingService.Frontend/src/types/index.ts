/**
 * Type-Definitionen für das Fabric Mapping Service Frontend
 */

/**
 * Spalten-Definition für Referenztabellen
 */
export interface ReferenceTableColumn {
  name: string;
  dataType: string;
  description?: string;
  order: number;
}

/**
 * Referenztabellen-Metadaten
 */
export interface ReferenceTableMetadata {
  tableName: string;
  columns: ReferenceTableColumn[];
  isVisible: boolean;
  notifyOnNewMapping: boolean;
  sourceLakehouseItemId?: string;
  sourceWorkspaceId?: string;
  sourceTableName?: string;
  sourceOneLakeLink?: string;
}

/**
 * Zeile in einer Referenztabelle
 */
export interface ReferenceTableRow {
  key: string;
  [columnName: string]: any;
}

/**
 * Referenztabellen-Daten (komplett)
 */
export interface ReferenceTableData {
  metadata: ReferenceTableMetadata;
  rows: ReferenceTableRow[];
}

/**
 * Sprach-Optionen
 */
export type Language = 'de-DE' | 'en-US';

/**
 * Bearbeitungsmodus
 */
export type EditMode = 'basic' | 'expert';

/**
 * Anwendungs-State
 */
export interface AppState {
  selectedTable: string | null;
  tables: string[];
  currentTableData: ReferenceTableData | null;
  searchFilter: string;
  language: Language;
  showActiveOnly: boolean;
  editMode: EditMode;
  isLoading: boolean;
  error: string | null;
}
