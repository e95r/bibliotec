using Bibliotec.Database;
using Bibliotec.Forms;
using Bibliotec.Models;

namespace Bibliotec;

public sealed class MainForm : Form
{
    private readonly LibraryRepository _repository = new(DatabaseInitializer.ConnectionString);

    private readonly DataGridView _readersGrid = new();
    private readonly DataGridView _booksGrid = new();
    private readonly DataGridView _loansGrid = new();
    private readonly DataGridView _overdueGrid = new();
    private readonly DataGridView _popularGrid = new();

    private readonly ComboBox _readerCombo = new();
    private readonly ComboBox _bookCombo = new();
    private readonly DateTimePicker _loanDatePicker = new();
    private readonly DateTimePicker _dueDatePicker = new();

    public MainForm()
    {
        Text = "Библиотека-2";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateReadersTab());
        tabs.TabPages.Add(CreateBooksTab());
        tabs.TabPages.Add(CreateLoansTab());
        tabs.TabPages.Add(CreateReportsTab());

        Controls.Add(tabs);

        LoadData();
    }

    private TabPage CreateReadersTab()
    {
        var page = new TabPage("Читатели");
        _readersGrid.Dock = DockStyle.Fill;
        _readersGrid.ReadOnly = true;
        _readersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _readersGrid.AutoGenerateColumns = false;
        _readersGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "ФИО", DataPropertyName = "FullName", Width = 220 },
            new DataGridViewTextBoxColumn { HeaderText = "Дата рождения", DataPropertyName = "BirthDate", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = "Телефон", DataPropertyName = "Phone", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = "Категория", DataPropertyName = "Category", Width = 120 }
        );

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45 };
        var addButton = new Button { Text = "Добавить читателя" };
        var deleteButton = new Button { Text = "Удалить" };
        addButton.Click += (_, _) => AddReader();
        deleteButton.Click += (_, _) => DeleteReader();
        buttonPanel.Controls.Add(addButton);
        buttonPanel.Controls.Add(deleteButton);

        page.Controls.Add(_readersGrid);
        page.Controls.Add(buttonPanel);

        return page;
    }

    private TabPage CreateBooksTab()
    {
        var page = new TabPage("Книги");
        _booksGrid.Dock = DockStyle.Fill;
        _booksGrid.ReadOnly = true;
        _booksGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _booksGrid.AutoGenerateColumns = false;
        _booksGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = "Title", Width = 220 },
            new DataGridViewTextBoxColumn { HeaderText = "Автор", DataPropertyName = "Author", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = "Издательство", DataPropertyName = "Publisher", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = "Год", DataPropertyName = "Year", Width = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "Ключевые слова", DataPropertyName = "Keywords", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "Экземпляры", DataPropertyName = "TotalCopies", Width = 90 }
        );

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45 };
        var addButton = new Button { Text = "Добавить книгу" };
        var deleteButton = new Button { Text = "Удалить" };
        addButton.Click += (_, _) => AddBook();
        deleteButton.Click += (_, _) => DeleteBook();
        buttonPanel.Controls.Add(addButton);
        buttonPanel.Controls.Add(deleteButton);

        page.Controls.Add(_booksGrid);
        page.Controls.Add(buttonPanel);

        return page;
    }

    private TabPage CreateLoansTab()
    {
        var page = new TabPage("Выдачи");

        var formPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            ColumnCount = 6,
            RowCount = 2,
            Padding = new Padding(10)
        };
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));

        _readerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _bookCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _loanDatePicker.Value = DateTime.Today;
        _dueDatePicker.Value = DateTime.Today.AddDays(14);

        formPanel.Controls.Add(new Label { Text = "Читатель", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        formPanel.Controls.Add(_readerCombo, 1, 0);
        formPanel.Controls.Add(new Label { Text = "Книга", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
        formPanel.Controls.Add(_bookCombo, 3, 0);
        formPanel.Controls.Add(new Label { Text = "Дата выдачи", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        formPanel.Controls.Add(_loanDatePicker, 1, 1);
        formPanel.Controls.Add(new Label { Text = "Срок возврата", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
        formPanel.Controls.Add(_dueDatePicker, 3, 1);

        var addLoanButton = new Button { Text = "Оформить", Dock = DockStyle.Fill };
        addLoanButton.Click += (_, _) => CreateLoan();
        var returnButton = new Button { Text = "Возврат", Dock = DockStyle.Fill };
        returnButton.Click += (_, _) => ReturnLoan();

        formPanel.Controls.Add(addLoanButton, 4, 0);
        formPanel.Controls.Add(returnButton, 5, 0);
        formPanel.SetRowSpan(addLoanButton, 2);
        formPanel.SetRowSpan(returnButton, 2);

        _loansGrid.Dock = DockStyle.Fill;
        _loansGrid.ReadOnly = true;
        _loansGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _loansGrid.AutoGenerateColumns = false;
        _loansGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
            new DataGridViewTextBoxColumn { HeaderText = "Читатель", DataPropertyName = "ReaderName", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = "BookTitle", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = "Дата выдачи", DataPropertyName = "LoanDate", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = "Срок возврата", DataPropertyName = "DueDate", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = "Возврат", DataPropertyName = "ReturnDate", Width = 120 }
        );

        page.Controls.Add(_loansGrid);
        page.Controls.Add(formPanel);

        return page;
    }

    private TabPage CreateReportsTab()
    {
        var page = new TabPage("Отчёты");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var refreshButton = new Button { Text = "Обновить отчёты", Dock = DockStyle.Left };
        refreshButton.Click += (_, _) => LoadReports();

        layout.Controls.Add(refreshButton, 0, 0);
        layout.SetColumnSpan(refreshButton, 2);

        _overdueGrid.Dock = DockStyle.Fill;
        _overdueGrid.ReadOnly = true;
        _overdueGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _overdueGrid.AutoGenerateColumns = false;
        _overdueGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Читатель", DataPropertyName = "ReaderName", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = "BookTitle", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "Срок возврата", DataPropertyName = "DueDate", Width = 120 }
        );

        _popularGrid.Dock = DockStyle.Fill;
        _popularGrid.ReadOnly = true;
        _popularGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _popularGrid.AutoGenerateColumns = false;
        _popularGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = "Title", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = "Автор", DataPropertyName = "Author", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = "Выдач", DataPropertyName = "TimesLoaned", Width = 80 }
        );

        var overdueGroup = new GroupBox { Text = "Просрочки", Dock = DockStyle.Fill };
        overdueGroup.Controls.Add(_overdueGrid);
        var popularGroup = new GroupBox { Text = "Востребованность", Dock = DockStyle.Fill };
        popularGroup.Controls.Add(_popularGrid);

        layout.Controls.Add(overdueGroup, 0, 1);
        layout.Controls.Add(popularGroup, 1, 1);

        page.Controls.Add(layout);
        return page;
    }

    private void LoadData()
    {
        LoadReaders();
        LoadBooks();
        LoadLoans();
        LoadReports();
    }

    private void LoadReaders()
    {
        var readers = _repository.GetReaders();
        _readersGrid.DataSource = readers;
        _readerCombo.DataSource = readers.Select(r => new { r.Id, r.FullName }).ToList();
        _readerCombo.DisplayMember = "FullName";
        _readerCombo.ValueMember = "Id";
    }

    private void LoadBooks()
    {
        var books = _repository.GetBooks();
        _booksGrid.DataSource = books;
        _bookCombo.DataSource = books.Select(b => new { b.Id, b.Title }).ToList();
        _bookCombo.DisplayMember = "Title";
        _bookCombo.ValueMember = "Id";
    }

    private void LoadLoans()
    {
        _loansGrid.DataSource = _repository.GetLoans();
    }

    private void LoadReports()
    {
        _overdueGrid.DataSource = _repository.GetOverdueLoans();
        _popularGrid.DataSource = _repository.GetPopularBooks();
    }

    private void AddReader()
    {
        using var form = new AddReaderForm();
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(form.FullName) || string.IsNullOrWhiteSpace(form.Phone))
        {
            MessageBox.Show(this, "Заполните ФИО и телефон.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _repository.AddReader(form.FullName, form.BirthDate, form.Phone, form.Category);
        LoadReaders();
    }

    private void DeleteReader()
    {
        if (_readersGrid.CurrentRow?.DataBoundItem is not Reader reader)
        {
            return;
        }

        _repository.DeleteReader(reader.Id);
        LoadReaders();
    }

    private void AddBook()
    {
        using var form = new AddBookForm();
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(form.TitleValue) || string.IsNullOrWhiteSpace(form.Author))
        {
            MessageBox.Show(this, "Заполните название и автора.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _repository.AddBook(form.TitleValue, form.Author, form.Publisher, form.Year, form.Keywords, form.TotalCopies);
        LoadBooks();
    }

    private void DeleteBook()
    {
        if (_booksGrid.CurrentRow?.DataBoundItem is not Book book)
        {
            return;
        }

        _repository.DeleteBook(book.Id);
        LoadBooks();
    }

    private void CreateLoan()
    {
        if (_readerCombo.SelectedValue is not int readerId || _bookCombo.SelectedValue is not int bookId)
        {
            return;
        }

        _repository.CreateLoan(readerId, bookId, _loanDatePicker.Value.Date, _dueDatePicker.Value.Date);
        LoadLoans();
        LoadReports();
    }

    private void ReturnLoan()
    {
        if (_loansGrid.CurrentRow?.DataBoundItem is not Loan loan)
        {
            return;
        }

        if (loan.ReturnDate is not null)
        {
            MessageBox.Show(this, "Эта выдача уже закрыта.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _repository.ReturnLoan(loan.Id, DateTime.Today);
        LoadLoans();
        LoadReports();
    }
}
