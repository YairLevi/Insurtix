import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book } from '../models/book';

@Injectable({ providedIn: 'root' })
export class BooksService {
  constructor(private http: HttpClient) {}

  getBooks(): Observable<Book[]> {
    return this.http.get<Book[]>('http://localhost:5000/books');
  }

  uploadXml(file: File): Observable<{ count: number }> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ count: number }>('http://localhost:5000/books/upload', form);
  }
}
