using Api.Books;
using Api.Settings;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using Xunit;

namespace Api.Tests.Unit;

public class FileBooksRepositoryTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    public void Dispose() => File.Delete(_tempFile);

    private FileBooksRepository RepoFromXml(string xml)
    {
        File.WriteAllText(_tempFile, xml);
        return new FileBooksRepository(Options.Create(new BookstoreSettings { FilePath = _tempFile }));
    }

    private FileBooksRepository EmptyRepo() =>
        new(Options.Create(new BookstoreSettings { FilePath = _tempFile }));

    private static Book Make(string isbn, string title = "Title") =>
        new(isbn, title, ["Author"], "fiction", null, 2020, 9.99m);

    // --- startup load ---

    [Fact]
    public void Load_FileNotExists_ReturnsEmpty()
    {
        File.Delete(_tempFile);
        var repo = new FileBooksRepository(Options.Create(new BookstoreSettings { FilePath = _tempFile }));
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Load_ValidXml_ReturnsBooks()
    {
        var repo = RepoFromXml("""
            <?xml version="1.0"?>
            <bookstore>
              <book category="tech">
                <isbn>isbn-1</isbn>
                <title>Book One</title>
                <author>Author</author>
                <year>2020</year>
                <price>9.99</price>
              </book>
            </bookstore>
            """);

        Assert.Single(repo.GetAll());
        Assert.Equal("isbn-1", repo.GetAll()[0].Isbn);
    }

    [Fact]
    public void Load_InvalidXml_ReturnsEmpty()
    {
        File.WriteAllText(_tempFile, "not xml");
        var repo = new FileBooksRepository(Options.Create(new BookstoreSettings { FilePath = _tempFile }));
        Assert.Empty(repo.GetAll());
    }

    // --- mutations persist to file ---

    [Fact]
    public void Add_AddsBook_AndPersists()
    {
        var repo = EmptyRepo();
        repo.Add(Make("isbn-1"));

        Assert.Single(repo.GetAll());
        var reloaded = EmptyRepo();
        Assert.Single(reloaded.GetAll());
        Assert.Equal("isbn-1", reloaded.GetAll()[0].Isbn);
    }

    [Fact]
    public void Update_ReplacesBook_AndPersists()
    {
        var repo = RepoFromXml("""
            <?xml version="1.0"?>
            <bookstore>
              <book category="tech">
                <isbn>isbn-1</isbn><title>Old</title><author>A</author><year>2020</year><price>1</price>
              </book>
            </bookstore>
            """);

        repo.Update(new("isbn-1", "New", ["A"], "tech", null, 2020, 1m));

        Assert.Equal("New", repo.GetByIsbn("isbn-1")!.Title);
        var reloaded = EmptyRepo();
        Assert.Equal("New", reloaded.GetByIsbn("isbn-1")!.Title);
    }

    [Fact]
    public void Delete_RemovesBook_AndPersists()
    {
        var repo = RepoFromXml("""
            <?xml version="1.0"?>
            <bookstore>
              <book category="tech">
                <isbn>isbn-1</isbn><title>T</title><author>A</author><year>2020</year><price>1</price>
              </book>
            </bookstore>
            """);

        repo.Delete("isbn-1");

        Assert.Empty(repo.GetAll());
        var reloaded = EmptyRepo();
        Assert.Empty(reloaded.GetAll());
    }

    [Fact]
    public void ReplaceAll_ReplacesAll_AndPersists()
    {
        var repo = EmptyRepo();
        repo.Add(Make("isbn-old"));

        repo.ReplaceAll([Make("isbn-new", "New")]);

        Assert.Single(repo.GetAll());
        Assert.Equal("isbn-new", repo.GetAll()[0].Isbn);
        var reloaded = EmptyRepo();
        Assert.Single(reloaded.GetAll());
        Assert.Equal("isbn-new", reloaded.GetAll()[0].Isbn);
    }
}
