namespace SistemaHEAVELYBackend.DTOs.Ponto;

public class HistoricoDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string Entrada { get; set; } = string.Empty;
    public string Saida { get; set; } = string.Empty;
    public double? HorasTrabalhadas { get; set; } // O tipo que arrumámos antes!
    public string Funcao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}