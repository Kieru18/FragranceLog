import { GenderEnum } from '../enums/gender.enum';

export class SetGenderVoteRequestDto {
  gender: GenderEnum | null;

  constructor(gender: GenderEnum | null) {
    this.gender = gender;
  }
}
