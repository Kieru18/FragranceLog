export interface SharedListPerfumePreviewDto {
  perfumeId: number;
  name: string;
  brand: string;
  avgRating: number | null;
  ratingCount: number;
  imageUrl?: string | null;
}
