import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    vi.spyOn(Storage.prototype, 'getItem').mockReturnValue(null);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {});
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation(() => {});

    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => vi.restoreAllMocks());

  function runGuard(url: string = '/home'): boolean | UrlTree {
    const routeSnapshot = {} as any;
    const stateSnapshot = { url } as any;
    return TestBed.runInInjectionContext(() => authGuard(routeSnapshot, stateSnapshot)) as
      | boolean
      | UrlTree;
  }

  it('should allow access when user is authenticated', () => {
    authService.isAuthenticated.set(true);

    const result = runGuard();

    expect(result).toBe(true);
  });

  it('should redirect to /login when user is not authenticated', () => {
    authService.isAuthenticated.set(false);

    const result = runGuard();

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/login');
  });

  it('should store the requested URL in localStorage for redirect after login', () => {
    authService.isAuthenticated.set(false);

    runGuard('/search');

    expect(localStorage.setItem).toHaveBeenCalledWith('redirectUrl', '/search');
  });
});
