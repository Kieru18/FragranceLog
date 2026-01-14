import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NativeScriptCommonModule, RouterExtensions } from '@nativescript/angular';
import { RecognitionFlowService } from '../services/recognitionflow.service'

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
    constructor(
      private routerExtensions: RouterExtensions,
      private readonly recognitionFlow: RecognitionFlowService) {}

  goToSearch() {
    this.routerExtensions.navigate(['/search']);
  }

  goToHome() {
    this.routerExtensions.navigate(['/home']);
  }

  goToListsOverview() {
    this.routerExtensions.navigate(['/lists-overview']);
  }

  goToProfile() {
    this.routerExtensions.navigate(['/profile']);
  }

  beginRecognitionFlow() {
    this.recognitionFlow.start();
  }
}
