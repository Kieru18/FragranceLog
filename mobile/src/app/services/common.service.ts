import { Injectable } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class CommonService {

  getErrorMessage(
    err: any,
    fallbackMessage: string = 'Something went wrong'
  ): string {

    if (err?.error) {
      if (typeof err.error === 'object' && typeof err.error.error === 'string') {
        return err.error.error;
      }

      if (typeof err.error === 'object' && typeof err.error.message === 'string') {
        return err.error.message;
      }

      if (typeof err.error === 'string') {
        return err.error;
      }
    }

    if (typeof err === 'string') {
      return err;
    }

    if (err instanceof Error && err.message) {
      return err.message;
    }

    return fallbackMessage;
  }
}
