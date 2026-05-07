using Api.Models;
using Api.Repositories;
using System.Text;
using System.Xml.Serialization;

namespace Api.Services;

public class BooksService(IBooksRepository repository) : IBooksService
{
    private static readonly XmlSerializer _serializer = new(typeof(BookstoreData));

    public List<Book> GetAll() => repository.GetAll();
    public Book? GetByIsbn(string isbn) => repository.GetByIsbn(isbn);
    public void Add(Book book) => repository.Add(book);

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

    public byte[] ExportXml()
    {
        var data = new BookstoreData
        {
            Books = repository.GetAll().Select(b => new BookData
            {
                Isbn = b.Isbn, Title = b.Title, Authors = b.Authors,
                Category = b.Category, Cover = b.Cover, Year = b.Year, Price = b.Price
            }).ToList()
        };

        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");

        using var stream = new MemoryStream();
        _serializer.Serialize(stream, data, ns);
        return stream.ToArray();
    }

    public string ExportHtml()
    {
        const string style = """
            body { font-family: sans-serif; padding: 2rem; background: #fafafa; color: #222; }
            h1 { margin-bottom: 1.5rem; }
            table { border-collapse: collapse; width: 100%; background: #fff; box-shadow: 0 1px 4px rgba(0,0,0,.1); }
            th, td { border: 1px solid #ddd; padding: 10px 14px; text-align: left; }
            th { background: #f0f0f0; font-weight: 600; }
            tr:hover td { background: #f9f9f9; }
            """;

        var rows = string.Join("\n", repository.GetAll().Select(b =>
            $"<tr><td>{b.Isbn}</td><td>{b.Title}</td><td>{string.Join(", ", b.Authors)}</td>" +
            $"<td>{b.Category}</td><td>{b.Year}</td><td>{b.Price:C}</td></tr>"));

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <title>Bookstore</title>
                <style>{style}</style>
            </head>
            <body>
                <h1>Bookstore</h1>
                <table>
                    <thead><tr><th>ISBN</th><th>Title</th><th>Authors</th><th>Category</th><th>Year</th><th>Price</th></tr></thead>
                    <tbody>{rows}</tbody>
                </table>
            </body>
            </html>
            """;
    }
}
