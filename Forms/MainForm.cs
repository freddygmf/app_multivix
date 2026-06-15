using PaleteControl.Models;
using PaleteControl.Services;

namespace PaleteControl.Forms;

public class MainForm : Form
{
    private readonly PaleteService _service;
    private Panel _contentPanel = null!;
    private DataGridView _grid = null!;

    public MainForm()
    {
        _service = new PaleteService();
        InitializeComponent();
        MostrarDashboard();
    }

    private void InitializeComponent()
    {
        Text = "Controle de Paletes";
        Size = new Size(1000, 640);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(800, 520);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9f);

        // Side panel
        var side = new Panel { Dock = DockStyle.Left, Width = 190, BackColor = Color.FromArgb(30, 41, 59) };
        side.Controls.Add(new Label
        {
            Text = "📦 PaleteControl",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 64,
            TextAlign = ContentAlignment.MiddleCenter
        });

        (string label, string tag)[] menus =
        [
            ("🏠  Dashboard",       "dashboard"),
            ("🔄  Movimentações",   "mov"),
            ("📊  Relatório",       "rel"),
        ];

        for (int i = menus.Length - 1; i >= 0; i--)
        {
            var (label, tag) = menus[i];
            var btn = new Button
            {
                Text = label, Tag = tag,
                Dock = DockStyle.Top, Height = 48,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(200, 210, 230),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, _) => { if (s is Button b) b.BackColor = Color.FromArgb(51, 65, 85); };
            btn.MouseLeave += (s, _) => { if (s is Button b) b.BackColor = Color.Transparent; };
            btn.Click += (s, _) =>
            {
                var t = (s as Button)?.Tag?.ToString();
                if (t == "dashboard") MostrarDashboard();
                else if (t == "mov")  MostrarMovimentacoes();
                else if (t == "rel")  MostrarRelatorio();
            };
            side.Controls.Add(btn);
        }

        _contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 20) };

        Controls.Add(_contentPanel);
        Controls.Add(side);
    }

    // ── Limpar ───────────────────────────────────────────────────
    private void Limpar()
    {
        var toRemove = _contentPanel.Controls.Cast<Control>().ToList();
        foreach (var c in toRemove) { _contentPanel.Controls.Remove(c); c.Dispose(); }
    }

    // ── DASHBOARD ────────────────────────────────────────────────
    private void MostrarDashboard()
    {
        Limpar();
        var resumo = _service.ObterResumo();

        // Grid últimas movimentações (Fill — PRIMEIRO)
        _grid = CriarGrid();
        _grid.Dock = DockStyle.Fill;
        PreencherGrid(_service.ObterTodas().Take(20).ToList());
        _contentPanel.Controls.Add(_grid);

        // Label (Top)
        _contentPanel.Controls.Add(Separador("Últimas Movimentações"));

        // Cards de saldo por tipo (Top)
        var painelCards = new Panel { Dock = DockStyle.Top, Height = 106 };
        int x = 0;
        foreach (var tipo in Enum.GetValues<TipoPalete>())
        {
            var saldo = resumo.Saldos.GetValueOrDefault(tipo, 0);
            var cor = saldo >= 0 ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 68, 68);
            var card = CriarCard(tipo.ToString(), saldo.ToString(), cor);
            card.Location = new Point(x, 0);
            painelCards.Controls.Add(card);
            x += 175;
        }
        _contentPanel.Controls.Add(painelCards);

        // Linha resumo hoje (Top)
        var painelHoje = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 4, 0, 0)
        };
        painelHoje.Controls.Add(new Label
        {
            Text = $"📅 Hoje — Entradas: {resumo.EntradasHoje} un   |   Saídas: {resumo.SaidasHoje} un   |   Total de registros: {resumo.TotalMovimentacoes}",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(71, 85, 105),
            AutoSize = true
        });
        _contentPanel.Controls.Add(painelHoje);

        // Título (Top — ÚLTIMO)
        _contentPanel.Controls.Add(Titulo("Dashboard — Saldo por Tipo de Palete"));
    }

    // ── MOVIMENTAÇÕES ────────────────────────────────────────────
    private void MostrarMovimentacoes(IReadOnlyList<Movimentacao>? lista = null)
    {
        Limpar();

        // Grid (Fill — PRIMEIRO)
        _grid = CriarGrid();
        _grid.Dock = DockStyle.Fill;
        PreencherGrid(lista ?? _service.ObterTodas());
        _contentPanel.Controls.Add(_grid);

        // Toolbar com filtros (Top)
        var bar = new Panel { Dock = DockStyle.Top, Height = 46 };

        var cmbTipo    = Combo(["Todos", "Entrada", "Saída"], 0);
        var cmbPalete  = Combo(["Todos", ..Enum.GetNames<TipoPalete>()], 0);
        var dtDe       = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Value = DateTime.Today.AddMonths(-1) };
        var dtAte      = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Value = DateTime.Today };
        var chkData    = new CheckBox { Text = "Período:", Checked = false, AutoSize = true };
        dtDe.Enabled   = dtAte.Enabled = false;
        chkData.CheckedChanged += (_, _) => dtDe.Enabled = dtAte.Enabled = chkData.Checked;

        var btnFiltrar = Botao("Filtrar", Color.FromArgb(59, 130, 246), 75);
        var btnNovo    = Botao("+ Nova", Color.FromArgb(34, 197, 94), 80);
        var btnExcluir = Botao("Excluir", Color.FromArgb(239, 68, 68), 75);

        btnFiltrar.Click += (_, _) =>
        {
            TipoPalete?       tp  = cmbPalete.SelectedIndex > 0 ? Enum.Parse<TipoPalete>(cmbPalete.SelectedItem!.ToString()!)       : null;
            TipoMovimentacao? tm  = cmbTipo.SelectedIndex   > 0 ? Enum.Parse<TipoMovimentacao>(cmbTipo.SelectedItem!.ToString()!)   : null;
            DateTime?         de  = chkData.Checked ? dtDe.Value  : null;
            DateTime?         ate = chkData.Checked ? dtAte.Value : null;
            PreencherGrid(_service.Filtrar(tp, tm, de, ate));
        };

        btnNovo.Click += (_, _) =>
        {
            var form = new MovimentacaoForm();
            if (form.ShowDialog() == DialogResult.OK && form.Resultado != null)
            {
                _service.Registrar(form.Resultado);
                PreencherGrid(_service.ObterTodas());
            }
        };

        btnExcluir.Click += (_, _) =>
        {
            if (_grid.SelectedRows.Count == 0) { MessageBox.Show("Selecione um registro."); return; }
            if (MessageBox.Show("Excluir este registro?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var id = (int)_grid.SelectedRows[0].Cells["Id"].Value;
                _service.Excluir(id);
                PreencherGrid(_service.ObterTodas());
            }
        };

        int cx = 0;
        void Add(Control c, int w = -1)
        {
            if (w > 0) c.Width = w;
            c.Location = new Point(cx, 8);
            if (c is not CheckBox) c.Height = 28;
            bar.Controls.Add(c);
            cx += c.Width + 6;
        }

        Add(new Label { Text = "Tipo:", Width = 35, TextAlign = ContentAlignment.MiddleLeft, Height = 28 });
        Add(cmbTipo, 100);
        Add(new Label { Text = "Palete:", Width = 46, TextAlign = ContentAlignment.MiddleLeft, Height = 28 });
        Add(cmbPalete, 110);
        chkData.Location = new Point(cx, 11); bar.Controls.Add(chkData); cx += chkData.Width + 6;
        Add(dtDe); Add(new Label { Text = "até", Width = 25, TextAlign = ContentAlignment.MiddleCenter, Height = 28 }); Add(dtAte);
        cx += 10;
        Add(btnFiltrar); Add(btnNovo); Add(btnExcluir);

        _contentPanel.Controls.Add(bar);

        // Título (Top — ÚLTIMO)
        _contentPanel.Controls.Add(Titulo("Movimentações"));
    }

    private void PreencherGrid(IEnumerable<Movimentacao> movs)
    {
        _grid.Rows.Clear();
        foreach (var m in movs)
        {
            var idx = _grid.Rows.Add(m.Id, m.Data.ToString("dd/MM/yyyy"), m.Tipo.ToString(),
                m.TipoPalete.ToString(), m.Quantidade, m.Observacao);
            _grid.Rows[idx].DefaultCellStyle.BackColor = m.Tipo == TipoMovimentacao.Entrada
                ? Color.FromArgb(240, 253, 244)
                : Color.FromArgb(254, 242, 242);
            _grid.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        }
    }

    private DataGridView CriarGrid()
    {
        var g = new DataGridView
        {
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 9f),
            GridColor = Color.FromArgb(226, 232, 240)
        };
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Padding = new Padding(4)
        };
        g.EnableHeadersVisualStyles = false;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        g.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
        g.RowTemplate.Height = 30;

        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",          HeaderText = "ID",          FillWeight = 40  });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Data",        HeaderText = "Data",        FillWeight = 80  });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tipo",        HeaderText = "Tipo",        FillWeight = 80  });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TipoPalete",  HeaderText = "Palete",      FillWeight = 90  });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantidade",  HeaderText = "Quantidade",  FillWeight = 80  });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Observacao",  HeaderText = "Observação",  FillWeight = 200 });
        return g;
    }

    // ── RELATÓRIO ────────────────────────────────────────────────
    private void MostrarRelatorio()
    {
        Limpar();
        var resumo = _service.ObterResumo();
        var todas  = _service.ObterTodas();

        var txt = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 10f),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        void H(string t) { txt.SelectionFont = new Font("Segoe UI", 11f, FontStyle.Bold); txt.AppendText(t + "\n"); txt.SelectionFont = new Font("Consolas", 10f); }
        void L(string t) { txt.SelectionFont = new Font("Consolas", 10f); txt.AppendText(t + "\n"); }

        txt.SelectionFont = new Font("Segoe UI", 14f, FontStyle.Bold);
        txt.AppendText($"RELATÓRIO DE PALETES — {DateTime.Now:dd/MM/yyyy HH:mm}\n");
        L(new string('─', 55));
        txt.AppendText("\n");

        H("SALDO ATUAL POR TIPO");
        foreach (var tipo in Enum.GetValues<TipoPalete>())
        {
            var entradas = todas.Where(m => m.TipoPalete == tipo && m.Tipo == TipoMovimentacao.Entrada).Sum(m => m.Quantidade);
            var saidas   = todas.Where(m => m.TipoPalete == tipo && m.Tipo == TipoMovimentacao.Saida).Sum(m => m.Quantidade);
            L($"  {tipo,-15} Entradas: {entradas,6}   Saídas: {saidas,6}   Saldo: {entradas - saidas,6}");
        }
        txt.AppendText("\n");

        H("MOVIMENTAÇÕES DE HOJE");
        var hoje = todas.Where(m => m.Data.Date == DateTime.Today).ToList();
        if (hoje.Count == 0) L("  Nenhuma movimentação hoje.");
        else foreach (var m in hoje)
            L($"  {m.Data:HH:mm}  {m.Tipo,-8}  {m.TipoPalete,-12}  {m.Quantidade,5} un   {m.Observacao}");
        txt.AppendText("\n");

        H("ÚLTIMAS 20 MOVIMENTAÇÕES");
        foreach (var m in todas.Take(20))
            L($"  {m.Data:dd/MM/yyyy}  {m.Tipo,-8}  {m.TipoPalete,-12}  {m.Quantidade,5} un   {m.Observacao}");

        _contentPanel.Controls.Add(txt);

        var btnExp = Botao("💾 Exportar TXT", Color.FromArgb(59, 130, 246), 140);
        btnExp.Dock = DockStyle.Top; btnExp.Height = 36;
        btnExp.Click += (_, _) =>
        {
            var dlg = new SaveFileDialog { Filter = "TXT|*.txt", FileName = $"relatorio_{DateTime.Now:yyyyMMdd}.txt" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, txt.Text);
                MessageBox.Show("Exportado com sucesso!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };
        _contentPanel.Controls.Add(btnExp);
        _contentPanel.Controls.Add(Titulo("Relatório"));
    }

    // ── Helpers ──────────────────────────────────────────────────
    private Label Titulo(string t) => new()
    {
        Text = t,
        Font = new Font("Segoe UI", 15f, FontStyle.Bold),
        ForeColor = Color.FromArgb(30, 41, 59),
        Dock = DockStyle.Top,
        Height = 50,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private Label Separador(string t) => new()
    {
        Text = t,
        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
        ForeColor = Color.FromArgb(71, 85, 105),
        Dock = DockStyle.Top,
        Height = 30
    };

    private Panel CriarCard(string titulo, string valor, Color cor)
    {
        var p = new Panel { Width = 160, Height = 96, BackColor = Color.White };
        p.Paint += (_, e) => e.Graphics.FillRectangle(new SolidBrush(cor), 0, 0, 5, p.Height);
        p.Controls.Add(new Label { Text = valor, Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = cor, Location = new Point(14, 10), Size = new Size(140, 38), TextAlign = ContentAlignment.MiddleLeft });
        p.Controls.Add(new Label { Text = titulo, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(14, 50), Size = new Size(140, 22) });
        p.Controls.Add(new Label { Text = "unidades em saldo", Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(14, 68), Size = new Size(140, 18) });
        return p;
    }

    private Button Botao(string text, Color cor, int w = 100) => new()
    {
        Text = text, Width = w, Height = 28,
        BackColor = cor, ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        Cursor = Cursors.Hand,
        FlatAppearance = { BorderSize = 0 }
    };

    private ComboBox Combo(string[] items, int sel)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        c.Items.AddRange(items);
        c.SelectedIndex = sel;
        return c;
    }
}
