using System.Text.Json;
using PaleteControl.Models;

namespace PaleteControl.Services;

public class PaleteService
{
    private readonly string _arquivo;
    private List<Movimentacao> _movimentacoes = [];
    private int _nextId = 1;

    public PaleteService()
    {
        var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PaleteControl");
        Directory.CreateDirectory(pasta);
        _arquivo = Path.Combine(pasta, "movimentacoes.json");
        Carregar();
    }

    private void Carregar()
    {
        if (!File.Exists(_arquivo)) return;
        var json = File.ReadAllText(_arquivo);
        _movimentacoes = JsonSerializer.Deserialize<List<Movimentacao>>(json) ?? [];
        _nextId = _movimentacoes.Count > 0 ? _movimentacoes.Max(m => m.Id) + 1 : 1;
    }

    private void Salvar()
    {
        File.WriteAllText(_arquivo, JsonSerializer.Serialize(_movimentacoes, new JsonSerializerOptions { WriteIndented = true }));
    }

    public IReadOnlyList<Movimentacao> ObterTodas() => _movimentacoes.OrderByDescending(m => m.Data).ToList();

    public IReadOnlyList<Movimentacao> Filtrar(TipoPalete? tipo, TipoMovimentacao? tipoMov, DateTime? de, DateTime? ate)
    {
        var q = _movimentacoes.AsEnumerable();
        if (tipo != null)    q = q.Where(m => m.TipoPalete == tipo);
        if (tipoMov != null) q = q.Where(m => m.Tipo == tipoMov);
        if (de != null)      q = q.Where(m => m.Data.Date >= de.Value.Date);
        if (ate != null)     q = q.Where(m => m.Data.Date <= ate.Value.Date);
        return q.OrderByDescending(m => m.Data).ToList();
    }

    public void Registrar(Movimentacao mov)
    {
        mov.Id = _nextId++;
        _movimentacoes.Add(mov);
        Salvar();
    }

    public void Excluir(int id)
    {
        _movimentacoes.RemoveAll(m => m.Id == id);
        Salvar();
    }

    public ResumoEstoque ObterResumo()
    {
        var resumo = new ResumoEstoque();
        foreach (var tipo in Enum.GetValues<TipoPalete>())
        {
            var entradas = _movimentacoes.Where(m => m.TipoPalete == tipo && m.Tipo == TipoMovimentacao.Entrada).Sum(m => m.Quantidade);
            var saidas   = _movimentacoes.Where(m => m.TipoPalete == tipo && m.Tipo == TipoMovimentacao.Saida).Sum(m => m.Quantidade);
            resumo.Saldos[tipo] = entradas - saidas;
        }
        resumo.EntradasHoje = _movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Entrada && m.Data.Date == DateTime.Today).Sum(m => m.Quantidade);
        resumo.SaidasHoje   = _movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Saida   && m.Data.Date == DateTime.Today).Sum(m => m.Quantidade);
        resumo.TotalMovimentacoes = _movimentacoes.Count;
        return resumo;
    }
}

public class ResumoEstoque
{
    public Dictionary<TipoPalete, int> Saldos { get; set; } = [];
    public int EntradasHoje { get; set; }
    public int SaidasHoje { get; set; }
    public int TotalMovimentacoes { get; set; }
}
