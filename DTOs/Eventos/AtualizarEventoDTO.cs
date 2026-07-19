namespace SistemaHEAVELYBackend.DTOs.Eventos;

public class AtualizarEventoDTO
{
    [MaxLength(150)]
    public string? TituloProva { get; set; }

    [MaxLength(150)]
    public string? LocalProva { get; set; }

    public DateTime? DataProva { get; set; }
    public DateTime? HorarioFim { get; set; }

    [Range(0, 999)]
    public int? VagasLedor { get; set; }

    [Range(0, 999)]
    public int? VagasFiscal { get; set; }

    [MaxLength(50)]
    public string? Serie { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Valor/hora deve ser positivo")]
    public decimal? ValorHora { get; set; }

    public string? Observacoes { get; set; }
}