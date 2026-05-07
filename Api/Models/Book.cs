namespace Api.Models;

public record Book(string Isbn, string Title, List<string> Authors, string Category, string? Cover, int Year, decimal Price)
{
    public virtual bool Equals(Book? other) => other is not null && Isbn == other.Isbn;
    public override int GetHashCode() => Isbn.GetHashCode();
}
