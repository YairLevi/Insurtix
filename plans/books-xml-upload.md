# Plan: Bookstore XML Upload Feature

## Context

User uploads an XML file describing a bookstore inventory. The API parses the XML and stores books in memory (no database). The web shows a loading spinner during processing, then renders the books in a modern table.

---

## API Contract

> Both agents implement against this contract. Neither deviates from it.

Base URL (dev): `http://localhost:5000`

### POST /books/upload

Upload and parse an XML file. Replaces any previously loaded books.

**Request:**
```
Content-Type: multipart/form-data
Field name:   file
```

**Response 200:**
```json
{ "count": 3 }
```

**Response 400:**
```json
{ "error": "Invalid XML format" }
```

---

### GET /books

Return all books currently in memory.

**Response 200:**
```json
[
  {
    "isbn": "9051234567897",
    "title": "Harry Potter",
    "authors": ["J K. Rowling"],
    "category": "children",
    "cover": null,
    "year": 2005,
    "price": 29.99
  },
  {
    "isbn": "9031234567897",
    "title": "XQuery Kick Start",
    "authors": ["James McGovern", "Per Bothner", "Kurt Cagle", "James Linn", "Vaidyanathan Nagarajan"],
    "category": "web",
    "cover": null,
    "year": 2003,
    "price": 49.99
  }
]
```

> `cover` is a nullable string from the XML `cover` attribute. Absent = `null`.

---

## Agent Split

Spawn two agents in parallel after approving this plan.

---

### Agent A — API

**Directory:** `C:\Git\Assignments\Insurtix\api\`

**Files:**

| File | Action |
|------|--------|
| `Models/Book.cs` | Create |
| `Services/BooksStore.cs` | Create |
| `Controllers/BooksController.cs` | Create |
| `Program.cs` | Modify — register singleton |

**Book model:**
```csharp
public record Book(
    string Isbn,
    string Title,
    List<string> Authors,
    string Category,
    string? Cover,
    int Year,
    decimal Price
);
```

**BooksStore (singleton):**
```csharp
public class BooksStore
{
    public List<Book> Books { get; private set; } = [];
    public void Replace(List<Book> books) => Books = books;
}
```

**BooksController:**
- `POST /books/upload` — receive `IFormFile file`, parse XML via `System.Xml.Linq`, call `store.Replace(...)`, return `{ count }`
- `GET /books` — return `store.Books`

**XML parsing rules:**
- Each `<book>` element → one `Book`
- `category` ← attribute `category`
- `cover` ← attribute `cover` (nullable, absent = null)
- `isbn` ← `<isbn>` child text
- `title` ← `<title>` child text (ignore `lang` attribute)
- `authors` ← ALL `<author>` children (multiple = multiple entries)
- `year` ← `<year>` child text → `int`
- `price` ← `<price>` child text → `decimal`

**Registration in `Program.cs`:**
```csharp
builder.Services.AddSingleton<BooksStore>();
```

---

### Agent B — Web

**Directory:** `C:\Git\Assignments\Insurtix\web\src\app\`

**Files:**

| File | Action |
|------|--------|
| `models/book.ts` | Create |
| `services/books.service.ts` | Create |
| `books/books.component.ts` | Create |
| `books/books.component.html` | Create |
| `app.routes.ts` | Modify — add `/books` route |
| `app.html` | Modify — add `<router-outlet>` if missing |

**TypeScript interface:**
```typescript
export interface Book {
  isbn: string;
  title: string;
  authors: string[];
  category: string;
  cover: string | null;
  year: number;
  price: number;
}
```

**BooksService (inject HttpClient):**
```typescript
getBooks(): Observable<Book[]>
// GET http://localhost:5000/books

uploadXml(file: File): Observable<{ count: number }>
// POST http://localhost:5000/books/upload  (FormData, field = 'file')
```

**Component state (Angular signals):**
```
state: signal<'idle' | 'uploading' | 'loading' | 'done' | 'error'>
books: signal<Book[]>
errorMsg: signal<string>
```

**Component behavior:**
1. On init: `GET /books` → if results, set books + state=done; else state=idle
2. User clicks "Upload XML" → file input triggers
3. On file selected: `POST /books/upload`, state=uploading
4. On upload success: `GET /books`, state=loading → set books, state=done
5. On any error: state=error, set errorMsg

**UI layout:**
- Header bar: app title left, "Upload XML" button right (visible in all states)
- `uploading` / `loading`: centered animated spinner
- `error`: red banner with errorMsg
- `done` or `idle` with books: table (see below)
- `idle` with no books: centered empty-state message — "Upload an XML file to get started"

**Table columns:** Title | Authors | Category | Year | Price
- Authors: `authors.join(', ')`
- Price: formatted as currency (e.g. `$29.99`)

**Styling (Tailwind CSS):**
- White card with shadow and rounded corners wrapping the table
- Alternating row background (striped)
- Row hover highlight
- Column headers: uppercase, small, muted color, semibold
- "Upload XML" button: solid accent color, rounded, hover darkens
- Spinner: simple Tailwind animate-spin border circle

---

## Verification

1. `dotnet run` in `api/` → listens on `http://localhost:5000`
2. `ng serve` in `web/` → opens `http://localhost:4200`
3. Navigate to `http://localhost:4200/books`
4. Page shows empty state with upload prompt
5. Click "Upload XML" → select sample XML → spinner appears
6. Table loads with 3 books:
   - Harry Potter | J K. Rowling | children | 2005 | $29.99
   - XQuery Kick Start | James McGovern, Per Bothner, ... | web | 2003 | $49.99
   - Learning XML | Erik T. Ray | web | 2003 | $39.95
7. Upload new XML → spinner → table refreshes
