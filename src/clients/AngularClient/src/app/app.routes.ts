import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { guestGuard } from './auth/guest.guard';

export const routes: Routes = [
    {
        path: '',
        canActivate: [authGuard],
        loadComponent: () => import('./home/home').then(m => m.Home)
    },
    {
        path: 'login',
        canActivate: [guestGuard],
        loadComponent: () => import('./auth/login/login').then(m => m.Login)
    },
    {
        path: 'register',
        canActivate: [guestGuard],
        loadComponent: () => import('./auth/register/register').then(m => m.Register)
    },
    {
        path: 'search',
        canActivate: [authGuard],
        loadComponent: () => import('./movies/search/search').then(m => m.Search)
    },
    {
        path: 'movie/:movieId',
        canActivate: [authGuard],
        loadComponent: () => import('./movies/movie-detail/movie-detail').then(m => m.MovieDetail)
    },
    {
        path: '**',
        redirectTo: '',
        pathMatch: 'full'
    }
];
