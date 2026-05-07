using Api.Books;
using Api.Reports;
using Xunit;

namespace Api.Tests.Unit;

public class ReportGeneratorTests
{
    private static Book Make(string isbn, string title = "Title") =>
        new(isbn, title, ["Author"], "fiction", null, 2020, 9.99m);

    // XmlReportGenerator

    [Fact]
    public void Xml_Generate_ContainsBookData()
    {
        var gen = new XmlReportGenerator();
        var xml = gen.Generate([Make("isbn-1", "My Book")]);
        Assert.Contains("isbn-1", xml);
        Assert.Contains("My Book", xml);
    }

    [Fact]
    public void Xml_Generate_EmptyList_ReturnsBookstoreElement()
    {
        var gen = new XmlReportGenerator();
        var xml = gen.Generate([]);
        Assert.Contains("bookstore", xml);
    }

    // HtmlReportGenerator

    [Fact]
    public void Html_Generate_ContainsBookData()
    {
        var gen = new HtmlReportGenerator();
        var html = gen.Generate([Make("isbn-1", "My Book")]);
        Assert.Contains("isbn-1", html);
        Assert.Contains("My Book", html);
        Assert.Contains("<table", html);
    }

    [Fact]
    public void Html_Generate_EmptyList_ReturnsEmptyTable()
    {
        var gen = new HtmlReportGenerator();
        var html = gen.Generate([]);
        Assert.Contains("<tbody>", html);
    }
}
