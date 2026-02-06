namespace Bibliotec.Models;

public sealed record Book(int Id, string Title, string Author, string Publisher, int Year, string Keywords, int TotalCopies);
