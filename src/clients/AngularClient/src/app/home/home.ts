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
    @defer (when !isLoading()) {
      <section aria-live="polite" aria-label="Movie collection">
      @if (isError()) {
        <p role="alert" class="text-center pt-10">
          Something went wrong loading your movies.
          <button class="underline text-blue-500" (click)="refresh()">Please refresh the page.</button>
        </p>
      } @else if (movies().length > 0) {
        <dl class="stats-container">
          <div class="stat-item">
            <dt class="stat-label">Total Movies</dt>
            <dd class="stat-value">{{ moviesSummaryCollection()?.totalResults }}</dd>
          </div>
          <div class="stat-item">
            <dt class="stat-label">Ultra HD</dt>
            <dd class="stat-value">{{ moviesSummaryCollection()?.totalBluRay4KCount }}</dd>
          </div>
          <div class="stat-item">
            <dt class="stat-label">Blu-ray</dt>
            <dd class="stat-value">{{ moviesSummaryCollection()?.totalBluRayCount }}</dd>
          </div>
          <div class="stat-item">
            <dt class="stat-label">DVD</dt>
            <dd class="stat-value">{{ moviesSummaryCollection()?.totalDvdCount }}</dd>
          </div>
          <div class="stat-item">
            <dt class="stat-label">Digital</dt>
            <dd class="stat-value">{{ moviesSummaryCollection()?.totalDigitalCount }}</dd>
          </div>
        </dl>

        <ul class="movie-grid gap-4" aria-label="Your movie collection">
          @for (movie of movies() || []; track movie.tmdbId) {
            <li>
              <movie-card [movieSummary]="movie"></movie-card>
            </li>
          }
        </ul>

        @if (hasMore()) {
          <load-more-button (loadMore)="loadMore()"></load-more-button>
        }
      } @else {
        <div class="flex flex-row items-center min-h-[75vh]">
          <div class="flex flex-col items-center text-center max-w-lg mx-auto">
            <div class="stacks"></div>

            <h3 class="text-center text-4xl mb-4">Your collection awaits</h3>
            
            <p class="pt-2 pb-6">Start building your personal movie library. Search for films
              you own and add them to your collection.</p>
              
            <a routerLink="/search" class="button-link button--gold"><img src="/images/icons/search.svg" alt="Search icon"/> Discover Movies</a>
          </div>
        </div>
      }
      </section>
    } @placeholder {
      <div class="fade-in">
        <p role="status" class="text-center text-2xl mb-4">Loading your movies...</p>
        <div class="movie-grid gap-4" aria-hidden="true">
          <div class="placeholder-movie-card">
            <div class="placeholder-movie-card__title"></div>
            <div class="placeholder-movie-card__poster"></div>
          </div>
  
          <div class="placeholder-movie-card">
            <div class="placeholder-movie-card__title"></div>
            <div class="placeholder-movie-card__poster"></div>
          </div>
  
          <div class="placeholder-movie-card">
            <div class="placeholder-movie-card__title"></div>
            <div class="placeholder-movie-card__poster"></div>
          </div>
  
          <div class="placeholder-movie-card">
            <div class="placeholder-movie-card__title"></div>
            <div class="placeholder-movie-card__poster"></div>
          </div>
  
          <div class="placeholder-movie-card">
            <div class="placeholder-movie-card__title"></div>
            <div class="placeholder-movie-card__poster"></div>
          </div>
        </div>
      </div>
    }  @error {
      <p role="alert" class="text-center pt-10">
        Failed to load the page.
        <button class="underline text-blue-500" (click)="refresh()">Please refresh the page.</button>
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
  isLoading = signal(true);
  isError = signal(false);

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
        this.currentPage(),
        this.pageSize(),
      )
      .subscribe({
        next: (response) => {
          this.currentPage.set(response.page);
          this.totalPages.set(response.totalPages);
          this.movies.update((current) => [...(current || []), ...(response.movies || [])]);
          this.moviesSummaryCollection.set(response);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to fetch user movies', err);
          this.isError.set(true);
          this.isLoading.set(false);
        },
      });
  }

  refresh() {
    this.loadMovies();
  }
}
