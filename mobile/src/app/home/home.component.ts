import { AfterViewInit, Component, NO_ERRORS_SCHEMA, OnDestroy, OnInit } from '@angular/core';
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
import { LocationService } from '../services/location.service';
import { environment } from '~/environments/environment';
import { HomeRecentReviewDto } from '../models/homerecentreview.dto';
import { ShortNumberPipe } from '../pipes/shortnumber.pipe';
import { HomeInsightDto } from '../models/homeinsight.dto';
import { InsightScopeEnum } from '../enums/insightscope.enum';
import { HomeInsightIconEnum } from '../enums/homeinsighticon.enum';
import { HOME_INSIGHT_ICONS } from '../const/HOME_INSIGHT_ICONS';
import { HomeCountryPerfumeDto } from '../models/homecountryperfume.dto';
import { Subscription } from 'rxjs';

registerElement('Image', () => Image);

type LocationConsent = 'unknown' | 'granted' | 'denied';

@Component({
  standalone: true,
  selector: 'app-home',
  templateUrl: './home.component.html',
  imports: [NgIf, NgForOf, FooterComponent, ShortNumberPipe],
  schemas: [NO_ERRORS_SCHEMA],
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  readonly InsightScopeEnum = InsightScopeEnum;

  readonly contentUrl = `${environment.contentUrl}`;

  stats = { perfumes: 0, reviews: 0, users: 0 };

  private snackBar = new SnackBar();
  private pendingMessage?: string;

  greetingLine = '';
  welcomeLine = '';
  showGreeting = false;

  potd: PerfumeOfTheDayDto | null = null;
  recentReviews: HomeRecentReviewDto[] = [];

  insights: HomeInsightDto[] = [];
  globalInsights: HomeInsightDto[] = [];
  personalInsights: HomeInsightDto[] = [];

  locationConsent: LocationConsent = 'unknown';
  countryPerfumes: HomeCountryPerfumeDto[] = [];
  countryLoading = false;

  private navHandlerAttached = false;
  private readonly subs = new Subscription();

  constructor(
    private page: Page,
    private routerExtensions: RouterExtensions,
    private router: Router,
    private readonly session: SessionStateService,
    private readonly userContext: UserContextService,
    private readonly homeService: HomeService,
    private readonly location: LocationService
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    const nav = this.router.getCurrentNavigation();
    this.pendingMessage = nav?.extras?.state?.['snackbarMessage'];

    if (this.session.consumeHomeGreeting()) {
      this.greetingLine = this.resolveGreeting();
      this.showGreeting = true;

      this.subs.add(
        this.userContext.getProfile().subscribe(profile => {
          this.welcomeLine = profile ? `Welcome back, ${profile.displayName}` : 'Welcome back';
        })
      );
    } else {
      this.showGreeting = false;
    }

    this.subs.add(
      this.homeService.getPerfumeOfTheDay().subscribe({
        next: r => (this.potd = r),
        error: () => (this.potd = null)
      })
    );

    this.subs.add(
      this.homeService.getRecentReviews(3).subscribe({
        next: r => (this.recentReviews = r),
        error: () => (this.recentReviews = [])
      })
    );

    this.subs.add(
      this.homeService.getStats().subscribe({
        next: s => (this.stats = s),
        error: () => {}
      })
    );

    this.subs.add(
      this.homeService.getInsights().subscribe({
        next: insights => {
          this.insights = insights;
          this.globalInsights = insights.filter(i => i.scope === InsightScopeEnum.Global);
          this.personalInsights = insights.filter(i => i.scope === InsightScopeEnum.Personal);
        },
        error: () => {
          this.insights = [];
          this.globalInsights = [];
          this.personalInsights = [];
        }
      })
    );

    this.locationConsent = this.location.getConsent();

    if (this.locationConsent === 'granted') {
      this.loadCountryPerfumes();
    }
  }

  ngAfterViewInit(): void {
    if (!this.navHandlerAttached) {
      this.navHandlerAttached = true;

      this.page.on(Page.navigatedToEvent, () => {
        if (!this.pendingMessage) {
          return;
        }

        this.snackBar.simple(
          this.pendingMessage,
          '#000000',
          '#D3A54A',
          1
        );

        this.pendingMessage = undefined;
      });
    }
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  async enableLocationAndLoad(): Promise<void> {
  if (this.countryLoading) {
    return;
  }

  console.log('enableLocationAndLoad called');
  this.countryLoading = true;
  this.countryPerfumes = [];

  try {
    const granted = await this.location.requestPermission();
    console.log('Permission granted:', granted);
    this.locationConsent = granted ? 'granted' : 'denied';

    if (!granted) {
      console.log('Permission not granted');
      this.countryLoading = false;
      return;
    }

    const coords = await this.location.getCoordinates();
    console.log('Coordinates:', coords);
    
    if (!coords) {
      console.log('No coordinates');
      this.countryLoading = false;
      return;
    }

    this.homeService.getTopPerfumesByCountry(coords.lat, coords.lng).subscribe({
      next: r => {
        console.log('API success, results:', r.length);
        this.countryPerfumes = r;
        this.countryLoading = false;
      },
      error: (error) => {
        console.log('API error:', error);
        this.countryPerfumes = [];
        this.countryLoading = false;
      }
    });
  } catch (error) {
    console.log('Enable location error:', error);
    this.countryLoading = false;
  }
}

  private loadCountryPerfumes(): void {
    if (this.countryLoading) return;

    this.countryLoading = true;
    this.countryPerfumes = [];

    this.location.getCoordinates().then(coords => {
      if (!coords) {
        this.countryLoading = false;
        return;
      }

      this.homeService.getTopPerfumesByCountry(coords.lat, coords.lng).subscribe({
        next: r => {
          this.countryPerfumes = r;
          this.countryLoading = false;
        },
        error: () => {
          this.countryLoading = false;
        }
      });
    }).catch(() => {
      this.countryLoading = false;
    });
  }

  getInsightIcon(icon: HomeInsightIconEnum): string {
    return HOME_INSIGHT_ICONS[icon] ?? '\uf128';
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

  getCountryPerfumeImage(r: HomeCountryPerfumeDto): string {
    if (!r.imageUrl) {
      return '~/assets/images/perfume-placeholder.png';
    }
    return `${this.contentUrl}${r.imageUrl}`;
  }

  goToPerfume(id: number): void {
    this.router.navigate(['/perfume', id]);
  }

  goToAddPerfume(): void {
    this.routerExtensions.navigate(['/add-perfume']);
  }

  private resolveGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }
}
