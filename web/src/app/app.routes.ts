import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: 'books', loadComponent: () => import('./books/books.component').then(m => m.BooksComponent) },
];
