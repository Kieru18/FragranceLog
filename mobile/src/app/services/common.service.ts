import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class CommonService {

  getErrorMessage(err: any, fallbackMessage: string = 'Something went wrong'): string {

    if (err?.error) {

      if (typeof err.error === 'string') {
        return err.error;
      }

      if (typeof err.error.error === 'string') {
        return err.error.error;
      }

      if (typeof err.error.message === 'string') {
        return err.error.message;
      }

      if (err.error.errors && typeof err.error.errors === 'object') {
        const messages = Object.values(err.error.errors)
          .flat()
          .filter(x => typeof x === 'string');

        if (messages.length > 0) {
          return messages.join('\n');
        }
      }

      if (typeof err.error.title === 'string') {
        return err.error.title;
      }
    }

    if (err instanceof Error && err.message) {
      return err.message;
    }

    if (typeof err === 'string') {
      return err;
    }

    return fallbackMessage;
  }
}
