import { Component, NO_ERRORS_SCHEMA, ViewContainerRef, AfterViewInit } from '@angular/core';
import { PageRouterOutlet, ModalDialogService } from '@nativescript/angular';
import { SharedListPreviewComponent } from './lists/shared/sharedlistpreview.component';

@Component({
  standalone: true,
  selector: 'ns-app',
  templateUrl: './app.component.html',
  imports: [PageRouterOutlet],
  schemas: [NO_ERRORS_SCHEMA],
})
export class AppComponent implements AfterViewInit {
  private ready = false;

  constructor(
    private vcRef: ViewContainerRef,
    private modalService: ModalDialogService
  ) {}

  ngAfterViewInit() {
    this.ready = true;

    (global as any).openSharedModal = (token: string) => {
      if (!this.ready) return;

      this.modalService.showModal(SharedListPreviewComponent, {
        viewContainerRef: this.vcRef,
        fullscreen: true,
        animated: false,
        context: { token },
      });
    };

    const token = (global as any).pendingSharedToken;
    if (token) {
      (global as any).pendingSharedToken = null;
      setTimeout(() => (global as any).openSharedModal(token), 0);
    }

    (global as any).openRecognitionModal = (component: any, context?: any) => {
      if (!this.ready) return;

      this.modalService.showModal(component, {
        viewContainerRef: this.vcRef,
        fullscreen: true,
        animated: false,
        context
      });
    };
  }
}
