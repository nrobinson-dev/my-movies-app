import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MovieService } from '../movie.service';
import { MovieSummary } from '../models/movie-summary';
import { MovieCard } from '../../shared/movie-card/movie-card';
import { LoadMoreButton } from '../../shared/load-more-button/load-more-button';

@Component({
  selector: 'search',
  imports: [MovieCard, LoadMoreButton],
  template: `
    <h1 class="page-title mt-10">Add movies to your collection</h1>
    <form
      (submit)="performSearch($event)"
      class="search-form"
    >
      <label for="search-field" class="sr-only">Search for movies:</label>
      <input
        id="search-field"
        class="search-field"
        type="text"
        placeholder="Search..."
        [value]="searchQuery()"
        (input)="setSearchQuery($event)"
      />
      <button
        type="submit"
        class="search-button button--gold"
        (click)="performSearch($event)"
        [disabled]="isSearching()"
      >
        Search
      </button>
    </form>

    @if (isSearching()) {
      <p role="status" class="result-title">Loading search results...</p>
    }

    @if (isError()) {
      <p role="alert" class="result-title result-title--error">
        An error occurred while searching. Please try again.
      </p>
    }

    @if (isSearched()) {
      @if (movies().length > 0) {
        <h2 class="result-title">Search Results</h2>
        <ul class="movie-grid gap-4" aria-label="Search results">
          @for (movie of movies(); track movie.tmdbId) {
            <li>
              <movie-card [movieSummary]="movie"></movie-card>
            </li>
          }
        </ul>
        @if (hasMore()) {
          <load-more-button (loadMore)="loadMore()"></load-more-button>
        }
      } @else {
        <p role="status" class="result-title">
          No movies found matching "{{ searchQuery() }}". Try a different search term?
        </p>
      }
    }
  `,
  styleUrls: ['./search.css'],
})
export class Search {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  movieService = inject(MovieService);
  currentPage = signal(1);
  totalPages = signal(1);
  hasMore = computed(() => this.currentPage() < this.totalPages());
  searchQuery = signal('');

  movies = signal<MovieSummary[]>([]);
  isSearched = signal(false);
  isSearching = signal(false);
  isError = signal(false);

  ngOnInit() {
    const q = this.route.snapshot.queryParamMap.get('q');
    if (q) {
      this.searchQuery.set(q);
      this.runSearch(q);
    }
  }

  setSearchQuery(event: Event) {
    this.searchQuery.set((event.target as HTMLInputElement).value.trim());
  }

  performSearch(event: Event) {
    event.preventDefault();
    if (!this.searchQuery()) {
      return;
    }

    this.movies.set([]);
    this.currentPage.set(1);

    this.router.navigate([], { queryParams: { q: this.searchQuery() }, replaceUrl: true });
    this.runSearch(this.searchQuery());
  }

  loadMore() {
    this.currentPage.set(this.currentPage() + 1);
    this.runSearch(this.searchQuery(), this.currentPage());
  }

  private runSearch(query: string, page: number = 1) {
    this.isSearching.set(true);
    this.isError.set(false);
    this.movieService
      .getSearchResults(query, page)
      .subscribe({
        next: (results) => {
          this.movies.update((current) => [...(current || []), ...(results.movies || [])]);
          this.currentPage.set(results.page);
          this.totalPages.set(results.totalPages)
          this.isSearched.set(true);
          this.isSearching.set(false);
        },
        error: (err) => {
          console.error('Search failed', err);
          this.isSearched.set(false);
          this.isError.set(true);
          this.isSearching.set(false);
        },
      });
  }
}
