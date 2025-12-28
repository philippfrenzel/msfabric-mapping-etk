/**
 * Bearbeitungsbereich-Komponente
 * 
 * Container für Basis- und Experten-Modus mit Tab-Navigation.
 * 
 * Fluent UI v9 für natives Microsoft Fabric Aussehen.
 */

import React from 'react';
import {
  makeStyles,
  tokens,
  TabList,
  Tab,
  Card,
  SelectTabData,
  SelectTabEvent,
} from '@fluentui/react-components';
import { TableRegular, CodeRegular } from '@fluentui/react-icons';
import { BasicModeEditor } from './BasicModeEditor';
import { ExpertModeEditor } from './ExpertModeEditor';
import { ReferenceTableData, ReferenceTableRow, EditMode } from '../types';

const useStyles = makeStyles({
  container: {
    backgroundColor: tokens.colorNeutralBackground1,
    borderRadius: tokens.borderRadiusMedium,
    boxShadow: tokens.shadow4,
    overflow: 'hidden',
  },
  tabList: {
    padding: `${tokens.spacingVerticalS} ${tokens.spacingHorizontalL}`,
    borderBottom: `1px solid ${tokens.colorNeutralStroke1}`,
  },
  content: {
    minHeight: '400px',
  },
});

interface EditingAreaProps {
  tableData: ReferenceTableData | null;
  editMode: EditMode;
  onEditModeChange: (mode: EditMode) => void;
  onSave: (rows: ReferenceTableRow[]) => Promise<void>;
  onCancel: () => void;
}

export const EditingArea: React.FC<EditingAreaProps> = ({
  tableData,
  editMode,
  onEditModeChange,
  onSave,
  onCancel,
}) => {
  const styles = useStyles();

  const handleTabSelect = (_: SelectTabEvent, data: SelectTabData) => {
    onEditModeChange(data.value as EditMode);
  };

  return (
    <Card className={styles.container}>
      <TabList
        selectedValue={editMode}
        onTabSelect={handleTabSelect}
        className={styles.tabList}
      >
        <Tab value="basic" icon={<TableRegular />}>
          Basis-Modus
        </Tab>
        <Tab value="expert" icon={<CodeRegular />}>
          Experten-Modus
        </Tab>
      </TabList>

      <div className={styles.content}>
        {editMode === 'basic' ? (
          <BasicModeEditor
            tableData={tableData}
            onSave={onSave}
            onCancel={onCancel}
          />
        ) : (
          <ExpertModeEditor
            tableData={tableData}
            onSave={onSave}
            onCancel={onCancel}
          />
        )}
      </div>
    </Card>
  );
};
