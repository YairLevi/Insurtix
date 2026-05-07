import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./books/books.component').then(m => m.BooksComponent) },
];
