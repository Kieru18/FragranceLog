import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { RegisterDto } from '../models/register.dto';
import { Page, Utils } from '@nativescript/core';

@Component({
  standalone: true,
  selector: 'app-register',
  templateUrl: './register.component.html',
  imports: [
    NativeScriptCommonModule,
    ReactiveFormsModule,
    NativeScriptFormsModule,
    RouterModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class RegisterComponent {
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
      username: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
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

    const dto: RegisterDto = this.form.value;

    this.auth.register(dto).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/home']);
      },
      error: err => {
        this.loading = false;
        console.log(err);
        this.error = err?.error?.message || err?.error?.error || err?.error?.errors?.Password || 'Registration failed'; // @TODO fix error string aquisition
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login'], <any>{
      transition: {
        name: 'slideRight',
        duration: 220,
        curve: 'easeInOut'
      }
    });
  }
}
