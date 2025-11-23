import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { LoginDto } from '../models/login.dto';
import { Page, Utils } from '@nativescript/core';

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
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private page: Page
  ) {
    this.page.actionBarHidden = true;
    this.form = this.fb.group({
      usernameOrEmail: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
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
        this.router.navigate(['/home']);
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message || 'Login failed';
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
}
