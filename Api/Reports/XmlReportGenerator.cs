using System.Text;
using System.Xml.Serialization;
using Api.Books;

namespace Api.Reports;

public class XmlReportGenerator : IReportGenerator
{
    private static readonly XmlSerializer _serializer = new(typeof(BookstoreData));
    private static readonly XmlSerializerNamespaces _ns = BuildNs();

    public string Format      => "xml";
    public string ContentType => "application/xml";
    public string FileName    => "bookstore.xml";

    private static XmlSerializerNamespaces BuildNs()
    {
        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        return ns;
    }

    public string Generate(IEnumerable<Book> books)
    {
        var data = new BookstoreData
        {
            Books = books.Select(b => new BookData
            {
                Isbn = b.Isbn, Title = b.Title, Authors = b.Authors,
                Category = b.Category, Cover = b.Cover, Year = b.Year, Price = b.Price
            }).ToList()
        };
        using var stream = new MemoryStream();
        _serializer.Serialize(stream, data, _ns);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
