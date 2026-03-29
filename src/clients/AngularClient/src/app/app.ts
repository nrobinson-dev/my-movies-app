import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLinkActive, RouterLink } from '@angular/router';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLinkActive, RouterLink],
  template: `
    <header>
      <nav>
        <img src="/images/angular-icon.svg" alt="Angular Logo" class="h-10 mr-4 my-2" />

        @if(isAuthenticated()) {
          <a
            routerLink="/"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{exact: true}"
            class="nav-link"
            tabindex="1"
            >My Movies</a
          >
          <a
            routerLink="/search"
            routerLinkActive="active"
            class="nav-link"
            tabindex="2"
            >Search</a
          >
          <button (click)="auth.logout()" class="logout-link" tabindex="3">Logout</button>
        }
        @else {
          <h1 class="welcome-title">Welcome to My Movies App!</h1>
        }
      </nav>
    </header>

    <router-outlet class="grow" />
  `,
  styleUrls: ['./app.css'],
})
export class App {
  auth = inject(AuthService);
  isAuthenticated = this.auth.isAuthenticated;
}
