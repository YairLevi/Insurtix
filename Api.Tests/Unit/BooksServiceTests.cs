using Api.Models;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Unit;

public class BooksServiceTests
{
    private static Book Make(string isbn, string title = "Title") =>
        new(isbn, title, ["Author"], "fiction", null, 2020, 9.99m);

    private static (BooksService service, Mock<IBooksRepository> repo) Setup()
    {
        var repo = new Mock<IBooksRepository>();
        return (new BooksService(repo.Object), repo);
    }

    // GetAll

    [Fact]
    public void GetAll_ReturnsBooksFromRepo()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetAll()).Returns([Make("isbn-1")]);
        Assert.Single(svc.GetAll());
    }

    // GetByIsbn

    [Fact]
    public void GetByIsbn_ReturnsBook_WhenFound()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetByIsbn("isbn-1")).Returns(Make("isbn-1"));
        Assert.NotNull(svc.GetByIsbn("isbn-1"));
    }

    [Fact]
    public void GetByIsbn_ReturnsNull_WhenNotFound()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetByIsbn(It.IsAny<string>())).Returns((Book?)null);
        Assert.Null(svc.GetByIsbn("missing"));
    }

    // Add

    [Fact]
    public void Add_CallsRepoAdd()
    {
        var (svc, repo) = Setup();
        var book = Make("isbn-1");
        svc.Add(book);
        repo.Verify(r => r.Add(book), Times.Once);
    }

    // Update

    [Fact]
    public void Update_ReturnsTrue_AndCallsRepoUpdate_WhenBookExists()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetByIsbn("isbn-1")).Returns(Make("isbn-1"));

        var result = svc.Update("isbn-1", Make("isbn-1", "New Title"));

        Assert.True(result);
        repo.Verify(r => r.Update(It.Is<Book>(b => b.Isbn == "isbn-1" && b.Title == "New Title")), Times.Once);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenNotFound()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetByIsbn(It.IsAny<string>())).Returns((Book?)null);

        Assert.False(svc.Update("missing", Make("missing")));
        repo.Verify(r => r.Update(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public void Update_OverridesIsbnWithRouteParam()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetByIsbn("isbn-1")).Returns(Make("isbn-1"));

        svc.Update("isbn-1", Make("isbn-other", "Title"));

        repo.Verify(r => r.Update(It.Is<Book>(b => b.Isbn == "isbn-1")), Times.Once);
    }

    // Delete

    [Fact]
    public void Delete_ReturnsTrue_AndCallsRepoDelete_WhenFound()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetByIsbn("isbn-1")).Returns(Make("isbn-1"));

        Assert.True(svc.Delete("isbn-1"));
        repo.Verify(r => r.Delete("isbn-1"), Times.Once);
    }

    [Fact]
    public void Delete_ReturnsFalse_WhenNotFound()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetByIsbn(It.IsAny<string>())).Returns((Book?)null);

        Assert.False(svc.Delete("missing"));
        repo.Verify(r => r.Delete(It.IsAny<string>()), Times.Never);
    }

    // ExportXml

    [Fact]
    public void ExportXml_ReturnsValidXml_ContainingBooks()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetAll()).Returns([Make("isbn-1", "My Book")]);

        var xml = System.Text.Encoding.UTF8.GetString(svc.ExportXml());

        Assert.Contains("isbn-1", xml);
        Assert.Contains("My Book", xml);
    }

    [Fact]
    public void ExportXml_EmptyRepo_ReturnsEmptyBookstore()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetAll()).Returns([]);

        var xml = System.Text.Encoding.UTF8.GetString(svc.ExportXml());

        Assert.Contains("bookstore", xml);
    }

    // ExportHtml

    [Fact]
    public void ExportHtml_ReturnsHtml_ContainingBooks()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetAll()).Returns([Make("isbn-1", "My Book")]);

        var html = svc.ExportHtml();

        Assert.Contains("isbn-1", html);
        Assert.Contains("My Book", html);
        Assert.Contains("<table", html);
    }

    [Fact]
    public void ExportHtml_EmptyRepo_ReturnsEmptyTable()
    {
        var (svc, repo) = Setup();
        repo.Setup(r => r.GetAll()).Returns([]);

        var html = svc.ExportHtml();

        Assert.Contains("<tbody>", html);
    }
}
