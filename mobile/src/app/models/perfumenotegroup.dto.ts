import { NoteTypeEnum } from './notetype.enum';
import { PerfumeNoteDto } from './perfumenote.dto';

export interface PerfumeNoteGroupDto {
  type: NoteTypeEnum;
  notes: PerfumeNoteDto[];
}
