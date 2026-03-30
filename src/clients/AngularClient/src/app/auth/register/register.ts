import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../auth.service';
import { RouterLink } from '@angular/router';
import { EMAIL_REGEX } from '../../shared/constants/constants';

@Component({
  selector: 'register',
  imports: [RouterLink],
  template: `
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
          [attr.aria-invalid]="emailTouched() && !isEmailValid()"
          (input)="setEmail($event)"
        />

        <label for="password">Password:</label>
        <input
          id="password"
          type="password"
          required
          autocomplete="new-password"
          class="auth-form__field"
          minlength="8"
          aria-required="true"
          aria-describedby="password-hint"
          [attr.aria-invalid]="passwordTouched() && !isPasswordValid()"
          (input)="setPassword($event)"
        />
        <p id="password-hint" class="auth-form__hint">Must be at least 8 characters</p>

        <button
          type="submit"
          [disabled]="!isFormValid() || isProcessing()"
          class="{{
            !isFormValid() || isProcessing() ? 'auth-form__submit--disabled' : 'auth-form__submit--enabled'
          }} auth-form__submit"
        >
          Register
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
  email = '';
  password = '';
  isProcessing = signal(false);
  isFormValid = signal(false);
  registerError = signal(false);
  emailTouched = signal(false);
  passwordTouched = signal(false);

  setEmail(event: Event) {
    this.emailTouched.set(true);
    this.email = (event.target as HTMLInputElement).value;
    this.validateForm();
  }

  setPassword(event: Event) {
    this.passwordTouched.set(true);
    this.password = (event.target as HTMLInputElement).value;
    this.validateForm();
  }

  isEmailValid() {
    return EMAIL_REGEX.test(this.email.trim());
  }
  
  isPasswordValid() {
    return this.password.trim().length >= 8;
  }
  
  validateForm() {
    this.isFormValid.set(this.isEmailValid() && this.isPasswordValid());
  }

  register(event?: Event) {
    event?.preventDefault();
    if (!this.isFormValid() || this.isProcessing()) {
      return;
    }

    this.email = this.email.trim();
    this.password = this.password.trim();

    this.isProcessing.set(true);
    this.authService.register(this.email, this.password).subscribe({
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
