import { PerfumeSearchResultDto } from './perfumesearchresult.dto'

export type PerfumeSortBy = 'relevance' | 'rating';
export type PerfumeSortDir = 'asc' | 'desc';

export type PerfumeSearchRow =
  | { type: 'item'; data: PerfumeSearchResultDto }
  | { type: 'spinner' };
