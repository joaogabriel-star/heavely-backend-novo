namespace SistemaHEAVELYBackend.DTOs.Usuarios;

public class ListaUsuariosDTO
{
    public int IdUsuario { get; set; } 
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string StatusConta { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? StatusAprovacao { get; set; } = string.Empty;
    public string? CargoInstituicao { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string DataNascimento { get; set; } = string.Empty;
    public string Escolaridade { get; set; } = string.Empty;
    public string CursoFormacao { get; set; } = string.Empty;
    public string MateriasFacilidade { get; set; } = string.Empty;
    public string? LinkDiplomaLedor { get; set; } = string.Empty;
    public string? ChavePix {get; set;} = string.Empty;
    public string? BancoNome {get; set;} 
}