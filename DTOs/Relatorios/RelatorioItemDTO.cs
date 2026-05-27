// DTOs/Relatorios/RelatorioItemDTO.cs
// Representa uma linha do relatório — um usuário por evento
namespace SistemaHEAVELYBackend.DTOs.Relatorios;

public class RelatorioItemDTO
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string PapelEvento { get; set; } = string.Empty;
    public string StatusParticipacao { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? HorasTrabalhadas { get; set; }
    public decimal ValorAPagar { get; set; }
}