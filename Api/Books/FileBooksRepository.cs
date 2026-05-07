using Api.Settings;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;

namespace Api.Books;

public class FileBooksRepository : IBooksRepository
{
    private readonly string _filePath;
    private readonly HashSet<Book> _books;
    private readonly object _lock = new();
    private static readonly XmlSerializer _serializer = new(typeof(BookstoreData));
    private static readonly XmlSerializerNamespaces _ns = BuildNs();

    public FileBooksRepository(IOptions<BookstoreSettings> settings)
    {
        _filePath = settings.Value.FilePath;
        _books = ReadFromFile(_filePath);
    }

    private static XmlSerializerNamespaces BuildNs()
    {
        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        return ns;
    }

    private static HashSet<Book> ReadFromFile(string path)
    {
        if (!File.Exists(path)) return [];
        try
        {
            using var stream = File.OpenRead(path);
            var data = (BookstoreData)_serializer.Deserialize(stream)!;
            return data.Books
                .Select(b => new Book(b.Isbn, b.Title, b.Authors, b.Category, b.Cover, b.Year, b.Price))
                .ToHashSet();
        }
        catch
        {
            return [];
        }
    }

    private void Save()
    {
        var data = new BookstoreData
        {
            Books = _books.Select(b => new BookData
            {
                Isbn = b.Isbn, Title = b.Title, Authors = b.Authors,
                Category = b.Category, Cover = b.Cover, Year = b.Year, Price = b.Price
            }).ToList()
        };
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        using var stream = File.Create(_filePath);
        _serializer.Serialize(stream, data, _ns);
    }

    public List<Book> GetAll()         { lock (_lock) return [.. _books]; }
    public Book? GetByIsbn(string isbn) { lock (_lock) return _books.FirstOrDefault(b => b.Isbn == isbn); }

    public void Add(Book book)
    {
        lock (_lock) { _books.Add(book); Save(); }
    }

    public void Update(Book book)
    {
        lock (_lock) { if (_books.Remove(book)) { _books.Add(book); Save(); } }
    }

    public void Delete(string isbn)
    {
        lock (_lock) { _books.RemoveWhere(b => b.Isbn == isbn); Save(); }
    }

    public void ReplaceAll(IEnumerable<Book> books)
    {
        lock (_lock)
        {
            _books.Clear();
            foreach (var b in books)
                _books.Add(b);
            Save();
        }
    }
}
