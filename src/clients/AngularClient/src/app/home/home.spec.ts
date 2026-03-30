import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Home } from './home';
import { MovieService } from '../movies/movie.service';
import { MovieSummaryCollection, MovieSummary } from '../movies/models/movie-summary';
import { of, throwError } from 'rxjs';

describe('Home', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;
  let movieService: any;

  const mockMovie: MovieSummary = {
    tmdbId: 1,
    title: 'Test Movie',
    releaseDate: '2024-01-01',
    posterPath: '/poster.jpg',
    formats: [],
    digitalRetailers: [],
  };

  const mockCollection: MovieSummaryCollection = {
    movies: [mockMovie],
    totalDvdCount: 2,
    totalBluRayCount: 5,
    totalBluRay4KCount: 3,
    totalDigitalCount: 4,
    page: 1,
    totalPages: 2,
    totalResults: 14,
  };

  beforeEach(async () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key: string) => {
      if (key === 'auth_user_id') return 'user-123';
      return null;
    });

    const movieServiceSpy = {
      getUserMovies: vi.fn().mockReturnValue(of(mockCollection)),
    };

    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MovieService, useValue: movieServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;
    movieService = TestBed.inject(MovieService);
  });

  afterEach(() => vi.restoreAllMocks());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should fetch user movies on init', () => {
      component.ngOnInit();

      expect(movieService.getUserMovies).toHaveBeenCalledWith('user-123', 1, 20);
    });

    it('should set moviesSummaryCollection after fetch', () => {
      component.ngOnInit();

      expect(component.moviesSummaryCollection()).toEqual(mockCollection);
    });

    it('should populate movies array', () => {
      component.ngOnInit();

      expect(component.movies().length).toBe(1);
      expect(component.movies()[0].title).toBe('Test Movie');
    });

    it('should set currentPage from response', () => {
      component.ngOnInit();

      expect(component.currentPage()).toBe(1);
    });

    it('should set totalPages from response', () => {
      component.ngOnInit();

      expect(component.totalPages()).toBe(2);
    });
  });

  describe('hasMore', () => {
    it('should be true when more pages exist', () => {
      component.ngOnInit();

      expect(component.hasMore()).toBe(true);
    });

    it('should be false when on last page', () => {
      movieService.getUserMovies = vi.fn().mockReturnValue(
        of({ ...mockCollection, page: 2, totalPages: 2 }),
      );

      component.ngOnInit();

      expect(component.hasMore()).toBe(false);
    });
  });

  describe('loadMore', () => {
    it('should increment page and fetch next page', () => {
      component.ngOnInit();

      component.loadMore();

      expect(movieService.getUserMovies).toHaveBeenCalledWith('user-123', 2, 20);
    });

    it('should append new movies to existing list', () => {
      component.ngOnInit();

      const page2Movie: MovieSummary = {
        tmdbId: 2,
        title: 'Second Movie',
        releaseDate: '2024-02-01',
        posterPath: null,
        formats: [],
        digitalRetailers: [],
      };
      movieService.getUserMovies = vi.fn().mockReturnValue(
        of({ ...mockCollection, movies: [page2Movie], page: 2 }),
      );

      component.loadMore();

      expect(component.movies().length).toBe(2);
      expect(component.movies()[1].title).toBe('Second Movie');
    });
  });

  describe('error handling', () => {
    it('should handle fetch error gracefully', () => {
      movieService.getUserMovies = vi.fn().mockReturnValue(
        throwError(() => new Error('Network error')),
      );

      // Should not throw
      expect(() => component.ngOnInit()).not.toThrow();
      expect(component.movies().length).toBe(0);
    });

    it('should set isError to true and isLoading to false on error', () => {
      movieService.getUserMovies = vi.fn().mockReturnValue(
        throwError(() => new Error('Network error')),
      );

      component.ngOnInit();

      expect(component.isError()).toBe(true);
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('isLoading', () => {
    it('should start as true', () => {
      expect(component.isLoading()).toBe(true);
    });

    it('should be false after successful fetch', () => {
      component.ngOnInit();

      expect(component.isLoading()).toBe(false);
    });
  });
});
