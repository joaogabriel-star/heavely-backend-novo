namespace SistemaHEAVELYBackend.DTOs.Auth;

public class EsqueciSenhaDTO
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;
}
