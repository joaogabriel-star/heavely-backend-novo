namespace SistemaHEAVELYBackend.DTOs.NotaFiscal;

public class NotaFiscalFiltroDTO
{
    // DateTime? (não DateTime) de propósito: [Required] não tem efeito nenhum
    // sobre um DateTime não-anulável — o model binder sempre entrega um valor
    // (0001-01-01 quando o parâmetro não vem na query string), então a
    // validação nunca disparava. A checagem real está em
    // NotaFiscalService.GerarDadosNotaFiscalAsync.
    public DateTime? DataInicio { get; set; }

    public DateTime? DataFim { get; set; }

    // "EF2" ou "EM" — omitido/null = todos os segmentos (+ "Sem Série", se houver) no mesmo relatório
    public string? Segmento { get; set; }

    // valor exato de Serie (ex: "7EF2") — filtro dentro do segmento
    public string? Serie { get; set; }

    public int? IdUsuario { get; set; }
}
