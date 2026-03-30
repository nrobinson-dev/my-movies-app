import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLinkActive, RouterLink } from '@angular/router';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLinkActive, RouterLink],
  template: `
    <a
      href="#main-content"
      class="sr-only focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-50 focus:px-4 focus:py-2 focus:bg-white focus:text-black focus:shadow-lg focus:rounded"
    >
      Skip to main content
    </a>

    <header>
      <nav aria-label="Main navigation">
        <img src="/images/angular-icon.svg" alt="Angular Logo" class="h-10 mr-4 my-2" />

        @if (isAuthenticated()) {
          <a
            routerLink="/"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{ exact: true }"
            class="nav-link"
            >My Movies</a
          >
          <a routerLink="/search" routerLinkActive="active" class="nav-link">Search</a>
          <button (click)="auth.logout()" class="logout-link">Logout</button>
        } @else {
          <h1 class="welcome-title">Welcome to My Movies App!</h1>
        }
      </nav>
    </header>

    <div class="main-wrapper">
      <main id="main-content">
        <router-outlet />
      </main>

      <footer>
        <p>
          This website uses TMDB and the TMDB APIs but is not endorsed, certified, or otherwise
          approved by TMDB.
        </p>
      </footer>
    </div>
  `,
  styleUrls: ['./app.css'],
})
export class App {
  auth = inject(AuthService);
  isAuthenticated = this.auth.isAuthenticated;
}
