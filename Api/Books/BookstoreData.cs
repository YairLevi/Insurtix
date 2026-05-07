using System.Xml.Serialization;

namespace Api.Books;

[XmlRoot("bookstore")]
public class BookstoreData
{
    [XmlElement("book")]
    public List<BookData> Books { get; set; } = [];
}

public class BookData
{
    [XmlAttribute("category")] public string Category { get; set; } = "";
    [XmlAttribute("cover")]    public string? Cover { get; set; }
    [XmlElement("isbn")]       public string Isbn { get; set; } = "";
    [XmlElement("title")]      public string Title { get; set; } = "";
    [XmlElement("author")]     public List<string> Authors { get; set; } = [];
    [XmlElement("year")]       public int Year { get; set; }
    [XmlElement("price")]      public decimal Price { get; set; }
}
