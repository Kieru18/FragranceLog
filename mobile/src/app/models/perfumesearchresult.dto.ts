export interface PerfumeSearchResultDto {
  perfumeId: number;
  name: string;
  brandName: string;
  averageRating: number | null;
  reviewsCount: number;
}
