using Api.Books;

namespace Api.Reports;

public interface IReportGenerator
{
    string Format      { get; }
    string ContentType { get; }
    string FileName    { get; }
    string Generate(IEnumerable<Book> books);
}
