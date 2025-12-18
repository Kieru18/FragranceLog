import { LongevityEnum } from '../enums/longevity.enum';

export class SetLongevityVoteRequestDto {
  longevity: LongevityEnum | null;

  constructor(longevity: LongevityEnum | null) {
    this.longevity = longevity;
  }
}
