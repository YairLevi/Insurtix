namespace Api.Services;

using Api.Models;

public class BooksStore
{
    public List<Book> Books { get; private set; } = [];
    public void Replace(List<Book> books) => Books = books;
}
