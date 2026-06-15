namespace PaleteControl.Models;

public enum TipoPalete
{
    PBR,
    CHEP,
    Descartavel,
    Outro
}

public enum TipoMovimentacao
{
    Entrada,
    Saida
}

public class Movimentacao
{
    public int Id { get; set; }
    public TipoPalete TipoPalete { get; set; }
    public TipoMovimentacao Tipo { get; set; }
    public int Quantidade { get; set; }
    public DateTime Data { get; set; }
    public string Observacao { get; set; } = string.Empty;
}
