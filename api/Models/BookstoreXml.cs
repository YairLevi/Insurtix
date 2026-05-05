using System.Xml.Serialization;

namespace Api.Models;

[XmlRoot("bookstore")]
public class BookstoreXml
{
    [XmlElement("book")]
    public List<BookXml> Books { get; set; } = [];
}

public class BookXml
{
    [XmlAttribute("category")] public string Category { get; set; } = "";
    [XmlAttribute("cover")]    public string? Cover { get; set; }
    [XmlElement("isbn")]       public string Isbn { get; set; } = "";
    [XmlElement("title")]      public string Title { get; set; } = "";
    [XmlElement("author")]     public List<string> Authors { get; set; } = [];
    [XmlElement("year")]       public int Year { get; set; }
    [XmlElement("price")]      public decimal Price { get; set; }
}
