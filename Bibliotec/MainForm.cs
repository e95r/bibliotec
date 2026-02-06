using System.ComponentModel;
using Bibliotec.Database;
using Bibliotec.Forms;
using Bibliotec.Models;

namespace Bibliotec;

public sealed class MainForm : Form
{
    private readonly LibraryRepository _repository = new(DatabaseInitializer.ConnectionString);

    private readonly TabControl _tabs = new();
    private readonly TabPage _homeTab;
    private readonly TabPage _readersTab;
    private readonly TabPage _booksTab;
    private readonly TabPage _loansTab;
    private readonly TabPage _reportsTab;

    private readonly DataGridView _readersGrid = new();
    private readonly DataGridView _booksGrid = new();
    private readonly DataGridView _loansGrid = new();
    private readonly DataGridView _ordersGrid = new();
    private readonly DataGridView _overdueGrid = new();
    private readonly DataGridView _popularGrid = new();

    private readonly ComboBox _readerCombo = new();
    private readonly ComboBox _bookCombo = new();
    private readonly ComboBox _orderReaderCombo = new();
    private readonly DateTimePicker _loanDatePicker = new();
    private readonly DateTimePicker _dueDatePicker = new();
    private readonly ComboBox _roleCombo = new();
    private readonly TextBox _orderTitleBox = new();
    private readonly TextBox _orderAuthorBox = new();
    private readonly TextBox _orderKeywordBox = new();
    private readonly TextBox _orderCodeBox = new();
    private readonly ListBox _selectedBooksList = new();
    private readonly BindingList<Book> _selectedLoanBooks = new();

    private readonly MenuStrip _menu = new();
    private ToolStripMenuItem _sectionsMenu = null!;
    private ToolStripMenuItem _directoriesMenu = null!;
    private ToolStripMenuItem _operationsMenu = null!;
    private ToolStripMenuItem _reportsMenu = null!;
    private ToolStripMenuItem _readersNavigateItem = null!;
    private ToolStripMenuItem _booksNavigateItem = null!;
    private ToolStripMenuItem _loansNavigateItem = null!;
    private ToolStripMenuItem _reportsNavigateItem = null!;

    private Button _addReaderButton = null!;
    private Button _deleteReaderButton = null!;
    private Button _addBookButton = null!;
    private Button _deleteBookButton = null!;
    private Button _addLoanButton = null!;
    private Button _returnLoanButton = null!;
    private Button _refreshReportsButton = null!;
    private Button _searchOrderButton = null!;
    private Button _createOrderButton = null!;
    private Button _issueOrderButton = null!;
    private Button _addBookToLoanButton = null!;
    private Button _removeBookFromLoanButton = null!;

    private UserRole _currentRole = UserRole.Reader;

    public MainForm()
    {
        Text = "Библиотека-2";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        _homeTab = CreateHomeTab();
        _readersTab = CreateReadersTab();
        _booksTab = CreateBooksTab();
        _loansTab = CreateLoansTab();
        _reportsTab = CreateReportsTab();

        _tabs.Dock = DockStyle.Fill;
        _tabs.TabPages.AddRange([_homeTab, _readersTab, _booksTab, _loansTab, _reportsTab]);

        BuildMenu();
        MainMenuStrip = _menu;
        Controls.Add(_tabs);
        Controls.Add(BuildRolePanel());
        Controls.Add(_menu);

        LoadData();
        ApplyRole(_currentRole);
    }

    private void BuildMenu()
    {
        var fileMenu = new ToolStripMenuItem("Файл");
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Обновить всё", null, (_, _) => LoadData()));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Выход", null, (_, _) => Close()));

        _sectionsMenu = new ToolStripMenuItem("Разделы");
        _readersNavigateItem = CreateNavigateItem("Читатели", _readersTab);
        _booksNavigateItem = CreateNavigateItem("Книги", _booksTab);
        _loansNavigateItem = CreateNavigateItem("Выдачи", _loansTab);
        _reportsNavigateItem = CreateNavigateItem("Отчёты", _reportsTab);
        _sectionsMenu.DropDownItems.Add(_readersNavigateItem);
        _sectionsMenu.DropDownItems.Add(_booksNavigateItem);
        _sectionsMenu.DropDownItems.Add(_loansNavigateItem);
        _sectionsMenu.DropDownItems.Add(_reportsNavigateItem);

        _directoriesMenu = new ToolStripMenuItem("Справочники");
        var readersMenu = new ToolStripMenuItem("Читатели");
        readersMenu.DropDownItems.Add(new ToolStripMenuItem("Добавить", null, (_, _) => AddReader()));
        readersMenu.DropDownItems.Add(new ToolStripMenuItem("Удалить", null, (_, _) => DeleteReader()));
        var booksMenu = new ToolStripMenuItem("Книги");
        booksMenu.DropDownItems.Add(new ToolStripMenuItem("Добавить", null, (_, _) => AddBook()));
        booksMenu.DropDownItems.Add(new ToolStripMenuItem("Удалить", null, (_, _) => DeleteBook()));
        _directoriesMenu.DropDownItems.Add(readersMenu);
        _directoriesMenu.DropDownItems.Add(booksMenu);

        _operationsMenu = new ToolStripMenuItem("Операции");
        var loansMenu = new ToolStripMenuItem("Выдачи");
        loansMenu.DropDownItems.Add(new ToolStripMenuItem("Оформить", null, (_, _) => CreateLoan()));
        loansMenu.DropDownItems.Add(new ToolStripMenuItem("Возврат", null, (_, _) => ReturnLoan()));
        _operationsMenu.DropDownItems.Add(loansMenu);

        _reportsMenu = new ToolStripMenuItem("Отчёты");
        _reportsMenu.DropDownItems.Add(new ToolStripMenuItem("Обновить", null, (_, _) => LoadReports()));

        var helpMenu = new ToolStripMenuItem("Справка");
        helpMenu.DropDownItems.Add(new ToolStripMenuItem("О программе", null, (_, _) =>
            MessageBox.Show(
                this,
                "Система управления библиотекой.\nИспользуйте вкладки и меню для работы со справочниками, выдачами и отчётами.",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)));

        _menu.Items.Add(fileMenu);
        _menu.Items.Add(_sectionsMenu);
        _menu.Items.Add(_directoriesMenu);
        _menu.Items.Add(_operationsMenu);
        _menu.Items.Add(_reportsMenu);
        _menu.Items.Add(helpMenu);
    }

    private ToolStripMenuItem CreateNavigateItem(string title, TabPage page)
    {
        return new ToolStripMenuItem(title, null, (_, _) => _tabs.SelectedTab = page);
    }

    private Control BuildRolePanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            Padding = new Padding(10, 6, 10, 6)
        };

        panel.Controls.Add(new Label
        {
            Text = "Роль пользователя:",
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 6, 8, 0)
        });

        _roleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _roleCombo.Width = 200;
        _roleCombo.DataSource = Enum.GetValues<UserRole>();
        _roleCombo.SelectedItem = _currentRole;
        _roleCombo.SelectedValueChanged += (_, _) =>
        {
            if (_roleCombo.SelectedItem is UserRole selectedRole)
            {
                ApplyRole(selectedRole);
            }
        };

        panel.Controls.Add(_roleCombo);
        return panel;
    }

    private TabPage CreateHomeTab()
    {
        var page = new TabPage("Главное меню");
        UpdateHomeTab(page, _currentRole);
        return page;
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
        _addReaderButton = new Button { Text = "Добавить читателя" };
        _deleteReaderButton = new Button { Text = "Удалить" };
        _addReaderButton.Click += (_, _) => AddReader();
        _deleteReaderButton.Click += (_, _) => DeleteReader();
        buttonPanel.Controls.Add(_addReaderButton);
        buttonPanel.Controls.Add(_deleteReaderButton);

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
        _addBookButton = new Button { Text = "Добавить книгу" };
        _deleteBookButton = new Button { Text = "Удалить" };
        _addBookButton.Click += (_, _) => AddBook();
        _deleteBookButton.Click += (_, _) => DeleteBook();
        buttonPanel.Controls.Add(_addBookButton);
        buttonPanel.Controls.Add(_deleteBookButton);

        var orderPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 95,
            ColumnCount = 6,
            RowCount = 2,
            Padding = new Padding(10)
        };
        orderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        orderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        orderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        orderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        orderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
        orderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));

        orderPanel.Controls.Add(new Label { Text = "Читатель", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        orderPanel.Controls.Add(_orderReaderCombo, 1, 0);
        orderPanel.Controls.Add(new Label { Text = "Название", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
        orderPanel.Controls.Add(_orderTitleBox, 3, 0);
        orderPanel.Controls.Add(new Label { Text = "Автор", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        orderPanel.Controls.Add(_orderAuthorBox, 1, 1);
        orderPanel.Controls.Add(new Label { Text = "Ключевые слова", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
        orderPanel.Controls.Add(_orderKeywordBox, 3, 1);

        _searchOrderButton = new Button { Text = "Поиск", Dock = DockStyle.Fill };
        _searchOrderButton.Click += (_, _) => SearchBooksForOrder();
        _createOrderButton = new Button { Text = "Оформить заказ", Dock = DockStyle.Fill };
        _createOrderButton.Click += (_, _) => CreateOrder();
        orderPanel.Controls.Add(_searchOrderButton, 4, 0);
        orderPanel.Controls.Add(_createOrderButton, 5, 0);
        orderPanel.SetRowSpan(_searchOrderButton, 2);
        orderPanel.SetRowSpan(_createOrderButton, 2);

        page.Controls.Add(_booksGrid);
        page.Controls.Add(orderPanel);
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
            ColumnCount = 8,
            RowCount = 2,
            Padding = new Padding(10)
        };
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5));
        formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5));

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
        formPanel.Controls.Add(new Label { Text = "Код заказа", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 4, 0);
        formPanel.Controls.Add(_orderCodeBox, 4, 1);

        _addLoanButton = new Button { Text = "Оформить", Dock = DockStyle.Fill };
        _addLoanButton.Click += (_, _) => CreateLoan();
        _returnLoanButton = new Button { Text = "Возврат", Dock = DockStyle.Fill };
        _returnLoanButton.Click += (_, _) => ReturnLoan();
        _issueOrderButton = new Button { Text = "Выдать по коду", Dock = DockStyle.Fill };
        _issueOrderButton.Click += (_, _) => IssueOrder();

        formPanel.Controls.Add(_addLoanButton, 5, 0);
        formPanel.Controls.Add(_returnLoanButton, 6, 0);
        formPanel.Controls.Add(_issueOrderButton, 7, 0);
        formPanel.SetRowSpan(_addLoanButton, 2);
        formPanel.SetRowSpan(_returnLoanButton, 2);
        formPanel.SetRowSpan(_issueOrderButton, 2);

        _selectedBooksList.DataSource = _selectedLoanBooks;
        _selectedBooksList.DisplayMember = "Title";
        _selectedBooksList.Dock = DockStyle.Fill;

        _addBookToLoanButton = new Button { Text = "➕ Добавить", Dock = DockStyle.Top };
        _addBookToLoanButton.Click += (_, _) => AddBookToLoan();
        _removeBookFromLoanButton = new Button { Text = "Удалить", Dock = DockStyle.Top };
        _removeBookFromLoanButton.Click += (_, _) => RemoveSelectedBookFromLoan();

        var selectionPanel = new Panel { Dock = DockStyle.Right, Width = 220, Padding = new Padding(10) };
        selectionPanel.Controls.Add(_selectedBooksList);
        selectionPanel.Controls.Add(_removeBookFromLoanButton);
        selectionPanel.Controls.Add(_addBookToLoanButton);
        selectionPanel.Controls.Add(new Label
        {
            Text = "Выбранные книги",
            Dock = DockStyle.Top,
            Height = 20,
            TextAlign = ContentAlignment.MiddleLeft
        });

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

        _ordersGrid.Dock = DockStyle.Fill;
        _ordersGrid.ReadOnly = true;
        _ordersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _ordersGrid.AutoGenerateColumns = false;
        _ordersGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
            new DataGridViewTextBoxColumn { HeaderText = "Читатель", DataPropertyName = "ReaderName", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = "BookTitle", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "Дата заказа", DataPropertyName = "OrderDate", Width = 110 },
            new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "Status", Width = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Код", DataPropertyName = "ConfirmationCode", Width = 90 }
        );

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 260
        };
        split.Panel1.Controls.Add(_loansGrid);
        split.Panel2.Controls.Add(_ordersGrid);

        page.Controls.Add(split);
        page.Controls.Add(formPanel);
        page.Controls.Add(selectionPanel);

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

        _refreshReportsButton = new Button { Text = "Обновить отчёты", Dock = DockStyle.Left };
        _refreshReportsButton.Click += (_, _) => LoadReports();

        layout.Controls.Add(_refreshReportsButton, 0, 0);
        layout.SetColumnSpan(_refreshReportsButton, 2);

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
        LoadOrders();
        LoadReports();
    }

    private void ApplyRole(UserRole role)
    {
        _currentRole = role;
        UpdateHomeTab(_homeTab, role);

        _tabs.TabPages.Clear();
        _tabs.TabPages.Add(_homeTab);

        switch (role)
        {
            case UserRole.Reader:
                _tabs.TabPages.Add(_booksTab);
                break;
            case UserRole.Librarian:
                _tabs.TabPages.Add(_readersTab);
                _tabs.TabPages.Add(_booksTab);
                _tabs.TabPages.Add(_loansTab);
                _tabs.TabPages.Add(_reportsTab);
                break;
            case UserRole.Manager:
                _tabs.TabPages.Add(_readersTab);
                _tabs.TabPages.Add(_booksTab);
                _tabs.TabPages.Add(_loansTab);
                _tabs.TabPages.Add(_reportsTab);
                break;
        }

        _sectionsMenu.Visible = true;
        _directoriesMenu.Visible = role is UserRole.Librarian or UserRole.Manager;
        _operationsMenu.Visible = role is UserRole.Librarian or UserRole.Manager;
        _reportsMenu.Visible = role is UserRole.Librarian or UserRole.Manager;

        _readersNavigateItem.Visible = role is UserRole.Librarian or UserRole.Manager;
        _booksNavigateItem.Visible = true;
        _loansNavigateItem.Visible = role is UserRole.Librarian or UserRole.Manager;
        _reportsNavigateItem.Visible = role is not UserRole.Reader;

        var allowManageCatalog = role is UserRole.Librarian or UserRole.Manager;
        _addReaderButton.Visible = allowManageCatalog;
        _deleteReaderButton.Visible = allowManageCatalog;
        _addBookButton.Visible = allowManageCatalog;
        _deleteBookButton.Visible = allowManageCatalog;
        _addLoanButton.Enabled = allowManageCatalog;
        _returnLoanButton.Enabled = allowManageCatalog;
        _issueOrderButton.Enabled = allowManageCatalog;
        _refreshReportsButton.Enabled = role is UserRole.Librarian or UserRole.Manager;

        var allowReaderOrder = role is UserRole.Reader;
        _orderReaderCombo.Enabled = allowReaderOrder;
        _orderTitleBox.Enabled = allowReaderOrder;
        _orderAuthorBox.Enabled = allowReaderOrder;
        _orderKeywordBox.Enabled = allowReaderOrder;
        _searchOrderButton.Enabled = allowReaderOrder;
        _createOrderButton.Enabled = allowReaderOrder;

        _tabs.SelectedTab = _homeTab;
    }

    private void UpdateHomeTab(TabPage page, UserRole role)
    {
        page.Controls.Clear();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var titleLabel = new Label
        {
            Text = $"Главное меню ({GetRoleTitle(role)})",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(titleLabel, 0, 0);
        layout.SetColumnSpan(titleLabel, 2);

        var description = new Label
        {
            Text = GetRoleDescription(role),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(description, 0, 1);
        layout.SetColumnSpan(description, 2);

        var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        AddRoleButton(buttonsPanel, "Книги", _booksTab, true);
        AddRoleButton(buttonsPanel, "Читатели", _readersTab, role is UserRole.Librarian or UserRole.Manager);
        AddRoleButton(buttonsPanel, "Выдачи", _loansTab, role is UserRole.Librarian or UserRole.Manager);
        AddRoleButton(buttonsPanel, "Отчёты", _reportsTab, role is not UserRole.Reader);

        layout.Controls.Add(buttonsPanel, 0, 2);
        layout.SetColumnSpan(buttonsPanel, 2);

        page.Controls.Add(layout);
    }

    private void AddRoleButton(Control container, string title, TabPage target, bool enabled)
    {
        var button = new Button
        {
            Text = title,
            Width = 160,
            Height = 50,
            Enabled = enabled
        };
        button.Click += (_, _) => _tabs.SelectedTab = target;
        container.Controls.Add(button);
    }

    private static string GetRoleTitle(UserRole role)
    {
        return role switch
        {
            UserRole.Reader => "Читатель",
            UserRole.Librarian => "Библиотекарь",
            UserRole.Manager => "Заведующий",
            _ => "Пользователь"
        };
    }

    private static string GetRoleDescription(UserRole role)
    {
        return role switch
        {
            UserRole.Reader =>
                "Вы можете просматривать каталог книг и переходить по основным разделам.",
            UserRole.Librarian =>
                "Доступны справочники читателей и книг, оформление выдач и оперативные отчёты.",
            UserRole.Manager =>
                "Полный доступ к справочникам, выдачам и отчётам для контроля работы библиотеки.",
            _ => string.Empty
        };
    }

    private void LoadReaders()
    {
        var readers = _repository.GetReaders();
        _readersGrid.DataSource = readers;
        _readerCombo.DataSource = readers.Select(r => new { r.Id, r.FullName }).ToList();
        _readerCombo.DisplayMember = "FullName";
        _readerCombo.ValueMember = "Id";
        _orderReaderCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _orderReaderCombo.DataSource = readers.Select(r => new { r.Id, r.FullName }).ToList();
        _orderReaderCombo.DisplayMember = "FullName";
        _orderReaderCombo.ValueMember = "Id";
    }

    private void LoadBooks()
    {
        var books = _repository.GetBooks();
        _booksGrid.DataSource = books;
        _bookCombo.DataSource = books;
        _bookCombo.DisplayMember = "Title";
        _bookCombo.ValueMember = "Id";
    }

    private void LoadLoans()
    {
        _loansGrid.DataSource = _repository.GetLoans();
    }

    private void LoadOrders()
    {
        _ordersGrid.DataSource = _repository.GetOrders();
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

    private void SearchBooksForOrder()
    {
        var books = _repository.SearchBooks(_orderTitleBox.Text, _orderAuthorBox.Text, _orderKeywordBox.Text);
        _booksGrid.DataSource = books;
    }

    private void CreateOrder()
    {
        if (_orderReaderCombo.SelectedValue is not int readerId)
        {
            MessageBox.Show(this, "Выберите читателя.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_booksGrid.CurrentRow?.DataBoundItem is not Book book)
        {
            MessageBox.Show(this, "Выберите книгу для заказа.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var code = _repository.CreateOrder(readerId, book.Id, DateTime.Today);
            LoadOrders();
            MessageBox.Show(this, $"Заказ оформлен. Код подтверждения: {code}.", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CreateLoan()
    {
        if (_readerCombo.SelectedValue is not int readerId)
        {
            return;
        }

        if (_selectedLoanBooks.Count == 0)
        {
            MessageBox.Show(this, "Добавьте хотя бы одну книгу через кнопку «➕ Добавить».", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var book in _selectedLoanBooks)
        {
            _repository.CreateLoan(readerId, book.Id, _loanDatePicker.Value.Date, _dueDatePicker.Value.Date);
        }

        _selectedLoanBooks.Clear();
        LoadLoans();
        LoadReports();
    }

    private void AddBookToLoan()
    {
        if (_bookCombo.SelectedItem is not Book book)
        {
            return;
        }

        if (_selectedLoanBooks.Any(selected => selected.Id == book.Id))
        {
            MessageBox.Show(this, "Эта книга уже добавлена в список.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _selectedLoanBooks.Add(book);
    }

    private void RemoveSelectedBookFromLoan()
    {
        if (_selectedBooksList.SelectedItem is not Book book)
        {
            return;
        }

        _selectedLoanBooks.Remove(book);
    }

    private void IssueOrder()
    {
        var code = _orderCodeBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            MessageBox.Show(this, "Введите код подтверждения.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var pendingOrder = _repository.GetPendingOrderByCode(code);
        if (pendingOrder is null)
        {
            MessageBox.Show(this, "Заказ с таким кодом не найден или уже выдан.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _repository.CreateLoan(pendingOrder.Value.ReaderId, pendingOrder.Value.BookId, _loanDatePicker.Value.Date, _dueDatePicker.Value.Date);
        _repository.MarkOrderIssued(pendingOrder.Value.OrderId);
        LoadLoans();
        LoadOrders();
        LoadReports();
        MessageBox.Show(this, "Заказ выдан и оформлен как выдача.", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        LoadOrders();
        LoadReports();
    }

    private enum UserRole
    {
        Reader,
        Librarian,
        Manager
    }
}
