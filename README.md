# Insurtix — Bookstore Management

Web app for managing a bookstore inventory. Load books from an XML file, edit them, filter and sort the table, and export reports.

## Features

- **Load data** — import an XML file to populate the inventory, or run it locally, modifying an existing file.
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

## Running with Docker

Web on port **80**, API on port **8080**.

### Build the images

```bash
docker build -f Api/Dockerfile -t insurtix-api .
docker build -f web/Dockerfile -t insurtix-web .
```

### Run the images

```bash
docker run -p 8080:8080 insurtix-api
docker run -p 80:80 insurtix-web
```

### Run with docker compose

```bash
docker compose up --build
```

- Web: http://localhost
- API: http://localhost:8080

### Override values

| Variable | What it sets |
|---|---|
| `Bookstore__FilePath` | XML file path inside the API container |
| `ASPNETCORE_URLS` | API listening URL/port |
| `API_URL` | Backend URL the web tells the browser to call |

**With `docker run`:**

```bash
docker run -p 8080:8080 -e Bookstore__FilePath=/app/data/mybooks.xml insurtix-api
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://+:5000 -p 5000:5000 insurtix-api
docker run -p 80:80 -e API_URL=http://api.example.com insurtix-web
```

**With docker compose:**

Set the host shell variable, then bring the stack up with `--build` so the new value is picked up:

```bash
BOOKSTORE_FILE_PATH=/app/data/mybooks.xml docker compose up --build
```

