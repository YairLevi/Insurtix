using Api.Models;

namespace Api.Repositories;

public class InMemoryBooksRepository : IBooksRepository
{
    private readonly List<Book> _books = [];

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
