import { Component, ElementRef, NO_ERRORS_SCHEMA, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { RegisterDto } from '../models/register.dto';
import { Page, Utils, View } from '@nativescript/core';
import { CommonService } from '../services/common.service';
import { PasswordStrengthService, PasswordStrengthState } from '../services/passwordstrength.service';

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

  showPassword = false;
  passwordStrength: PasswordStrengthState;

  showStrength = false;

  @ViewChild('strengthWrapper', { static: false }) strengthWrapperRef?: ElementRef<View>;
  @ViewChild('strengthFill', { static: false }) strengthFillRef?: ElementRef<View>;
  @ViewChild('checklist', { static: false }) checklistRef?: ElementRef<View>;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private page: Page,
    private common: CommonService,
    private passwordStrengthService: PasswordStrengthService
  ) {
    this.page.actionBarHidden = true;

    this.form = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });

    this.passwordStrength = this.passwordStrengthService.evaluate('');

    this.form.get('password')!.valueChanges.subscribe(value => {
      const text = (value ?? '').toString();
      const hasValue = text.length > 0;

      this.passwordStrength = this.passwordStrengthService.evaluate(text);

      setTimeout(() => {
        if (hasValue && !this.showStrength) {
          this.showStrength = true;
          setTimeout(() => {
            this.animateStrengthIn();
          }, 0);
          return;
        }

        if (!hasValue && this.showStrength) {
          this.animateStrengthOut();
          return;
        }

        if (hasValue && this.showStrength) {
          this.animateFill();
        }
      }, 0);
    });
  }

  private animateStrengthIn(): void {
    const wrapper = this.strengthWrapperRef?.nativeElement;
    const fill = this.strengthFillRef?.nativeElement;
    const checklist = this.checklistRef?.nativeElement;

    const targetScale = this.passwordStrength.percent / 100;

    if (wrapper) {
      wrapper.opacity = 0;
      wrapper.translateY = 4;
      wrapper.animate({
        opacity: 1,
        translate: { x: 0, y: 0 },
        duration: 180,
        curve: 'easeOut'
      });
    }

    if (fill) {
      fill.originX = 0;
      fill.scaleX = 0;
      fill.animate({
        scale: { x: targetScale, y: 1 },
        duration: 200,
        curve: 'easeOut'
      });
    }

    if (checklist) {
      checklist.opacity = 0;
      checklist.translateY = 6;
      checklist.animate({
        opacity: 1,
        translate: { x: 0, y: 0 },
        duration: 200,
        curve: 'easeOut'
      });
    }
  }

  private animateFill(): void {
    const fill = this.strengthFillRef?.nativeElement;
    if (!fill) return;

    const targetScale = this.passwordStrength.percent / 100;

    fill.originX = 0;
    fill.animate({
      scale: { x: targetScale, y: 1 },
      duration: 180,
      curve: 'easeOut'
    });
  }

  private animateStrengthOut(): void {
    const wrapper = this.strengthWrapperRef?.nativeElement;
    if (!wrapper) {
      this.showStrength = false;
      return;
    }

    wrapper.animate({
      opacity: 0,
      translate: { x: 0, y: 4 },
      duration: 140,
      curve: 'easeIn'
    }).then(() => {
      this.showStrength = false;
    }).catch(() => {
      this.showStrength = false;
    });
  }

  onSubmit() {
    Utils.dismissSoftInput();
    if (this.form.invalid || this.loading || this.passwordStrength.score < 5) {
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
        this.error = this.common.getErrorMessage(err, 'Registration failed');
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login'], {
      transition: {
        name: 'slideRight',
        duration: 220,
        curve: 'easeInOut'
      }
    } as any);
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}
