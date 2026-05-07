namespace Api.Books;

public interface IBooksService
{
    List<Book> GetAll();
    Book?      GetByIsbn(string isbn);
    void       Add(Book book);
    bool       Update(string isbn, Book book);
    bool       Delete(string isbn);
    void       Load(string xmlContent);
}
