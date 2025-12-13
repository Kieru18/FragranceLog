import { PerfumeNoteGroupDto } from './perfumenotegroup.dto';
import { ReviewDto } from './review.dto';

export interface PerfumeDetailsDto {
  perfumeId: number;
  name: string;
  brand: string;
  imageUrl?: string | null;

  avgRating: number;
  ratingCount: number;
  commentCount: number;

  gender?: string | null;
  longevity?: string | null;
  sillage?: string | null;
  seasons: string[];
  daytimes: string[];

  myRating?: number | null;
  myReview?: string | null;
  myGenderVote?: string | null;
  myLongevityVote?: string | null;
  mySillageVote?: string | null;

  groups: string[];
  noteGroups: PerfumeNoteGroupDto[];
  reviews: ReviewDto[];
}
