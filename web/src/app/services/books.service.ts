import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Book } from '../models/book';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BooksService {
  private readonly base = environment.apiUrl;

  // Owned state
  readonly state = signal<'idle' | 'uploading' | 'loading' | 'done' | 'error'>('idle');
  readonly books = signal<Book[]>([]);
  readonly errorMsg = signal('');

  // Sort / filter — written by component, read by displayedBooks
  readonly filterText = signal('');
  readonly filterCategory = signal('');
  readonly sortKey = signal<keyof Book | null>(null);
  readonly sortDir = signal<'asc' | 'desc'>('asc');

  readonly displayedBooks = computed(() => {
    const text = this.filterText().toLowerCase();
    const cat = this.filterCategory();

    let result = this.books().filter(b => {
      const matchText =
        !text ||
        b.title.toLowerCase().includes(text) ||
        b.authors.some(a => a.toLowerCase().includes(text));
      const matchCat = !cat || b.category === cat;
      return matchText && matchCat;
    });

    const key = this.sortKey();
    const dir = this.sortDir();
    if (key) {
      result = [...result].sort((a, b) => {
        const av = a[key] as string | number;
        const bv = b[key] as string | number;
        if (av === bv) return 0;
        const cmp = av > bv ? 1 : -1;
        return dir === 'asc' ? cmp : -cmp;
      });
    }
    return result;
  });

  constructor(private http: HttpClient) {}

  loadBooks(): void {
    this.state.set('loading');
    this.http.get<Book[]>(`${this.base}/books`).subscribe({
      next: results => {
        this.books.set(results);
        this.state.set(results.length > 0 ? 'done' : 'idle');
      },
      error: err => {
        this.state.set('error');
        this.errorMsg.set(err.error?.error ?? 'Failed to load books.');
      },
    });
  }

  uploadXml(file: File): void {
    this.state.set('uploading');
    file.text().then(content => {
      this.http
        .post(`${this.base}/books/load`, content, { headers: { 'Content-Type': 'application/xml' } })
        .subscribe({
          next: () => this.loadBooks(),
          error: err => {
            this.state.set('error');
            this.errorMsg.set(err.error ?? 'Upload failed.');
          },
        });
    });
  }

  exportAs(format: 'xml' | 'html'): Observable<Blob> {
    return this.http.get(`${this.base}/reports/${format}`, { responseType: 'blob' });
  }

  addBook(book: Book): Observable<Book> {
    return this.http.post<Book>(`${this.base}/books`, book).pipe(
      tap(created => {
        this.books.update(list => [...list, created]);
        this.state.set('done');
      }),
    );
  }

  updateBook(isbn: string, book: Book): Observable<void> {
    return this.http.put<void>(`${this.base}/books/${isbn}`, book);
  }

  deleteBook(isbn: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/books/${isbn}`);
  }

  replaceBook(isbn: string, updated: Book): void {
    this.books.update(list => list.map(b => (b.isbn === isbn ? updated : b)));
  }

}
