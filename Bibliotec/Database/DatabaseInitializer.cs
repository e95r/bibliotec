using Microsoft.Data.Sqlite;

namespace Bibliotec.Database;

public static class DatabaseInitializer
{
    private const string DatabaseFile = "library.db";

    public static string ConnectionString => new SqliteConnectionStringBuilder
    {
        DataSource = DatabaseFile
    }.ToString();

    public static void EnsureCreated()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS Readers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    BirthDate TEXT NOT NULL,
    Phone TEXT NOT NULL,
    Category TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Books (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Author TEXT NOT NULL,
    Publisher TEXT NOT NULL,
    Year INTEGER NOT NULL,
    Keywords TEXT NOT NULL,
    TotalCopies INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Loans (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReaderId INTEGER NOT NULL,
    BookId INTEGER NOT NULL,
    LoanDate TEXT NOT NULL,
    DueDate TEXT NOT NULL,
    ReturnDate TEXT,
    FOREIGN KEY(ReaderId) REFERENCES Readers(Id),
    FOREIGN KEY(BookId) REFERENCES Books(Id)
);

CREATE TABLE IF NOT EXISTS Orders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReaderId INTEGER NOT NULL,
    BookId INTEGER NOT NULL,
    OrderDate TEXT NOT NULL,
    Status TEXT NOT NULL,
    ConfirmationCode TEXT NOT NULL,
    FOREIGN KEY(ReaderId) REFERENCES Readers(Id),
    FOREIGN KEY(BookId) REFERENCES Books(Id)
);
";
        command.ExecuteNonQuery();

        SeedData(connection);
    }

    private static void SeedData(SqliteConnection connection)
    {
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(*) FROM Readers";
        var existingReaders = (long)checkCommand.ExecuteScalar()!;

        if (existingReaders > 0)
        {
            return;
        }

        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
INSERT INTO Readers (FullName, BirthDate, Phone, Category) VALUES
('Иванова Ольга Сергеевна', '2003-04-12', '+7-900-111-22-33', 'Студент'),
('Петров Алексей Игоревич', '1985-11-08', '+7-900-222-33-44', 'Преподаватель'),
('Смирнова Мария Андреевна', '1998-02-19', '+7-900-333-44-55', 'Сотрудник');

INSERT INTO Books (Title, Author, Publisher, Year, Keywords, TotalCopies) VALUES
('Основы программирования', 'Н. В. Кузнецов', 'Инфра-М', 2021, 'программирование;алгоритмы', 4),
('Базы данных', 'А. А. Лебедев', 'Питер', 2020, 'SQL;моделирование', 3),
('Математическая логика', 'В. И. Сидоров', 'Юрайт', 2019, 'логика;дискретная математика', 2);
";
        insertCommand.ExecuteNonQuery();

        var loanSeedCommand = connection.CreateCommand();
        loanSeedCommand.CommandText = @"
INSERT INTO Loans (ReaderId, BookId, LoanDate, DueDate, ReturnDate) VALUES
(1, 1, '2025-09-01', '2025-09-21', NULL),
(2, 2, '2025-08-15', '2025-09-05', '2025-09-03');
";
        loanSeedCommand.ExecuteNonQuery();
    }
}
