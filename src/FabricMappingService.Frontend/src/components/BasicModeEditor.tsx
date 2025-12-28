/**
 * Basis-Modus Editor: Inline-editierbare Datentabelle
 * 
 * CRUD-Funktionalität für Referenztabellen-Zeilen mit
 * Inline-Bearbeitung, Hinzufügen und Löschen von Einträgen.
 * 
 * Fluent UI v9 für natives Microsoft Fabric Aussehen.
 */

import React, { useState, useEffect } from 'react';
import {
  makeStyles,
  tokens,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Button,
  Input,
  Toolbar,
  ToolbarButton,
  MessageBar,
  MessageBarBody,
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
  Card,
  CardHeader,
  Text,
  Field,
} from '@fluentui/react-components';
import {
  AddRegular,
  SaveRegular,
  DismissRegular,
  EditRegular,
  DeleteRegular,
} from '@fluentui/react-icons';
import { ReferenceTableData, ReferenceTableRow } from '../types';

const useStyles = makeStyles({
  container: {
    padding: tokens.spacingHorizontalL,
    height: 'calc(100vh - 320px)',
    overflow: 'auto',
  },
  toolbar: {
    marginBottom: tokens.spacingVerticalM,
    padding: tokens.spacingHorizontalS,
    backgroundColor: tokens.colorNeutralBackground3,
    borderRadius: tokens.borderRadiusMedium,
  },
  table: {
    width: '100%',
  },
  actionCell: {
    display: 'flex',
    gap: tokens.spacingHorizontalXS,
  },
  newRowCard: {
    marginBottom: tokens.spacingVerticalM,
    padding: tokens.spacingHorizontalM,
    backgroundColor: tokens.colorNeutralBackground3,
  },
  newRowForm: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
    gap: tokens.spacingHorizontalM,
    marginBottom: tokens.spacingVerticalM,
  },
  newRowActions: {
    display: 'flex',
    gap: tokens.spacingHorizontalS,
    justifyContent: 'flex-end',
  },
  emptyState: {
    textAlign: 'center',
    padding: tokens.spacingVerticalXXL,
    color: tokens.colorNeutralForeground3,
  },
});

interface BasicModeEditorProps {
  tableData: ReferenceTableData | null;
  onSave: (rows: ReferenceTableRow[]) => Promise<void>;
  onCancel: () => void;
}

export const BasicModeEditor: React.FC<BasicModeEditorProps> = ({
  tableData,
  onSave,
  onCancel,
}) => {
  const styles = useStyles();
  const [rows, setRows] = useState<ReferenceTableRow[]>([]);
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [isAddingNew, setIsAddingNew] = useState(false);
  const [newRow, setNewRow] = useState<Partial<ReferenceTableRow>>({});
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [rowToDelete, setRowToDelete] = useState<string | null>(null);

  useEffect(() => {
    if (tableData) {
      setRows([...tableData.rows]);
    }
  }, [tableData]);

  const handleCellEdit = (key: string, columnName: string, value: string) => {
    setRows((prevRows) =>
      prevRows.map((row) => (row.key === key ? { ...row, [columnName]: value } : row))
    );
  };

  const handleSaveRow = (key: string) => {
    setEditingKey(null);
    setMessage({ type: 'success', text: 'Zeile gespeichert' });
    setTimeout(() => setMessage(null), 3000);
  };

  const handleDeleteClick = (key: string) => {
    setRowToDelete(key);
    setShowDeleteDialog(true);
  };

  const handleDeleteConfirm = () => {
    if (rowToDelete) {
      setRows((prevRows) => prevRows.filter((row) => row.key !== rowToDelete));
      setMessage({ type: 'success', text: 'Zeile gelöscht' });
      setTimeout(() => setMessage(null), 3000);
    }
    setShowDeleteDialog(false);
    setRowToDelete(null);
  };

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

  const handleSaveNew = () => {
    if (!newRow.key) {
      setMessage({ type: 'error', text: 'Schlüssel ist erforderlich' });
      return;
    }
    setRows([...rows, newRow as ReferenceTableRow]);
    setIsAddingNew(false);
    setNewRow({});
    setMessage({ type: 'success', text: 'Neue Zeile hinzugefügt' });
    setTimeout(() => setMessage(null), 3000);
  };

  const handleCancelNew = () => {
    setIsAddingNew(false);
    setNewRow({});
  };

  const handleSaveAll = async () => {
    setIsSaving(true);
    try {
      await onSave(rows);
      setMessage({ type: 'success', text: 'Alle Änderungen gespeichert' });
    } catch (error) {
      setMessage({ type: 'error', text: `Fehler beim Speichern: ${error}` });
    } finally {
      setIsSaving(false);
    }
  };

  if (!tableData) {
    return (
      <div className={styles.container}>
        <div className={styles.emptyState}>
          <Text>Keine Tabelle ausgewählt</Text>
        </div>
      </div>
    );
  }

  const columns = tableData.metadata.columns;

  return (
    <div className={styles.container}>
      {message && (
        <MessageBar intent={message.type === 'success' ? 'success' : 'error'} style={{ marginBottom: '16px' }}>
          <MessageBarBody>{message.text}</MessageBarBody>
        </MessageBar>
      )}

      <Toolbar className={styles.toolbar}>
        <ToolbarButton
          icon={<AddRegular />}
          onClick={handleAddNew}
          disabled={isAddingNew}
        >
          Neue Zeile
        </ToolbarButton>
        <ToolbarButton
          icon={<SaveRegular />}
          onClick={handleSaveAll}
          disabled={isSaving}
          appearance="primary"
        >
          Speichern
        </ToolbarButton>
        <ToolbarButton
          icon={<DismissRegular />}
          onClick={onCancel}
        >
          Abbrechen
        </ToolbarButton>
      </Toolbar>

      {/* Formular für neue Zeile */}
      {isAddingNew && (
        <Card className={styles.newRowCard}>
          <CardHeader header={<Text weight="semibold">Neue Zeile hinzufügen</Text>} />
          <div className={styles.newRowForm}>
            <Field label="Schlüssel" required>
              <Input
                value={newRow.key || ''}
                onChange={(_, data) => setNewRow({ ...newRow, key: data.value })}
              />
            </Field>
            {columns.map((col) => (
              <Field key={col.name} label={col.name}>
                <Input
                  value={String(newRow[col.name] || '')}
                  onChange={(_, data) => setNewRow({ ...newRow, [col.name]: data.value })}
                />
              </Field>
            ))}
          </div>
          <div className={styles.newRowActions}>
            <Button appearance="primary" onClick={handleSaveNew}>Hinzufügen</Button>
            <Button onClick={handleCancelNew}>Abbrechen</Button>
          </div>
        </Card>
      )}

      {/* Datentabelle */}
      <Table className={styles.table}>
        <TableHeader>
          <TableRow>
            <TableHeaderCell>Schlüssel</TableHeaderCell>
            {columns.map((col) => (
              <TableHeaderCell key={col.name}>{col.name}</TableHeaderCell>
            ))}
            <TableHeaderCell>Aktionen</TableHeaderCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.key}>
              <TableCell>{row.key}</TableCell>
              {columns.map((col) => (
                <TableCell key={col.name}>
                  {editingKey === row.key ? (
                    <Input
                      size="small"
                      value={String(row[col.name] || '')}
                      onChange={(_, data) => handleCellEdit(row.key, col.name, data.value)}
                    />
                  ) : (
                    String(row[col.name] || '')
                  )}
                </TableCell>
              ))}
              <TableCell>
                <div className={styles.actionCell}>
                  {editingKey === row.key ? (
                    <>
                      <Button size="small" appearance="primary" onClick={() => handleSaveRow(row.key)}>
                        Speichern
                      </Button>
                      <Button size="small" onClick={() => setEditingKey(null)}>
                        Abbrechen
                      </Button>
                    </>
                  ) : (
                    <>
                      <Button size="small" icon={<EditRegular />} onClick={() => setEditingKey(row.key)}>
                        Bearbeiten
                      </Button>
                      <Button size="small" icon={<DeleteRegular />} onClick={() => handleDeleteClick(row.key)}>
                        Löschen
                      </Button>
                    </>
                  )}
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      {/* Löschen-Bestätigungsdialog */}
      <Dialog open={showDeleteDialog} onOpenChange={(_, data) => setShowDeleteDialog(data.open)}>
        <DialogSurface>
          <DialogBody>
            <DialogTitle>Zeile löschen</DialogTitle>
            <DialogContent>
              Möchten Sie diese Zeile wirklich löschen?
            </DialogContent>
            <DialogActions>
              <Button appearance="primary" onClick={handleDeleteConfirm}>Löschen</Button>
              <Button onClick={() => setShowDeleteDialog(false)}>Abbrechen</Button>
            </DialogActions>
          </DialogBody>
        </DialogSurface>
      </Dialog>
    </div>
  );
};
