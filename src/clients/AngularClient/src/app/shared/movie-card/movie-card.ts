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
      class="relative movie-card border border-gray-300 shadow-sm rounded-lg overflow-hidden hover:shadow-xl transition"
    >
      <div class="py-1 px-2 bg-gray-800 text-white text-center">
        <p>
          <a [routerLink]="['/movie', movieSummary().tmdbId]" class="mr-1"
            >{{ movieSummary().title }}
            <span class="absolute left-0 top-0 w-full h-full z-10"></span>
          </a>
          <span class="text-xs">({{ this.movieSummary().releaseDate | date: 'yyyy' }})</span>
        </p>
      </div>

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

        @if (ownedFormats().length > 0) {
          <div class="movie-card__formats p-1 bg-gray-800 text-white text-center">
            <p><small>Physical Media:</small></p>

            <div class="flex align-items-center justify-evenly">
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
          <div class="movie-card__digital-retailers p-1 bg-gray-800 text-white text-center">
            <p class="mb-1"><small>Digital Retailers:</small></p>

            <div class="flex align-items-center justify-evenly pb-2">
              @for (retailer of ownedDigitalRetailers(); track retailer.value) {
                <img
                  [src]="retailer.image"
                  [alt]="retailer.label"
                  [class]="retailer.label"
                  [title]="retailer.label"
                  class="movie-card__platform--digital-retailer"
                />
              }
            </div>
          </div>
        }
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
