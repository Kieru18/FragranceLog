import { NoteGroupDto } from "./notegroup.dto";

export interface PerfumeSuggestionRequestDto {
  brand: string;
  name: string;
  groups: string[];
  noteGroups: NoteGroupDto[];
  comment?: string;
  imageUrl?: string;
  imageBase64?: string;
}
