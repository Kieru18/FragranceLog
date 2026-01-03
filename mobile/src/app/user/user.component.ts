import { Component, NO_ERRORS_SCHEMA, OnInit, ViewChild, ElementRef } from '@angular/core';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { AuthService } from '../services/auth.service';
import { UserProfileDto } from '../models/userprofile.dto';
import { UpdateProfileDto } from '../models/updateprofile.dto';
import { ChangePasswordDto } from '../models/changepassword.dto';
import { Page, Utils, View } from '@nativescript/core';
import { FooterComponent } from '../footer/footer.component';
import { CommonService } from '../services/common.service';
import { PasswordStrengthService, PasswordStrengthState } from '../services/passwordstrength.service';

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

  dialog = {
    visible: false,
    mode: 'delete' as 'delete'
  };

  private isAnimating = false;

  @ViewChild('dialogBackdrop', { static: false }) dialogBackdropRef?: ElementRef<View>;
  @ViewChild('dialogPanel', { static: false }) dialogPanelRef?: ElementRef<View>;

  showCurrentPassword = false;
  showNewPassword = false;

  passwordStrength: PasswordStrengthState;
  private strengthVisible = false;

  @ViewChild('strengthFill', { static: false }) strengthFillRef?: ElementRef<View>;
  @ViewChild('checklist', { static: false }) checklistRef?: ElementRef<View>;

  constructor(
    private readonly fb: FormBuilder,
    private readonly userService: UserService,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly common: CommonService,
    private readonly passwordStrengthService: PasswordStrengthService,
    private page: Page
  ) {
    this.page.actionBarHidden = true;
    this.passwordStrength = this.passwordStrengthService.evaluate('');
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

    this.passwordForm.get('newPassword')!.valueChanges.subscribe(value => {
      this.passwordStrength = this.passwordStrengthService.evaluate(value ?? '');
      this.updateStrengthVisibility(!!value);
      this.animateStrengthBar();
    });

    this.load();
  }

  toggleCurrentPassword(): void {
    this.showCurrentPassword = !this.showCurrentPassword;
  }

  toggleNewPassword(): void {
    this.showNewPassword = !this.showNewPassword;
  }

  private updateStrengthVisibility(show: boolean): void {
    const checklist = this.checklistRef?.nativeElement;
    if (!checklist) return;

    if (show && !this.strengthVisible) {
      this.strengthVisible = true;
      checklist.opacity = 0;
      checklist.translateY = 6;
      checklist.animate({
        opacity: 1,
        translate: { x: 0, y: 0 },
        duration: 220,
        curve: 'easeOut'
      });
    }

    if (!show && this.strengthVisible) {
      this.strengthVisible = false;
      checklist.animate({
        opacity: 0,
        translate: { x: 0, y: 6 },
        duration: 160,
        curve: 'easeIn'
      });
    }
  }

  private animateStrengthBar(): void {
    const fill = this.strengthFillRef?.nativeElement;
    if (!fill) return;

    fill.originX = 0;
    fill.scaleY = 1;

    fill.animate({
      scale: {
        x: this.passwordStrength.percent / 100,
        y: 1
      },
      duration: 180,
      curve: 'easeOut'
    });
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
        error: err => {
          this.error = this.common.getErrorMessage(err, 'Failed to load profile.');
        }
      });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;

    const dto: UpdateProfileDto = this.profileForm.value;
    this.loading = true;

    this.userService.updateProfile(dto)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: updated => this.profile = updated,
        error: err => {
          this.error = this.common.getErrorMessage(err, 'Failed to update profile.');
        }
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid || this.passwordStrength.score < 5) return;

    const dto: ChangePasswordDto = this.passwordForm.value;
    this.loading = true;

    this.userService.changePassword(dto)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.passwordForm.reset();
          this.passwordStrength = this.passwordStrengthService.evaluate('');
          this.strengthVisible = false;
        },
        error: err => {
          this.error = this.common.getErrorMessage(err, 'Failed to change password.');
        }
      });
  }

   openDeleteDialog(): void {
    if (this.dialog.visible || this.isAnimating) return;

    Utils.dismissSoftInput();
    this.dialog.visible = true;

    setTimeout(() => {
      this.animateDialogIn();
    }, 10);
  }

  closeDialog(): void {
    if (!this.dialog.visible || this.isAnimating) return;

    this.animateDialogOut().then(() => {
      this.dialog.visible = false;
    });
  }

  confirmDelete(): void {
    if (this.isAnimating) return;

    this.animateDialogOut().then(() => {
      this.dialog.visible = false;
      this.performDelete();
    });
  }

  private performDelete(): void {
    this.loading = true;

    this.userService.deleteAccount()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.authService.logout();
          this.router.navigate(['/login'], {
            queryParams: { deleted: '1' }
          });
        },
        error: err => {
          this.error = this.common.getErrorMessage(err, 'Failed to delete account.');
        }
      });
  }

  private animateDialogIn(): void {
    if (this.isAnimating) return;
    this.isAnimating = true;

    const backdrop = this.dialogBackdropRef?.nativeElement;
    const panel = this.dialogPanelRef?.nativeElement;

    if (!backdrop || !panel) {
      this.isAnimating = false;
      return;
    }

    backdrop.opacity = 0;
    panel.opacity = 0;
    panel.scaleX = 0.96;
    panel.scaleY = 0.96;

    backdrop.animate({
      opacity: 0.6,
      duration: 200,
      curve: 'easeOut'
    });

    panel.animate({
      opacity: 1,
      scale: { x: 1, y: 1 },
      duration: 220,
      curve: 'easeOut'
    }).finally(() => {
      this.isAnimating = false;
    });
  }

  private animateDialogOut(): Promise<void> {
    if (this.isAnimating) return Promise.resolve();
    this.isAnimating = true;

    const backdrop = this.dialogBackdropRef?.nativeElement;
    const panel = this.dialogPanelRef?.nativeElement;

    if (!backdrop || !panel) {
      this.isAnimating = false;
      return Promise.resolve();
    }

    const p1 = backdrop.animate({
      opacity: 0,
      duration: 160,
      curve: 'easeIn'
    });

    const p2 = panel.animate({
      opacity: 0,
      scale: { x: 0.96, y: 0.96 },
      duration: 160,
      curve: 'easeIn'
    });

    return Promise
      .all([p1, p2])
      .then(() => undefined)
      .finally(() => {
        this.isAnimating = false;
      });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
