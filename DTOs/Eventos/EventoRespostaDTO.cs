namespace SistemaHEAVELYBackend.DTOs.Eventos;

public class EventoRespostaDTO
{
    public int IdEvento { get; set; }
    public string TituloProva { get; set; } = string.Empty;
    public string LocalProva { get; set; } = string.Empty;
    public DateTime DataProva { get; set; }
    public DateTime HorarioFim { get; set; }
    public int VagasLedor { get; set; }
    public int VagasFiscal { get; set; }
    public string StatusEvento { get; set; } = string.Empty;
    public string CriadoPor { get; set; } = string.Empty; // nome do admin

    // Vagas que ainda restam — calculado no Service
    public int VagasLedorDisponiveis { get; set; }
    public int VagasFiscalDisponiveis { get; set; }
    public int VagasPreenchidas { get; set; }
}