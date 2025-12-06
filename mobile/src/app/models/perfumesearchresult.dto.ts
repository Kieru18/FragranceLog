export interface PerfumeSearchResultDto {
  perfumeId: number;
  name: string;
  brand: string;
  rating: number | null;
  ratingCount: number;
  countryCode: string;
}
