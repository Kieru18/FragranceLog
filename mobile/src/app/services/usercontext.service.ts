import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserProfileDto } from '../models/userprofile.dto';

@Injectable({ providedIn: 'root' })
export class UserContextService {
  private readonly baseUrl = `${environment.apiUrl}/user`;

  private readonly profile$ = new BehaviorSubject<UserProfileDto | null>(null);
  private loaded = false;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfileDto | null> {
    if (this.loaded) {
      return of(this.profile$.value);
    }

    return this.http.get<UserProfileDto>(`${this.baseUrl}/me`).pipe(
      tap(profile => {
        this.profile$.next(profile);
        this.loaded = true;
      })
    );
  }

  clear(): void {
    this.profile$.next(null);
    this.loaded = false;
  }
}
