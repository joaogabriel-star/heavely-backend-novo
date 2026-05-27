namespace SistemaHEAVELYBackend.DTOs.Usuarios;

public class AtualizarPerfilDTO
{
    [MaxLength(150)]
    public string? NomeCompleto { get; set; }

    [MaxLength(15)]
    public string? Celular { get; set; }

    public string? Endereco { get; set; }

    public string? EscolaridadeNivel { get; set; }
    public string? EscolaridadeStatus { get; set; }
    public string? CursoFormacao { get; set; }
    public string? MateriasFacilidade { get; set; }
    public string? ExperienciaProfissional { get; set; }
    public string? NivelIngles { get; set; }
    public string? NivelEspanhol { get; set; }
}