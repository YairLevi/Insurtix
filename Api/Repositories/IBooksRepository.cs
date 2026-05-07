using Api.Models;

namespace Api.Repositories;

public interface IBooksRepository
{
    List<Book> GetAll();
    void ReplaceAll(List<Book> books);
}
