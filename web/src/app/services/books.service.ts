import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book } from '../models/book';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BooksService {
  private readonly base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getBooks(): Observable<Book[]> {
    return this.http.get<Book[]>(`${this.base}/books`);
  }

  uploadXml(file: File): Observable<{ count: number }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ count: number }>(`${this.base}/books/upload`, form);
  }

  exportBooks(books: Book[]): Observable<Blob> {
    return this.http.post(`${this.base}/books/export`, books, { responseType: 'blob' });
  }
}
