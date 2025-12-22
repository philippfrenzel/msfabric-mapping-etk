/**
 * API-Client für die Kommunikation mit dem Fabric Mapping Service Backend
 */

import { ReferenceTableData, ReferenceTableMetadata, ReferenceTableRow } from '../types';

/**
 * Basis-URL für die API (kann über Umgebungsvariablen konfiguriert werden)
 */
const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:5001/api';

/**
 * API-Client Klasse für Backend-Kommunikation
 */
export class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  /**
   * Listet alle verfügbaren Referenztabellen auf
   */
  async listReferenceTables(): Promise<string[]> {
    const response = await fetch(`${this.baseUrl}/reference-tables`);
    if (!response.ok) {
      throw new Error(`Fehler beim Laden der Tabellenliste: ${response.statusText}`);
    }
    return response.json();
  }

  /**
   * Lädt eine bestimmte Referenztabelle mit allen Daten
   */
  async getReferenceTable(tableName: string): Promise<ReferenceTableData> {
    const response = await fetch(`${this.baseUrl}/reference-tables/${tableName}`);
    if (!response.ok) {
      throw new Error(`Fehler beim Laden der Tabelle ${tableName}: ${response.statusText}`);
    }
    return response.json();
  }

  /**
   * Erstellt eine neue Referenztabelle
   */
  async createReferenceTable(metadata: ReferenceTableMetadata): Promise<void> {
    const response = await fetch(`${this.baseUrl}/reference-tables`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(metadata),
    });
    if (!response.ok) {
      throw new Error(`Fehler beim Erstellen der Tabelle: ${response.statusText}`);
    }
  }

  /**
   * Aktualisiert oder fügt eine Zeile in der Referenztabelle hinzu
   */
  async updateRow(tableName: string, row: ReferenceTableRow): Promise<void> {
    const { key, ...attributes } = row;
    const response = await fetch(`${this.baseUrl}/reference-tables/${tableName}/rows`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        key,
        attributes,
      }),
    });
    if (!response.ok) {
      throw new Error(`Fehler beim Aktualisieren der Zeile: ${response.statusText}`);
    }
  }

  /**
   * Löscht eine Referenztabelle
   */
  async deleteReferenceTable(tableName: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/reference-tables/${tableName}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error(`Fehler beim Löschen der Tabelle: ${response.statusText}`);
    }
  }

  /**
   * Synchronisiert Daten mit einer Referenztabelle
   */
  async syncReferenceTable(
    tableName: string,
    keyAttributeName: string,
    data: any[]
  ): Promise<{ newKeysAdded: number; totalKeys: number }> {
    const response = await fetch(`${this.baseUrl}/reference-tables/sync`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        mappingTableName: tableName,
        keyAttributeName,
        data,
      }),
    });
    if (!response.ok) {
      throw new Error(`Fehler beim Synchronisieren der Tabelle: ${response.statusText}`);
    }
    return response.json();
  }
}

// Singleton-Instanz exportieren
export const apiClient = new ApiClient();
