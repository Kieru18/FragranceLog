import { NoteTypeEnum } from './notetype.enum';

export interface PerfumeNoteDto {
  noteId: number;
  name: string;
  type: NoteTypeEnum;
}
