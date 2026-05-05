namespace Api.Models;

public record Book(
    string Isbn,
    string Title,
    List<string> Authors,
    string Category,
    string? Cover,
    int Year,
    decimal Price
);
