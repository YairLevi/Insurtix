import { Component, OnInit, signal, ViewChild, ElementRef, HostListener, Directive } from '@angular/core';
import { BooksService } from '../services/books.service';
import { Book } from '../models/book';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Directive({ selector: '[appAutoFocus]', standalone: true })
export class AutoFocusDirective implements OnInit {
  constructor(private el: ElementRef) {}
  ngOnInit() { setTimeout(() => this.el.nativeElement.focus(), 0); }
}

type EditableField = 'title' | 'authors' | 'category' | 'year' | 'price';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, FormsModule, AutoFocusDirective],
  templateUrl: './books.component.html',
})
export class BooksComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('tableContainer') tableContainer!: ElementRef<HTMLElement>;

  state = signal<'idle' | 'uploading' | 'loading' | 'done' | 'error'>('idle');
  books = signal<Book[]>([]);
  errorMsg = signal<string>('');

  editingCell: { isbn: string; field: EditableField } | null = null;
  editingValue = '';
  selectedCurrency = 'USD';
  readonly currencies = ['USD', 'EUR', 'GBP', 'JPY', 'ILS'];
  readonly currencySymbols: Record<string, string> = { USD: '$', EUR: '€', GBP: '£', JPY: '¥', ILS: '₪' };

  constructor(private booksService: BooksService) {}

  @HostListener('document:mousedown', ['$event'])
  onDocumentMousedown(event: MouseEvent) {
    if (!this.editingCell) return;
    const container = this.tableContainer?.nativeElement;
    if (container && !container.contains(event.target as Node)) {
      this.commitEdit();
    }
  }

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

  startEdit(isbn: string, field: EditableField, value: string | number | string[]) {
    if (this.editingCell?.isbn === isbn && this.editingCell?.field === field) return;
    this.commitEdit();
    this.editingCell = { isbn, field };
    this.editingValue = field === 'authors' ? (value as string[]).join(', ') : String(value);
  }

  commitEdit() {
    if (!this.editingCell) return;
    const { isbn, field } = this.editingCell;
    this.books.update(list => list.map(book => {
      if (book.isbn !== isbn) return book;
      if (field === 'authors') return { ...book, authors: this.editingValue.split(',').map(a => a.trim()).filter(Boolean) };
      if (field === 'year') { const v = parseInt(this.editingValue, 10); return { ...book, year: isNaN(v) ? book.year : v }; }
      if (field === 'price') { const v = parseFloat(this.editingValue); return { ...book, price: isNaN(v) ? book.price : v }; }
      return { ...book, [field]: this.editingValue };
    }));
    this.editingCell = null;
    this.editingValue = '';
  }

  isEditing(isbn: string, field: EditableField): boolean {
    return this.editingCell?.isbn === isbn && this.editingCell?.field === field;
  }

  onInputKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === 'Escape') this.commitEdit();
  }
}
