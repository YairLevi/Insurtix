namespace Api.Books;

public class BooksService(IBooksRepository repository) : IBooksService
{
    public List<Book> GetAll()         => repository.GetAll();
    public Book? GetByIsbn(string isbn) => repository.GetByIsbn(isbn);
    public void Add(Book book)          => repository.Add(book);
    public void Load(string xmlContent) => repository.Load(xmlContent);

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
