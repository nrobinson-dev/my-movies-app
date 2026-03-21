import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MovieSummaryCollection } from './models/movie-summary';
import { MovieDetail } from './models/movie-detail';
import { SaveMovieRequest } from './models/save-movie-request';

@Injectable({
  providedIn: 'root',
})
export class MovieService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getUserMovies(userId: string): Observable<MovieSummaryCollection> {
    return this.http.get<MovieSummaryCollection>(`${this.baseUrl}/users/${userId}/movies`);
  }
  
  getSearchResults(search: string, userId: string = ''): Observable<MovieSummaryCollection> {
    return this.http.get<MovieSummaryCollection>(`${this.baseUrl}/movies`, {
      params: { search, userId }
    });
  }

  getMovieById(userId: string, movieId: string): Observable<MovieDetail> {
    return this.http.get<MovieDetail>(`${this.baseUrl}/users/${userId}/movies/${movieId}`);
  }

  saveMovieOwnership(userId: string, movieData: SaveMovieRequest) {
    return this.http.post(`${this.baseUrl}/users/${userId}/movies`, movieData);
  }
  
  deleteMovieOwnership(userId: string, movieId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/users/${userId}/movies/${movieId}`);
  }
}