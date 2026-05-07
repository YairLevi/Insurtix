using Api.Models;
using Api.Settings;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;

namespace Api.Repositories;

public class FileBooksRepository : IBooksRepository
{
    private readonly HashSet<Book> _books;
    private static readonly XmlSerializer _serializer = new(typeof(BookstoreData));

    public FileBooksRepository(IOptions<BookstoreSettings> settings)
    {
        _books = Load(settings.Value.FilePath);
    }

    private static HashSet<Book> Load(string path)
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

    public List<Book> GetAll() => [.. _books];
    public Book? GetByIsbn(string isbn) => _books.FirstOrDefault(b => b.Isbn == isbn);
    public void Add(Book book) => _books.Add(book);
    public void Update(Book book) { if (_books.Remove(book)) _books.Add(book); }
    public void Delete(string isbn) => _books.RemoveWhere(b => b.Isbn == isbn);
}
