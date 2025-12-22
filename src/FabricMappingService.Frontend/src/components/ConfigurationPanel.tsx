/**
 * Konfigurationsbereich-Komponente
 * 
 * Ermöglicht die Auswahl und Filterung von Referenztabellen sowie
 * Einstellungen für Sprache und Anzeigeoptionen.
 */

import React from 'react';
import {
  Dropdown,
  IDropdownOption,
  SearchBox,
  Toggle,
  Stack,
  Label,
  mergeStyles,
  Text,
} from '@fluentui/react';
import { Language } from '../types';

// Styling
const containerStyle = mergeStyles({
  padding: '20px',
  backgroundColor: '#faf9f8',
  borderBottom: '1px solid #edebe9',
  minHeight: '120px',
});

const sectionStyle = mergeStyles({
  marginBottom: '12px',
});

/**
 * Props für die ConfigurationPanel-Komponente
 */
interface ConfigurationPanelProps {
  tables: string[];
  selectedTable: string | null;
  searchFilter: string;
  language: Language;
  showActiveOnly: boolean;
  isLoading: boolean;
  onTableSelect: (tableName: string | null) => void;
  onSearchChange: (filter: string) => void;
  onLanguageChange: (language: Language) => void;
  onShowActiveOnlyChange: (checked: boolean) => void;
}

/**
 * Konfigurationsbereich für Referenztabellen
 */
export const ConfigurationPanel: React.FC<ConfigurationPanelProps> = ({
  tables,
  selectedTable,
  searchFilter,
  language,
  showActiveOnly,
  isLoading,
  onTableSelect,
  onSearchChange,
  onLanguageChange,
  onShowActiveOnlyChange,
}) => {
  // Dropdown-Optionen für Tabellen
  const tableOptions: IDropdownOption[] = tables.map((table) => ({
    key: table,
    text: table,
  }));

  // Dropdown-Optionen für Sprache
  const languageOptions: IDropdownOption[] = [
    { key: 'de-DE', text: 'Deutsch' },
    { key: 'en-US', text: 'English' },
  ];

  return (
    <div className={containerStyle}>
      <Stack tokens={{ childrenGap: 16 }}>
        {/* Überschrift */}
        <Text variant="xLarge" block>
          Konfiguration
        </Text>

        <Stack horizontal tokens={{ childrenGap: 20 }} wrap>
          {/* Referenztabellen-Auswahl */}
          <Stack className={sectionStyle} styles={{ root: { minWidth: 300 } }}>
            <Label>Referenztabelle auswählen</Label>
            <Dropdown
              placeholder="Tabelle auswählen..."
              options={tableOptions}
              selectedKey={selectedTable}
              onChange={(_, option) => onTableSelect(option ? String(option.key) : null)}
              disabled={isLoading || tables.length === 0}
              styles={{ dropdown: { width: 300 } }}
            />
          </Stack>

          {/* Such-/Filterfeld */}
          <Stack className={sectionStyle} styles={{ root: { minWidth: 300 } }}>
            <Label>Tabellen filtern</Label>
            <SearchBox
              placeholder="Suchen..."
              value={searchFilter}
              onChange={(_, newValue) => onSearchChange(newValue || '')}
              disabled={isLoading}
              styles={{ root: { width: 300 } }}
            />
          </Stack>

          {/* Spracheinstellungen */}
          <Stack className={sectionStyle} styles={{ root: { minWidth: 200 } }}>
            <Label>Sprache</Label>
            <Dropdown
              options={languageOptions}
              selectedKey={language}
              onChange={(_, option) => onLanguageChange(option?.key as Language)}
              disabled={isLoading}
              styles={{ dropdown: { width: 200 } }}
            />
          </Stack>

          {/* Nur aktive Werte anzeigen */}
          <Stack className={sectionStyle} styles={{ root: { minWidth: 200 } }}>
            <Label>Anzeigeoptionen</Label>
            <Toggle
              label="Nur aktive Werte anzeigen"
              checked={showActiveOnly}
              onChange={(_, checked) => onShowActiveOnlyChange(checked || false)}
              disabled={isLoading}
            />
          </Stack>
        </Stack>
      </Stack>
    </div>
  );
};
