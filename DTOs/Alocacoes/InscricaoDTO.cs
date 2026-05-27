namespace SistemaHEAVELYBackend.DTOs.Alocacoes;

public class InscricaoDTO
{
    [Required(ErrorMessage = "Papel é obrigatório")]
    public string PapelEvento { get; set; } = string.Empty;
    // "Ledor" ou "Fiscal"
}