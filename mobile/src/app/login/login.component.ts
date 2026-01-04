import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { CommonService } from '../services/common.service';
import { SessionStateService } from '../services/sessionstate.service';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { LoginDto } from '../models/login.dto';
import { Page, Utils } from '@nativescript/core';
import { take } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [
    NativeScriptCommonModule,
    ReactiveFormsModule,
    NativeScriptFormsModule,
    RouterModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class LoginComponent implements OnInit {
  form: FormGroup;
  loading = false;
  error: string | null = null;

  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private page: Page,
    private common: CommonService,
    private session: SessionStateService
  ) {
    this.page.actionBarHidden = true;
    this.form = this.fb.group({
      usernameOrEmail: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit() {
    this.auth.isAuthenticated()
      .pipe(take(1))
      .subscribe(isAuth => {
        if (isAuth) {
          this.router.navigate(['/search']);
        }
      });
  }

  onSubmit() {
    Utils.dismissSoftInput();
    if (this.form.invalid || this.loading) {
      return;
    }

    this.loading = true;
    this.error = null;

    const dto: LoginDto = this.form.value;

    this.auth.login(dto).subscribe({
      next: () => {
        this.loading = false;
        this.session.reset();
        this.router.navigate(['/home']);
      },
      error: err => {
        this.loading = false;
        this.error = this.common.getErrorMessage(err, 'Login failed');
      }
    });
  }

  goToRegister() {
    this.router.navigate(['/register'], <any>{
      transition: {
        name: 'slide',
        duration: 220,
        curve: 'easeInOut'
      }
    });
  }

  goToForgotPassword() {
    this.router.navigate(['/forgot-password']);
  }

  togglePassword(): void { 
    this.showPassword = !this.showPassword; 
  }
}
