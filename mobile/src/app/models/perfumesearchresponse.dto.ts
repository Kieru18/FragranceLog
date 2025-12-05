import { PerfumeSearchItemDto } from "./perfumesearchitem.dto";

export interface PerfumeSearchResponseDto {
  totalCount: number;
  page: number;
  pageSize: number;
  items: PerfumeSearchItemDto[];
}
