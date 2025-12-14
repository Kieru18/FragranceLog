export interface CreateReviewRequestDto {
  perfumeId: number;
  rating: number;
  text?: string | null;
}
