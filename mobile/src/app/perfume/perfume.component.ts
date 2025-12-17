import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { Page } from '@nativescript/core';
import { PerfumeService } from '../services/perfume.service';
import { ReviewService } from '../services/review.service';
import { VoteService } from '../services/vote.service';
import { PerfumeDetailsDto } from '../models/perfumedetails.dto';
import { NoteTypeEnum } from '../enums/notetype.enum';
import { FooterComponent } from '../footer/footer.component';
import { GROUP_COLORS } from '../const/GROUP_COLORS';
import { environment } from '~/environments/environment';
import { DatePipe } from '@angular/common';
import { GenderEnum } from '../enums/gender.enum';
import { LongevityEnum } from '../enums/longevity.enum';
import { SillageEnum } from '../enums/sillage.enum';
import { SeasonEnum } from '../enums/season.enum';
import { DaytimeEnum } from '../enums/daytime.enum';
import { SetLongevityVoteRequestDto } from '../models/setlongevityvoterequest.dto';
import { SetGenderVoteRequestDto } from '../models/setgendervoterequest.dto';
import { SetSillageVoteRequestDto } from '../models/setsillagevoterequest.dto';
import { SetSeasonVoteRequestDto } from '../models/setseasonvoterequest.dto';
import { SetDaytimeVoteRequestDto } from '../models/setdaytimevoterequest.dto';

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
  GenderEnum = GenderEnum;
  LongevityEnum = LongevityEnum;
  SillageEnum = SillageEnum;
  SeasonEnum = SeasonEnum;
  DaytimeEnum = DaytimeEnum;

  loading = true;
  details: PerfumeDetailsDto | null = null;
  perfumeId!: number;

  userRating?: number;
  reviewText = '';
  isDirty = false;
  isSubmitting = false;

  private initialRating: number | null = null;
  private initialText: string = '';

  myGender: GenderEnum | null = null;
  myLongevity: LongevityEnum | null = null;
  mySillage: SillageEnum | null = null;
  mySeason: SeasonEnum | null = null;
  myDaytime: DaytimeEnum | null = null;

  private readonly baseUrl = `${environment.contentUrl}`;

  constructor(
    private route: ActivatedRoute,
    private perfumeService: PerfumeService,
    private page: Page,
    private reviewService: ReviewService,
    private voteService: VoteService
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
        this.initialRating = d.myRating ?? null;
        this.initialText = d.myReview ?? '';
        this.isDirty = false;

        this.myGender = d.myGenderVote ?? null;
        this.myLongevity = d.myLongevityVote ?? null;
        this.mySillage = d.mySillageVote ?? null;
        this.mySeason = d.mySeasonVote ?? null;
        this.myDaytime = d.myDaytimeVote ?? null;

        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  get perfumeImageSrc(): string {
    const path = this.details?.imageUrl;
    if (!path) return '~/assets/images/perfume-placeholder.png';
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
    this.recomputeDirty();
  }

  onReviewTextChange(value: string) {
    this.reviewText = value ?? '';
    this.recomputeDirty();
  }

  private recomputeDirty(): void {
    const currentRating = this.userRating ?? null;
    const currentText = (this.reviewText ?? '').trim();
    const baselineText = (this.initialText ?? '').trim();

    this.isDirty = currentRating !== this.initialRating || currentText !== baselineText;
  }

  submitReview(): void {
    if (!this.userRating || !this.details) return;

    this.isSubmitting = true;

    const rating = this.userRating;
    const text = this.reviewText?.trim() || null;

    this.reviewService.createOrUpdate({
      perfumeId: this.details.perfumeId,
      rating,
      text
    }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.isDirty = false;
        this.applyLocalAggregates(rating, text);
        this.initialRating = this.details?.myRating ?? null;
        this.initialText = this.details?.myReview ?? '';
        this.recomputeDirty();
        this.refreshCurrentUserReview();
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }

  private applyLocalAggregates(rating: number, text: string | null): void {
    if (!this.details) return;

    const hadRatingBefore = this.details.myRating != null;
    const hadReviewBefore = !!this.details.myReview;

    this.details.myRating = rating;
    this.details.myReview = text;

    if (!hadRatingBefore) {
      this.details.avgRating = this.recalculateAverage(
        this.details.avgRating,
        this.details.ratingCount,
        rating
      );
      this.details.ratingCount++;
    }

    if (!hadReviewBefore && text) {
      this.details.commentCount++;
    }
  }

  private recalculateAverage(currentAvg: number, 
                             count: number,
                             newRating: number): number {
    if (count === 0) return newRating;
    return ((currentAvg * count) + newRating) / (count + 1);
  }

  private refreshCurrentUserReview(): void {
    if (!this.details) return;

    this.reviewService.getCurrentUserReview(this.details.perfumeId).subscribe({
      next: review => {
        const index = this.details!.reviews.findIndex(
          r => r.author === review.author
        );

        if (index >= 0) {
          this.details!.reviews[index] = review;
        } else {
          this.details!.reviews.unshift(review);
        }
      }
    });
  }

  get canSubmitReview(): boolean {
    return !!this.userRating && this.isDirty && !this.isSubmitting;
  }

  isStarFilled(star: number): boolean {
    return star <= (this.userRating || 0);
  }

  selectGender(value: GenderEnum) {
    if (this.myGender === value) return;

    this.myGender = value;
    this.voteService
      .setGenderVote(this.perfumeId, new SetGenderVoteRequestDto(value))
      .subscribe();
  }

  selectLongevity(value: LongevityEnum) {
    if (this.myLongevity === value) return;

    this.myLongevity = value;
    this.voteService
      .setLongevityVote(this.perfumeId, new SetLongevityVoteRequestDto(value))
      .subscribe();
  }

  selectSillage(value: SillageEnum) {
    if (this.mySillage === value) return;

    this.mySillage = value;
    this.voteService
      .setSillageVote(this.perfumeId, new SetSillageVoteRequestDto(value))
      .subscribe();
  }

  selectSeason(value: SeasonEnum) {
    if (this.mySeason === value) return;

    this.mySeason = value;
    this.voteService
      .setSeasonVote(this.perfumeId, new SetSeasonVoteRequestDto(value))
      .subscribe();
  }

  selectDaytime(value: DaytimeEnum) {
    if (this.myDaytime === value) return;

    this.myDaytime = value;
    this.voteService
      .setDaytimeVote(this.perfumeId, new SetDaytimeVoteRequestDto(value))
      .subscribe();
  }

  get longevityLevel(): number {
    if (this.details?.longevity == null) return 0;
    return Math.round(this.details.longevity);
  }

  get sillageLevel(): number {
    if (this.details?.sillage == null) return 0;
    return Math.round(this.details.sillage);
  }
}
