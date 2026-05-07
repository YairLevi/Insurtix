# Frontend Features Plan

## Architecture: Centralized Service State

All HTTP calls and book state live in `BooksService`. Components only read signals and call service methods.

### Changes to `web/src/app/services/books.service.ts`

Move `books`, `state`, `errorMsg` signals here. Add sorting/filtering signals and a `computed` `displayedBooks`.

```typescript
// State owned by service
books = signal<Book[]>([]);
state = signal<'idle' | 'loading' | 'uploading' | 'done' | 'error'>('idle');
errorMsg = signal('');

// Sorting/filtering signals
filterText = signal('');
filterCategory = signal('');
sortKey = signal<keyof Book | null>(null);
sortDir = signal<'asc' | 'desc'>('asc');

// Derived — what the table actually renders
displayedBooks = computed(() => { /* filter + sort books() */ });
```

`BooksComponent` removes its own `books`/`state`/`errorMsg` signals and reads from the service instead.

---

## Feature 1: Add Book

**Assumed endpoint:** `POST /books` — body: `Book` — returns created `Book`

**UI:**
- "Add Book" button in header
- Modal form with fields: ISBN, Title, Authors (comma-separated), Category, Year, Price
- Submit → calls service → closes modal on success

**Service method:**
```typescript
addBook(book: Book): Observable<Book>
// On success: books.update(list => [...list, newBook])
```

**Component changes in `books.component.ts`:**
- Add `showAddModal = signal(false)`
- `addBookDraft` plain object for form binding
- Call `booksService.addBook(draft)` on submit

---

## Feature 2: Edit & Update Book (backend sync)

Currently edits are local-only. Change: on cell blur/save, persist to backend.

**Assumed endpoint:** `PUT /books/:isbn` — body: updated `Book` — returns updated `Book`

**Service method:**
```typescript
updateBook(isbn: string, changes: Partial<Book>): Observable<Book>
// On success: books.update(list => list.map(b => b.isbn === isbn ? updated : b))
```

**Component changes:**
- On edit confirm (Enter / blur), call `booksService.updateBook(isbn, { field: value })` instead of doing a local update
- Show per-row saving indicator (small spinner in cell while request is in flight)

---

## Feature 3: Delete Book

**Assumed endpoint:** `DELETE /books/:isbn` — returns 204 or `{ success: true }`

**Service method:**
```typescript
deleteBook(isbn: string): Observable<void>
// On success: books.update(list => list.filter(b => b.isbn !== isbn))
```

**Component changes:**
- Add Delete button (trash icon) to each row
- Confirm before delete (`window.confirm` is fine)
- Call `booksService.deleteBook(isbn)`

---

## Feature 4: Sorting

Frontend-only — no backend call. State lives in `BooksService`.

**Signals (in service):**
```
sortKey  = signal<keyof Book | null>(null)
sortDir  = signal<'asc' | 'desc'>('asc')
```

**`displayedBooks` computed applies sort after filter:**
```typescript
const key = sortKey();
const dir = sortDir();
if (!key) return filtered;
return [...filtered].sort((a, b) => {
  const av = a[key], bv = b[key];
  return dir === 'asc' ? (av > bv ? 1 : -1) : (av < bv ? 1 : -1);
});
```

**Service helper:**
```typescript
setSortKey(key: keyof Book) {
  if (sortKey() === key) sortDir.update(d => d === 'asc' ? 'desc' : 'asc');
  else { sortKey.set(key); sortDir.set('asc'); }
}
```

**Component:** Column headers become clickable; show ▲/▼ indicator based on `sortKey`/`sortDir`.

---

## Feature 5: Filtering

Frontend-only — no backend call. State lives in `BooksService`.

**Signals (in service):**
```
filterText     = signal('')   // searches title + authors
filterCategory = signal('')   // exact category match
```

**`displayedBooks` computed applies filter before sort:**
```typescript
const text = filterText().toLowerCase();
const cat  = filterCategory();
return books().filter(b => {
  const matchText = !text
    || b.title.toLowerCase().includes(text)
    || b.authors.some(a => a.toLowerCase().includes(text));
  const matchCat = !cat || b.category === cat;
  return matchText && matchCat;
});
```

**Component:**
- Text search input above table bound via `[(ngModel)]` → `booksService.filterText.set(v)`
- Category dropdown populated from unique categories derived from `books()`

---

## File Changeset Summary

| File | Change |
|------|--------|
| `web/src/app/services/books.service.ts` | Own all signals + computed; add `addBook`, `updateBook`, `deleteBook`; add sort/filter helpers |
| `web/src/app/books/books.component.ts` | Drop local signals; read from service; wire modal, delete, sort clicks, filter inputs |
| `web/src/app/books/books.component.html` | Add Book modal, Delete buttons, clickable sort headers, filter/search bar |
| `web/src/app/models/book.ts` | No change |

---

## Verification (per feature)

1. **Add:** Fill modal → submit → new row appears without page reload
2. **Edit:** Click cell → change value → press Enter → network tab shows `PUT /books/:isbn` → value persists on page refresh
3. **Delete:** Click delete → confirm → row removed → network tab shows `DELETE /books/:isbn`
4. **Sort:** Click column header → rows reorder → click again → direction reverses
5. **Filter:** Type in search box → table narrows in real time; select category → filters by category
