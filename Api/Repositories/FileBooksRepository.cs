using Api.Models;
using Api.Settings;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;

namespace Api.Repositories;

public class FileBooksRepository : IBooksRepository
{
    private readonly List<Book> _books;
    private static readonly XmlSerializer _serializer = new(typeof(BookstoreData));

    public FileBooksRepository(IOptions<BookstoreSettings> settings)
    {
        _books = Load(settings.Value.FilePath);
    }

    private static List<Book> Load(string path)
    {
        if (!File.Exists(path)) return [];
        try
        {
            using var stream = File.OpenRead(path);
            var data = (BookstoreData)_serializer.Deserialize(stream)!;
            return data.Books
                .Select(b => new Book(b.Isbn, b.Title, b.Authors, b.Category, b.Cover, b.Year, b.Price))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public List<Book> GetAll() => _books;
    public Book? GetByIsbn(string isbn) => _books.FirstOrDefault(b => b.Isbn == isbn);
    public void Add(Book book) => _books.Add(book);
    public void Update(Book book)
    {
        var i = _books.FindIndex(b => b.Isbn == book.Isbn);
        if (i >= 0) _books[i] = book;
    }
    public void Delete(string isbn) => _books.RemoveAll(b => b.Isbn == isbn);
}
