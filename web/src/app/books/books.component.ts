import { Component, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { BooksService } from '../services/books.service';
import { Book } from '../models/book';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AutoFocusDirective } from '../shared/auto-focus.directive';
import { AddBookModalComponent } from './add-book-modal.component';
import { ButtonComponent } from '../components/button.component';
import { TextInputComponent } from '../components/text-input.component';
import { PlusIconComponent } from '../icons/plus-icon.component';
import { TrashIconComponent } from '../icons/trash-icon.component';
import { FileIconComponent } from '../icons/file-icon.component';

type EditableField = 'title' | 'authors' | 'category' | 'year' | 'price';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AutoFocusDirective,
    AddBookModalComponent,
    ButtonComponent,
    TextInputComponent,
    PlusIconComponent,
    TrashIconComponent,
    FileIconComponent,
  ],
  templateUrl: './books.component.html',
})
export class BooksComponent implements OnInit, OnDestroy {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('addModal') addModal!: AddBookModalComponent;

  // Pure UI state — stays in component
  editingCell: { isbn: string; field: EditableField } | null = null;
  editingValue = '';
  private enterPressed = false;
  readonly sortableColumns: { key: keyof Book; label: string }[] = [
    { key: 'title', label: 'Title' },
    { key: 'authors', label: 'Authors' },
    { key: 'category', label: 'Category' },
    { key: 'year', label: 'Year' },
    { key: 'price', label: 'Price' },
  ];
  isExporting = false;
  showExportModal = signal(false);
  savingIsbn = signal<string | null>(null);

  // Filter inputs — local vars kept in sync with route params
  filterTextInput = '';
  filterCategoryInput = '';

  readonly categories = computed(() =>
    [...new Set(this.booksService.books().map(b => b.category))].sort(),
  );

  private routeSub?: Subscription;

  constructor(
    readonly booksService: BooksService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    this.booksService.loadBooks();
    this.routeSub = this.route.queryParamMap.subscribe(params => {
      const sortKey = (params.get('sort') ?? null) as keyof Book | null;
      const sortDir = params.get('dir') === 'desc' ? 'desc' : 'asc';
      const q = params.get('q') ?? '';
      const cat = params.get('category') ?? '';

      this.booksService.sortKey.set(sortKey);
      this.booksService.sortDir.set(sortDir as 'asc' | 'desc');
      this.booksService.filterText.set(q);
      this.booksService.filterCategory.set(cat);
      this.filterTextInput = q;
      this.filterCategoryInput = cat;
    });
  }

  ngOnDestroy() {
    this.routeSub?.unsubscribe();
  }

  onSortClick(key: keyof Book) {
    const newDir =
      this.booksService.sortKey() === key && this.booksService.sortDir() === 'asc' ? 'desc' : 'asc';
    this.router.navigate([], {
      relativeTo: this.route,
      queryParamsHandling: 'merge',
      queryParams: { sort: key, dir: newDir },
      replaceUrl: true,
    });
  }

  onFilterTextChange(value: string) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParamsHandling: 'merge',
      queryParams: { q: value || null },
      replaceUrl: true,
    });
  }

  onFilterCategoryChange(value: string) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParamsHandling: 'merge',
      queryParams: { category: value || null },
      replaceUrl: true,
    });
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
    this.cancelEdit();
    this.editingCell = { isbn, field };
    this.editingValue = field === 'authors' ? (value as string[]).join(', ') : String(value);
  }

  cancelEdit() {
    this.editingCell = null;
    this.editingValue = '';
    this.enterPressed = false;
  }

  commitEdit() {
    if (!this.editingCell) return;
    const { isbn, field } = this.editingCell;
    const original = this.booksService.books().find(b => b.isbn === isbn);
    if (!original) { this.cancelEdit(); return; }

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

    this.cancelEdit();
    this.savingIsbn.set(isbn);
    this.booksService.replaceBook(isbn, updated);
    this.booksService.updateBook(isbn, updated).subscribe({
      next: () => this.savingIsbn.set(null),
      error: () => { this.savingIsbn.set(null); this.booksService.replaceBook(isbn, original); },
    });
  }

  isEditing(isbn: string, field: EditableField): boolean {
    return this.editingCell?.isbn === isbn && this.editingCell?.field === field;
  }

  onInputKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      this.enterPressed = true;
      (event.target as HTMLElement).blur();
    } else if (event.key === 'Escape') {
      this.cancelEdit();
    }
  }

  onInputBlur() {
    if (this.enterPressed) {
      this.enterPressed = false;
      this.commitEdit();
    } else {
      this.cancelEdit();
    }
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
    this.showExportModal.set(true);
  }

  exportAs(format: 'xml' | 'html') {
    this.showExportModal.set(false);
    this.isExporting = true;
    this.booksService.exportAs(format).subscribe({
      next: async blob => {
        this.isExporting = false;
        const fileName = `bookstore.${format}`;
        const mimeType = format === 'xml' ? 'application/xml' : 'text/html';
        const description = format === 'xml' ? 'XML File' : 'HTML File';
        const ext = `.${format}`;
        const saveFile = (window as any).showSaveFilePicker;
        if (saveFile) {
          try {
            const handle = await saveFile({
              suggestedName: fileName,
              types: [{ description, accept: { [mimeType]: [ext] } }],
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
          a.download = fileName;
          a.click();
          URL.revokeObjectURL(url);
        }
      },
      error: () => { this.isExporting = false; },
    });
  }
}
