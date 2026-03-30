import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Login } from './login';
import { AuthService } from '../auth.service';
import { of, throwError } from 'rxjs';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let authService: any;
  let store: Record<string, string>;

  beforeEach(async () => {
    store = {};
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key: string) => store[key] ?? null);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation((key, val) => {
      store[key] = val;
    });
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation((key) => {
      delete store[key];
    });

    Object.defineProperty(window, 'location', {
      writable: true,
      configurable: true,
      value: { href: '' },
    });

    const authServiceSpy = {
      login: vi.fn().mockReturnValue(of({ userId: 'u1', token: 't1', expiration: '' })),
      isAuthenticated: vi.fn().mockReturnValue(false),
    };

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => vi.restoreAllMocks());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('form validation', () => {
    it('should start with form invalid', () => {
      expect(component.isFormValid()).toBe(false);
    });

    it('should be invalid with invalid email', () => {
      component.email = 'not-an-email';
      component.password = 'password123';
      component.validateForm();

      expect(component.isFormValid()).toBe(false);
    });

    it('should be invalid with short password', () => {
      component.email = 'test@example.com';
      component.password = 'short';
      component.validateForm();

      expect(component.isFormValid()).toBe(false);
    });

    it('should be valid with proper email and password', () => {
      component.email = 'test@example.com';
      component.password = 'password123';
      component.validateForm();

      expect(component.isFormValid()).toBe(true);
    });

    it('should validate on email input', () => {
      component.password = 'password123';
      component.setEmail({ target: { value: 'test@example.com' } } as any);

      expect(component.isFormValid()).toBe(true);
    });

    it('should validate on password input', () => {
      component.email = 'test@example.com';
      component.setPassword({ target: { value: 'password123' } } as any);

      expect(component.isFormValid()).toBe(true);
    });
  });

  describe('isEmailValid', () => {
    it('should accept valid email', () => {
      component.email = 'user@domain.com';
      expect(component.isEmailValid()).toBe(true);
    });

    it('should reject email without @', () => {
      component.email = 'invalid';
      expect(component.isEmailValid()).toBe(false);
    });

    it('should reject email without domain', () => {
      component.email = 'user@';
      expect(component.isEmailValid()).toBe(false);
    });

    it('should trim whitespace before validating', () => {
      component.email = '  user@domain.com  ';
      expect(component.isEmailValid()).toBe(true);
    });
  });

  describe('isPasswordValid', () => {
    it('should accept password with 8+ characters', () => {
      component.password = '12345678';
      expect(component.isPasswordValid()).toBe(true);
    });

    it('should reject password shorter than 8 characters', () => {
      component.password = '1234567';
      expect(component.isPasswordValid()).toBe(false);
    });

    it('should trim whitespace before validating', () => {
      component.password = '   short   ';
      expect(component.isPasswordValid()).toBe(false);
    });
  });

  describe('login', () => {
    const event = { preventDefault: vi.fn() } as any;

    it('should not login when form is invalid', () => {
      component.isFormValid.set(false);

      component.login(event);

      expect(authService.login).not.toHaveBeenCalled();
    });

    it('should not login when already processing', () => {
      component.email = 'test@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);
      component.isProcessing.set(true);

      component.login(event);

      expect(authService.login).not.toHaveBeenCalled();
    });

    it('should call authService.login with trimmed credentials', () => {
      component.email = '  test@example.com  ';
      component.password = '  password123  ';
      component.isFormValid.set(true);

      component.login(event);

      expect(authService.login).toHaveBeenCalledWith('test@example.com', 'password123');
    });

    it('should redirect to stored redirectUrl on success', () => {
      store['redirectUrl'] = '/search';
      component.email = 'test@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.login(event);

      expect(window.location.href).toBe('/search');
    });

    it('should remove redirectUrl from localStorage after use', () => {
      store['redirectUrl'] = '/search';
      component.email = 'test@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.login(event);

      expect(store['redirectUrl']).toBeUndefined();
    });

    it('should redirect to / when no redirectUrl exists', () => {
      component.email = 'test@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.login(event);

      expect(window.location.href).toBe('/');
    });

    it('should set loginError on failure', () => {
      authService.login = vi.fn().mockReturnValue(throwError(() => new Error('Bad creds')));
      component.email = 'test@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.login(event);

      expect(component.loginError()).toBe(true);
      expect(component.isProcessing()).toBe(false);
    });

    it('should prevent default form event', () => {
      component.email = 'test@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.login(event);

      expect(event.preventDefault).toHaveBeenCalled();
    });
  });
});
