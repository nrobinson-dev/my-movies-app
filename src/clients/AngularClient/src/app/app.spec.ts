import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';
import { AuthService } from './auth/auth.service';

describe('App', () => {
  let authService: AuthService;

  beforeEach(async () => {
    vi.spyOn(Storage.prototype, 'getItem').mockReturnValue(null);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {});
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation(() => {});

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    authService = TestBed.inject(AuthService);
  });

  afterEach(() => vi.restoreAllMocks());

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should show welcome title when not authenticated', async () => {
    authService.isAuthenticated.set(false);

    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('h1')?.textContent).toContain('Welcome to My Movies App');
  });

  it('should show nav links when authenticated', async () => {
    authService.isAuthenticated.set(true);

    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('h1')).toBeNull();
    expect(compiled.querySelector('a[href="/"]')).toBeTruthy();
    expect(compiled.querySelector('a[href="/search"]')).toBeTruthy();
  });

  it('should expose isAuthenticated from AuthService', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance.isAuthenticated).toBe(authService.isAuthenticated);
  });
});
