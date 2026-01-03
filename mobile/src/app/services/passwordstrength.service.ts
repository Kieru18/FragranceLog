import { Injectable } from '@angular/core';

export interface PasswordStrengthState {
  length: boolean;
  lowercase: boolean;
  uppercase: boolean;
  digit: boolean;
  special: boolean;
  score: number;
  percent: number;
}

@Injectable({ providedIn: 'root' })
export class PasswordStrengthService {

  evaluate(password: string): PasswordStrengthState {
    const length = password.length >= 8;
    const lowercase = /[a-z]/.test(password);
    const uppercase = /[A-Z]/.test(password);
    const digit = /\d/.test(password);
    const special = /[^\da-zA-Z]/.test(password);

    const score =
      (length ? 1 : 0) +
      (lowercase ? 1 : 0) +
      (uppercase ? 1 : 0) +
      (digit ? 1 : 0) +
      (special ? 1 : 0);

    return {
      length,
      lowercase,
      uppercase,
      digit,
      special,
      score,
      percent: (score / 5) * 100
    };
  }
}
