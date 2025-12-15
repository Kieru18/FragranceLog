import { DaytimeEnum } from '../enums/daytime.enum';

export class SetDaytimeVoteRequestDto {
  daytime: DaytimeEnum | null;

  constructor(daytime: DaytimeEnum | null) {
    this.daytime = daytime;
  }
}
