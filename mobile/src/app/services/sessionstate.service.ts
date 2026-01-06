import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SessionStateService {
  homeGreetingVisible = true;

  consumeHomeGreeting(): boolean {
    if (!this.homeGreetingVisible) return false;
    this.homeGreetingVisible = false;
    return true;
  }

  reset(): void {
    this.homeGreetingVisible = true;
  }
}
