namespace SistemaHEAVELYBackend.DTOs.Auth;

public class CadastroAdminDTO
{
    [Required(ErrorMessage = "Nome completo é obrigatório")]
    [MaxLength(150)]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 números")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Celular é obrigatório")]
    public string Celular { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email institucional é obrigatório")]
    [EmailAddress(ErrorMessage = "Email institucional inválido")]
    public string EmailInstitucional { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cargo é obrigatório")]
    public string CargoInstituicao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }
}