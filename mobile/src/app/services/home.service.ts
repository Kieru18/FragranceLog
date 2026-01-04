import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { PerfumeOfTheDayDto } from '../models/perfumeoftheday.dto';
import { HomeRecentReviewDto } from '../models/homerecentreview.dto';
import { HomeStatsDto } from '../models/homestats.dto';

@Injectable({ providedIn: 'root' })
export class HomeService {
  private readonly baseUrl = `${environment.apiUrl}/home`;

  constructor(private http: HttpClient) {}

  getPerfumeOfTheDay() {
    return this.http.get<PerfumeOfTheDayDto>(
      `${this.baseUrl}/perfume-of-the-day`
    );
  }

  getRecentReviews(take = 3) {
    return this.http.get<HomeRecentReviewDto[]>(
      `${this.baseUrl}/recent-reviews`,
      { params: { take } }
    );
  }

  getStats() {
    return this.http.get<HomeStatsDto>(`${this.baseUrl}/stats`);
  }
}
