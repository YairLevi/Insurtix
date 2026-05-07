using Api.Models;

namespace Api.Repositories;

public class InMemoryBooksRepository : IBooksRepository
{
    private readonly HashSet<Book> _books = [];

    public List<Book> GetAll() => [.. _books];
    public Book? GetByIsbn(string isbn) => _books.FirstOrDefault(b => b.Isbn == isbn);
    public void Add(Book book) => _books.Add(book);
    public void Update(Book book) { _books.Remove(book); _books.Add(book); }
    public void Delete(string isbn) => _books.RemoveWhere(b => b.Isbn == isbn);
}
