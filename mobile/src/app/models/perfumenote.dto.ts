import { NoteTypeEnum } from '../enums/notetype.enum';

export interface PerfumeNoteDto {
  noteId: number;
  name: string;
  type: NoteTypeEnum;
}
