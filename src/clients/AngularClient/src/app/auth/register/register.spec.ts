import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Register } from './register';
import { AuthService } from '../auth.service';
import { of, throwError } from 'rxjs';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
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
      register: vi.fn().mockReturnValue(of({ userId: 'u1', token: 't1', expiration: '' })),
      isAuthenticated: vi.fn().mockReturnValue(false),
    };

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
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

    it('should be valid with proper email and password', () => {
      component.email = 'new@example.com';
      component.password = 'password123';
      component.validateForm();

      expect(component.isFormValid()).toBe(true);
    });

    it('should be invalid with bad email', () => {
      component.email = 'not-email';
      component.password = 'password123';
      component.validateForm();

      expect(component.isFormValid()).toBe(false);
    });

    it('should be invalid with short password', () => {
      component.email = 'new@example.com';
      component.password = 'short';
      component.validateForm();

      expect(component.isFormValid()).toBe(false);
    });
  });

  describe('register', () => {
    it('should not register when form is invalid', () => {
      component.isFormValid.set(false);

      component.register();

      expect(authService.register).not.toHaveBeenCalled();
    });

    it('should not register when already processing', () => {
      component.email = 'new@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);
      component.isProcessing.set(true);

      component.register();

      expect(authService.register).not.toHaveBeenCalled();
    });

    it('should call authService.register with trimmed credentials', () => {
      component.email = '  new@example.com  ';
      component.password = '  password123  ';
      component.isFormValid.set(true);

      component.register();

      expect(authService.register).toHaveBeenCalledWith('new@example.com', 'password123');
    });

    it('should redirect to stored redirectUrl on success', () => {
      store['redirectUrl'] = '/search';
      component.email = 'new@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.register();

      expect(window.location.href).toBe('/search');
    });

    it('should redirect to / when no redirectUrl exists', () => {
      component.email = 'new@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.register();

      expect(window.location.href).toBe('/');
    });

    it('should set registerError on failure', () => {
      authService.register = vi.fn().mockReturnValue(
        throwError(() => new Error('Registration failed')),
      );
      component.email = 'new@example.com';
      component.password = 'password123';
      component.isFormValid.set(true);

      component.register();

      expect(component.registerError()).toBe(true);
      expect(component.isProcessing()).toBe(false);
    });
  });
});
