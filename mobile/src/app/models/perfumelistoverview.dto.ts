export interface PerfumeListOverviewDto {
  perfumeListId: number;
  name: string;
  isSystem: boolean;

  perfumeCount: number;
  previewImages: string[];
}
