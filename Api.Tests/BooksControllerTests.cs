using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Api.Books;
using Xunit;

namespace Api.Tests;

public class BooksControllerTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly WebApplicationFactory<BooksController> _factory;
    private readonly HttpClient _client;

    public BooksControllerTests()
    {
        var tempFile = _tempFile;
        _factory = new WebApplicationFactory<BooksController>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Bookstore:FilePath"] = tempFile
                }));
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public void Dispose()
    {
        _factory.Dispose();
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    // GET /books

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmptyList()
    {
        var response = await _client.GetAsync("/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        Assert.Empty(books!);
    }

    // GET /books/{isbn}

    [Fact]
    public async Task GetByIsbn_NotFound_WhenMissing()
    {
        var response = await _client.GetAsync("/books/nonexistent-isbn");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByIsbn_ReturnsBook_WhenExists()
    {
        await _client.PostAsJsonAsync("/books", SampleBook());

        var response = await _client.GetAsync($"/books/{SampleBook().Isbn}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<Book>();
        Assert.Equal(SampleBook().Isbn, book!.Isbn);
    }

    // POST /books

    [Fact]
    public async Task Add_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/books", SampleBook());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<Book>();
        Assert.Equal(SampleBook().Isbn, book!.Isbn);
    }

    // PUT /books/{isbn}

    [Fact]
    public async Task Update_ReturnsNoContent_WhenExists()
    {
        await _client.PostAsJsonAsync("/books", SampleBook());
        var updated = SampleBook() with { Title = "Updated Title" };

        var response = await _client.PutAsJsonAsync($"/books/{SampleBook().Isbn}", updated);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.PutAsJsonAsync("/books/nonexistent-isbn", SampleBook());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // DELETE /books/{isbn}

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenExists()
    {
        await _client.PostAsJsonAsync("/books", SampleBook());

        var response = await _client.DeleteAsync($"/books/{SampleBook().Isbn}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.DeleteAsync("/books/nonexistent-isbn");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static Book SampleBook() =>
        new("isbn-001", "Test Book", ["Author One"], "fiction", null, 2020, 14.99m);
}
