namespace SistemaHEAVELYBackend.DTOs.NotaFiscal;

public class NotaFiscalFiltroDTO
{
    [Required(ErrorMessage = "Data de início é obrigatória")]
    public DateTime DataInicio { get; set; }

    [Required(ErrorMessage = "Data de fim é obrigatória")]
    public DateTime DataFim { get; set; }

    // "EF2" ou "EM" — omitido/null = todos os segmentos (+ "Sem Série", se houver) no mesmo relatório
    public string? Segmento { get; set; }

    // valor exato de Serie (ex: "7EF2") — filtro dentro do segmento
    public string? Serie { get; set; }

    public int? IdUsuario { get; set; }
}
