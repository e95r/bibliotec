namespace Bibliotec.Forms;

public sealed class AddBookForm : Form
{
    private readonly TextBox _titleBox = new();
    private readonly TextBox _authorBox = new();
    private readonly TextBox _publisherBox = new();
    private readonly NumericUpDown _yearBox = new();
    private readonly TextBox _keywordsBox = new();
    private readonly NumericUpDown _copiesBox = new();

    public AddBookForm()
    {
        Text = "Новая книга";
        Width = 460;
        Height = 320;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        _yearBox.Minimum = 1900;
        _yearBox.Maximum = DateTime.Now.Year;
        _yearBox.Value = DateTime.Now.Year;

        _copiesBox.Minimum = 1;
        _copiesBox.Maximum = 50;
        _copiesBox.Value = 1;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        layout.Controls.Add(new Label { Text = "Название", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(_titleBox, 1, 0);
        layout.Controls.Add(new Label { Text = "Автор", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(_authorBox, 1, 1);
        layout.Controls.Add(new Label { Text = "Издательство", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        layout.Controls.Add(_publisherBox, 1, 2);
        layout.Controls.Add(new Label { Text = "Год", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        layout.Controls.Add(_yearBox, 1, 3);
        layout.Controls.Add(new Label { Text = "Ключевые слова", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
        layout.Controls.Add(_keywordsBox, 1, 4);
        layout.Controls.Add(new Label { Text = "Экземпляры", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
        layout.Controls.Add(_copiesBox, 1, 5);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var saveButton = new Button { Text = "Сохранить", DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);
        layout.Controls.Add(buttonPanel, 0, 6);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    public string TitleValue => _titleBox.Text.Trim();
    public string Author => _authorBox.Text.Trim();
    public string Publisher => _publisherBox.Text.Trim();
    public int Year => (int)_yearBox.Value;
    public string Keywords => _keywordsBox.Text.Trim();
    public int TotalCopies => (int)_copiesBox.Value;
}
