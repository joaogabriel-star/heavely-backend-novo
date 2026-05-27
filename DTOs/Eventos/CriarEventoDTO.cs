namespace SistemaHEAVELYBackend.DTOs.Eventos;

public class CriarEventoDTO
{
    [Required(ErrorMessage = "Título da prova é obrigatório")]
    [MaxLength(150)]
    public string TituloProva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Local da prova é obrigatório")]
    [MaxLength(150)]
    public string LocalProva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de início é obrigatória")]
    public DateTime DataProva { get; set; }

    [Required(ErrorMessage = "Horário de fim é obrigatório")]
    public DateTime HorarioFim { get; set; }

    [Required(ErrorMessage = "Vagas para ledor são obrigatórias")]
    [Range(0, 999, ErrorMessage = "Vagas deve ser entre 0 e 999")]
    public int VagasLedor { get; set; }

    [Required(ErrorMessage = "Vagas para fiscal são obrigatórias")]
    [Range(0, 999, ErrorMessage = "Vagas deve ser entre 0 e 999")]
    public int VagasFiscal { get; set; }
}