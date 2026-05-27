// DTOs/Usuarios/PerfilRespostaDTO.cs
// O que a API devolve quando alguém pede o perfil
namespace SistemaHEAVELYBackend.DTOs.Usuarios;

public class PerfilRespostaDTO
{
    public int IdUsuario { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public DateTime DataNascimento { get; set; }
    public string StatusConta { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string ChavePix {get; set;} = string.Empty;
    public string BancoNome {get; set;} = string.Empty;

    // Dados acadêmicos — só para Ledor/Fiscal
    public DadosAcademicoDTO? DadosAcademicos { get; set; }

    // Dados administrativos — só para Admin
    public DadosAdministrativoDTO? DadosAdministrativos { get; set; }
}