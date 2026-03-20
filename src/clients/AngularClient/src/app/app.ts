import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLinkActive, RouterLink } from '@angular/router';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLinkActive, RouterLink],
  template: `
    <header class="bg-gray-300 fixed top-0 left-0 right-0 z-50 shadow-md">
      <nav class="container flex flex-row items-center mx-auto px-4 align-items-stretch ">
        <img src="/images/angular-icon.svg" alt="Angular Logo" class="h-10 mr-4 my-2" />

        @if(isAuthenticated()) {
          <a
            routerLink="/"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{exact: true}"
            class="block transition border-b-2 border-l-1 border-r-1 border-l-white border-r-white border-b-transparent p-4 hover:bg-blue-900 hover:text-white"
            >My Movies</a
          >
          <a
            routerLink="/search"
            routerLinkActive="active"
            class="block transition border-b-2 border-l-1 border-r-1 border-l-white border-r-white border-b-transparent p-4 hover:bg-blue-900 hover:text-white"
            >Search</a
          >
          <button (click)="auth.logout()" class="ml-auto px-2 hover:underline cursor-pointer">Logout</button>
        }
        @else {
          <span class="flex-grow text-center p-2 text-2xl text-gray-700 mr-4 ">Welcome to My Movies App!</span>
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
