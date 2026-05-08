using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Api.Books;
using Xunit;

namespace Api.Tests;

public class LocalBooksXmlTests : IDisposable
{
    private readonly WebApplicationFactory<BooksController> _factory;
    private readonly HttpClient _client;

    public LocalBooksXmlTests()
    {
        var testDir = AppContext.BaseDirectory;
        var booksXmlPath = Path.Combine(testDir, "books.xml");

        _factory = new WebApplicationFactory<BooksController>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile(Path.Combine(testDir, "appsettings.Testing.json"), optional: false);
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Bookstore:FilePath"] = booksXmlPath
                });
            });
        });
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAll_ReadsBooksFromLocalBooksXml()
    {
        var response = await _client.GetAsync("/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        Assert.NotNull(books);
        Assert.Equal(3, books!.Count);
        Assert.Contains(books, b => b.Title == "Harry Potter");
        Assert.Contains(books, b => b.Title == "XQuery Kick Start");
        Assert.Contains(books, b => b.Title == "Learning XML");
    }
}
