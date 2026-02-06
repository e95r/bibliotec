namespace Bibliotec.Models;

public sealed record Loan(int Id, string ReaderName, string BookTitle, DateTime LoanDate, DateTime DueDate, DateTime? ReturnDate);
