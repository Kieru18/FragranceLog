import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { ModalDialogParams, RouterExtensions } from '@nativescript/angular';
import { DecimalPipe, NgForOf, NgIf } from '@angular/common';
import { SnackBar } from '@nativescript-community/ui-material-snackbar';
import { PerfumeRecognitionResultDto } from '../../models/perfumerecognitionresult.dto';
import { PerfumeRecognitionConfidence } from '../../enums/perfumerecognitionconfidence.enum';
import { environment } from '~/environments/environment';

@Component({
  standalone: true,
  templateUrl: './perfumerecognitionmodal.component.html',
  imports: [NgForOf, NgIf, DecimalPipe],
  schemas: [NO_ERRORS_SCHEMA]
})
export class PerfumeRecognitionModalComponent {

  readonly contentUrl = environment.contentUrl;
  readonly results: PerfumeRecognitionResultDto[];
  readonly PerfumeRecognitionConfidence = PerfumeRecognitionConfidence;

  private readonly snackBar = new SnackBar();

  constructor(
    private readonly params: ModalDialogParams,
    private readonly router: RouterExtensions
  ) {
    this.results = params.context?.results ?? [];
  }

  openPerfume(id: number): void {
    this.params.closeCallback();

    setTimeout(() => {
      this.router.navigate(['/perfume', id]);
    }, 0);
  }

  dismiss(): void {
    this.params.closeCallback();

    this.snackBar.simple(
      'Sorry — we couldn’t confidently identify this perfume.',
      '#FFFFFF',
      '#333333',
      2
    );
  }

  getConfidenceLabel(c: PerfumeRecognitionConfidence): string {
    if (c === PerfumeRecognitionConfidence.High) return 'High probability';
    if (c === PerfumeRecognitionConfidence.Medium) return 'Medium probability';
    return 'Low probability';
  }

  getConfidenceColor(c: PerfumeRecognitionConfidence): string {
    if (c === PerfumeRecognitionConfidence.High) return '#2ECC71';
    if (c === PerfumeRecognitionConfidence.Medium) return '#F1C40F';
    return '#E74C3C';
  }
}
