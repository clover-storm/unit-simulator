import { useCallback, useMemo, useRef } from 'react';
import { AgGridReact } from 'ag-grid-react';
import {
  AllCommunityModule,
  ModuleRegistry,
  type ColDef,
  type CellValueChangedEvent,
  type GridReadyEvent,
  type GridApi,
  type RowDoubleClickedEvent,
  type RowClassParams,
} from 'ag-grid-community';

ModuleRegistry.registerModules([AllCommunityModule]);

type Props = {
  data: unknown[];
  onDataChange: (newData: unknown[]) => void;
  onRowSelect?: (index: number) => void;
  selectedIndex?: number | null;
};

export default function SpreadsheetView({ data, onDataChange, onRowSelect, selectedIndex }: Props) {
  const gridRef = useRef<AgGridReact>(null);
  const gridApiRef = useRef<GridApi | null>(null);

  const isObjectArray = useMemo(() => {
    return data.every(item => item && typeof item === 'object' && !Array.isArray(item));
  }, [data]);

  const columns = useMemo(() => {
    if (!isObjectArray) return ['__value'];
    const keys = new Set<string>();
    for (let i = 0; i < Math.min(data.length, 50); i++) {
      const item = data[i];
      if (item && typeof item === 'object' && !Array.isArray(item)) {
        Object.keys(item as Record<string, unknown>).forEach(key => keys.add(key));
      }
    }
    return Array.from(keys);
  }, [data, isObjectArray]);

  const rowData = useMemo(() => {
    return data.map((record, index) => {
      if (isObjectArray) {
        return { __rowIndex: index, ...(record as Record<string, unknown>) };
      }
      return { __rowIndex: index, __value: record };
    });
  }, [data, isObjectArray]);

  const columnDefs = useMemo<ColDef[]>(() => {
    const indexCol: ColDef = {
      headerName: '#',
      field: '__rowIndex',
      width: 60,
      pinned: 'left',
      editable: false,
      sortable: false,
      filter: false,
      cellClass: 'row-index-cell',
    };

    const dataCols: ColDef[] = columns.map(col => ({
      headerName: col === '__value' ? 'Value' : col,
      field: col,
      editable: true,
      sortable: true,
      filter: true,
      resizable: true,
      minWidth: 100,
      cellEditor: 'agTextCellEditor',
      valueFormatter: (params) => {
        const value = params.value;
        if (value === null || value === undefined) return '';
        if (typeof value === 'object') {
          try {
            return JSON.stringify(value);
          } catch {
            return String(value);
          }
        }
        return String(value);
      },
      valueSetter: (params) => {
        const newValue = params.newValue;
        let parsedValue: unknown = newValue;

        // Try to parse as JSON for objects/arrays/numbers/booleans
        if (typeof newValue === 'string') {
          const trimmed = newValue.trim();
          if (trimmed === '') {
            parsedValue = null;
          } else if (trimmed === 'true') {
            parsedValue = true;
          } else if (trimmed === 'false') {
            parsedValue = false;
          } else if (trimmed === 'null') {
            parsedValue = null;
          } else if (/^-?\d+(\.\d+)?$/.test(trimmed)) {
            parsedValue = Number(trimmed);
          } else if (trimmed.startsWith('{') || trimmed.startsWith('[')) {
            try {
              parsedValue = JSON.parse(trimmed);
            } catch {
              parsedValue = newValue;
            }
          } else {
            parsedValue = newValue;
          }
        }

        params.data[params.colDef.field!] = parsedValue;
        return true;
      },
    }));

    return [indexCol, ...dataCols];
  }, [columns]);

  const defaultColDef = useMemo<ColDef>(() => ({
    flex: 1,
    minWidth: 100,
    resizable: true,
  }), []);

  const onGridReady = useCallback((params: GridReadyEvent) => {
    gridApiRef.current = params.api;
    params.api.sizeColumnsToFit();
  }, []);

  const onCellValueChanged = useCallback((event: CellValueChangedEvent) => {
    const rowIndex = event.data.__rowIndex;
    const newData = [...data];

    if (isObjectArray) {
      const { __rowIndex, ...rest } = event.data;
      newData[rowIndex] = rest;
    } else {
      newData[rowIndex] = event.data.__value;
    }

    onDataChange(newData);
  }, [data, isObjectArray, onDataChange]);

  const onRowDoubleClicked = useCallback((event: RowDoubleClickedEvent) => {
    const rowIndex = event.data.__rowIndex;
    onRowSelect?.(rowIndex);
  }, [onRowSelect]);

  const getRowClass = useCallback((params: RowClassParams) => {
    if (params.data?.__rowIndex === selectedIndex) {
      return 'ag-row-selected-custom';
    }
    return '';
  }, [selectedIndex]);

  return (
    <div className="spreadsheet-view ag-theme-alpine-dark">
      <AgGridReact
        ref={gridRef}
        rowData={rowData}
        columnDefs={columnDefs}
        defaultColDef={defaultColDef}
        onGridReady={onGridReady}
        onCellValueChanged={onCellValueChanged}
        onRowDoubleClicked={onRowDoubleClicked}
        getRowClass={getRowClass}
        rowSelection="single"
        animateRows={false}
        enableCellTextSelection={true}
        ensureDomOrder={true}
        suppressRowClickSelection={true}
        stopEditingWhenCellsLoseFocus={true}
        undoRedoCellEditing={true}
        undoRedoCellEditingLimit={20}
      />
    </div>
  );
}
