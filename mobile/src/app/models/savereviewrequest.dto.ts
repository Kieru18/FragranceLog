export interface SaveReviewRequestDto {
  perfumeId: number;
  rating: number;
  text?: string | null;
}
