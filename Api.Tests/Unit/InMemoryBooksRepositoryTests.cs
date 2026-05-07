using Api.Models;
using Api.Repositories;
using Xunit;

namespace Api.Tests.Unit;

public class InMemoryBooksRepositoryTests
{
    private static Book Make(string isbn, string title = "Title") =>
        new(isbn, title, ["Author"], "fiction", null, 2020, 9.99m);

    [Fact]
    public void GetAll_Initially_ReturnsEmpty()
    {
        var repo = new InMemoryBooksRepository();
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Add_AddsBook()
    {
        var repo = new InMemoryBooksRepository();
        repo.Add(Make("isbn-1"));
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void Add_DuplicateIsbn_DoesNotAdd()
    {
        var repo = new InMemoryBooksRepository();
        repo.Add(Make("isbn-1", "First"));
        repo.Add(Make("isbn-1", "Second"));
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void GetByIsbn_ReturnsBook_WhenExists()
    {
        var repo = new InMemoryBooksRepository();
        repo.Add(Make("isbn-1"));
        Assert.NotNull(repo.GetByIsbn("isbn-1"));
    }

    [Fact]
    public void GetByIsbn_ReturnsNull_WhenMissing()
    {
        var repo = new InMemoryBooksRepository();
        Assert.Null(repo.GetByIsbn("nonexistent"));
    }

    [Fact]
    public void Update_ReplacesBook()
    {
        var repo = new InMemoryBooksRepository();
        repo.Add(Make("isbn-1", "Old Title"));
        repo.Update(Make("isbn-1", "New Title"));
        Assert.Equal("New Title", repo.GetByIsbn("isbn-1")!.Title);
    }

    [Fact]
    public void Update_NonExistent_DoesNotAdd()
    {
        var repo = new InMemoryBooksRepository();
        repo.Update(Make("isbn-1"));
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Delete_RemovesBook()
    {
        var repo = new InMemoryBooksRepository();
        repo.Add(Make("isbn-1"));
        repo.Delete("isbn-1");
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Delete_NonExistent_DoesNotThrow()
    {
        var repo = new InMemoryBooksRepository();
        var ex = Record.Exception(() => repo.Delete("nonexistent"));
        Assert.Null(ex);
    }
}
