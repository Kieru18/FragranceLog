import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NativeScriptCommonModule, RouterExtensions } from '@nativescript/angular';

@Component({
  standalone: true,
  selector: 'app-footer',
  imports: [
    NativeScriptCommonModule,
    RouterModule
  ],
  schemas: [NO_ERRORS_SCHEMA],
  templateUrl: './footer.component.html'
})
export class FooterComponent {
    constructor(private routerExtensions: RouterExtensions) {}

  goToSearch() {
    this.routerExtensions.navigate(['/search']);
  }

  goToHome() {
    this.routerExtensions.navigate(['/home']);
  }

  goToListsOverview() {
    this.routerExtensions.navigate(['/lists-overview']);
  }
}
