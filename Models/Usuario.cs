using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("usuarios")]
[Index("Celular", Name = "usuarios_celular_key", IsUnique = true)]
[Index("Cpf", Name = "usuarios_cpf_key", IsUnique = true)]
[Index("Email", Name = "usuarios_email_key", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_perfil")]
    public int IdPerfil { get; set; }

    [Column("nome_completo")]
    [StringLength(150)]
    public string NomeCompleto { get; set; } = null!;

    [Column("cpf")]
    [StringLength(14)]
    public string Cpf { get; set; } = null!;

    [Column("celular")]
    [StringLength(15)]
    public string Celular { get; set; } = null!;

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("senha_hash")]
    [StringLength(255)]
    public string SenhaHash { get; set; } = null!;

    [Column("foto_perfil_url")]
    [StringLength(255)]
    public string? FotoPerfilUrl { get; set; }

    [Column("endereco")]
    public string? Endereco { get; set; }

    [Column("data_nascimento")]
    public DateOnly DataNascimento { get; set; }

    [Column("status_conta")]
    [StringLength(20)]
    public string? StatusConta { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Alocaco> Alocacos { get; set; } = new List<Alocaco>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual DadosAcademico? DadosAcademico { get; set; }

    [InverseProperty("AprovadoPorNavigation")]
    public virtual ICollection<DadosAdministrativo> DadosAdministrativoAprovadoPorNavigations { get; set; } = new List<DadosAdministrativo>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual DadosAdministrativo? DadosAdministrativoIdUsuarioNavigation { get; set; }

    [InverseProperty("IdCriadorAdminNavigation")]
    public virtual ICollection<EventosProva> EventosProvas { get; set; } = new List<EventosProva>();

    [ForeignKey("IdPerfil")]
    [InverseProperty("Usuarios")]
    public virtual Perfi IdPerfilNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("IdUsuarios")]
    public virtual ICollection<Materia> IdMateria { get; set; } = new List<Materia>();

    [Column("chave_pix")]
    public string? ChavePix { get; set; }

    [Column("banco_nome")]
    public string? BancoNome { get; set; }
}