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

registerElement('Image', () => Image);

@Component({
  standalone: true,
  selector: 'app-home',
  templateUrl: './home.component.html',
  imports: [NgIf, NgForOf, FooterComponent],
  schemas: [NO_ERRORS_SCHEMA],
})
export class HomeComponent implements OnInit, AfterViewInit {
  featured = {
    image: '~/assets/images/bleu.jpg',
    name: 'Bleu de Chanel',
    tagline: 'Masculine, fresh and timeless',
    rating: 4.6,
  };

  actions = [
    { id: 'add', title: 'Add Perfume', icon: '➕', route: '/add', primary: true },
    { id: 'collection', title: 'My Collection', icon: '📚', route: '/collection' },
    { id: 'explore', title: 'Explore', icon: '🔍', route: '/explore' },
  ];

  recentReviews = [
    { id: 1, perfume: 'Dior Sauvage', rating: 4.5, excerpt: 'Amazing projection, compliments all day.', image: '~/assets/images/dior_sauvage.png' },
    { id: 2, perfume: 'Aventus by Creed', rating: 5.0, excerpt: 'A true classic. Timeless masculine scent.', image: '~/assets/images/aventus.png' },
    { id: 3, perfume: 'Awaan', rating: 4.2, excerpt: 'Nice sweetness and woody base.', image: '~/assets/images/awaan.png' }
  ];

  stats = {
    perfumes: 128,
    reviews: 342,
    users: 4_200,
  };

  private snackBar = new SnackBar();
  private pendingMessage?: string;

  greetingLine = '';
  welcomeLine = '';
  showGreeting = false;

  potd: PerfumeOfTheDayDto | null = null;

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

  toFixedRating(r: number) {
    return r.toFixed(1);
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
