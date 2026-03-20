import { Component, inject, signal, WritableSignal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MovieService } from '../movie.service';
import { MovieDetail as MovieDetailModel } from '../models/movie-detail';
import { DatePipe, Location } from '@angular/common';
import { TMDB_IMAGE_BASE_URL } from '../../shared/constants/constants';
import { FORMAT_OPTIONS } from '../../shared/constants/format-options';
import { DIGITAL_RETAILER_OPTIONS } from '../../shared/constants/digital-retailer-options';
import { DigitalRetailer, Format } from '../models/lookup';
import { SaveMovieRequest } from '../models/save-movie-request';
import { PlatformOption } from '../../shared/models/platform-option';

@Component({
  selector: 'movie-detail',
  imports: [DatePipe],
  templateUrl: './movie-detail.html',
  styleUrls: ['./movie-detail.css'],
})
export class MovieDetail {
  private route = inject(ActivatedRoute);
  private movieService = inject(MovieService);
  protected location = inject(Location);

  movieDetail = signal<MovieDetailModel | null>(null);
  notFound = signal(false);
  backgroundImageUrl = signal<string>('');
  posterUrl = signal<string>('');
  isProcessing = signal(false);
  ownedFormats = signal<PlatformOption[]>([]);
  ownedDigitalRetailers = signal<PlatformOption[]>([]);
  saveMovieRequest = {} as SaveMovieRequest;
  messageResult = signal<{ message: string; type: 'success' | 'error' } | null>(null);

  ngOnInit() {
    const movieId = this.route.snapshot.paramMap.get('movieId');

    if (movieId) {
      this.movieService
        .getMovieById(localStorage.getItem('auth_user_id') || '', movieId)
        .subscribe({
          next: (detail) => {
            this.movieDetail.set(detail);

            this.initImage(this.posterUrl, detail.posterPath);
            this.initImage(this.backgroundImageUrl, detail.backdropPath);

            this.ownedFormats.set(this.filterPlatformOwnership(detail.formats, FORMAT_OPTIONS));
            this.ownedDigitalRetailers.set(
              this.filterPlatformOwnership(detail.digitalRetailers, DIGITAL_RETAILER_OPTIONS),
            );
          },
          error: (err) => {
            console.error('Failed to fetch movie detail', err);
            this.notFound.set(true);
          },
        });
    }
  }

  initImage(imageUrlSignal: any, imagePath: string | null) {
    if (imagePath) {
      imageUrlSignal.set(TMDB_IMAGE_BASE_URL + imagePath);
    } else {
      imageUrlSignal.set('');
    }
  }

  filterPlatformOwnership(
    platforms: Format[] | DigitalRetailer[],
    platformOptions: PlatformOption[],
  ) {
    return platformOptions.map((option) => ({
      ...option,
      isOwned: platforms.some((p) => p.id === option.value),
    }));
  }

  togglePlatformOwnership(
    platformId: number,
    ownedPlatformsSignal: WritableSignal<PlatformOption[]>,
  ) {
    let updatedPlatforms = ownedPlatformsSignal().map((platform) => {
      if (platform.value === platformId) {
        return { ...platform, isOwned: !platform.isOwned };
      }
      return platform;
    });
    ownedPlatformsSignal.set(updatedPlatforms);
  }

  getOwnedPlatformIds(platforms: PlatformOption[]) {
    return platforms.filter((p) => p.isOwned).map((p) => p.value);
  }

  private showMessageResult(message: string, type: 'success' | 'error') {
    this.messageResult.set({ message, type });
    setTimeout(() => this.messageResult.set(null), 3000);
  }

  saveOwnership() {
    if (this.isProcessing()) {
      return;
    }

    this.isProcessing.set(true);

    this.saveMovieRequest.tmdbId = this.movieDetail()?.tmdbId || 0;
    this.saveMovieRequest.title = this.movieDetail()?.title || '';
    this.saveMovieRequest.releaseDate = this.movieDetail()?.releaseDate || '';
    this.saveMovieRequest.posterPath = this.movieDetail()?.posterPath || null;
    this.saveMovieRequest.formats = this.getOwnedPlatformIds(this.ownedFormats());
    this.saveMovieRequest.digitalRetailers = this.getOwnedPlatformIds(this.ownedDigitalRetailers());

    this.movieService
      .saveMovieOwnership(localStorage.getItem('auth_user_id') || '', this.saveMovieRequest)
      .subscribe({
        next: () => {
          this.isProcessing.set(false);
          this.showMessageResult('Ownership saved successfully.', 'success');
        },
        error: (err) => {
          console.error('Failed to update ownership', err);
          this.isProcessing.set(false);
          this.showMessageResult('Failed to save ownership. Please try again.', 'error');
        },
      });
  }

  deleteMovieOwnership() {
    if (this.isProcessing()) {
      return;
    }

    this.isProcessing.set(true);

    this.movieService
      .deleteMovieOwnership(
        localStorage.getItem('auth_user_id') || '',
        this.route.snapshot.paramMap.get('movieId') || '',
      )
      .subscribe({
        next: () => {
          this.isProcessing.set(false);
          this.showMessageResult('Ownership deleted successfully.', 'success');
          this.ownedFormats.set(
            this.ownedFormats().map((format) => ({ ...format, isOwned: false })),
          );
          this.ownedDigitalRetailers.set(
            this.ownedDigitalRetailers().map((retailer) => ({ ...retailer, isOwned: false })),
          );
        },
        error: (err) => {
          console.error('Failed to delete ownership', err);
          this.isProcessing.set(false);
          this.showMessageResult('Failed to delete ownership. Please try again.', 'error');
        },
      });
  }
}
