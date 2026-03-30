import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuthService, AuthResponse } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let store: Record<string, string>;

  function setupLocalStorage(initial: Record<string, string> = {}) {
    store = { ...initial };
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key: string) => store[key] ?? null);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation((key: string, value: string) => {
      store[key] = value;
    });
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation((key: string) => {
      delete store[key];
    });
  }

  function createService() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    return {
      service: TestBed.inject(AuthService),
      httpTesting: TestBed.inject(HttpTestingController),
    };
  }

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('when no token exists', () => {
    let service: AuthService;
    let httpTesting: HttpTestingController;

    beforeEach(() => {
      setupLocalStorage();
      ({ service, httpTesting } = createService());
    });

    afterEach(() => httpTesting.verify());

    it('should be created', () => {
      expect(service).toBeTruthy();
    });

    it('should initialize as not authenticated', () => {
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should initialize userId as null', () => {
      expect(service.userId()).toBeNull();
    });

    describe('login', () => {
      const mockResponse: AuthResponse = {
        userId: 'user-123',
        token: 'jwt-token-abc',
        expiration: new Date().toISOString(),
      };

      it('should POST to /auth/login with credentials', () => {
        service.login('test@example.com', 'password123').subscribe();

        const req = httpTesting.expectOne(`${environment.apiUrl}/auth/login`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ email: 'test@example.com', password: 'password123' });
        req.flush(mockResponse);
      });

      it('should store token in localStorage on success', () => {
        service.login('test@example.com', 'password123').subscribe();

        httpTesting.expectOne(`${environment.apiUrl}/auth/login`).flush(mockResponse);

        expect(store['auth_token']).toBe('jwt-token-abc');
      });

      it('should store userId in localStorage on success', () => {
        service.login('test@example.com', 'password123').subscribe();

        httpTesting.expectOne(`${environment.apiUrl}/auth/login`).flush(mockResponse);

        expect(store['auth_user_id']).toBe('user-123');
      });

      it('should set token expiration to approximately 1 hour from now', () => {
        const before = Date.now();
        service.login('test@example.com', 'password123').subscribe();

        httpTesting.expectOne(`${environment.apiUrl}/auth/login`).flush(mockResponse);

        const expiration = new Date(store['auth_token_expiration']).getTime();
        const oneHour = 60 * 60 * 1000;
        expect(expiration).toBeGreaterThanOrEqual(before + oneHour - 1000);
        expect(expiration).toBeLessThanOrEqual(Date.now() + oneHour + 1000);
      });

      it('should set isAuthenticated to true on success', () => {
        service.login('test@example.com', 'password123').subscribe();

        httpTesting.expectOne(`${environment.apiUrl}/auth/login`).flush(mockResponse);

        expect(service.isAuthenticated()).toBe(true);
      });

      it('should set userId signal on success', () => {
        service.login('test@example.com', 'password123').subscribe();

        httpTesting.expectOne(`${environment.apiUrl}/auth/login`).flush(mockResponse);

        expect(service.userId()).toBe('user-123');
      });
    });

    describe('register', () => {
      const mockResponse: AuthResponse = {
        userId: 'user-456',
        token: 'jwt-token-def',
        expiration: new Date().toISOString(),
      };

      it('should POST to /auth/create with credentials', () => {
        service.register('new@example.com', 'password123').subscribe();

        const req = httpTesting.expectOne(`${environment.apiUrl}/auth/create`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ email: 'new@example.com', password: 'password123' });
        req.flush(mockResponse);
      });

      it('should store session and update signals on success', () => {
        service.register('new@example.com', 'password123').subscribe();

        httpTesting.expectOne(`${environment.apiUrl}/auth/create`).flush(mockResponse);

        expect(store['auth_token']).toBe('jwt-token-def');
        expect(store['auth_user_id']).toBe('user-456');
        expect(service.isAuthenticated()).toBe(true);
        expect(service.userId()).toBe('user-456');
      });
    });

    describe('logout', () => {
      beforeEach(() => {
        store['auth_token'] = 'some-token';
        store['auth_user_id'] = 'user-123';
        store['redirectUrl'] = '/search';

        Object.defineProperty(window, 'location', {
          writable: true,
          configurable: true,
          value: { href: '' },
        });
      });

      afterEach(() => {
        Object.defineProperty(window, 'location', {
          writable: true,
          configurable: true,
          value: globalThis.window.location,
        });
      });

      it('should remove auth_token from localStorage', () => {
        service.logout();
        expect(store['auth_token']).toBeUndefined();
      });

      it('should remove auth_user_id from localStorage', () => {
        service.logout();
        expect(store['auth_user_id']).toBeUndefined();
      });

      it('should remove redirectUrl from localStorage', () => {
        service.logout();
        expect(store['redirectUrl']).toBeUndefined();
      });

      it('should set isAuthenticated to false', () => {
        service.logout();
        expect(service.isAuthenticated()).toBe(false);
      });

      it('should set userId to null', () => {
        service.logout();
        expect(service.userId()).toBeNull();
      });

      it('should redirect to /login', () => {
        service.logout();
        expect(window.location.href).toBe('/login');
      });
    });

    describe('getToken', () => {
      it('should return null when no token exists', () => {
        expect(service.getToken()).toBeNull();
      });

      it('should return token from localStorage', () => {
        store['auth_token'] = 'my-token';
        expect(service.getToken()).toBe('my-token');
      });
    });
  });

  describe('when valid token exists', () => {
    let service: AuthService;

    beforeEach(() => {
      const futureDate = new Date();
      futureDate.setHours(futureDate.getHours() + 1);
      setupLocalStorage({
        auth_token: 'existing-token',
        auth_token_expiration: futureDate.toISOString(),
        auth_user_id: 'existing-user',
      });
      ({ service } = createService());
    });

    it('should initialize as authenticated', () => {
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should initialize userId from localStorage', () => {
      expect(service.userId()).toBe('existing-user');
    });
  });

  describe('when expired token exists', () => {
    let service: AuthService;

    beforeEach(() => {
      const pastDate = new Date();
      pastDate.setHours(pastDate.getHours() - 1);
      setupLocalStorage({
        auth_token: 'expired-token',
        auth_token_expiration: pastDate.toISOString(),
        auth_user_id: 'old-user',
      });
      ({ service } = createService());
    });

    it('should initialize as not authenticated', () => {
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('when token exists but no expiration', () => {
    let service: AuthService;

    beforeEach(() => {
      setupLocalStorage({
        auth_token: 'token-no-expiry',
      });
      ({ service } = createService());
    });

    it('should initialize as not authenticated', () => {
      expect(service.isAuthenticated()).toBe(false);
    });
  });
});
