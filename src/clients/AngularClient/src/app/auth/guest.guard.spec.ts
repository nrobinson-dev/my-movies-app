import { TestBed } from '@angular/core/testing';
import { UrlTree } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { guestGuard } from './guest.guard';
import { AuthService } from './auth.service';

describe('guestGuard', () => {
  let authService: AuthService;

  beforeEach(() => {
    vi.spyOn(Storage.prototype, 'getItem').mockReturnValue(null);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {});
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation(() => {});

    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => vi.restoreAllMocks());

  function runGuard(): boolean | UrlTree {
    const routeSnapshot = {} as any;
    const stateSnapshot = { url: '/login' } as any;
    return TestBed.runInInjectionContext(() => guestGuard(routeSnapshot, stateSnapshot)) as
      | boolean
      | UrlTree;
  }

  it('should allow access when user is NOT authenticated', () => {
    authService.isAuthenticated.set(false);

    expect(runGuard()).toBe(true);
  });

  it('should redirect to / when user IS authenticated', () => {
    authService.isAuthenticated.set(true);

    const result = runGuard();

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/');
  });
});
