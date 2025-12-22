/**
 * Experten-Modus Editor: JSON-Editor mit Syntax-Highlighting
 * 
 * Ermöglicht die direkte Bearbeitung der gesamten Referenztabelle als JSON
 * mit Syntax-Highlighting, Formatierung und Validierung.
 */

import React, { useState, useEffect } from 'react';
import Editor from '@monaco-editor/react';
import {
  Stack,
  PrimaryButton,
  DefaultButton,
  CommandBar,
  ICommandBarItemProps,
  MessageBar,
  MessageBarType,
  mergeStyles,
} from '@fluentui/react';
import { ReferenceTableData, ReferenceTableRow } from '../types';

const containerStyle = mergeStyles({
  padding: '20px',
  height: 'calc(100vh - 300px)',
});

const editorContainerStyle = mergeStyles({
  height: 'calc(100% - 120px)',
  border: '1px solid #d1d1d1',
  borderRadius: '4px',
  overflow: 'hidden',
});

/**
 * Props für die ExpertModeEditor-Komponente
 */
interface ExpertModeEditorProps {
  tableData: ReferenceTableData | null;
  onSave: (rows: ReferenceTableRow[]) => Promise<void>;
  onCancel: () => void;
}

/**
 * Experten-Modus Editor für Referenztabellen (JSON)
 */
export const ExpertModeEditor: React.FC<ExpertModeEditorProps> = ({
  tableData,
  onSave,
  onCancel,
}) => {
  const [jsonContent, setJsonContent] = useState<string>('');
  const [originalJson, setOriginalJson] = useState<string>('');
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: MessageBarType; text: string } | null>(null);
  const [hasChanges, setHasChanges] = useState(false);

  // Initialisiere JSON-Inhalt, wenn tableData sich ändert
  useEffect(() => {
    if (tableData) {
      const jsonString = JSON.stringify(tableData.rows, null, 2);
      setJsonContent(jsonString);
      setOriginalJson(jsonString);
      setHasChanges(false);
    }
  }, [tableData]);

  // Handler für Editor-Änderungen
  const handleEditorChange = (value: string | undefined) => {
    if (value !== undefined) {
      setJsonContent(value);
      setHasChanges(value !== originalJson);
    }
  };

  // Handler für Formatierung
  const handleFormat = () => {
    try {
      const parsed = JSON.parse(jsonContent);
      const formatted = JSON.stringify(parsed, null, 2);
      setJsonContent(formatted);
      setMessage({ type: MessageBarType.success, text: 'JSON formatiert' });
      setTimeout(() => setMessage(null), 2000);
    } catch (error) {
      setMessage({
        type: MessageBarType.error,
        text: `Formatierungsfehler: ${error instanceof Error ? error.message : 'Unbekannter Fehler'}`,
      });
    }
  };

  // Handler für Validierung
  const handleValidate = () => {
    try {
      JSON.parse(jsonContent);
      setMessage({ type: MessageBarType.success, text: 'JSON ist gültig' });
      setTimeout(() => setMessage(null), 2000);
    } catch (error) {
      setMessage({
        type: MessageBarType.error,
        text: `Validierungsfehler: ${error instanceof Error ? error.message : 'Unbekannter Fehler'}`,
      });
    }
  };

  // Handler für Speichern
  const handleSave = async () => {
    try {
      // Validiere JSON
      const parsed = JSON.parse(jsonContent);
      
      // Prüfe, ob es ein Array ist
      if (!Array.isArray(parsed)) {
        throw new Error('JSON muss ein Array von Zeilen sein');
      }

      // Speichere die Änderungen
      setIsSaving(true);
      await onSave(parsed as ReferenceTableRow[]);
      
      setOriginalJson(jsonContent);
      setHasChanges(false);
      setMessage({ type: MessageBarType.success, text: 'Änderungen gespeichert' });
    } catch (error) {
      setMessage({
        type: MessageBarType.error,
        text: `Fehler beim Speichern: ${error instanceof Error ? error.message : 'Unbekannter Fehler'}`,
      });
    } finally {
      setIsSaving(false);
    }
  };

  // Handler für Zurücksetzen
  const handleReset = () => {
    setJsonContent(originalJson);
    setHasChanges(false);
    setMessage({ type: MessageBarType.info, text: 'Änderungen zurückgesetzt' });
    setTimeout(() => setMessage(null), 2000);
  };

  // Command Bar Elemente
  const commandBarItems: ICommandBarItemProps[] = [
    {
      key: 'format',
      text: 'Formatieren',
      iconProps: { iconName: 'Code' },
      onClick: handleFormat,
    },
    {
      key: 'validate',
      text: 'Validieren',
      iconProps: { iconName: 'CheckMark' },
      onClick: handleValidate,
    },
    {
      key: 'save',
      text: 'Speichern',
      iconProps: { iconName: 'Save' },
      onClick: handleSave,
      disabled: !hasChanges || isSaving,
    },
    {
      key: 'reset',
      text: 'Zurücksetzen',
      iconProps: { iconName: 'Undo' },
      onClick: handleReset,
      disabled: !hasChanges,
    },
    {
      key: 'cancel',
      text: 'Abbrechen',
      iconProps: { iconName: 'Cancel' },
      onClick: onCancel,
    },
  ];

  if (!tableData) {
    return <div className={containerStyle}>Keine Tabelle ausgewählt</div>;
  }

  return (
    <div className={containerStyle}>
      {message && (
        <MessageBar
          messageBarType={message.type}
          onDismiss={() => setMessage(null)}
          styles={{ root: { marginBottom: '12px' } }}
        >
          {message.text}
        </MessageBar>
      )}

      <CommandBar items={commandBarItems} />

      <div className={editorContainerStyle}>
        <Editor
          height="100%"
          defaultLanguage="json"
          value={jsonContent}
          onChange={handleEditorChange}
          theme="vs-light"
          options={{
            minimap: { enabled: true },
            fontSize: 14,
            lineNumbers: 'on',
            scrollBeyondLastLine: false,
            automaticLayout: true,
            formatOnPaste: true,
            formatOnType: true,
            tabSize: 2,
          }}
        />
      </div>

      {hasChanges && (
        <MessageBar
          messageBarType={MessageBarType.warning}
          styles={{ root: { marginTop: '12px' } }}
        >
          Sie haben ungespeicherte Änderungen
        </MessageBar>
      )}
    </div>
  );
};
