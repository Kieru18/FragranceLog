import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { PerfumeOfTheDayDto } from '../models/perfumeoftheday.dto';

@Injectable({ providedIn: 'root' })
export class HomeService {
  private readonly baseUrl = `${environment.apiUrl}/home`;

  constructor(private http: HttpClient) {}

  getPerfumeOfTheDay() {
    return this.http.get<PerfumeOfTheDayDto>(
      `${this.baseUrl}/perfume-of-the-day`
    );
  }
}
