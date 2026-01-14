import { Injectable } from '@angular/core';
import { HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';


@Injectable()
export class AuthRefreshInterceptor implements HttpInterceptor {

  private refreshing = false;

  constructor(private auth: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    if (req.url.includes('/auth/login') || req.url.includes('/auth/refresh')) {
        return next.handle(req);
    }

    return next.handle(req).pipe(
      catchError(err => {

        if (err.status !== 401) {
          return throwError(() => err);
        }

        if (this.refreshing) {
          this.auth.logout();
          return throwError(() => err);
        }

        this.refreshing = true;

        return this.auth.refresh()!.pipe(
          switchMap(() => {
            this.refreshing = false;

            const token = this.auth.getAccessToken();
            if (!token) {
              this.auth.logout();
              return throwError(() => err);
            }

            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${token}` }
            });

            return next.handle(retryReq);
          }),
          catchError(refreshErr => {
            this.refreshing = false;
            this.auth.logout();
            return throwError(() => refreshErr);
          })
        );
      })
    );
  }
}
