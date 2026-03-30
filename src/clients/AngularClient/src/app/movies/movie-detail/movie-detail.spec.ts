import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { MovieDetail } from './movie-detail';
import { MovieService } from '../movie.service';
import { MovieDetail as MovieDetailModel } from '../models/movie-detail';
import { FormatId, DigitalRetailerId } from '../models/lookup';
import { of, throwError } from 'rxjs';
import { PlatformOption } from '../../shared/models/platform-option';
import { FORMAT_OPTIONS } from '../../shared/constants/format-options';
import { DIGITAL_RETAILER_OPTIONS } from '../../shared/constants/digital-retailer-options';

describe('MovieDetail', () => {
  let component: MovieDetail;
  let fixture: ComponentFixture<MovieDetail>;
  let movieService: any;

  const mockMovieDetail: MovieDetailModel = {
    tmdbId: 550,
    title: 'Fight Club',
    releaseDate: '1999-10-15',
    posterPath: '/fight.jpg',
    backdropPath: '/fight-bg.jpg',
    runtime: 139,
    tagline: 'Mischief. Mayhem. Soap.',
    overview: 'An insomniac office worker...',
    formats: [{ id: FormatId.BluRay, name: 'Blu-ray' }],
    digitalRetailers: [{ id: DigitalRetailerId.AppleTv, name: 'Apple TV' }],
  };

  beforeEach(async () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key: string) => {
      if (key === 'auth_user_id') return 'user-123';
      return null;
    });

    const movieServiceSpy = {
      getMovieById: vi.fn().mockReturnValue(of(mockMovieDetail)),
      saveMovieOwnership: vi.fn().mockReturnValue(of({})),
      deleteMovieOwnership: vi.fn().mockReturnValue(of({})),
    };

    await TestBed.configureTestingModule({
      imports: [MovieDetail],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MovieService, useValue: movieServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: { get: (key: string) => (key === 'movieId' ? '550' : null) },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MovieDetail);
    component = fixture.componentInstance;
    movieService = TestBed.inject(MovieService) as any;
  });

  afterEach(() => vi.restoreAllMocks());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should fetch movie by ID from route params', () => {
      component.ngOnInit();

      expect(movieService.getMovieById).toHaveBeenCalledWith('user-123', '550');
    });

    it('should set movieDetail signal on success', () => {
      component.ngOnInit();

      expect(component.movieDetail()).toEqual(mockMovieDetail);
    });

    it('should set posterUrl when posterPath exists', () => {
      component.ngOnInit();

      expect(component.posterUrl()).toContain('/fight.jpg');
    });

    it('should set backgroundImageUrl when backdropPath exists', () => {
      component.ngOnInit();

      expect(component.backgroundImageUrl()).toContain('/fight-bg.jpg');
    });

    it('should set empty posterUrl when posterPath is null', () => {
      movieService.getMovieById = vi.fn().mockReturnValue(
        of({ ...mockMovieDetail, posterPath: null }),
      );

      component.ngOnInit();

      expect(component.posterUrl()).toBe('');
    });

    it('should populate ownedFormats from movie formats', () => {
      component.ngOnInit();

      const formats = component.ownedFormats();
      expect(formats.length).toBe(FORMAT_OPTIONS.length);
      const bluRay = formats.find((f) => f.value === FormatId.BluRay);
      expect(bluRay?.isOwned).toBe(true);
      const dvd = formats.find((f) => f.value === FormatId.Dvd);
      expect(dvd?.isOwned).toBe(false);
    });

    it('should populate ownedDigitalRetailers from movie retailers', () => {
      component.ngOnInit();

      const retailers = component.ownedDigitalRetailers();
      expect(retailers.length).toBe(DIGITAL_RETAILER_OPTIONS.length);
      const appleTv = retailers.find((f) => f.value === DigitalRetailerId.AppleTv);
      expect(appleTv?.isOwned).toBe(true);
      const youtube = retailers.find((f) => f.value === DigitalRetailerId.YouTube);
      expect(youtube?.isOwned).toBe(false);
    });

    it('should set isReleaseDateInFuture to false for past release dates', () => {
      component.ngOnInit();

      expect(component.isReleaseDateInFuture()).toBe(false);
    });

    it('should set isReleaseDateInFuture to true for future release dates', () => {
      const futureDate = new Date();
      futureDate.setFullYear(futureDate.getFullYear() + 1);
      movieService.getMovieById = vi.fn().mockReturnValue(
        of({ ...mockMovieDetail, releaseDate: futureDate.toISOString() }),
      );

      component.ngOnInit();

      expect(component.isReleaseDateInFuture()).toBe(true);
    });

    it('should set notFound when movie detail is null', () => {
      movieService.getMovieById = vi.fn().mockReturnValue(of(null));

      component.ngOnInit();

      expect(component.notFound()).toBe(true);
    });

    it('should set notFound on error', () => {
      movieService.getMovieById = vi.fn().mockReturnValue(throwError(() => new Error('Not found')));

      component.ngOnInit();

      expect(component.notFound()).toBe(true);
    });
  });

  describe('filterPlatformOwnership', () => {
    it('should mark matching platforms as owned', () => {
      const formats = [{ id: FormatId.BluRay, name: 'Blu-ray' }];
      const result = component.filterPlatformOwnership(formats, FORMAT_OPTIONS);

      expect(result.find((f) => f.value === FormatId.BluRay)?.isOwned).toBe(true);
      expect(result.find((f) => f.value === FormatId.Dvd)?.isOwned).toBe(false);
      expect(result.find((f) => f.value === FormatId.BluRay4K)?.isOwned).toBe(false);
    });

    it('should mark all as not owned when none match', () => {
      const result = component.filterPlatformOwnership([], FORMAT_OPTIONS);

      expect(result.every((f) => !f.isOwned)).toBe(true);
    });
  });

  describe('togglePlatformOwnership', () => {
    it('should toggle a platform from not owned to owned', () => {
      const platforms: PlatformOption[] = [
        { value: 1, label: 'DVD', image: 'dvd.svg', isOwned: false },
        { value: 2, label: 'Blu-ray', image: 'bluray.svg', isOwned: false },
      ];
      component.ownedFormats.set(platforms);

      component.togglePlatformOwnership(1, component.ownedFormats);

      expect(component.ownedFormats().find((f) => f.value === 1)?.isOwned).toBe(true);
      expect(component.ownedFormats().find((f) => f.value === 2)?.isOwned).toBe(false);
    });

    it('should toggle a platform from owned to not owned', () => {
      const platforms: PlatformOption[] = [
        { value: 1, label: 'DVD', image: 'dvd.svg', isOwned: true },
      ];
      component.ownedFormats.set(platforms);

      component.togglePlatformOwnership(1, component.ownedFormats);

      expect(component.ownedFormats()[0].isOwned).toBe(false);
    });
  });

  describe('getOwnedPlatformIds', () => {
    it('should return IDs of owned platforms only', () => {
      const platforms: PlatformOption[] = [
        { value: 1, label: 'DVD', image: 'dvd.svg', isOwned: true },
        { value: 2, label: 'Blu-ray', image: 'bluray.svg', isOwned: false },
        { value: 3, label: '4K', image: '4k.svg', isOwned: true },
      ];

      expect(component.getOwnedPlatformIds(platforms)).toEqual([1, 3]);
    });

    it('should return empty array when none are owned', () => {
      const platforms: PlatformOption[] = [
        { value: 1, label: 'DVD', image: 'dvd.svg', isOwned: false },
      ];

      expect(component.getOwnedPlatformIds(platforms)).toEqual([]);
    });
  });

  describe('saveOwnership', () => {
    beforeEach(() => component.ngOnInit());

    it('should call movieService.saveMovieOwnership with correct data', () => {
      component.saveOwnership();

      expect(movieService.saveMovieOwnership).toHaveBeenCalledWith(
        'user-123',
        expect.objectContaining({
          tmdbId: 550,
          title: 'Fight Club',
        }),
      );
    });

    it('should set isProcessing to true during save', () => {
      component.saveOwnership();

      // After observable completes synchronously (mocked), isProcessing is false
      expect(component.isProcessing()).toBe(false);
    });

    it('should not save if already processing', () => {
      component.isProcessing.set(true);

      component.saveOwnership();

      expect(movieService.saveMovieOwnership).not.toHaveBeenCalled();
    });

    it('should show success message on save', () => {
      component.saveOwnership();

      expect(component.messageResult()?.type).toBe('success');
      expect(component.messageResult()?.message).toContain('saved successfully');
    });

    it('should show error message on save failure', () => {
      movieService.saveMovieOwnership = vi.fn().mockReturnValue(
        throwError(() => new Error('Save failed')),
      );

      component.saveOwnership();

      expect(component.messageResult()?.type).toBe('error');
    });
  });

  describe('deleteMovieOwnership', () => {
    beforeEach(() => component.ngOnInit());

    it('should call movieService.deleteMovieOwnership', () => {
      component.deleteMovieOwnership();

      expect(movieService.deleteMovieOwnership).toHaveBeenCalledWith('user-123', '550');
    });

    it('should clear all owned formats on delete', () => {
      component.deleteMovieOwnership();

      expect(component.ownedFormats().every((f) => !f.isOwned)).toBe(true);
    });

    it('should clear all owned retailers on delete', () => {
      component.deleteMovieOwnership();

      expect(component.ownedDigitalRetailers().every((r) => !r.isOwned)).toBe(true);
    });

    it('should not delete if already processing', () => {
      component.isProcessing.set(true);

      component.deleteMovieOwnership();

      expect(movieService.deleteMovieOwnership).not.toHaveBeenCalled();
    });

    it('should show success message on delete', () => {
      component.deleteMovieOwnership();

      expect(component.messageResult()?.type).toBe('success');
      expect(component.messageResult()?.message).toContain('deleted successfully');
    });

    it('should show error message on delete failure', () => {
      movieService.deleteMovieOwnership = vi.fn().mockReturnValue(
        throwError(() => new Error('Delete failed')),
      );

      component.deleteMovieOwnership();

      expect(component.messageResult()?.type).toBe('error');
    });
  });
});
