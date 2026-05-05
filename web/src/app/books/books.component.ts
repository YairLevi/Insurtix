import { Component, OnInit, signal, ViewChild, ElementRef } from '@angular/core';
import { BooksService } from '../services/books.service';
import { Book } from '../models/book';
import { CommonModule, CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './books.component.html',
})
export class BooksComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  state = signal<'idle' | 'uploading' | 'loading' | 'done' | 'error'>('idle');
  books = signal<Book[]>([]);
  errorMsg = signal<string>('');

  constructor(private booksService: BooksService) {}

  ngOnInit() {
    this.booksService.getBooks().subscribe({
      next: (results) => {
        if (results.length > 0) {
          this.books.set(results);
          this.state.set('done');
        } else {
          this.state.set('idle');
        }
      },
      error: () => this.state.set('idle'),
    });
  }

  onUploadClick() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.state.set('uploading');
    this.booksService.uploadXml(file).subscribe({
      next: () => {
        this.state.set('loading');
        this.booksService.getBooks().subscribe({
          next: (results) => {
            this.books.set(results);
            this.state.set('done');
          },
          error: (err) => {
            this.state.set('error');
            this.errorMsg.set(err.error?.error ?? 'Failed to load books.');
          },
        });
      },
      error: (err) => {
        this.state.set('error');
        this.errorMsg.set(err.error?.error ?? 'Upload failed.');
      },
    });
  }
}
