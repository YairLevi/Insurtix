# Insurtix — Bookstore Management

Web app for managing a bookstore inventory. Load books from an XML file, edit them, filter and sort the table, and export reports.

## Features

- **Load data** — import an XML file to populate the inventory
- **CRUD** — add, edit (inline), and delete books
- **Filter & sort** — search by title/author, filter by category, sort any column
- **Export reports** — download the current inventory as XML or HTML

## XML File Format

The XML file you load must follow this schema:

```xml
<?xml version="1.0" encoding="utf-8"?>
<bookstore>
  <book category="Fiction" cover="https://example.com/cover.jpg">
    <isbn>978-3-16-148410-0</isbn>
    <title>Example Book</title>
    <author>Jane Doe</author>
    <author>John Smith</author>
    <year>2023</year>
    <price>19.99</price>
  </book>
</bookstore>
```

- `category` and `cover` are XML attributes on `<book>` (`cover` is optional)
- Multiple `<author>` elements are supported
- `<isbn>` must be unique per book

## Running the API

```bash
cd Api
dotnet run
```

Runs on `http://localhost:5293`. By default reads/writes `data/bookstore.xml` relative to the project directory.

### Custom file path

Use the `Bookstore__FilePath` environment variable to point the API at a different XML file.

**PowerShell:**
```powershell
$env:Bookstore__FilePath="C:\path\to\books.xml"; dotnet run
```

**Bash / Command Prompt:**
```bash
Bookstore__FilePath=/path/to/books.xml dotnet run
```

## Running the Web

```bash
cd web
npm install
npm start
```

Opens on `http://localhost:4200`. Expects the API running on port 5293.
