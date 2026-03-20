import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AuthResponse {
  userId: string;
  token: string;
  expiration: string;
}

const TOKEN_KEY = 'auth_token';
const USER_ID_KEY = 'auth_user_id';
const TOKEN_EXPIRATION_KEY = 'auth_token_expiration';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);

  readonly isAuthenticated = signal(this.hasValidToken());
  readonly userId = signal<string | null>(localStorage.getItem(USER_ID_KEY));

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(
      tap({ next: response => this.storeSession(response) })
    );
  }

  register(email: string, password: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/create`, { email, password }).pipe(
      tap({ next: response => this.storeSession(response) })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_ID_KEY);
    localStorage.removeItem('redirectUrl');
    
    this.isAuthenticated.set(false);
    this.userId.set(null);

    window.location.href = "/login";
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_ID_KEY, response.userId);
    const expiration = new Date();
    expiration.setHours(expiration.getHours() + 1); // Token expires in 1 hour
    localStorage.setItem(TOKEN_EXPIRATION_KEY, expiration.toISOString());
    this.isAuthenticated.set(true);
    this.userId.set(response.userId);
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    const expiration = localStorage.getItem(TOKEN_EXPIRATION_KEY);
    
    if (!token || !expiration) {
      return false;
    }

    const expirationDate = new Date(expiration);
    return expirationDate > new Date();
  }
}
