import { GenderEnum } from '../enums/gender.enum';
import { SeasonEnum } from '../enums/season.enum';
import { DaytimeEnum } from '../enums/daytime.enum';
import { LongevityEnum } from '../enums/longevity.enum';
import { SillageEnum } from '../enums/sillage.enum';
import { ReviewDto } from './review.dto';
import { PerfumeNoteGroupDto } from './perfumenotegroup.dto';

export interface PerfumeDetailsDto {
  perfumeId: number;
  name: string;
  brand: string;
  imageUrl?: string | null;

  avgRating: number;
  ratingCount: number;
  commentCount: number;

  gender?: GenderEnum | null;
  season?: SeasonEnum | null;
  daytime?: DaytimeEnum | null;
  longevity?: number | null;
  sillage?: number | null;

  myRating?: number | null;
  myReview?: string | null;

  myGenderVote?: GenderEnum | null;
  mySeasonVote?: SeasonEnum | null;
  myDaytimeVote?: DaytimeEnum | null;
  myLongevityVote?: LongevityEnum | null;
  mySillageVote?: SillageEnum | null;

  groups: string[];
  noteGroups: PerfumeNoteGroupDto[];
  reviews: ReviewDto[];
}
