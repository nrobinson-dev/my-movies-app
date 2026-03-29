import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { MovieSummaryCollection } from './models/movie-summary';
import { MovieDetail } from './models/movie-detail';
import { SaveMovieRequest } from './models/save-movie-request';
import { PlatformOption } from '../shared/models/platform-option';
import { DigitalRetailer, Format } from './models/lookup';
import { FORMAT_OPTIONS } from '../shared/constants/format-options';
import { DIGITAL_RETAILER_OPTIONS } from '../shared/constants/digital-retailer-options';

@Injectable({
  providedIn: 'root',
})
export class MovieService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private movieDetailCache = new Map<string, MovieDetail>();
  private lastMovieResults: MovieSummaryCollection | null = null;

  getUserMovies(
    userId: string,
    pageNumber: number = 1,
    pageSize: number = 10,
  ): Observable<MovieSummaryCollection> {
    return this.http.get<MovieSummaryCollection>(`${this.baseUrl}/users/${userId}/movies`, {
      params: { page: pageNumber.toString(), pageSize: pageSize.toString() },
    });
  }

  getSearchResults(search: string, page: number = 1): Observable<MovieSummaryCollection> {
    return this.http.get<MovieSummaryCollection>(`${this.baseUrl}/movies`, {
      params: { search, page: page.toString() },
    });
  }

  getMovieById(userId: string, movieId: string): Observable<MovieDetail> {
    if (this.movieDetailCache.has(movieId)) {
      return of(this.movieDetailCache.get(movieId)!);
    }

    return this.http
      .get<MovieDetail>(`${this.baseUrl}/users/${userId}/movies/${movieId}`)
      .pipe(tap((movieDetail) => this.movieDetailCache.set(movieId, movieDetail)));
  }

  saveMovieOwnership(userId: string, movieData: SaveMovieRequest) {
    return this.http.post(`${this.baseUrl}/users/${userId}/movies`, movieData).pipe(
      tap(() => {
        const movieId = movieData.tmdbId.toString();
        if (this.movieDetailCache.has(movieId)) {
          const cachedMovie = this.movieDetailCache.get(movieId)!;

          cachedMovie.formats = this.updateFormats(movieData.formats, FORMAT_OPTIONS) as Format[];
          cachedMovie.digitalRetailers = this.updateDigitalRetailers(
            movieData.digitalRetailers,
            DIGITAL_RETAILER_OPTIONS,
          ) as DigitalRetailer[];

          this.movieDetailCache.set(movieId, { ...cachedMovie });
        }
      }),
    );
  }

  deleteMovieOwnership(userId: string, movieId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/users/${userId}/movies/${movieId}`).pipe(
      tap(() => {
        if (this.movieDetailCache.has(movieId)) {
          const movie = this.movieDetailCache.get(movieId)!;
          movie.formats = [];
          movie.digitalRetailers = [];
          this.movieDetailCache.set(movieId, { ...movie });
        }

        if (this.lastMovieResults) {
          const index = this.lastMovieResults.movies.findIndex(
            (m) => m.tmdbId === Number.parseInt(movieId),
          );
          if (index !== -1) {
            this.lastMovieResults.movies[index].formats = [];
            this.lastMovieResults.movies[index].digitalRetailers = [];
          }
        }
      }),
    );
  }

  updateDigitalRetailers(
    digitalRetailers: number[],
    DIGITAL_RETAILER_OPTIONS: PlatformOption[],
  ): DigitalRetailer[] {
    return DIGITAL_RETAILER_OPTIONS.filter((option) => digitalRetailers.includes(option.value)).map(
      (option) => ({
        id: option.value,
        name: option.label,
      }),
    );
  }

  updateFormats(formats: number[], FORMAT_OPTIONS: PlatformOption[]): Format[] {
    return FORMAT_OPTIONS.filter((option) => formats.includes(option.value)).map((option) => ({
      id: option.value,
      name: option.label,
    }));
  }
}
