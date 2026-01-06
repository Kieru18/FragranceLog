import { PerfumeHighlightType } from '../enums/perfumehighlighttype.enum';

export interface PerfumeOfTheDayDto {
  perfumeId: number;
  name: string;
  brand: string;
  imageUrl: string;

  avgRating: number;
  ratingCount: number;
  score: number;

  type: PerfumeHighlightType;
  reason: string;
}
