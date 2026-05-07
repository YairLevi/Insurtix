using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Api.Books;
using Xunit;

namespace Api.Tests;

public class ReportsControllerTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();
    private readonly WebApplicationFactory<BooksController> _factory;
    private readonly HttpClient _client;

    public ReportsControllerTests()
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

    [Fact]
    public async Task Generate_Xml_ReturnsXmlFile()
    {
        await _client.PostAsJsonAsync("/books", SampleBook());

        var response = await _client.GetAsync("/reports/xml");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);
        var xml = await response.Content.ReadAsStringAsync();
        Assert.Contains(SampleBook().Isbn, xml);
    }

    [Fact]
    public async Task Generate_Html_ReturnsHtmlFile()
    {
        await _client.PostAsJsonAsync("/books", SampleBook());

        var response = await _client.GetAsync("/reports/html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(SampleBook().Isbn, html);
        Assert.Contains("<table", html);
    }

    [Fact]
    public async Task Generate_UnknownFormat_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/reports/csv");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static Book SampleBook() =>
        new("isbn-001", "Test Book", ["Author One"], "fiction", null, 2020, 14.99m);
}
