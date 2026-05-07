using System.Xml.Serialization;

namespace Api.Books;

public class BooksService(IBooksRepository repository) : IBooksService
{
    private static readonly XmlSerializer _serializer = new(typeof(BookstoreData));

    public List<Book> GetAll()         => repository.GetAll();
    public Book? GetByIsbn(string isbn) => repository.GetByIsbn(isbn);
    public void Add(Book book)          => repository.Add(book);

    public void Load(string xmlContent)
    {
        try
        {
            using var reader = new StringReader(xmlContent);
            var data = (BookstoreData)_serializer.Deserialize(reader)!;
            var books = data.Books.Select(b => new Book(b.Isbn, b.Title, b.Authors, b.Category, b.Cover, b.Year, b.Price));
            repository.ReplaceAll(books);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException("Invalid XML content.");
        }
    }

    public bool Update(string isbn, Book book)
    {
        if (repository.GetByIsbn(isbn) is null) return false;
        repository.Update(book with { Isbn = isbn });
        return true;
    }

    public bool Delete(string isbn)
    {
        if (repository.GetByIsbn(isbn) is null) return false;
        repository.Delete(isbn);
        return true;
    }
}
