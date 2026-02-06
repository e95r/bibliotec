using Bibliotec.Models;
using Microsoft.Data.Sqlite;

namespace Bibliotec.Database;

public sealed class LibraryRepository
{
    private readonly string _connectionString;

    public LibraryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<Reader> GetReaders()
    {
        var readers = new List<Reader>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, FullName, BirthDate, Phone, Category FROM Readers ORDER BY FullName";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            readers.Add(new Reader(
                reader.GetInt32(0),
                reader.GetString(1),
                DateTime.Parse(reader.GetString(2)),
                reader.GetString(3),
                reader.GetString(4)));
        }

        return readers;
    }

    public void AddReader(string fullName, DateTime birthDate, string phone, string category)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Readers (FullName, BirthDate, Phone, Category) VALUES ($name, $birth, $phone, $category)";
        command.Parameters.AddWithValue("$name", fullName);
        command.Parameters.AddWithValue("$birth", birthDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$phone", phone);
        command.Parameters.AddWithValue("$category", category);
        command.ExecuteNonQuery();
    }

    public void DeleteReader(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Readers WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<Book> GetBooks()
    {
        var books = new List<Book>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Author, Publisher, Year, Keywords, TotalCopies FROM Books ORDER BY Title";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            books.Add(new Book(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetString(5),
                reader.GetInt32(6)));
        }

        return books;
    }

    public void AddBook(string title, string author, string publisher, int year, string keywords, int totalCopies)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Books (Title, Author, Publisher, Year, Keywords, TotalCopies) VALUES ($title, $author, $publisher, $year, $keywords, $totalCopies)";
        command.Parameters.AddWithValue("$title", title);
        command.Parameters.AddWithValue("$author", author);
        command.Parameters.AddWithValue("$publisher", publisher);
        command.Parameters.AddWithValue("$year", year);
        command.Parameters.AddWithValue("$keywords", keywords);
        command.Parameters.AddWithValue("$totalCopies", totalCopies);
        command.ExecuteNonQuery();
    }

    public void DeleteBook(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Books WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<Loan> GetLoans()
    {
        var loans = new List<Loan>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Loans.Id, Readers.FullName, Books.Title, Loans.LoanDate, Loans.DueDate, Loans.ReturnDate
FROM Loans
JOIN Readers ON Loans.ReaderId = Readers.Id
JOIN Books ON Loans.BookId = Books.Id
ORDER BY Loans.LoanDate DESC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            loans.Add(new Loan(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                DateTime.Parse(reader.GetString(3)),
                DateTime.Parse(reader.GetString(4)),
                reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5))));
        }

        return loans;
    }

    public void CreateLoan(int readerId, int bookId, DateTime loanDate, DateTime dueDate)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Loans (ReaderId, BookId, LoanDate, DueDate) VALUES ($readerId, $bookId, $loanDate, $dueDate)";
        command.Parameters.AddWithValue("$readerId", readerId);
        command.Parameters.AddWithValue("$bookId", bookId);
        command.Parameters.AddWithValue("$loanDate", loanDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$dueDate", dueDate.ToString("yyyy-MM-dd"));
        command.ExecuteNonQuery();
    }

    public void ReturnLoan(int loanId, DateTime returnDate)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Loans SET ReturnDate = $returnDate WHERE Id = $id";
        command.Parameters.AddWithValue("$returnDate", returnDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$id", loanId);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<Loan> GetOverdueLoans()
    {
        var loans = new List<Loan>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Loans.Id, Readers.FullName, Books.Title, Loans.LoanDate, Loans.DueDate, Loans.ReturnDate
FROM Loans
JOIN Readers ON Loans.ReaderId = Readers.Id
JOIN Books ON Loans.BookId = Books.Id
WHERE Loans.ReturnDate IS NULL AND date(Loans.DueDate) < date('now')
ORDER BY Loans.DueDate";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            loans.Add(new Loan(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                DateTime.Parse(reader.GetString(3)),
                DateTime.Parse(reader.GetString(4)),
                reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5))));
        }

        return loans;
    }

    public IReadOnlyList<PopularBook> GetPopularBooks()
    {
        var books = new List<PopularBook>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Books.Title, Books.Author, COUNT(Loans.Id) as TimesLoaned
FROM Books
LEFT JOIN Loans ON Loans.BookId = Books.Id
GROUP BY Books.Id
ORDER BY TimesLoaned DESC, Books.Title";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            books.Add(new PopularBook(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2)));
        }

        return books;
    }
}
