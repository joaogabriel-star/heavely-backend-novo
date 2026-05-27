
namespace SistemaHEAVELYBackend.DTOs.Relatorios;

public class RelatorioEventoDTO
{
    public string TituloProva { get; set; } = string.Empty;
    public string LocalProva { get; set; } = string.Empty;
    public DateTime DataProva { get; set; }
    public DateTime HorarioFim { get; set; }
    public List<RelatorioItemDTO> Participantes { get; set; } = new();

    // Totalizadores
    public int TotalPresentes { get; set; }
    public double TotalHoras { get; set; }
    public decimal TotalValorGeral { get; set; }
}