import { Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../auth.service';
import { RouterLink } from '@angular/router';
import { EMAIL_REGEX } from '../../shared/constants/constants';

@Component({
  selector: 'register',
  imports: [RouterLink],
  template: `
  <div class="logo-wrapper logo-wrapper--large">
    <div class="logo"></div>
  </div>
  <div class="auth-wrapper">

    <h2 id="register-title" class="page-title">Create Account</h2>
    <div class="flex justify-center">
      <form
        (submit)="register($event)"
        class="auth-form"
        novalidate
        aria-labelledby="register-title"
      >
        <label for="email">Email:</label>
        <input id="email" 
          type="email" 
          required 
          autocomplete="email username" 
          class="auth-form__field" 
          aria-required="true"
          maxlength="254"
          [attr.aria-invalid]="emailTouched() && !isEmailValid()"
          (input)="setEmail($event)"
        />

        <div class="password-container flex flex-col">
          <label for="password">Password:</label>
          <div class="input-wrapper flex items-start">
            <input
              id="password"
              [type]="isPasswordVisible() ? 'text' : 'password'"
              required
              autocomplete="new-password"
              class="auth-form__field grow"
              minlength="8"
              aria-required="true"
              aria-describedby="password-criteria"
              [attr.aria-invalid]="passwordTouched() && !isPasswordValid()"
              (input)="setPassword($event)"
            />
            <button 
              type="button" 
              (click)="togglePasswordVisibility()"
              aria-label="Password visibility toggler."
              class="btn-password-toggler button--gold">
              {{ isPasswordVisible() ? 'Hide' : 'Show' }}
            </button>
          </div>
        </div>

        <div id="password-criteria">
          <p><small id="password-criteria-uppercase" [class]="passwordUppercaseHintClass()" class="auth-form__hint">Contains an uppercase letter.</small></p>
          <p><small id="password-criteria-lowercase" [class]="passwordLowercaseHintClass()" class="auth-form__hint">Contains a lowercase letter.</small></p>
          <p><small id="password-criteria-number" [class]="passwordNumberHintClass()" class="auth-form__hint">Contains a number.</small></p>
          <p><small id="password-criteria-length" [class]="passwordMinCharacterHintClass()" class="auth-form__hint">Has at least 8 characters.</small></p>
        </div>

        <button
          type="submit"
          [disabled]="!isFormValid() || isProcessing()"
          class="{{
            !isFormValid() || isProcessing() ? 'auth-form__submit--disabled' : 'auth-form__submit--enabled'
          }} auth-form__submit button--gold"
        >
        @if (isProcessing()) {
          Registerring...
        } @else {
          Register
        }
        </button>
        @if (registerError()) {
          <p id="register-error" role="alert" class="auth-form__error-message">Registration failed. Please try again.</p>
        }
      </form>
    </div>
    <p class="text-center mt-4">
      Already have an account?
      <a routerLink="/login" class="link">Login</a>
    </p>
  </div>
  `,
  styleUrls: ['./register.css'],
})
export class Register {
  authService = inject(AuthService);
  email = signal('');
  password = signal('');

  isProcessing = signal(false);
  registerError = signal(false);
  emailTouched = signal(false);
  passwordTouched = signal(false);
  isPasswordVisible = signal(false);

  isEmailValid = computed(() => EMAIL_REGEX.test(this.email().trim()));

  passwordCriteria = computed(() => {
    const p = this.password().trim();
    return {
      hasMinLength: p.length >= 8,
      hasUppercase: /[A-Z]/.test(p),
      hasLowercase: /[a-z]/.test(p),
      hasNumber: /[0-9]/.test(p),
    };
  });

  togglePasswordVisibility() {
    this.isPasswordVisible.update(v => !v);
  }

  isPasswordValid = computed(() => {
    const c = this.passwordCriteria();
    return c.hasMinLength && c.hasUppercase && c.hasLowercase && c.hasNumber;
  });

  isFormValid = computed(() => this.isEmailValid() && this.isPasswordValid());

  passwordMinCharacterHintClass = computed(() => 
    !this.passwordTouched() ? '' : 
    this.passwordCriteria().hasMinLength ? 'auth-form__hint--valid' : 'auth-form__hint--invalid'
  );
  passwordUppercaseHintClass = computed(() => 
    !this.passwordTouched() ? '' : 
    this.passwordCriteria().hasUppercase ? 'auth-form__hint--valid' : 'auth-form__hint--invalid'
  );
  passwordLowercaseHintClass = computed(() => 
    !this.passwordTouched() ? '' : 
    this.passwordCriteria().hasLowercase ? 'auth-form__hint--valid' : 'auth-form__hint--invalid'
  );
  passwordNumberHintClass = computed(() => 
    !this.passwordTouched() ? '' : 
    this.passwordCriteria().hasNumber ? 'auth-form__hint--valid' : 'auth-form__hint--invalid'
  );

  setEmail(event: Event) {
    this.emailTouched.set(true);
    this.email.set((event.target as HTMLInputElement).value);
  }

  setPassword(event: Event) {
    this.passwordTouched.set(true);
    this.password.set((event.target as HTMLInputElement).value);
  }


  register(event?: Event) {
    event?.preventDefault();
    if (!this.isFormValid() || this.isProcessing()) {
      return;
    }

    this.isProcessing.set(true);
    this.authService.register(this.email().trim(), this.password().trim()).subscribe({
      next: () => {
        const redirectUrl = localStorage.getItem('redirectUrl') || '/';
        localStorage.removeItem('redirectUrl');

        window.location.href = redirectUrl;
      },
      error: (err) => {
        console.error('Registration failed', err);
        this.registerError.set(true);
        this.isProcessing.set(false);
      },
    });
  }
}
