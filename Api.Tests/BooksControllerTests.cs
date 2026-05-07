using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Api.Controllers;
using Xunit;

namespace Api.Tests;

public class BooksControllerTests : IDisposable
{
    private readonly WebApplicationFactory<BooksController> _factory;
    private readonly HttpClient _client;

    public BooksControllerTests()
    {
        _factory = new WebApplicationFactory<BooksController>().WithWebHostBuilder(b =>
            b.UseEnvironment("Testing"));
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public void Dispose() => _factory.Dispose();

    // GET /books

    [Fact]
    public async Task GetBooks_ReturnsOk_WithEmptyList()
    {
        var response = await _client.GetAsync("/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, books.ValueKind);
        Assert.Equal(0, books.GetArrayLength());
    }

    [Fact]
    public async Task GetBooks_AfterUpload_ReturnsBooksFromStore()
    {
        await _client.PostAsync("/books/upload", ValidXmlContent());

        var response = await _client.GetAsync("/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, books.GetArrayLength());
    }

    // POST /books/upload

    [Fact]
    public async Task Upload_ValidXml_ReturnsOkWithCount()
    {
        var response = await _client.PostAsync("/books/upload", ValidXmlContent());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task Upload_MissingFile_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/books/upload", new MultipartFormDataContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_InvalidXml_ReturnsBadRequest()
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("not xml at all")), "file", "bad.xml");

        var response = await _client.PostAsync("/books/upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // POST /books/export

    [Fact]
    public async Task Export_ValidBooks_ReturnsXmlFile()
    {
        var books = new[]
        {
            new { Isbn = "isbn-001", Title = "Test Book", Authors = new[] { "Author One" }, Category = "fiction", Cover = (string?)null, Year = 2020, Price = 14.99m }
        };

        var response = await _client.PostAsJsonAsync("/books/export", books);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);
        var xml = await response.Content.ReadAsStringAsync();
        Assert.Contains("isbn-001", xml);
        Assert.Contains("Test Book", xml);
    }

    [Fact]
    public async Task Export_EmptyList_ReturnsEmptyXmlFile()
    {
        var response = await _client.PostAsJsonAsync("/books/export", Array.Empty<object>());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);
    }

    private static MultipartFormDataContent ValidXmlContent()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <bookstore>
              <book category="tech">
                <isbn>isbn-001</isbn>
                <title>Test Book</title>
                <author>Author One</author>
                <year>2020</year>
                <price>19.99</price>
              </book>
            </bookstore>
            """;
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(xml)), "file", "bookstore.xml");
        return content;
    }
}
