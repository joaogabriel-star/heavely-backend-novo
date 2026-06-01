namespace SistemaHEAVELYBackend.DTOs.Alocacoes;

public class ListaInscritosDTO
{
    public int IdAlocacao { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string PapelEvento { get; set; } = string.Empty;
    public string StatusParticipacao { get; set; } = string.Empty;
    public int? PosicaoReserva { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? HorasTrabalhadas { get; set; }
    public string? MotivoCancelamento  { get; set; } 
    public DateTime? DataCancelamento  { get; set; }
    public string? Antecedencia        { get; set; } 
}