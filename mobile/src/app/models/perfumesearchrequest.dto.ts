import { PerfumeSortBy, PerfumeSortDir } from "./types";

export interface PerfumeSearchRequestDto {
  query?: string | null;
  page: number;
  pageSize: number;
  brandId?: number | null;
  groupIds?: number[];
  sortBy: PerfumeSortBy;
  sortDir: PerfumeSortDir;
}
