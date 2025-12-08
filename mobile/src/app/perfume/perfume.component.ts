import { Component, NO_ERRORS_SCHEMA, OnDestroy, OnInit } from '@angular/core';
import { NativeScriptCommonModule } from '@nativescript/angular';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Page } from '@nativescript/core';
import { Subscription } from 'rxjs';
import { PerfumeService } from '../services/perfume.service';
import { PerfumeDetailsDto } from '../models/perfumedetails.dto';
import { NgIf, NgForOf } from '@angular/common';
import { FooterComponent } from '../footer/footer.component';

@Component({
  standalone: true,
  selector: 'app-perfume',
  templateUrl: './perfume.component.html',
  imports: [
    NativeScriptCommonModule,
    RouterModule,
    NgIf,
    NgForOf,
    FooterComponent
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class PerfumeComponent implements OnInit, OnDestroy {
  perfume: PerfumeDetailsDto | null = null;
  loading = true;
  error: string | null = null;

  private sub?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private perfumeService: PerfumeService,
    private page: Page
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.params['id'];
    const id = Number(idParam);

    if (!id || Number.isNaN(id)) {
      this.router.navigate(['/home']);
      return;
    }

    this.loading = true;
    this.error = null;

    this.sub = this.perfumeService.getPerfumeDetails(id).subscribe({
      next: perfume => {
        this.perfume = perfume;
        this.loading = false;
      },
      error: err => {
        console.log('Perfume details error', err);
        this.error = 'Could not load perfume details.';
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
