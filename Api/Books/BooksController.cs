using Microsoft.AspNetCore.Mvc;

namespace Api.Books;

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

    [HttpPost("load")]
    public async Task<IActionResult> Load()
    {
        using var reader = new StreamReader(Request.Body);
        var content = await reader.ReadToEndAsync();
        try
        {
            service.Load(content);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
