using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BooksController(IBooksService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(service.GetAll());

    [HttpGet("{isbn}")]
    public IActionResult GetByIsbn(string isbn)
    {
        var book = service.GetByIsbn(isbn);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public IActionResult Add([FromBody] Book book)
    {
        service.Add(book);
        return CreatedAtAction(nameof(GetByIsbn), new { isbn = book.Isbn }, book);
    }

    [HttpPut("{isbn}")]
    public IActionResult Update(string isbn, [FromBody] Book book)
    {
        return service.Update(isbn, book) ? NoContent() : NotFound();
    }

    [HttpDelete("{isbn}")]
    public IActionResult Delete(string isbn)
    {
        return service.Delete(isbn) ? NoContent() : NotFound();
    }

    [HttpGet("export/xml")]
    public IActionResult ExportXml()
    {
        return File(service.ExportXml(), "application/xml", "bookstore.xml");
    }

    [HttpGet("export/html")]
    public IActionResult ExportHtml()
    {
        return File(Encoding.UTF8.GetBytes(service.ExportHtml()), "text/html", "bookstore.html");
    }
}
