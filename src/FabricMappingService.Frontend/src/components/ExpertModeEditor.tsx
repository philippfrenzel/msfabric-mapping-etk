/**
 * Experten-Modus Editor: JSON-Editor mit Syntax-Highlighting
 * 
 * Ermöglicht die direkte Bearbeitung der gesamten Referenztabelle als JSON
 * mit Syntax-Highlighting, Formatierung und Validierung.
 * 
 * Fluent UI v9 für natives Microsoft Fabric Aussehen.
 */

import React, { useState, useEffect } from 'react';
import Editor from '@monaco-editor/react';
import {
  makeStyles,
  tokens,
  Button,
  Toolbar,
  ToolbarButton,
  MessageBar,
  MessageBarBody,
  Text,
} from '@fluentui/react-components';
import {
  CodeRegular,
  CheckmarkRegular,
  SaveRegular,
  ArrowUndoRegular,
  DismissRegular,
} from '@fluentui/react-icons';
import { ReferenceTableData, ReferenceTableRow } from '../types';

const useStyles = makeStyles({
  container: {
    padding: tokens.spacingHorizontalL,
    height: 'calc(100vh - 320px)',
    display: 'flex',
    flexDirection: 'column',
  },
  toolbar: {
    marginBottom: tokens.spacingVerticalM,
    padding: tokens.spacingHorizontalS,
    backgroundColor: tokens.colorNeutralBackground3,
    borderRadius: tokens.borderRadiusMedium,
  },
  editorContainer: {
    flex: 1,
    border: `1px solid ${tokens.colorNeutralStroke1}`,
    borderRadius: tokens.borderRadiusMedium,
    overflow: 'hidden',
  },
  emptyState: {
    textAlign: 'center',
    padding: tokens.spacingVerticalXXL,
    color: tokens.colorNeutralForeground3,
  },
  warningBar: {
    marginTop: tokens.spacingVerticalM,
  },
});

interface ExpertModeEditorProps {
  tableData: ReferenceTableData | null;
  onSave: (rows: ReferenceTableRow[]) => Promise<void>;
  onCancel: () => void;
}

export const ExpertModeEditor: React.FC<ExpertModeEditorProps> = ({
  tableData,
  onSave,
  onCancel,
}) => {
  const styles = useStyles();
  const [jsonContent, setJsonContent] = useState<string>('');
  const [originalJson, setOriginalJson] = useState<string>('');
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error' | 'info'; text: string } | null>(null);
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    if (tableData) {
      const jsonString = JSON.stringify(tableData.rows, null, 2);
      setJsonContent(jsonString);
      setOriginalJson(jsonString);
      setHasChanges(false);
    }
  }, [tableData]);

  const handleEditorChange = (value: string | undefined) => {
    if (value !== undefined) {
      setJsonContent(value);
      setHasChanges(value !== originalJson);
    }
  };

  const handleFormat = () => {
    try {
      const parsed = JSON.parse(jsonContent);
      const formatted = JSON.stringify(parsed, null, 2);
      setJsonContent(formatted);
      setMessage({ type: 'success', text: 'JSON formatted' });
      setTimeout(() => setMessage(null), 2000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Format error: ${error instanceof Error ? error.message : 'Unknown error'}`,
      });
    }
  };

  const handleValidate = () => {
    try {
      JSON.parse(jsonContent);
      setMessage({ type: 'success', text: 'JSON is valid' });
      setTimeout(() => setMessage(null), 2000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Validation error: ${error instanceof Error ? error.message : 'Unknown error'}`,
      });
    }
  };

  const handleSave = async () => {
    try {
      const parsed = JSON.parse(jsonContent);
      if (!Array.isArray(parsed)) {
        throw new Error('JSON must be an array of rows');
      }
      setIsSaving(true);
      await onSave(parsed as ReferenceTableRow[]);
      setOriginalJson(jsonContent);
      setHasChanges(false);
      setMessage({ type: 'success', text: 'Changes saved' });
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Error saving: ${error instanceof Error ? error.message : 'Unknown error'}`,
      });
    } finally {
      setIsSaving(false);
    }
  };

  const handleReset = () => {
    setJsonContent(originalJson);
    setHasChanges(false);
    setMessage({ type: 'info', text: 'Changes reset' });
    setTimeout(() => setMessage(null), 2000);
  };

  if (!tableData) {
    return (
      <div className={styles.container}>
        <div className={styles.emptyState}>
          <Text>No table selected</Text>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      {message && (
        <MessageBar 
          intent={message.type === 'success' ? 'success' : message.type === 'error' ? 'error' : 'info'} 
          style={{ marginBottom: '12px' }}
        >
          <MessageBarBody>{message.text}</MessageBarBody>
        </MessageBar>
      )}

      <Toolbar className={styles.toolbar}>
        <ToolbarButton icon={<CodeRegular />} onClick={handleFormat}>
          Format
        </ToolbarButton>
        <ToolbarButton icon={<CheckmarkRegular />} onClick={handleValidate}>
          Validate
        </ToolbarButton>
        <ToolbarButton
          icon={<SaveRegular />}
          onClick={handleSave}
          disabled={!hasChanges || isSaving}
          appearance="primary"
        >
          Save
        </ToolbarButton>
        <ToolbarButton
          icon={<ArrowUndoRegular />}
          onClick={handleReset}
          disabled={!hasChanges}
        >
          Reset
        </ToolbarButton>
        <ToolbarButton icon={<DismissRegular />} onClick={onCancel}>
          Cancel
        </ToolbarButton>
      </Toolbar>

      <div className={styles.editorContainer}>
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
        <MessageBar intent="warning" className={styles.warningBar}>
          <MessageBarBody>You have unsaved changes</MessageBarBody>
        </MessageBar>
      )}
    </div>
  );
};
