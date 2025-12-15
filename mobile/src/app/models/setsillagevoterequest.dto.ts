import { SillageEnum } from '../enums/sillage.enum';

export class SetSillageVoteRequestDto {
  sillage: SillageEnum | null;

  constructor(sillage: SillageEnum | null) {
    this.sillage = sillage;
  }
}
