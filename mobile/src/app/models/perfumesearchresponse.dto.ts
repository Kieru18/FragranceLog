import { PerfumeSearchResultDto } from "./perfumesearchresult.dto";

export interface PerfumeSearchResponseDto {
  totalCount: number;
  page: number;
  pageSize: number;
  items: PerfumeSearchResultDto[];
}
