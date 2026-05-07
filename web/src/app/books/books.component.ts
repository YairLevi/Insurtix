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

  // Pure UI state — stays in component
  editingCell: { isbn: string; field: EditableField } | null = null;
  editingValue = '';
  selectedCurrency = 'USD';
  readonly currencies = ['USD', 'EUR', 'GBP', 'JPY', 'ILS'];
  readonly currencySymbols: Record<string, string> = { USD: '$', EUR: '€', GBP: '£', JPY: '¥', ILS: '₪' };
  isExporting = false;
  savingIsbn: string | null = null;

  // Add modal
  showAddModal = signal(false);
  addDraft = this.emptyDraft();
  addError = signal('');
  isAdding = false;

  constructor(readonly booksService: BooksService) {}

  @HostListener('document:mousedown', ['$event'])
  onDocumentMousedown(event: MouseEvent) {
    if (!this.editingCell) return;
    const container = this.tableContainer?.nativeElement;
    if (container && !container.contains(event.target as Node)) {
      this.commitEdit();
    }
  }

  ngOnInit() {
    this.booksService.loadBooks();
  }

  onUploadClick() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.booksService.uploadXml(file);
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
    const original = this.booksService.books().find(b => b.isbn === isbn);
    if (!original) { this.editingCell = null; this.editingValue = ''; return; }

    let updated: Book = { ...original };
    if (field === 'authors') {
      updated = { ...original, authors: this.editingValue.split(',').map(a => a.trim()).filter(Boolean) };
    } else if (field === 'year') {
      const v = parseInt(this.editingValue, 10);
      updated = { ...original, year: isNaN(v) ? original.year : v };
    } else if (field === 'price') {
      const v = parseFloat(this.editingValue);
      updated = { ...original, price: isNaN(v) ? original.price : v };
    } else {
      updated = { ...original, [field]: this.editingValue };
    }

    this.editingCell = null;
    this.editingValue = '';

    this.booksService.replaceBook(isbn, updated); // optimistic
    this.savingIsbn = isbn;
    this.booksService.updateBook(isbn, updated).subscribe({
      next: () => { this.savingIsbn = null; },
      error: () => { this.savingIsbn = null; this.booksService.replaceBook(isbn, original); },
    });
  }

  isEditing(isbn: string, field: EditableField): boolean {
    return this.editingCell?.isbn === isbn && this.editingCell?.field === field;
  }

  onInputKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === 'Escape') this.commitEdit();
  }

  generateIsbn() {
    this.addDraft.isbn = Array.from({ length: 13 }, () => Math.floor(Math.random() * 10)).join('');
  }

  private emptyDraft() {
    return { isbn: '', title: '', authors: '', category: '', year: new Date().getFullYear(), price: 0 };
  }

  openAddModal() {
    this.addDraft = this.emptyDraft();
    this.addError.set('');
    this.showAddModal.set(true);
  }

  closeAddModal() {
    this.showAddModal.set(false);
  }

  submitAdd() {
    const isbn = this.addDraft.isbn.trim();
    const title = this.addDraft.title.trim();
    if (!isbn || !title) {
      this.addError.set('ISBN and title are required.');
      return;
    }
    const book: Book = {
      isbn,
      title,
      authors: this.addDraft.authors.split(',').map(a => a.trim()).filter(Boolean),
      category: this.addDraft.category.trim(),
      year: this.addDraft.year,
      price: this.addDraft.price,
      cover: null,
    };
    this.isAdding = true;
    this.booksService.addBook(book).subscribe({
      next: () => { this.isAdding = false; this.closeAddModal(); },
      error: err => { this.isAdding = false; this.addError.set(err.error?.error ?? 'Failed to add book.'); },
    });
  }

  deleteBook(isbn: string) {
    if (!confirm('Delete this book?')) return;
    this.booksService.books.update(list => list.filter(b => b.isbn !== isbn)); // optimistic
    this.booksService.deleteBook(isbn).subscribe({
      error: () => this.booksService.loadBooks(), // revert by reloading
    });
  }

  onExportClick() {
    this.commitEdit();
    this.isExporting = true;
    this.booksService.exportBooks().subscribe({
      next: async blob => {
        this.isExporting = false;
        const saveFile = (window as any).showSaveFilePicker;
        if (saveFile) {
          try {
            const handle = await saveFile({
              suggestedName: 'bookstore.xml',
              types: [{ description: 'XML File', accept: { 'application/xml': ['.xml'] } }],
            });
            const writable = await handle.createWritable();
            await writable.write(blob);
            await writable.close();
          } catch {
            // user cancelled
          }
        } else {
          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'bookstore.xml';
          a.click();
          URL.revokeObjectURL(url);
        }
      },
      error: () => { this.isExporting = false; },
    });
  }
}
