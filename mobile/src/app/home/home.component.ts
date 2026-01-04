import { AfterViewInit, Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { registerElement, RouterExtensions } from '@nativescript/angular';
import { Image, Page } from '@nativescript/core';
import { NgForOf, NgIf } from '@angular/common';
import { FooterComponent } from '../footer/footer.component';
import { Router } from '@angular/router';
import { SnackBar } from '@nativescript-community/ui-material-snackbar';
import { SessionStateService } from '../services/sessionstate.service';
import { PerfumeOfTheDayDto } from '../models/perfumeoftheday.dto';
import { UserContextService } from '../services/usercontext.service';
import { HomeService } from '../services/home.service';
import { environment } from '~/environments/environment';
import { HomeRecentReviewDto } from '../models/homerecentreview.dto';

registerElement('Image', () => Image);

@Component({
  standalone: true,
  selector: 'app-home',
  templateUrl: './home.component.html',
  imports: [NgIf, NgForOf, FooterComponent],
  schemas: [NO_ERRORS_SCHEMA],
})
export class HomeComponent implements OnInit, AfterViewInit {
  private readonly contentUrl = `${environment.contentUrl}`;

  stats = {
    perfumes: 0,
    reviews: 0,
    users: 0
  };

  private snackBar = new SnackBar();
  private pendingMessage?: string;

  greetingLine = '';
  welcomeLine = '';
  showGreeting = false;

  potd: PerfumeOfTheDayDto | null = null;

  recentReviews: HomeRecentReviewDto[] = [];

  constructor(
    private page: Page,
    private routerExtensions: RouterExtensions,
    private router: Router,
    private readonly session: SessionStateService,
    private readonly userContext: UserContextService,
    private readonly homeService: HomeService
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    const nav = this.router.getCurrentNavigation();
    this.pendingMessage = nav?.extras?.state?.['snackbarMessage'];

    if (this.session.consumeHomeGreeting()) {
      this.greetingLine = this.resolveGreeting();
      this.showGreeting = true;

      this.userContext.getProfile().subscribe(profile => {
        this.welcomeLine = profile
          ? `Welcome back, ${profile.displayName}`
          : 'Welcome back';
      });
    } else {
      this.showGreeting = false;
    }

    this.homeService.getPerfumeOfTheDay().subscribe({
      next: r => this.potd = r,
      error: () => this.potd = null
    });

    this.homeService.getRecentReviews(3).subscribe({
      next: r => this.recentReviews = r,
      error: () => this.recentReviews = []
    });

    this.homeService.getStats().subscribe({
      next: s => this.stats = s,
      error: () => {}
    });
  }


  ngAfterViewInit(): void {
    if (!this.pendingMessage) return;

    this.page.on(Page.navigatedToEvent, () => {
      this.snackBar.simple(
        this.pendingMessage!,
        '#000000',
        '#D3A54A',
        1
      );

      this.pendingMessage = undefined;
    });
  }

  get potdImageSrc(): string {
    if (!this.potd?.imageUrl) {
      return '~/assets/images/perfume-placeholder.png';
    }
    return `${this.contentUrl}${this.potd.imageUrl}`;
  }

  getReviewImageSrc(r: HomeRecentReviewDto): string {
    if (!r.perfumeImageUrl) {
      return '~/assets/images/perfume-placeholder.png';
    }
    return `${this.contentUrl}${r.perfumeImageUrl}`;
  }

  toFixedRating(r: number) {
    return r.toFixed(1);
  }

  goToPerfume(id: number) {
    this.router.navigate(['/perfume', id]);
  }

  goToAddPerfume() {
    this.routerExtensions.navigate(['/add-perfume']);
  }

  private resolveGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }
}
