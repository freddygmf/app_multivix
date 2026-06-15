using PaleteControl.Models;

namespace PaleteControl.Forms;

public class MovimentacaoForm : Form
{
    public Movimentacao? Resultado { get; private set; }

    private readonly Movimentacao? _edicao;

    public MovimentacaoForm(Movimentacao? edicao = null)
    {
        _edicao = edicao;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = _edicao == null ? "Nova Movimentação" : "Editar Movimentação";
        Size = new Size(360, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9f);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(16, 12, 16, 0),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var cmbTipo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbTipo.Items.AddRange(Enum.GetNames<TipoMovimentacao>());
        cmbTipo.SelectedIndex = 0;

        var cmbPalete = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbPalete.Items.AddRange(Enum.GetNames<TipoPalete>());
        cmbPalete.SelectedIndex = 0;

        var numQtd = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 99999, Value = 1 };

        var dtpData = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

        var txtObs = new TextBox { Dock = DockStyle.Fill };

        string[] labels = ["Tipo *", "Palete *", "Quantidade *", "Data *", "Observação"];
        Control[] controls = [cmbTipo, cmbPalete, numQtd, dtpData, txtObs];

        for (int i = 0; i < labels.Length; i++)
        {
            layout.Controls.Add(new Label
            {
                Text = labels[i],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(51, 65, 85)
            }, 0, i);
            layout.Controls.Add(controls[i], 1, i);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        }

        // Preenche se for edição
        if (_edicao != null)
        {
            cmbTipo.SelectedItem   = _edicao.Tipo.ToString();
            cmbPalete.SelectedItem = _edicao.TipoPalete.ToString();
            numQtd.Value           = _edicao.Quantidade;
            dtpData.Value          = _edicao.Data;
            txtObs.Text            = _edicao.Observacao;
        }

        // Botões
        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            BackColor = Color.White
        };

        var btnCancelar = new Button
        {
            Text = "Cancelar", Width = 90, Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(226, 232, 240),
            ForeColor = Color.FromArgb(51, 65, 85)
        };
        btnCancelar.FlatAppearance.BorderSize = 0;
        btnCancelar.Click += (_, _) => DialogResult = DialogResult.Cancel;

        var btnSalvar = new Button
        {
            Text = "Salvar", Width = 90, Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnSalvar.FlatAppearance.BorderSize = 0;
        btnSalvar.Click += (_, _) =>
        {
            Resultado = new Movimentacao
            {
                Id         = _edicao?.Id ?? 0,
                Tipo       = Enum.Parse<TipoMovimentacao>(cmbTipo.SelectedItem!.ToString()!),
                TipoPalete = Enum.Parse<TipoPalete>(cmbPalete.SelectedItem!.ToString()!),
                Quantidade = (int)numQtd.Value,
                Data       = dtpData.Value,
                Observacao = txtObs.Text.Trim()
            };
            DialogResult = DialogResult.OK;
        };

        btnPanel.Controls.AddRange([btnCancelar, btnSalvar]);
        Controls.Add(layout);
        Controls.Add(btnPanel);
    }
}
