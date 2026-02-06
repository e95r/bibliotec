namespace Bibliotec.Forms;

public sealed class AddReaderForm : Form
{
    private readonly TextBox _fullNameBox = new();
    private readonly DateTimePicker _birthDatePicker = new();
    private readonly TextBox _phoneBox = new();
    private readonly ComboBox _categoryBox = new();

    public AddReaderForm()
    {
        Text = "Новый читатель";
        Width = 420;
        Height = 280;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        _categoryBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _categoryBox.Items.AddRange(new object[] { "Студент", "Преподаватель", "Сотрудник" });
        _categoryBox.SelectedIndex = 0;

        layout.Controls.Add(new Label { Text = "ФИО", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(_fullNameBox, 1, 0);
        layout.Controls.Add(new Label { Text = "Дата рождения", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(_birthDatePicker, 1, 1);
        layout.Controls.Add(new Label { Text = "Телефон", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        layout.Controls.Add(_phoneBox, 1, 2);
        layout.Controls.Add(new Label { Text = "Категория", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        layout.Controls.Add(_categoryBox, 1, 3);

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var saveButton = new Button { Text = "Сохранить", DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);
        layout.Controls.Add(buttonPanel, 0, 4);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    public string FullName => _fullNameBox.Text.Trim();
    public DateTime BirthDate => _birthDatePicker.Value.Date;
    public string Phone => _phoneBox.Text.Trim();
    public string Category => _categoryBox.SelectedItem?.ToString() ?? string.Empty;
}
