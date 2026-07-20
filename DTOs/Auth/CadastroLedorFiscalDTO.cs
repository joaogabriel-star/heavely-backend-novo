using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaHEAVELYBackend.DTOs.Auth;

public class CadastroLedorFiscalDTO
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

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    public string? Endereco { get; set; }

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    [Required(ErrorMessage = "Informe se possui certificado de ledor")]
    public bool PossuiCertificadoLedor { get; set; }

    public string? EscolaridadeNivel { get; set; }
    public string? EscolaridadeStatus { get; set; }
    public string? InstituicaoEnsino { get; set; }
    public string? NivelIngles { get; set; }
    public string? NivelEspanhol { get; set; }
    public string? ExperienciaProfissional { get; set; }
    public string? MateriasFacilidade { get; set; }
}