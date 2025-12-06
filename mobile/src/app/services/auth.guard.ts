import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { map, take } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate() {
    return this.auth.isAuthenticated().pipe(
      take(1),
      map(isAuth => {
        if (isAuth) return true;
        this.router.navigate(['/login']);
        return false;
      })
    );
  }
}
