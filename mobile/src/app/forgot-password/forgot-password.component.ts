import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { Page } from '@nativescript/core';
import { openUrl } from '@nativescript/core/utils';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  templateUrl: './forgot-password.component.html',
  schemas: [NO_ERRORS_SCHEMA]
})
export class ForgotPasswordComponent {

  constructor(
        private router: Router,
        private page: Page
  ) {
    this.page.actionBarHidden = true;
  }

  goBack() {
    this.router.navigate(['/login']);
  }

  onEmailTap() {
    openUrl('mailto:jakub.kieruczenko18@gmail.com');
  }
}
