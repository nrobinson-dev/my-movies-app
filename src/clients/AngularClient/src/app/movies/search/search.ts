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
    <h2 class="text-center text-2xl mb-4">Search the Movie Database</h2>
    <form
      (submit)="performSearch($event)"
      class="flex align-items-center gap-2 mb-4 max-w-96 mx-auto"
    >
      <input
        type="text"
        placeholder="Search for movies..."
        class="flex-grow border rounded py-2 px-3"
        [value]="searchQuery()"
        (input)="setSearchQuery($event)"
      />
      <button
        class="transition cursor-pointer bg-blue-500 text-white rounded py-2 px-4 hover:bg-blue-600"
        (click)="performSearch($event)"
        [disabled]="isSearching()"
      >
        Search
      </button>
    </form>

    @if (isError()) {
      <h2 class="text-center pt-10 text-xl mb-4 text-red-500">
        An error occurred while searching. Please try again.
      </h2>
    }

    @if (isSearching()) {
      <h1 class="text-center pt-10 text-3xl mb-4">Loading search results...</h1>
    }

    @if (isSearched()) {
      @if (movies().length > 0) {
        <h2 class="text-center text-2xl mb-4">Search Results</h2>
        <div class="movie-grid gap-4">
          @for (movie of movies(); track movie.tmdbId) {
            <movie-card [movieSummary]="movie"></movie-card>
          }
        </div>
        @if (hasMore()) {
          <load-more-button (loadMore)="loadMore()"></load-more-button>
        }
      } @else {
        <h2 class="text-center pt-10 text-xl mb-4">
          No movies found matching "{{ searchQuery() }}". Try a different search term?
        </h2>
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
      .getSearchResults(query, localStorage.getItem('auth_user_id') || '', page)
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
