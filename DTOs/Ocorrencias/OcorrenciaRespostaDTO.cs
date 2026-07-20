namespace SistemaHEAVELYBackend.DTOs.Ocorrencias;

public class OcorrenciaRespostaDTO
{
    public int IdOcorrencia { get; set; }
    public int IdEvento { get; set; }
    public string TituloProva { get; set; } = string.Empty;
    public string NomeUsuario { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
