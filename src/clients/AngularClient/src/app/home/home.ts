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
      <p class="mb-4 text-center">Total Movies Owned: {{ moviesSummaryCollection()?.totalCount }} | DVD: {{ moviesSummaryCollection()?.totalDvdCount }} | Blu-ray:
      {{ moviesSummaryCollection()?.totalBluRayCount }} | Ultra HD Blu-ray: {{ moviesSummaryCollection()?.totalBluRay4KCount }} | Digital:
      {{ moviesSummaryCollection()?.totalDigitalCount }}</p>

      <div class="flex flex-wrap gap-7 justify-center">
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
