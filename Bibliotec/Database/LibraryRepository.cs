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

    public IReadOnlyList<Book> SearchBooks(string? title, string? author, string? keywords)
    {
        var books = new List<Book>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, Title, Author, Publisher, Year, Keywords, TotalCopies
FROM Books
WHERE ($title IS NULL OR Title LIKE $title)
  AND ($author IS NULL OR Author LIKE $author)
  AND ($keywords IS NULL OR Keywords LIKE $keywords)
ORDER BY Title";
        command.Parameters.AddWithValue("$title", string.IsNullOrWhiteSpace(title) ? DBNull.Value : $"%{title}%");
        command.Parameters.AddWithValue("$author", string.IsNullOrWhiteSpace(author) ? DBNull.Value : $"%{author}%");
        command.Parameters.AddWithValue("$keywords", string.IsNullOrWhiteSpace(keywords) ? DBNull.Value : $"%{keywords}%");

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

    public string CreateOrder(int readerId, int bookId, DateTime orderDate)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        var availabilityCommand = connection.CreateCommand();
        availabilityCommand.Transaction = transaction;
        availabilityCommand.CommandText = @"
SELECT Books.TotalCopies
       - (SELECT COUNT(*) FROM Loans WHERE BookId = $bookId AND ReturnDate IS NULL)
       - (SELECT COUNT(*) FROM Orders WHERE BookId = $bookId AND Status = 'Pending')
FROM Books
WHERE Books.Id = $bookId";
        availabilityCommand.Parameters.AddWithValue("$bookId", bookId);
        var available = Convert.ToInt32(availabilityCommand.ExecuteScalar() ?? 0);

        if (available <= 0)
        {
            throw new InvalidOperationException("Нет доступных экземпляров для заказа.");
        }

        var confirmationCode = GenerateConfirmationCode();
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
INSERT INTO Orders (ReaderId, BookId, OrderDate, Status, ConfirmationCode)
VALUES ($readerId, $bookId, $orderDate, 'Pending', $code)";
        command.Parameters.AddWithValue("$readerId", readerId);
        command.Parameters.AddWithValue("$bookId", bookId);
        command.Parameters.AddWithValue("$orderDate", orderDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$code", confirmationCode);
        command.ExecuteNonQuery();
        transaction.Commit();

        return confirmationCode;
    }

    public IReadOnlyList<Order> GetOrders()
    {
        var orders = new List<Order>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Orders.Id, Readers.FullName, Books.Title, Orders.OrderDate, Orders.Status, Orders.ConfirmationCode
FROM Orders
JOIN Readers ON Orders.ReaderId = Readers.Id
JOIN Books ON Orders.BookId = Books.Id
ORDER BY Orders.OrderDate DESC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            orders.Add(new Order(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                DateTime.Parse(reader.GetString(3)),
                reader.GetString(4),
                reader.GetString(5)));
        }

        return orders;
    }

    public (int OrderId, int ReaderId, int BookId)? GetPendingOrderByCode(string code)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, ReaderId, BookId
FROM Orders
WHERE ConfirmationCode = $code AND Status = 'Pending'";
        command.Parameters.AddWithValue("$code", code);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return (reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2));
    }

    public void MarkOrderIssued(int orderId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Orders SET Status = 'Issued' WHERE Id = $id";
        command.Parameters.AddWithValue("$id", orderId);
        command.ExecuteNonQuery();
    }

    private static string GenerateConfirmationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
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
