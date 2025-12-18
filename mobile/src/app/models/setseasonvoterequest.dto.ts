import { SeasonEnum } from '../enums/season.enum';

export class SetSeasonVoteRequestDto {
  season: SeasonEnum | null;

  constructor(season: SeasonEnum | null) {
    this.season = season;
  }
}
