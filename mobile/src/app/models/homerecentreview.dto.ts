export interface HomeRecentReviewDto {
  reviewId: number;
  perfumeId: number;

  perfumeName: string;
  brand: string;
  perfumeImageUrl: string | null;

  author: string;
  rating: number;
  comment: string;
  createdAt: string;
}
