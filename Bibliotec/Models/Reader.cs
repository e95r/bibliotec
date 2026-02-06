namespace Bibliotec.Models;

public sealed record Reader(int Id, string FullName, DateTime BirthDate, string Phone, string Category);
