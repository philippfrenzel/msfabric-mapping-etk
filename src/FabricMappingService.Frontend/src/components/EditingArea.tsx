/**
 * Bearbeitungsbereich-Komponente
 * 
 * Container für Basis- und Experten-Modus mit Tab-Navigation.
 */

import React from 'react';
import {
  Pivot,
  PivotItem,
  mergeStyles,
} from '@fluentui/react';
import { BasicModeEditor } from './BasicModeEditor';
import { ExpertModeEditor } from './ExpertModeEditor';
import { ReferenceTableData, ReferenceTableRow, EditMode } from '../types';

const containerStyle = mergeStyles({
  backgroundColor: '#ffffff',
  borderRadius: '4px',
  boxShadow: '0 1.6px 3.6px 0 rgba(0,0,0,0.132), 0 0.3px 0.9px 0 rgba(0,0,0,0.108)',
  overflow: 'hidden',
});

/**
 * Props für die EditingArea-Komponente
 */
interface EditingAreaProps {
  tableData: ReferenceTableData | null;
  editMode: EditMode;
  onEditModeChange: (mode: EditMode) => void;
  onSave: (rows: ReferenceTableRow[]) => Promise<void>;
  onCancel: () => void;
}

/**
 * Bearbeitungsbereich mit Tab-Navigation für Basis- und Experten-Modus
 */
export const EditingArea: React.FC<EditingAreaProps> = ({
  tableData,
  editMode,
  onEditModeChange,
  onSave,
  onCancel,
}) => {
  // Handler für Tab-Wechsel
  const handlePivotChange = (item?: PivotItem) => {
    if (item) {
      onEditModeChange(item.props.itemKey as EditMode);
    }
  };

  return (
    <div className={containerStyle}>
      <Pivot
        selectedKey={editMode}
        onLinkClick={handlePivotChange}
        styles={{
          root: {
            padding: '0 20px',
          },
        }}
      >
        {/* Basis-Modus Tab */}
        <PivotItem
          headerText="Basis-Modus"
          itemKey="basic"
          itemIcon="Table"
        >
          <BasicModeEditor
            tableData={tableData}
            onSave={onSave}
            onCancel={onCancel}
          />
        </PivotItem>

        {/* Experten-Modus Tab */}
        <PivotItem
          headerText="Experten-Modus"
          itemKey="expert"
          itemIcon="Code"
        >
          <ExpertModeEditor
            tableData={tableData}
            onSave={onSave}
            onCancel={onCancel}
          />
        </PivotItem>
      </Pivot>
    </div>
  );
};
