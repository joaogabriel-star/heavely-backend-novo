namespace SistemaHEAVELYBackend.DTOs.Ocorrencias;

public class CriarOcorrenciaDTO
{
    [Required(ErrorMessage = "Tipo é obrigatório")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    public string Descricao { get; set; } = string.Empty;
}
