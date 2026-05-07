using Api.Models;

namespace Api.Repositories;

public class InMemoryBooksRepository : IBooksRepository
{
    private List<Book> _books = [];

    public List<Book> GetAll() => _books;
    public void ReplaceAll(List<Book> books) => _books = books;
}
