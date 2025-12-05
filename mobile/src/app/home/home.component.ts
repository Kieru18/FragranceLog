import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { registerElement } from '@nativescript/angular';
import { Image, Page } from '@nativescript/core';
import { NgForOf } from '@angular/common';
import { RouterExtensions } from '@nativescript/angular';

registerElement('Image', () => Image);

@Component({
  standalone: true,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  imports: [NgForOf],
  schemas: [NO_ERRORS_SCHEMA],
})
export class HomeComponent {
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

  constructor(private page: Page, private routerExtensions: RouterExtensions) {
    this.page.actionBarHidden = true;
  }

  toFixedRating(r: number) {
    return r.toFixed(1);
  }

  goToSearch() {
    this.routerExtensions.navigate(['/search']);
  }
}
