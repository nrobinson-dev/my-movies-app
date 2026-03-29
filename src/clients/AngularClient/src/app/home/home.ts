import { Component, computed, inject, signal } from '@angular/core';
import { MovieService } from '../movies/movie.service';
import { MovieCard } from '../shared/movie-card/movie-card';
import { MovieSummaryCollection, MovieSummary } from '../movies/models/movie-summary';
import { RouterLink } from '@angular/router';
import { LoadMoreButton } from '../shared/load-more-button/load-more-button';

@Component({
  selector: 'home',
  imports: [MovieCard, RouterLink, LoadMoreButton],
  template: `
    @if ((moviesSummaryCollection()?.movies?.length ?? 0) > 0) {
      <h1 class="text-center text-2xl mb-4">My Movies</h1>

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
        @for (movie of movies() || []; track movie.tmdbId) {
          <movie-card [movieSummary]="movie"></movie-card>
        }
      </div>

      @if (hasMore()) {
        <load-more-button (loadMore)="loadMore()"></load-more-button>
      }
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
  currentPage = signal(1);
  pageSize = signal(20);
  totalPages = signal(1);
  hasMore = computed(() => this.currentPage() < this.totalPages());

  moviesSummaryCollection = signal<MovieSummaryCollection | null>(null);
  movies = signal<MovieSummary[]>([]);

  ngOnInit() {
    this.loadMovies();
  }

  loadMore() {
    this.currentPage.set(this.currentPage() + 1);
    this.loadMovies();
  }

  loadMovies() {
    this.movieService
      .getUserMovies(
        localStorage.getItem('auth_user_id') || '',
        this.currentPage(),
        this.pageSize(),
      )
      .subscribe({
        next: (response) => {
          this.currentPage.set(response.page);
          this.totalPages.set(response.totalPages);
          this.movies.update((current) => [...(current || []), ...(response.movies || [])]);
          this.moviesSummaryCollection.set(response);
        },
        error: (err) => {
          console.error('Failed to fetch user movies', err);
        },
      });
  }
}
