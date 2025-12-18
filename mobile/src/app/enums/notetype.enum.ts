export enum NoteTypeEnum {
  Top = 1,
  Middle = 2,
  Base = 3
}

export const NoteTypeLabels: Record<NoteTypeEnum, string> = {
  [NoteTypeEnum.Top]: 'Top notes',
  [NoteTypeEnum.Middle]: 'Heart notes',
  [NoteTypeEnum.Base]: 'Base notes'
};
