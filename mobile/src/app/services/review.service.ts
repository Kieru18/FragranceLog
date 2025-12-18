import { SaveReviewRequestDto } from '../models/savereviewrequest.dto'
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReviewDto } from '../models/review.dto';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly baseUrl = `${environment.apiUrl}/reviews`;

  constructor(private http: HttpClient) {}

  createOrUpdate(req: SaveReviewRequestDto): Observable<void> {
    return this.http.post<void>(this.baseUrl, req);
  }

  getCurrentUserReview(perfumeId: number) {
    return this.http.get<ReviewDto>(
      `${this.baseUrl}/current`,
      { params: { perfumeId } }
    );
  }

  delete(perfumeId: number) {
    return this.http.delete<void>(
      `${this.baseUrl}/${perfumeId}`
    );
  }
}
