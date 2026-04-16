import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { MovieService } from './movie.service';
import { MovieDetail } from './models/movie-detail';
import { MovieSummaryCollection } from './models/movie-summary';
import { SaveMovieRequest } from './models/save-movie-request';
import { FormatId, DigitalRetailerId } from './models/lookup';
import { FORMAT_OPTIONS } from '../shared/constants/format-options';
import { DIGITAL_RETAILER_OPTIONS } from '../shared/constants/digital-retailer-options';
import { environment } from '../../environments/environment';

describe('MovieService', () => {
  let service: MovieService;
  let httpTesting: HttpTestingController;
  const baseUrl = environment.apiUrl;
  const userId = 'user-123';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(MovieService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getUserMovies', () => {
    const mockCollection: MovieSummaryCollection = {
      movies: [
        {
          tmdbId: 1,
          title: 'Test Movie',
          releaseDate: '2024-01-01',
          posterPath: '/poster.jpg',
          formats: [{ id: FormatId.BluRay, name: 'Blu-ray' }],
          digitalRetailers: [],
        },
      ],
      totalDvdCount: 0,
      totalBluRayCount: 1,
      totalBluRay4KCount: 0,
      totalDigitalCount: 0,
      page: 1,
      totalPages: 2,
      totalResults: 15,
    };

    it('should GET user movies with default pagination', () => {
      service.getUserMovies().subscribe();

      const req = httpTesting.expectOne(
        (r) => r.url === `${baseUrl}/users/me/movies`,
      );
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('10');
      req.flush(mockCollection);
    });

    it('should GET user movies with custom pagination', () => {
      service.getUserMovies(3, 25).subscribe();

      const req = httpTesting.expectOne(
        (r) => r.url === `${baseUrl}/users/me/movies`,
      );
      expect(req.request.params.get('page')).toBe('3');
      expect(req.request.params.get('pageSize')).toBe('25');
      req.flush(mockCollection);
    });

    it('should return the movie collection', () => {
      let result: MovieSummaryCollection | undefined;
      service.getUserMovies().subscribe((r) => (result = r));

      httpTesting
        .expectOne((r) => r.url === `${baseUrl}/users/me/movies`)
        .flush(mockCollection);

      expect(result).toEqual(mockCollection);
    });
  });

  describe('getSearchResults', () => {
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
      totalPages: 1,
      totalResults: 1,
    };

    it('should GET search results with query and default page', () => {
      service.getSearchResults('Fight Club').subscribe();

      const req = httpTesting.expectOne((r) => r.url === `${baseUrl}/movies`);
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('search')).toBe('Fight Club');
      expect(req.request.params.get('page')).toBe('1');
      req.flush(mockResults);
    });

    it('should GET search results with custom page', () => {
      service.getSearchResults('Fight Club', 2).subscribe();

      const req = httpTesting.expectOne((r) => r.url === `${baseUrl}/movies`);
      expect(req.request.params.get('page')).toBe('2');
      req.flush(mockResults);
    });

    it('should return search results', () => {
      let result: MovieSummaryCollection | undefined;
      service.getSearchResults('Fight Club').subscribe((r) => (result = r));

      httpTesting.expectOne((r) => r.url === `${baseUrl}/movies`).flush(mockResults);

      expect(result).toEqual(mockResults);
    });
  });

  describe('getMovieById', () => {
    const mockDetail: MovieDetail = {
      tmdbId: 550,
      title: 'Fight Club',
      releaseDate: '1999-10-15',
      posterPath: '/fight.jpg',
      backdropPath: '/fight-bg.jpg',
      runtime: 139,
      tagline: 'Mischief. Mayhem. Soap.',
      overview: 'A ticking-Loss bomb insomniac...',
      formats: [{ id: FormatId.BluRay, name: 'Blu-ray' }],
      digitalRetailers: [{ id: DigitalRetailerId.AppleTv, name: 'Apple TV' }],
    };

    it('should GET movie detail by userId and movieId', () => {
      service.getMovieById('550').subscribe();

      const req = httpTesting.expectOne(`${baseUrl}/movies/550`);
      expect(req.request.method).toBe('GET');
      req.flush(mockDetail);
    });

    it('should return movie detail', () => {
      let result: MovieDetail | undefined;
      service.getMovieById('550').subscribe((r) => (result = r));

      httpTesting.expectOne(`${baseUrl}/movies/550`).flush(mockDetail);

      expect(result).toEqual(mockDetail);
    });

    it('should cache movie detail after first fetch', () => {
      service.getMovieById('550').subscribe();
      httpTesting.expectOne(`${baseUrl}/movies/550`).flush(mockDetail);

      // Second call should NOT make an HTTP request
      let cachedResult: MovieDetail | undefined;
      service.getMovieById('550').subscribe((r) => (cachedResult = r));
      httpTesting.expectNone(`${baseUrl}/movies/550`);

      expect(cachedResult).toEqual(mockDetail);
    });

    it('should fetch different movies separately', () => {
      service.getMovieById('550').subscribe();
      httpTesting.expectOne(`${baseUrl}/movies/550`).flush(mockDetail);

      service.getMovieById('999').subscribe();
      httpTesting
        .expectOne(`${baseUrl}/movies/999`)
        .flush({ ...mockDetail, tmdbId: 999 });
    });
  });

  describe('saveMovieOwnership', () => {
    const saveRequest: SaveMovieRequest = {
      tmdbId: 550,
      title: 'Fight Club',
      releaseDate: '1999-10-15',
      posterPath: '/fight.jpg',
      formats: [FormatId.BluRay, FormatId.BluRay4K],
      digitalRetailers: [DigitalRetailerId.AppleTv],
    };

    it('should POST to save movie ownership', () => {
      service.saveMovieOwnership(saveRequest).subscribe();

      const req = httpTesting.expectOne(`${baseUrl}/users/me/movies`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(saveRequest);
      req.flush({});
    });

    it('should update cache when movie is cached', () => {
      // First, cache the movie
      const mockDetail: MovieDetail = {
        tmdbId: 550,
        title: 'Fight Club',
        releaseDate: '1999-10-15',
        posterPath: '/fight.jpg',
        backdropPath: '/fight-bg.jpg',
        runtime: 139,
        tagline: 'Mischief. Mayhem. Soap.',
        overview: 'Overview text',
        formats: [],
        digitalRetailers: [],
      };
      service.getMovieById('550').subscribe();
      httpTesting.expectOne(`${baseUrl}/movies/550`).flush(mockDetail);

      // Now save ownership
      service.saveMovieOwnership(saveRequest).subscribe();
      httpTesting.expectOne(`${baseUrl}/users/me/movies`).flush({});

      // Verify cache was updated (no new HTTP request)
      let cachedMovie: MovieDetail | undefined;
      service.getMovieById('550').subscribe((r) => (cachedMovie = r));

      expect(cachedMovie!.formats.length).toBe(2);
      expect(cachedMovie!.formats.map((f) => f.id)).toContain(FormatId.BluRay);
      expect(cachedMovie!.formats.map((f) => f.id)).toContain(FormatId.BluRay4K);
      expect(cachedMovie!.digitalRetailers.length).toBe(1);
      expect(cachedMovie!.digitalRetailers[0].id).toBe(DigitalRetailerId.AppleTv);
    });
  });

  describe('deleteMovieOwnership', () => {
    it('should DELETE movie ownership', () => {
      service.deleteMovieOwnership('550').subscribe();

      const req = httpTesting.expectOne(`${baseUrl}/users/me/movies/550`);
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should clear formats and retailers in cache on delete', () => {
      // Cache a movie with formats
      const mockDetail: MovieDetail = {
        tmdbId: 550,
        title: 'Fight Club',
        releaseDate: '1999-10-15',
        posterPath: '/fight.jpg',
        backdropPath: null,
        runtime: 139,
        tagline: null,
        overview: 'Overview',
        formats: [{ id: FormatId.BluRay, name: 'Blu-ray' }],
        digitalRetailers: [{ id: DigitalRetailerId.AppleTv, name: 'Apple TV' }],
      };
      service.getMovieById('550').subscribe();
      httpTesting.expectOne(`${baseUrl}/movies/550`).flush(mockDetail);

      // Delete ownership
      service.deleteMovieOwnership('550').subscribe();
      httpTesting.expectOne(`${baseUrl}/users/me/movies/550`).flush({});

      // Verify cache was updated
      let cachedMovie: MovieDetail | undefined;
      service.getMovieById('550').subscribe((r) => (cachedMovie = r));

      expect(cachedMovie!.formats).toEqual([]);
      expect(cachedMovie!.digitalRetailers).toEqual([]);
    });
  });

  describe('updateFormats', () => {
    it('should map format IDs to Format objects', () => {
      const result = service.updateFormats([FormatId.Dvd, FormatId.BluRay4K], FORMAT_OPTIONS);

      expect(result).toEqual([
        { id: FormatId.Dvd, name: 'DVD' },
        { id: FormatId.BluRay4K, name: 'Ultra HD Blu-ray' },
      ]);
    });

    it('should return empty array when no IDs match', () => {
      const result = service.updateFormats([], FORMAT_OPTIONS);
      expect(result).toEqual([]);
    });

    it('should ignore IDs not in options', () => {
      const result = service.updateFormats([99 as FormatId], FORMAT_OPTIONS);
      expect(result).toEqual([]);
    });
  });

  describe('updateDigitalRetailers', () => {
    it('should map retailer IDs to DigitalRetailer objects', () => {
      const result = service.updateDigitalRetailers(
        [DigitalRetailerId.AppleTv, DigitalRetailerId.YouTube],
        DIGITAL_RETAILER_OPTIONS,
      );

      expect(result).toEqual([
        { id: DigitalRetailerId.AppleTv, name: 'Apple TV' },
        { id: DigitalRetailerId.YouTube, name: 'YouTube' },
      ]);
    });

    it('should return empty array when no IDs match', () => {
      const result = service.updateDigitalRetailers([], DIGITAL_RETAILER_OPTIONS);
      expect(result).toEqual([]);
    });
  });
});
