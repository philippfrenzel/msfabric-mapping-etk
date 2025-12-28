/**
 * Konfigurationsbereich-Komponente
 * 
 * Ermöglicht die Auswahl und Filterung von Referenztabellen sowie
 * Einstellungen für Sprache und Anzeigeoptionen.
 * 
 * Fluent UI v9 für natives Microsoft Fabric Aussehen.
 */

import React from 'react';
import {
  makeStyles,
  tokens,
  Dropdown,
  Option,
  SearchBox,
  Switch,
  Label,
  Text,
} from '@fluentui/react-components';
import { Language } from '../types';

// Fabric-native Styles
const useStyles = makeStyles({
  container: {
    padding: tokens.spacingHorizontalL,
    backgroundColor: tokens.colorNeutralBackground1,
  },
  title: {
    marginBottom: tokens.spacingVerticalM,
    fontWeight: tokens.fontWeightSemibold,
  },
  grid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
    gap: tokens.spacingHorizontalL,
    alignItems: 'start',
  },
  field: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalXS,
  },
  switchContainer: {
    display: 'flex',
    alignItems: 'center',
    gap: tokens.spacingHorizontalS,
    marginTop: tokens.spacingVerticalM,
  },
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
  const styles = useStyles();

  return (
    <div className={styles.container}>
      <Text size={500} weight="semibold" className={styles.title} block>
        Configuration
      </Text>

      <div className={styles.grid}>
        {/* Reference table selection */}
        <div className={styles.field}>
          <Label htmlFor="table-dropdown">Reference Table</Label>
          <Dropdown
            id="table-dropdown"
            placeholder="Select table..."
            value={selectedTable || ''}
            selectedOptions={selectedTable ? [selectedTable] : []}
            onOptionSelect={(_, data) => onTableSelect(data.optionValue || null)}
            disabled={isLoading || tables.length === 0}
          >
            {tables.map((table) => (
              <Option key={table} value={table}>
                {table}
              </Option>
            ))}
          </Dropdown>
        </div>

        {/* Search/Filter field */}
        <div className={styles.field}>
          <Label htmlFor="search-box">Filter Tables</Label>
          <SearchBox
            id="search-box"
            placeholder="Search..."
            value={searchFilter}
            onChange={(_, data) => onSearchChange(data.value)}
            disabled={isLoading}
          />
        </div>

        {/* Language settings */}
        <div className={styles.field}>
          <Label htmlFor="language-dropdown">Language</Label>
          <Dropdown
            id="language-dropdown"
            value={language === 'de-DE' ? 'German' : 'English'}
            selectedOptions={[language]}
            onOptionSelect={(_, data) => onLanguageChange(data.optionValue as Language)}
            disabled={isLoading}
          >
            <Option value="de-DE">German</Option>
            <Option value="en-US">English</Option>
          </Dropdown>
        </div>

        {/* Show active values only */}
        <div className={styles.field}>
          <Label>Display Options</Label>
          <div className={styles.switchContainer}>
            <Switch
              checked={showActiveOnly}
              onChange={(_, data) => onShowActiveOnlyChange(data.checked)}
              disabled={isLoading}
            />
            <Text>Active values only</Text>
          </div>
        </div>
      </div>
    </div>
  );
};
