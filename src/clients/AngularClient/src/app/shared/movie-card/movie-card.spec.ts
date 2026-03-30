import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MovieCard } from './movie-card';
import { MovieSummary } from '../../movies/models/movie-summary';
import { FormatId, DigitalRetailerId } from '../../movies/models/lookup';
import { FORMAT_OPTIONS } from '../constants/format-options';
import { DIGITAL_RETAILER_OPTIONS } from '../constants/digital-retailer-options';
import { TMDB_IMAGE_POSTER_SMALL_BASE_URL } from '../constants/constants';

describe('MovieCard', () => {
  let component: MovieCard;
  let fixture: ComponentFixture<MovieCard>;

  const mockMovie: MovieSummary = {
    tmdbId: 550,
    title: 'Fight Club',
    releaseDate: '1999-10-15',
    posterPath: '/fight.jpg',
    formats: [{ id: FormatId.BluRay, name: 'Blu-ray' }],
    digitalRetailers: [{ id: DigitalRetailerId.AppleTv, name: 'Apple TV' }],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MovieCard],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(MovieCard);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('movieSummary', mockMovie);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should set posterPath with TMDB base URL when posterPath exists', () => {
      component.ngOnInit();

      expect(component.posterPath()).toBe(`${TMDB_IMAGE_POSTER_SMALL_BASE_URL}/fight.jpg`);
    });

    it('should set posterPath to null when movie has no poster', () => {
      fixture.componentRef.setInput('movieSummary', { ...mockMovie, posterPath: null });

      component.ngOnInit();

      expect(component.posterPath()).toBeNull();
    });

    it('should populate ownedFormats from movie formats', () => {
      component.ngOnInit();

      expect(component.ownedFormats().length).toBe(1);
      expect(component.ownedFormats()[0].value).toBe(FormatId.BluRay);
    });

    it('should populate ownedDigitalRetailers from movie retailers', () => {
      component.ngOnInit();

      expect(component.ownedDigitalRetailers().length).toBe(1);
      expect(component.ownedDigitalRetailers()[0].value).toBe(DigitalRetailerId.AppleTv);
    });

    it('should have empty formats when movie has none', () => {
      fixture.componentRef.setInput('movieSummary', {
        ...mockMovie,
        formats: [],
        digitalRetailers: [],
      });

      component.ngOnInit();

      expect(component.ownedFormats().length).toBe(0);
      expect(component.ownedDigitalRetailers().length).toBe(0);
    });
  });

  describe('filterFormats', () => {
    it('should return only matching formats from options', () => {
      const formats = [{ id: FormatId.Dvd, name: 'DVD' }];
      const result = component.filterFormats(formats, FORMAT_OPTIONS);

      expect(result.length).toBe(1);
      expect(result[0].value).toBe(FormatId.Dvd);
    });

    it('should return empty array when no formats match', () => {
      const result = component.filterFormats([], FORMAT_OPTIONS);
      expect(result.length).toBe(0);
    });
  });

  describe('filterDigitalRetailers', () => {
    it('should return only matching retailers from options', () => {
      const retailers = [
        { id: DigitalRetailerId.YouTube, name: 'YouTube' },
        { id: DigitalRetailerId.AppleTv, name: 'Apple TV' },
      ];
      const result = component.filterDigitalRetailers(retailers, DIGITAL_RETAILER_OPTIONS);

      expect(result.length).toBe(2);
    });

    it('should return empty array when no retailers match', () => {
      const result = component.filterDigitalRetailers([], DIGITAL_RETAILER_OPTIONS);
      expect(result.length).toBe(0);
    });
  });
});
