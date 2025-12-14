import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { Page } from '@nativescript/core';
import { PerfumeService } from '../services/perfume.service';
import { ReviewService } from '../services/review.service';
import { PerfumeDetailsDto } from '../models/perfumedetails.dto';
import { NoteTypeEnum } from '../models/notetype.enum';
import { FooterComponent } from '../footer/footer.component';
import { GROUP_COLORS } from '../const/GROUP_COLORS'
import { environment } from '~/environments/environment';
import { DatePipe } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-perfume',
  templateUrl: './perfume.component.html',
  imports: [
    NativeScriptCommonModule,
    NativeScriptFormsModule,
    FooterComponent,
    DatePipe
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class PerfumeComponent implements OnInit {
  loading = true;
  details: PerfumeDetailsDto | null = null;
  perfumeId!: number;

  userRating?: number;
  reviewText = '';
  isDirty = false;
  isSubmitting = false;


  private readonly baseUrl = `${environment.contentUrl}`;

  constructor(
    private route: ActivatedRoute,
    private perfumeService: PerfumeService,
    private page: Page,
    private reviewService: ReviewService
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.perfumeId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.perfumeId) return;

    this.perfumeService.getPerfumeDetails(this.perfumeId).subscribe({
      next: d => {
        this.details = d;

        this.userRating = d.myRating ?? undefined;
        this.reviewText = d.myReview ?? '';
        this.isDirty = false;

        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  get perfumeImageSrc(): string {
    const path = this.details?.imageUrl;

    if (!path) {
      return '~/assets/images/perfume-placeholder.png';
    }

    return `${this.baseUrl}${path}`;
  }

  onImageLoaded(e: any) {
    e.object.opacity = 0;
    e.object.animate({ opacity: 1, duration: 250 });
  }

  noteTypeLabel(type: NoteTypeEnum): string {
    switch (type) {
      case NoteTypeEnum.Top: return 'Top notes';
      case NoteTypeEnum.Middle: return 'Heart notes';
      case NoteTypeEnum.Base: return 'Base notes';
      default: return '';
    }
  }

  getGroupColor(name: string): string {
    return GROUP_COLORS[name] ?? '#444444';
  }

  rate(value: number) {
    if (this.userRating === value) return;

    this.userRating = value;
    this.isDirty = true;
  }

  submitReview(): void {
    if (!this.userRating) return;

    this.isSubmitting = true;

    this.reviewService.createOrUpdate({
      perfumeId: this.details.perfumeId,
      rating: this.userRating,
      text: this.reviewText?.trim() || null
    }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.isDirty = false;
        this.reloadDetails();
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }

  private reloadDetails(): void {
    this.loading = true;

    this.perfumeService.getPerfumeDetails(this.perfumeId).subscribe({
      next: d => {
        this.details = d;

        this.userRating = d.myRating ?? undefined;
        this.reviewText = d.myReview ?? '';
        this.isDirty = false;

        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}
