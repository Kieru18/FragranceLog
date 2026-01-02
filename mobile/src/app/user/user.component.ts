import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { AuthService } from '../services/auth.service';
import { UserProfileDto } from '../models/userprofile.dto';
import { UpdateProfileDto } from '../models/updateprofile.dto';
import { ChangePasswordDto } from '../models/changepassword.dto';
import { Page } from '@nativescript/core';
import { FooterComponent } from '../footer/footer.component';

@Component({
  standalone: true,
  selector: 'app-user',
  templateUrl: './user.component.html',
  imports: [
    NativeScriptCommonModule,
    ReactiveFormsModule,
    NativeScriptFormsModule,
    FooterComponent
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class UserComponent implements OnInit {
  loading = false;
  error: string | null = null;

  profile!: UserProfileDto;

  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly userService: UserService,
    private readonly authService: AuthService,
    private readonly router: Router,
    private page: Page
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      displayName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', Validators.required]
    });

    this.load();
  }

  load(): void {
    this.loading = true;

    this.userService.getMe()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: user => {
          this.profile = user;
          this.profileForm.setValue({
            displayName: user.displayName,
            email: user.email
          });
        },
        error: () => {
          this.error = 'Failed to load profile.';
        }
      });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;

    const dto: UpdateProfileDto = this.profileForm.value as UpdateProfileDto;

    this.loading = true;
    this.userService.updateProfile(dto)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: updated => {
          this.profile = updated;
        },
        error: () => {
          this.error = 'Failed to update profile.';
        }
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;

    const dto: ChangePasswordDto = this.passwordForm.value as ChangePasswordDto;

    this.loading = true;
    this.userService.changePassword(dto)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.passwordForm.reset();
        },
        error: () => {
          this.error = 'Failed to change password.';
        }
      });
  }

  deleteAccount(): void {
    this.loading = true;

    this.userService.deleteAccount()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.authService.logout();
          this.router.navigate(['/login']);
        },
        error: () => {
          this.error = 'Failed to delete account.';
        }
      });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
