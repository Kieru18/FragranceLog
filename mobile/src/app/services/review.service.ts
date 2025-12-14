import { CreateReviewRequestDto } from '../models/createreviewrequest.dto'
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly baseUrl = `${environment.apiUrl}/reviews`;

  constructor(private http: HttpClient) {}

  createOrUpdate(req: CreateReviewRequestDto): Observable<void> {
    return this.http.post<void>(this.baseUrl, req);
  }
}
