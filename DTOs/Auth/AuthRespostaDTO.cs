namespace SistemaHEAVELYBackend.DTOs.Auth;

public class AuthRespostaDTO
{
    public int IdUsuario { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string StatusConta { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}