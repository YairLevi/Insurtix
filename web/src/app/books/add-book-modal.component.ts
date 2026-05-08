import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BooksService } from '../services/books.service';
import { Book } from '../models/book';
import { ButtonComponent } from '../components/button.component';
import { TextInputComponent } from '../components/text-input.component';

@Component({
  selector: 'app-add-book-modal',
  standalone: true,
  imports: [FormsModule, ButtonComponent, TextInputComponent],
  templateUrl: './add-book-modal.component.html',
})
export class AddBookModalComponent {
  readonly show = signal(false);
  addDraft = this.emptyDraft();
  readonly addError = signal('');
  isAdding = false;

  constructor(private booksService: BooksService) {}

  open() {
    this.addDraft = this.emptyDraft();
    this.addError.set('');
    this.show.set(true);
  }

  close() {
    this.show.set(false);
  }

  generateIsbn() {
    this.addDraft.isbn = Array.from({ length: 13 }, () => Math.floor(Math.random() * 10)).join('');
  }

  submit() {
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
      next: () => { this.isAdding = false; this.close(); },
      error: () => { this.isAdding = false; this.close(); },
    });
  }

  private emptyDraft() {
    return { isbn: '', title: '', authors: '', category: '', year: new Date().getFullYear(), price: 0 };
  }
}
