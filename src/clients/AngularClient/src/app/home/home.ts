import { Component, inject, signal } from '@angular/core';
import { MovieService } from '../movies/movie.service';
import { MovieCard } from '../shared/movie-card/movie-card';
import { MovieSummaryCollection } from '../movies/models/movie-summary';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'home',
  imports: [MovieCard, RouterLink],
  template: `
    @if ((moviesSummaryCollection()?.movies?.length ?? 0) > 0) {
      <h2 class="text-center text-2xl mb-4">My Movies</h2>

      <div class="stats-container">
        <div class="stat-item">
          <span class="stat-label">Total Movies</span>
          <span class="stat-value">{{ moviesSummaryCollection()?.totalResults }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Ultra HD</span>
          <span class="stat-value">{{ moviesSummaryCollection()?.totalBluRay4KCount }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Blu-ray</span>
          <span class="stat-value">{{ moviesSummaryCollection()?.totalBluRayCount }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">DVD</span>
          <span class="stat-value">{{ moviesSummaryCollection()?.totalDvdCount }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Digital</span>
          <span class="stat-value">{{ moviesSummaryCollection()?.totalDigitalCount }}</span>
        </div>
      </div>

      <div class="movie-grid gap-4">
        @for (movie of moviesSummaryCollection()?.movies || []; track movie.tmdbId) {
          <movie-card [movieSummary]="movie"></movie-card>
        }
      </div>
    } @else {
      <p class="text-center pt-10">
        You haven't added any movies yet. Start by
        <a routerLink="/search" class="hover:underline text-blue-500">searching</a> for the movies
        you own and adding them to your collection!
      </p>
    }
  `,
  styleUrls: ['./home.css'],
})
export class Home {
  movieService = inject(MovieService);

  moviesSummaryCollection = signal<MovieSummaryCollection | null>(null);

  constructor() {
    this.movieService.getUserMovies(localStorage.getItem('auth_user_id') || '').subscribe({
      next: (results) => {
        this.moviesSummaryCollection.set(results);
      },
      error: (err) => {
        console.error('Failed to fetch user movies', err);
      },
    });
  }
}
