import { Component, NO_ERRORS_SCHEMA, OnInit, OnDestroy } from '@angular/core';
import { ModalDialogParams } from '@nativescript/angular';

@Component({
  standalone: true,
  templateUrl: './perfumerecognitionloading.component.html',
  schemas: [NO_ERRORS_SCHEMA]
})
export class PerfumeRecognitionLoadingComponent
  implements OnInit, OnDestroy {

  constructor(private params: ModalDialogParams) {}

  ngOnInit(): void {
    (global as any).closeRecognitionLoading = () => {
      this.params.closeCallback();
    };
  }

  ngOnDestroy(): void {
    if ((global as any).closeRecognitionLoading) {
      (global as any).closeRecognitionLoading = null;
    }
  }
}
