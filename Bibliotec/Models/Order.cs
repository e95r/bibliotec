namespace Bibliotec.Models;

public sealed record Order(
    int Id,
    string ReaderName,
    string BookTitle,
    DateTime OrderDate,
    string Status,
    string ConfirmationCode);
