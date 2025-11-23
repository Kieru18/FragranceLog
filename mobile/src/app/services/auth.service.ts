import { Injectable, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ApplicationSettings } from '@nativescript/core';
import { BehaviorSubject, map, tap } from 'rxjs';
import { LoginDto } from '../models/login.dto';
import { RegisterDto } from '../models/register.dto';
import { AuthResponseDto } from '../models/authresponse.dto';
import { environment } from '../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;
  private readonly accessTokenKey = 'accessToken';
  private readonly refreshTokenKey = 'refreshToken';
  private refreshTimerId: any = null;

  private authenticated$ = new BehaviorSubject<boolean>(this.hasStoredTokens());

  constructor(
    private http: HttpClient,
    private router: Router,
    private zone: NgZone
  ) {
    const token = this.getAccessToken();
    if (token) {
      this.scheduleRefresh(token, this.getRefreshToken());
    }
  }

  login(dto: LoginDto) {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/login`, dto).pipe(
      tap(response => {
        if (response.accessToken && response.refreshToken) {
          this.storeTokens(response.accessToken, response.refreshToken);
          this.scheduleRefresh(response.accessToken, response.refreshToken);
          this.authenticated$.next(true);
        }
      })
    );
  }

  register(dto: RegisterDto) {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/register`, dto).pipe(
      tap(response => {
        if (response.accessToken && response.refreshToken) {
          this.storeTokens(response.accessToken, response.refreshToken);
          this.scheduleRefresh(response.accessToken, response.refreshToken);
          this.authenticated$.next(true);
        }
      })
    );
  }

  refresh() {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.logout();
      return;
    }

    return this.http.post<AuthResponseDto>(`${this.baseUrl}/refresh`, { refreshToken }).pipe(
      tap(response => {
        if (response.accessToken && response.refreshToken) {
          this.storeTokens(response.accessToken, response.refreshToken);
          this.scheduleRefresh(response.accessToken, response.refreshToken);
          this.authenticated$.next(true);
        } else {
          this.logout();
        }
      })
    );
  }

  logout() {
    ApplicationSettings.remove(this.accessTokenKey);
    ApplicationSettings.remove(this.refreshTokenKey);
    if (this.refreshTimerId) {
      clearTimeout(this.refreshTimerId);
      this.refreshTimerId = null;
    }
    this.authenticated$.next(false);
    this.zone.run(() => this.router.navigate(['/login']));
  }

  isAuthenticated() {
    return this.authenticated$.asObservable();
  }

  getAccessToken(): string | null {
    const v = ApplicationSettings.getString(this.accessTokenKey);
    return v ?? null;
  }

  private getRefreshToken(): string | null {
    const v = ApplicationSettings.getString(this.refreshTokenKey);
    return v ?? null;
  }


  private storeTokens(accessToken: string, refreshToken: string) {
    ApplicationSettings.setString(this.accessTokenKey, accessToken);
    ApplicationSettings.setString(this.refreshTokenKey, refreshToken);
  }

  private hasStoredTokens(): boolean {
    const access = this.getAccessToken();
    const refresh = this.getRefreshToken();
    return !!access && !!refresh;
  }

  private scheduleRefresh(accessToken: string | null, refreshToken: string | null) {
    if (!accessToken || !refreshToken) {
      return;
    }

    try {
      const payload = JSON.parse(atob(accessToken.split('.')[1]));
      const expSeconds = payload.exp as number | undefined;
      if (!expSeconds) {
        return;
      }

      const expMs = expSeconds * 1000;
      const nowMs = Date.now();
      let delay = expMs - nowMs - 30000;
      if (delay < 5000) {
        delay = 5000;
      }

      if (this.refreshTimerId) {
        clearTimeout(this.refreshTimerId);
      }

      this.refreshTimerId = setTimeout(() => {
        const observable = this.refresh();
        if (observable) {
          observable.subscribe({
            error: () => this.logout()
          });
        }
      }, delay);
    } catch {
    }
  }
}
