using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BooksController(BooksStore store) : ControllerBase
{
    private static readonly XmlSerializer _serializer = new(typeof(BookstoreXml));

    [HttpPost("upload")]
    public IActionResult Upload(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var bookstore = (BookstoreXml)_serializer.Deserialize(stream)!;
            var books = bookstore.Books
                .Select(b => new Book(b.Isbn, b.Title, b.Authors, b.Category, b.Cover, b.Year, b.Price))
                .ToList();

            store.Replace(books);
            return Ok(new { count = books.Count });
        }
        catch
        {
            return BadRequest(new { error = "Invalid XML format" });
        }
    }

    [HttpGet]
    public IActionResult GetBooks() => Ok(store.Books);

    [HttpPost("export")]
    public IActionResult Export([FromBody] List<Book> books)
    {
        var bookstore = new BookstoreXml
        {
            Books = books.Select(b => new BookXml
            {
                Category = b.Category,
                Cover = b.Cover,
                Isbn = b.Isbn,
                Title = b.Title,
                Authors = b.Authors,
                Year = b.Year,
                Price = b.Price
            }).ToList()
        };

        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");

        var stream = new MemoryStream();
        _serializer.Serialize(stream, bookstore, ns);
        stream.Position = 0;

        return File(stream, "application/xml", "bookstore.xml");
    }
}
