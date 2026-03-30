import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, ActivatedRoute, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Search } from './search';
import { MovieService } from '../movie.service';
import { MovieSummaryCollection } from '../models/movie-summary';
import { of, throwError } from 'rxjs';

describe('Search', () => {
  let component: Search;
  let fixture: ComponentFixture<Search>;
  let movieService: any;
  let router: Router;

  const mockResults: MovieSummaryCollection = {
    movies: [
      {
        tmdbId: 550,
        title: 'Fight Club',
        releaseDate: '1999-10-15',
        posterPath: '/fight.jpg',
        formats: [],
        digitalRetailers: [],
      },
    ],
    totalDvdCount: 0,
    totalBluRayCount: 0,
    totalBluRay4KCount: 0,
    totalDigitalCount: 0,
    page: 1,
    totalPages: 3,
    totalResults: 25,
  };

  function setup(queryParam: string | null = null) {
    const movieServiceSpy = {
      getSearchResults: vi.fn().mockReturnValue(of(mockResults)),
    };

    TestBed.configureTestingModule({
      imports: [Search],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MovieService, useValue: movieServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: { get: (key: string) => (key === 'q' ? queryParam : null) },
            },
          },
        },
      ],
    });

    fixture = TestBed.createComponent(Search);
    component = fixture.componentInstance;
    movieService = TestBed.inject(MovieService);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  }

  afterEach(() => vi.restoreAllMocks());

  describe('initialization', () => {
    it('should create', () => {
      setup();
      expect(component).toBeTruthy();
    });

    it('should auto-search when ?q= param exists', () => {
      setup('Fight Club');

      component.ngOnInit();

      expect(component.searchQuery()).toBe('Fight Club');
      expect(movieService.getSearchResults).toHaveBeenCalledWith('Fight Club', 1);
    });

    it('should not search when no query param exists', () => {
      setup(null);

      component.ngOnInit();

      expect(movieService.getSearchResults).not.toHaveBeenCalled();
    });
  });

  describe('setSearchQuery', () => {
    it('should update search query from input event', () => {
      setup();

      const event = { target: { value: '  Matrix  ' } } as any;
      component.setSearchQuery(event);

      expect(component.searchQuery()).toBe('Matrix');
    });
  });

  describe('performSearch', () => {
    beforeEach(() => setup());

    it('should not search when query is empty', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('');

      component.performSearch(event);

      expect(movieService.getSearchResults).not.toHaveBeenCalled();
    });

    it('should prevent default form submission', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');

      component.performSearch(event);

      expect(event.preventDefault).toHaveBeenCalled();
    });

    it('should call getSearchResults with query', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');

      component.performSearch(event);

      expect(movieService.getSearchResults).toHaveBeenCalledWith('Matrix', 1);
    });

    it('should reset movies and page on new search', () => {
      component.movies.set([
        { tmdbId: 1, title: 'Old', releaseDate: '', posterPath: null, formats: [], digitalRetailers: [] },
      ]);
      component.currentPage.set(5);

      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');
      component.performSearch(event);

      // Movies were reset before search, then populated with results
      expect(component.currentPage()).toBe(mockResults.page);
    });

    it('should update URL query params', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');

      component.performSearch(event);

      expect(router.navigate).toHaveBeenCalledWith([], {
        queryParams: { q: 'Matrix' },
        replaceUrl: true,
      });
    });

    it('should set isSearched to true after results', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');

      component.performSearch(event);

      expect(component.isSearched()).toBe(true);
    });

    it('should set isSearching to false after results', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');

      component.performSearch(event);

      expect(component.isSearching()).toBe(false);
    });

    it('should populate movies signal with results', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');

      component.performSearch(event);

      expect(component.movies().length).toBe(1);
      expect(component.movies()[0].title).toBe('Fight Club');
    });

    it('should set totalPages from response', () => {
      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');

      component.performSearch(event);

      expect(component.totalPages()).toBe(3);
    });
  });

  describe('error handling', () => {
    it('should set isError on search failure', () => {
      setup();
      movieService.getSearchResults = vi.fn().mockReturnValue(
        throwError(() => new Error('Network error')),
      );

      const event = { preventDefault: vi.fn() } as any;
      component.searchQuery.set('Matrix');
      component.performSearch(event);

      expect(component.isError()).toBe(true);
      expect(component.isSearched()).toBe(false);
      expect(component.isSearching()).toBe(false);
    });
  });

  describe('loadMore', () => {
    it('should increment page and fetch next page', () => {
      setup();
      component.searchQuery.set('Matrix');
      component.currentPage.set(1);

      component.loadMore();

      expect(movieService.getSearchResults).toHaveBeenCalledWith('Matrix', 2);
    });

    it('should append results to existing movies', () => {
      setup();
      component.movies.set([
        { tmdbId: 1, title: 'Existing', releaseDate: '', posterPath: null, formats: [], digitalRetailers: [] },
      ]);
      component.searchQuery.set('Matrix');

      component.loadMore();

      expect(component.movies().length).toBe(2);
    });
  });

  describe('hasMore', () => {
    it('should return true when currentPage < totalPages', () => {
      setup();
      component.currentPage.set(1);
      component.totalPages.set(3);

      expect(component.hasMore()).toBe(true);
    });

    it('should return false when currentPage equals totalPages', () => {
      setup();
      component.currentPage.set(3);
      component.totalPages.set(3);

      expect(component.hasMore()).toBe(false);
    });
  });
});
