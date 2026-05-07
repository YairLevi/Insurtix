namespace Api.Books;

public interface IBooksRepository
{
    List<Book> GetAll();
    Book? GetByIsbn(string isbn);
    void Add(Book book);
    void Update(Book book);
    void Delete(string isbn);
    void ReplaceAll(IEnumerable<Book> books);
}
