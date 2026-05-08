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

## Running with Docker

Builds and runs the full stack from `docker-compose.yml`. Web on **port 80**, API on **port 8080**.

```bash
docker compose up --build
```

- Web:  http://localhost
- API:  http://localhost:8080

### Build images individually

```bash
docker build -f Api/Dockerfile -t insurtix-api .
docker build -f web/Dockerfile -t insurtix-web .
```

### Run images individually

```bash
docker run -p 8080:8080 insurtix-api
docker run -p 80:80 insurtix-web
```

### Overrides

| Variable | Where | Default | Purpose |
|---|---|---|---|
| `BOOKSTORE_FILE_PATH` | compose `.env` or shell | `/app/data/bookstore.xml` | XML file path inside API container |
| `Bookstore__FilePath` | `docker run -e` | `/app/data/bookstore.xml` | Same, for standalone API container |
| `ASPNETCORE_URLS` | `docker run -e` | `http://+:8080` | API listening port |
| `Cors__AllowedOrigins__0` | `docker run -e` | `http://localhost` | Origin allowed by API CORS |
| `API_URL` | `docker run -e` | `http://localhost:8080` | URL the web tells the browser to call |

#### Note on `BOOKSTORE_FILE_PATH` vs `Bookstore__FilePath`

These look similar but live at different layers. In `docker-compose.yml`:

```yaml
Bookstore__FilePath: ${BOOKSTORE_FILE_PATH:-/app/data/bookstore.xml}
```

- `${BOOKSTORE_FILE_PATH}` — **shell** env var on the host. Compose reads it before launching containers.
- `Bookstore__FilePath` — what Compose injects **into the API container** (the ASP.NET config key).

Compose acts as a bridge: takes the shell variable, sets it as the container variable. The `:-/app/data/bookstore.xml` part is the fallback if the shell var is unset.

So when overriding via Compose, set `BOOKSTORE_FILE_PATH`. When running the API container directly with `docker run`, set `Bookstore__FilePath`.

**Examples:**

```bash
# Override bookstore file path (compose)
BOOKSTORE_FILE_PATH=/app/data/mybooks.xml docker compose up

# Mount a host XML file into API container
docker run -p 8080:8080 \
  -e Bookstore__FilePath=/app/data/books.xml \
  -v ./mybooks.xml:/app/data/books.xml \
  insurtix-api

# Point web at a different API URL
docker run -p 80:80 -e API_URL=http://api.example.com insurtix-web

# Run API on a different port
docker run -p 5000:5000 -e ASPNETCORE_URLS=http://+:5000 insurtix-api
```

### Rebuild and clean old images

```bash
docker compose up --build && docker image prune -f
```

