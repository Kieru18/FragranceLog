import { SharedListPerfumePreviewDto } from './sharedlistperfumepreview.dto';

export interface SharedListPreviewDto {
  shareToken: string;
  listName: string;
  ownerName: string;
  perfumeCount: number;
  perfumes: SharedListPerfumePreviewDto[];
}
