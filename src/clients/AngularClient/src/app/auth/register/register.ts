import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../auth.service';
import { RouterLink } from '@angular/router';
import { EMAIL_REGEX } from '../../shared/constants/constants';

@Component({
  selector: 'register',
  imports: [RouterLink],
  template: `
  <div class="auth-wrapper">
    <h2 class="page-title">Create Account</h2>
    <div class="flex justify-center">
      <form
        (submit)="register()"
        class="auth-form"
      >
        <label for="email">Email:</label>
        <input id="email" 
          type="email" 
          required 
          autocomplete="email username" 
          class="auth-form__field" 
          (input)="setEmail($event)"
          tabindex="1"
        />

        <label for="password">Password:</label>
        <input
          id="password"
          type="password"
          required
          autocomplete="new-password"
          class="auth-form__field"
          minlength="8"
          tabindex="2"
          (input)="setPassword($event)"
        />

        <button
          type="submit"
          [disabled]="!isFormValid() || isProcessing()"
          tabindex="3"
          class="{{
            !isFormValid() || isProcessing() ? 'auth-form__submit--disabled' : 'auth-form__submit--enabled'
          }} auth-form__submit"
        >
          Register
        </button>
        @if (registerError()) {
          <p class="auth-form__error-message">Registration failed. Please try again.</p>
        }
      </form>
    </div>
    <p class="text-center mt-4">
      Already have an account?
      <a routerLink="/login" tabindex="4" class="link">Login</a>
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

  setEmail(event: Event) {
    this.email = (event.target as HTMLInputElement).value;
    this.validateForm();
  }

  setPassword(event: Event) {
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

  register() {
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
