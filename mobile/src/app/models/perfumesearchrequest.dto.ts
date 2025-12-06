export interface PerfumeSearchRequestDto {
  query?: string | null;
  brandId?: number | null;
  countryCode?: string | null;
  minRating?: number | null;
  groupIds?: number[];
  page: number;
  pageSize: number;
}
