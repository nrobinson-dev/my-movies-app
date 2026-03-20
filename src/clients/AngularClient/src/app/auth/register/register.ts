import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../auth.service';
import { RouterLink } from '@angular/router';
import { EMAIL_REGEX } from '../../shared/constants/constants';

@Component({
  selector: 'register',
  imports: [RouterLink],
  template: `
    <h2 class="text-center text-2xl mb-4">Create Account</h2>
    <div class="flex justify-center">
      <form
        (submit)="register()"
        class="flex flex-col justify-stretch w-96 p-6 bg-white rounded-lg shadow-md border border-gray-300"
      >
        <label for="email">Email:</label>
        <input id="email" type="email" required autocomplete="email username" class="border rounded p-1 mb-4" (input)="setEmail($event)"/>

        <label for="password">Password:</label>
        <input
          id="password"
          type="password"
          required
          autocomplete="new-password"
          class="border rounded p-1 mb-4"
          minlength="8"
          (input)="setPassword($event)"
        />

        <button
          type="submit"
          [disabled]="!isFormValid() || isProcessing()"
          class="{{
            !isFormValid() || isProcessing() ? 'opacity-50 cursor-not-allowed' : 'hover:bg-blue-600 cursor-pointer'
          }} transition mt-4 bg-blue-500 text-white px-4 py-2 rounded-md"
        >
          Register
        </button>
        @if (registerError()) {
          <p class="text-center mt-4 text-red-500">Registration failed. Please try again.</p>
        }
      </form>
    </div>
    <p class="text-center mt-4">
      Already have an account?
      <a routerLink="/login" class="text-blue-500 hover:underline">Login</a>
    </p>
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
