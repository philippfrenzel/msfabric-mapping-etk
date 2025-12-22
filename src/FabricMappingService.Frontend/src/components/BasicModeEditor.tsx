/**
 * Basis-Modus Editor: Inline-editierbare Datentabelle
 * 
 * CRUD-Funktionalität für Referenztabellen-Zeilen mit
 * Inline-Bearbeitung, Hinzufügen und Löschen von Einträgen.
 */

import React, { useState, useEffect } from 'react';
import {
  DetailsList,
  DetailsListLayoutMode,
  IColumn,
  SelectionMode,
  CommandBar,
  ICommandBarItemProps,
  TextField,
  PrimaryButton,
  DefaultButton,
  Stack,
  MessageBar,
  MessageBarType,
  mergeStyles,
  Dialog,
  DialogType,
  DialogFooter,
} from '@fluentui/react';
import { ReferenceTableData, ReferenceTableRow } from '../types';

const containerStyle = mergeStyles({
  padding: '20px',
  height: 'calc(100vh - 300px)',
  overflow: 'auto',
});

/**
 * Props für die BasicModeEditor-Komponente
 */
interface BasicModeEditorProps {
  tableData: ReferenceTableData | null;
  onSave: (rows: ReferenceTableRow[]) => Promise<void>;
  onCancel: () => void;
}

/**
 * Basis-Modus Editor für Referenztabellen
 */
export const BasicModeEditor: React.FC<BasicModeEditorProps> = ({
  tableData,
  onSave,
  onCancel,
}) => {
  const [rows, setRows] = useState<ReferenceTableRow[]>([]);
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [isAddingNew, setIsAddingNew] = useState(false);
  const [newRow, setNewRow] = useState<Partial<ReferenceTableRow>>({});
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: MessageBarType; text: string } | null>(null);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [rowToDelete, setRowToDelete] = useState<string | null>(null);

  // Initialisiere Zeilen, wenn tableData sich ändert
  useEffect(() => {
    if (tableData) {
      setRows([...tableData.rows]);
    }
  }, [tableData]);

  // Erstelle Spalten-Definitionen aus Tabellen-Metadaten
  const columns: IColumn[] = React.useMemo(() => {
    if (!tableData) return [];

    const cols: IColumn[] = [
      {
        key: 'key',
        name: 'Schlüssel',
        fieldName: 'key',
        minWidth: 100,
        maxWidth: 200,
        isResizable: true,
      },
    ];

    // Füge Spalten für jedes Attribut hinzu
    tableData.metadata.columns.forEach((col) => {
      cols.push({
        key: col.name,
        name: col.name,
        fieldName: col.name,
        minWidth: 150,
        maxWidth: 300,
        isResizable: true,
        onRender: (item: ReferenceTableRow) => {
          if (editingKey === item.key) {
            return (
              <TextField
                value={String(item[col.name] || '')}
                onChange={(_, newValue) => handleCellEdit(item.key, col.name, newValue || '')}
              />
            );
          }
          return <span>{String(item[col.name] || '')}</span>;
        },
      });
    });

    // Aktionen-Spalte
    cols.push({
      key: 'actions',
      name: 'Aktionen',
      minWidth: 150,
      maxWidth: 200,
      onRender: (item: ReferenceTableRow) => (
        <Stack horizontal tokens={{ childrenGap: 8 }}>
          {editingKey === item.key ? (
            <>
              <PrimaryButton text="Speichern" onClick={() => handleSaveRow(item.key)} />
              <DefaultButton text="Abbrechen" onClick={() => setEditingKey(null)} />
            </>
          ) : (
            <>
              <DefaultButton text="Bearbeiten" onClick={() => setEditingKey(item.key)} />
              <DefaultButton text="Löschen" onClick={() => handleDeleteClick(item.key)} />
            </>
          )}
        </Stack>
      ),
    });

    return cols;
  }, [tableData, editingKey, rows]);

  // Handler für Zell-Bearbeitung
  const handleCellEdit = (key: string, columnName: string, value: string) => {
    setRows((prevRows) =>
      prevRows.map((row) => (row.key === key ? { ...row, [columnName]: value } : row))
    );
  };

  // Handler für Zeilen-Speicherung
  const handleSaveRow = (key: string) => {
    setEditingKey(null);
    setMessage({ type: MessageBarType.success, text: 'Zeile gespeichert' });
    setTimeout(() => setMessage(null), 3000);
  };

  // Handler für Löschen-Button
  const handleDeleteClick = (key: string) => {
    setRowToDelete(key);
    setShowDeleteDialog(true);
  };

  // Handler für bestätigtes Löschen
  const handleDeleteConfirm = () => {
    if (rowToDelete) {
      setRows((prevRows) => prevRows.filter((row) => row.key !== rowToDelete));
      setMessage({ type: MessageBarType.success, text: 'Zeile gelöscht' });
      setTimeout(() => setMessage(null), 3000);
    }
    setShowDeleteDialog(false);
    setRowToDelete(null);
  };

  // Handler für neue Zeile hinzufügen
  const handleAddNew = () => {
    setIsAddingNew(true);
    const initialRow: Partial<ReferenceTableRow> = { key: '' };
    if (tableData) {
      tableData.metadata.columns.forEach((col) => {
        initialRow[col.name] = '';
      });
    }
    setNewRow(initialRow);
  };

  // Handler für Speichern der neuen Zeile
  const handleSaveNew = () => {
    if (!newRow.key) {
      setMessage({ type: MessageBarType.error, text: 'Schlüssel ist erforderlich' });
      return;
    }

    setRows([...rows, newRow as ReferenceTableRow]);
    setIsAddingNew(false);
    setNewRow({});
    setMessage({ type: MessageBarType.success, text: 'Neue Zeile hinzugefügt' });
    setTimeout(() => setMessage(null), 3000);
  };

  // Handler für Abbrechen der neuen Zeile
  const handleCancelNew = () => {
    setIsAddingNew(false);
    setNewRow({});
  };

  // Handler für Speichern aller Änderungen
  const handleSaveAll = async () => {
    setIsSaving(true);
    try {
      await onSave(rows);
      setMessage({ type: MessageBarType.success, text: 'Alle Änderungen gespeichert' });
    } catch (error) {
      setMessage({
        type: MessageBarType.error,
        text: `Fehler beim Speichern: ${error}`,
      });
    } finally {
      setIsSaving(false);
    }
  };

  // Command Bar Elemente
  const commandBarItems: ICommandBarItemProps[] = [
    {
      key: 'add',
      text: 'Neue Zeile',
      iconProps: { iconName: 'Add' },
      onClick: handleAddNew,
      disabled: isAddingNew,
    },
    {
      key: 'save',
      text: 'Speichern',
      iconProps: { iconName: 'Save' },
      onClick: handleSaveAll,
      disabled: isSaving,
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
        <MessageBar messageBarType={message.type} onDismiss={() => setMessage(null)}>
          {message.text}
        </MessageBar>
      )}

      <CommandBar items={commandBarItems} />

      {/* Formular für neue Zeile */}
      {isAddingNew && (
        <Stack
          tokens={{ childrenGap: 12 }}
          styles={{
            root: {
              padding: '16px',
              backgroundColor: '#f3f2f1',
              marginBottom: '16px',
              borderRadius: '4px',
            },
          }}
        >
          <TextField
            label="Schlüssel"
            value={newRow.key || ''}
            onChange={(_, value) => setNewRow({ ...newRow, key: value })}
            required
          />
          {tableData.metadata.columns.map((col) => (
            <TextField
              key={col.name}
              label={col.name}
              value={String(newRow[col.name] || '')}
              onChange={(_, value) => setNewRow({ ...newRow, [col.name]: value })}
            />
          ))}
          <Stack horizontal tokens={{ childrenGap: 8 }}>
            <PrimaryButton text="Hinzufügen" onClick={handleSaveNew} />
            <DefaultButton text="Abbrechen" onClick={handleCancelNew} />
          </Stack>
        </Stack>
      )}

      {/* Datentabelle */}
      <DetailsList
        items={rows}
        columns={columns}
        selectionMode={SelectionMode.none}
        layoutMode={DetailsListLayoutMode.justified}
        isHeaderVisible={true}
      />

      {/* Löschen-Bestätigungsdialog */}
      <Dialog
        hidden={!showDeleteDialog}
        onDismiss={() => setShowDeleteDialog(false)}
        dialogContentProps={{
          type: DialogType.normal,
          title: 'Zeile löschen',
          subText: 'Möchten Sie diese Zeile wirklich löschen?',
        }}
      >
        <DialogFooter>
          <PrimaryButton onClick={handleDeleteConfirm} text="Löschen" />
          <DefaultButton onClick={() => setShowDeleteDialog(false)} text="Abbrechen" />
        </DialogFooter>
      </Dialog>
    </div>
  );
};
