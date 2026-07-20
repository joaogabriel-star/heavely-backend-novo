namespace SistemaHEAVELYBackend.DTOs.Auth;

public class EnviarDocumentosRespostaDTO
{
    public bool DiplomaEnviado { get; set; }
    public bool NadaConstaEnviado { get; set; }
    public string? ErroDiploma { get; set; }
    public string? ErroNadaConsta { get; set; }
}
