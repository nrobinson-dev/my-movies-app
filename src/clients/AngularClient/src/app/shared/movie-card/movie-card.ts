import { Component, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FORMAT_OPTIONS } from '../constants/format-options';
import { DIGITAL_RETAILER_OPTIONS } from '../constants/digital-retailer-options';
import { DigitalRetailer, Format } from '../../movies/models/lookup';
import { MovieSummary } from '../../movies/models/movie-summary';
import { TMDB_IMAGE_BASE_URL } from '../constants/constants';
import { RouterLink } from '@angular/router';
import { PlatformOption } from '../models/platform-option';

@Component({
  selector: 'movie-card',
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    <div
      class="movie-card"
    >
      <div class="movie-card__header">
        <p>
          <a [routerLink]="['/movie', movieSummary().tmdbId]" class="mr-1"
            >{{ movieSummary().title }}
            <span class="movie-card__title"></span>
          </a>
          <span class="movie-card__release-date">({{ this.movieSummary().releaseDate | date: 'yyyy' }})</span>
        </p>
      </div>

      <div class="movie-card__poster-wrapper">
        @if (posterPath()) {
          <img
            class="movie-card__poster"
            [src]="posterPath()"
            [alt]="this.movieSummary().title + ' poster'"
          />
        } @else {
          <div class="movie-card__placeholder-poster">
            <img
              src="/images/icons/image.svg"
              alt="No poster available"
              class="movie-card__placeholder-icon"
            />
          </div>
        }
      </div>

      <div class="movie-card__ownership-info">
        @if (ownedFormats().length > 0) {
          <div class="movie-card__formats">
            <p><small>Physical Media:</small></p>

            <div class="movie-card__platform-wrapper">
              @for (format of ownedFormats(); track format.value) {
                <img
                  [src]="format.image"
                  [alt]="format.label"
                  [class]="format.label"
                  [title]="format.label"
                  class="movie-card__platform--format"
                />
              }
            </div>
          </div>
        }

        @if (ownedDigitalRetailers().length > 0) {
          <div class="movie-card__digital-retailers">
            <p class="mb-1"><small>Digital Retailers:</small></p>

            <div class="movie-card__platform-wrapper">
              @for (retailer of ownedDigitalRetailers(); track retailer.value) {
                <div class="flex items-center justify-center">
                  <img
                  [src]="retailer.image"
                  [alt]="retailer.label"
                  [class]="retailer.label"
                  [title]="retailer.label"
                  class="movie-card__platform--digital-retailer"
                  />
                </div>
              }
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styleUrls: ['./movie-card.css'],
})
export class MovieCard {
  movieSummary = input.required<MovieSummary>();

  posterPath = signal<string | null>(null);
  ownedFormats = signal<PlatformOption[]>([]);
  ownedDigitalRetailers = signal<PlatformOption[]>([]);

  filterFormats(
    formats: Format[],
    FORMAT_OPTIONS: PlatformOption[],
  ) {
    return FORMAT_OPTIONS.filter((option) => formats.some((f) => f.id === option.value));
  }

  filterDigitalRetailers(
    digitalRetailers: DigitalRetailer[],
    DIGITAL_RETAILER_OPTIONS: PlatformOption[],
  ) {
    return DIGITAL_RETAILER_OPTIONS.filter((option) =>
      digitalRetailers.some((r) => r.id === option.value),
    );
  }

  ngOnInit() {
    this.posterPath.set(
      this.movieSummary().posterPath
        ? `${TMDB_IMAGE_BASE_URL}${this.movieSummary().posterPath}`
        : null,
    );
    this.ownedFormats.set(this.filterFormats(this.movieSummary().formats, FORMAT_OPTIONS));
    this.ownedDigitalRetailers.set(
      this.filterDigitalRetailers(this.movieSummary().digitalRetailers, DIGITAL_RETAILER_OPTIONS),
    );
  }
}
