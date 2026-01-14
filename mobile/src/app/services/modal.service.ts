import { Injectable, ViewContainerRef, Injector, inject } from '@angular/core';
import { ModalDialogService } from '@nativescript/angular';
import { SharedListPreviewComponent } from '../lists/shared/sharedlistpreview.component';
import { PerfumeRecognitionLoadingComponent } from '../recognition/loading/perfumerecognitionloading.component';
import { PerfumeRecognitionModalComponent } from '../recognition/modal/perfumerecognitionmodal.component';
import { PerfumeRecognitionResultDto } from '../models/perfumerecognitionresult.dto';


@Injectable({ providedIn: 'root' })
export class ModalService {
  private _viewContainerRef: ViewContainerRef | null = null;
  private injector = inject(Injector);

  setViewContainerRef(vcr: ViewContainerRef) {
    this._viewContainerRef = vcr;
  }

  async openSharedListModal(token: string): Promise<boolean> {
    if (!this._viewContainerRef) {
      console.error('ViewContainerRef not set!');
      return false;
    }

    try {
      const modalService = this.injector.get(ModalDialogService);
      
      await modalService.showModal(SharedListPreviewComponent, {
        viewContainerRef: this._viewContainerRef,
        fullscreen: true,
        animated: false,
        context: { token }
      });
      return true;
    } catch (error) {
      console.error('Failed to open modal:', error);
      return false;
    }
  }

  async openRecognitionLoading(): Promise<void> {
    if (!this._viewContainerRef) {
      console.error('ViewContainerRef not set!');
      return;
    }

    const modalService = this.injector.get(ModalDialogService);

    await modalService.showModal(PerfumeRecognitionLoadingComponent, {
      viewContainerRef: this._viewContainerRef,
      fullscreen: true,
      animated: false
    });
  }

  async openRecognitionResults(
    results: PerfumeRecognitionResultDto[]
  ): Promise<void> {
    if (!this._viewContainerRef) {
      console.error('ViewContainerRef not set!');
      return;
    }

    const modalService = this.injector.get(ModalDialogService);

    await modalService.showModal(PerfumeRecognitionModalComponent, {
      viewContainerRef: this._viewContainerRef,
      fullscreen: true,
      animated: false,
      context: { results }
    });
  }


}
