import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { Page, View, Screen, Utils, PanGestureEventData } from '@nativescript/core';
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

  modalGender: GenderEnum | null = null;
  modalLongevity: LongevityEnum | null = null;
  modalSillage: SillageEnum | null = null;
  modalSeason: SeasonEnum | null = null;
  modalDaytime: DaytimeEnum | null = null;

  showVotingModal = false;
  isSavingVotes = false;

  private sheetView!: View;
  private backdropView!: View;
  private readonly screenHeight = Screen.mainScreen.heightDIPs;
  private panStartY = 0;

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

        this.modalGender = d.myGenderVote ?? null;
        this.modalLongevity = d.myLongevityVote ?? null;
        this.modalSillage = d.mySillageVote ?? null;
        this.modalSeason = d.mySeasonVote ?? null;
        this.modalDaytime = d.myDaytimeVote ?? null;

        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  get reviewAction(): 'save' | 'delete' | 'none' {
    if (!this.details) return 'none';

    if (this.isDirty) return 'save';

    if (this.details.myRating != null) return 'delete';

    return 'none';
  }

  get canPerformReviewAction(): boolean {
    return this.reviewAction !== 'none' && !this.isSubmitting;
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

  hasAnyVotes(): boolean {
    return this.myGender !== null ||
           this.myLongevity !== null ||
           this.mySillage !== null ||
           this.mySeason !== null ||
           this.myDaytime !== null;
  }

  onVotingSheetLoaded(args: any): void {
    this.sheetView = args.object as View;
    const parent = this.sheetView.parent as View;
    this.backdropView = parent.getViewById('votingBackdrop') as View;

    this.sheetView.translateY = this.screenHeight;
    this.backdropView.opacity = 0;
  }

  onVotingSheetPan(args: PanGestureEventData): void {
    if (!this.sheetView) return;

    switch (args.state) {
      case 1:
        this.panStartY = this.sheetView.translateY || 0;
        break;

      case 2:
        const newY = Math.max(0, this.panStartY + args.deltaY);
        this.sheetView.translateY = newY;
        break;

      case 3:
      case 4:
        if (this.sheetView.translateY > this.screenHeight * 0.3) {
          this.closeVotingModal();
        } else {
          this.sheetView.animate({
            translate: { x: 0, y: 0 },
            duration: 200,
            curve: 'easeOut'
          });
        }
        break;
    }
  }

  openVotingModal(): void {
    if (this.showVotingModal) return;
    Utils.dismissSoftInput();

    this.modalGender = this.myGender;
    this.modalLongevity = this.myLongevity;
    this.modalSillage = this.mySillage;
    this.modalSeason = this.mySeason;
    this.modalDaytime = this.myDaytime;

    this.showVotingModal = true;

    this.sheetView.translateY = this.screenHeight;
    this.backdropView.opacity = 0;

    this.backdropView.animate({
      opacity: 0.6,
      duration: 240
    });

    this.sheetView.animate({
      translate: { x: 0, y: 0 },
      duration: 240,
      curve: 'easeOut'
    });
  }

  closeVotingModal(): void {
    if (!this.showVotingModal) return;
    Utils.dismissSoftInput();

    this.sheetView.animate({
      translate: { x: 0, y: this.screenHeight },
      duration: 180,
      curve: 'easeIn'
    });

    this.backdropView.animate({
      opacity: 0,
      duration: 150
    }).then(() => {
      this.showVotingModal = false;
    });
  }

  selectModalGender(value: GenderEnum) {
    this.modalGender = this.modalGender === value ? null : value;
  }

  selectModalLongevity(value: LongevityEnum) {
    this.modalLongevity = this.modalLongevity === value ? null : value;
  }

  selectModalSillage(value: SillageEnum) {
    this.modalSillage = this.modalSillage === value ? null : value;
  }

  selectModalSeason(value: SeasonEnum) {
    this.modalSeason = this.modalSeason === value ? null : value;
  }

  selectModalDaytime(value: DaytimeEnum) {
    this.modalDaytime = this.modalDaytime === value ? null : value;
  }

  private refreshPerfumeFeaturesAfterVotes(): void {
    if (!this.details) return;

    this.perfumeService.getPerfumeDetails(this.perfumeId).subscribe({
      next: d => {
        if (!this.details) return;

        this.details.gender = d.gender ?? null;
        this.details.season = d.season ?? null;
        this.details.daytime = d.daytime ?? null;
        this.details.longevity = d.longevity ?? null;
        this.details.sillage = d.sillage ?? null;

        this.details.myGenderVote = d.myGenderVote ?? null;
        this.details.mySeasonVote = d.mySeasonVote ?? null;
        this.details.myDaytimeVote = d.myDaytimeVote ?? null;
        this.details.myLongevityVote = d.myLongevityVote ?? null;
        this.details.mySillageVote = d.mySillageVote ?? null;

        this.myGender = d.myGenderVote ?? null;
        this.myLongevity = d.myLongevityVote ?? null;
        this.mySillage = d.mySillageVote ?? null;
        this.mySeason = d.mySeasonVote ?? null;
        this.myDaytime = d.myDaytimeVote ?? null;

        if (this.showVotingModal) {
          this.modalGender = this.myGender;
          this.modalLongevity = this.myLongevity;
          this.modalSillage = this.mySillage;
          this.modalSeason = this.mySeason;
          this.modalDaytime = this.myDaytime;
        }
      }
    });
  }

  saveAllVotes(): void {
    if (this.isSavingVotes || !this.details) return;

    this.isSavingVotes = true;

    const requests = [];

    if (this.modalGender !== this.myGender) {
      if (this.modalGender !== null) {
        requests.push(
          this.voteService.setGenderVote(
            this.perfumeId,
            new SetGenderVoteRequestDto(this.modalGender)
          )
        );
      }
    }

    if (this.modalLongevity !== this.myLongevity) {
      if (this.modalLongevity !== null) {
        requests.push(
          this.voteService.setLongevityVote(
            this.perfumeId,
            new SetLongevityVoteRequestDto(this.modalLongevity)
          )
        );
      }
    }

    if (this.modalSillage !== this.mySillage) {
      if (this.modalSillage !== null) {
        requests.push(
          this.voteService.setSillageVote(
            this.perfumeId,
            new SetSillageVoteRequestDto(this.modalSillage)
          )
        );
      }
    }

    if (this.modalSeason !== this.mySeason) {
      if (this.modalSeason !== null) {
        requests.push(
          this.voteService.setSeasonVote(
            this.perfumeId,
            new SetSeasonVoteRequestDto(this.modalSeason)
          )
        );
      }
    }

    if (this.modalDaytime !== this.myDaytime) {
      if (this.modalDaytime !== null) {
        requests.push(
          this.voteService.setDaytimeVote(
            this.perfumeId,
            new SetDaytimeVoteRequestDto(this.modalDaytime)
          )
        );
      }
    }

    if (requests.length === 0) {
      this.isSavingVotes = false;
      this.closeVotingModal();
      return;
    }

    Promise.all(requests.map(req => req.toPromise()))
      .then(() => {
        this.myGender = this.modalGender;
        this.myLongevity = this.modalLongevity;
        this.mySillage = this.modalSillage;
        this.mySeason = this.modalSeason;
        this.myDaytime = this.modalDaytime;

        if (this.details) {
          this.details.myGenderVote = this.myGender;
          this.details.myLongevityVote = this.myLongevity;
          this.details.mySillageVote = this.mySillage;
          this.details.mySeasonVote = this.mySeason;
          this.details.myDaytimeVote = this.myDaytime;
        }

        this.isSavingVotes = false;
        this.closeVotingModal();
        this.refreshPerfumeFeaturesAfterVotes();
      })
      .catch(() => {
        this.isSavingVotes = false;
      });
  }

  get longevityLevel(): number {
    if (this.details?.longevity == null) return 0;
    return Math.round(this.details.longevity);
  }

  get sillageLevel(): number {
    if (this.details?.sillage == null) return 0;
    return Math.round(this.details.sillage);
  }

  deleteMyReview(): void {
    if (!this.details || this.isSubmitting) return;

    this.isSubmitting = true;

    this.reviewService.delete(this.details.perfumeId).subscribe({
      next: () => {
        this.applyLocalReviewDelete();
        this.isSubmitting = false;
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }

  private applyLocalReviewDelete(): void {
    if (!this.details) return;

    const hadRating = this.details.myRating != null;
    const hadComment = !!this.details.myReview;

    if (hadRating) {
      const oldRating = this.details.myRating!;
      const oldCount = this.details.ratingCount;

      this.details.ratingCount--;

      this.details.avgRating =
        this.details.ratingCount > 0
          ? ((this.details.avgRating * oldCount) - oldRating) / this.details.ratingCount
          : 0;
    }

    if (hadComment) {
      this.details.commentCount--;
    }

    this.details.myRating = null;
    this.details.myReview = null;

    this.userRating = undefined;
    this.reviewText = '';
    this.initialRating = null;
    this.initialText = '';
    this.isDirty = false;

    const idx = this.details.reviews.findIndex(
      r => r.author === this.details!.reviews.find(rr => rr.author)?.author
    );

    if (idx >= 0) {
      this.details.reviews.splice(idx, 1);
    }
  }

}
