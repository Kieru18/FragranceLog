import { GenderEnum } from './gender.enum'

export interface PerfumeSearchRequestDto {
  query?: string | null;
  brandId?: number | null;
  countryCode?: string | null;
  minRating?: number | null;
  gender?: GenderEnum | null;
  groupIds?: number[];
  page: number;
  pageSize: number;
}
