using Api.Models;

namespace Api.Reports;

public class HtmlReportGenerator : IReportGenerator
{
    public string Format      => "html";
    public string ContentType => "text/html";
    public string FileName    => "bookstore.html";

    public string Generate(IEnumerable<Book> books)
    {
        const string style = """
            body { font-family: sans-serif; padding: 2rem; background: #fafafa; color: #222; }
            h1 { margin-bottom: 1.5rem; }
            table { border-collapse: collapse; width: 100%; background: #fff; box-shadow: 0 1px 4px rgba(0,0,0,.1); }
            th, td { border: 1px solid #ddd; padding: 10px 14px; text-align: left; }
            th { background: #f0f0f0; font-weight: 600; }
            tr:hover td { background: #f9f9f9; }
            """;

        var rows = string.Join("\n", books.Select(b =>
            $"<tr><td>{b.Isbn}</td><td>{b.Title}</td><td>{string.Join(", ", b.Authors)}</td>" +
            $"<td>{b.Category}</td><td>{b.Year}</td><td>{b.Price:C}</td></tr>"));

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <title>Bookstore</title>
                <style>{style}</style>
            </head>
            <body>
                <h1>Bookstore</h1>
                <table>
                    <thead><tr><th>ISBN</th><th>Title</th><th>Authors</th><th>Category</th><th>Year</th><th>Price</th></tr></thead>
                    <tbody>{rows}</tbody>
                </table>
            </body>
            </html>
            """;
    }
}
