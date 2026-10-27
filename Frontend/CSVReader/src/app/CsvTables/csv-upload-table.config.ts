// Centralise les constantes configurables du composant.
// Modifier ici pour changer le comportement sans toucher au composant.

export const CSV_TABLE_CONFIG = {
  pagination: {
    defaultRows: 5,
    rowsPerPageOptions: [5, 10, 20],
  },
  colors: {
    csv1:   '#3b82f6',
    csv2:   '#8b5cf6',
    diff:   '#f59e0b',
    only1Bg: '#dbeafe',
    only2Bg: '#ede9fe',
    diffBg:  '#fef3c7',
  },
  upload: {
    accept: '.csv',
    maxFileSize: 10_000_000, // 10 Mo
  },
} as const;

export type CsvTableConfig = typeof CSV_TABLE_CONFIG;