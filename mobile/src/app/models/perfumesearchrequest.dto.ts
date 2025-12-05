export interface PerfumeSearchRequestDto {
  query?: string | null;
  brandId?: number | null;
  countryCode?: string | null;
  minRating?: number | null;
  page: number;
  pageSize: number;
}
