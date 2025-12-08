export interface ReviewDto {
  reviewId: number;
  author: string;
  rating: number;
  text?: string | null;
  createdAt: string;
}
