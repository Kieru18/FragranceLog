export interface PerfumeListItemDto {
  perfumeId: number;
  name: string;
  brand: string;
  imageUrl?: string | null;

  avgRating: number;
  ratingCount: number;

  myRating?: number | null;
}
