using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("dados_academicos")]
[Index("IdUsuario", Name = "dados_academicos_id_usuario_key", IsUnique = true)]
public partial class DadosAcademico
{
    [Key]
    [Column("id_dados_academico")]
    public int IdDadosAcademico { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("escolaridade_nivel")]
    [StringLength(50)]
    public string? EscolaridadeNivel { get; set; }

    [Column("escolaridade_status")]
    [StringLength(20)]
    public string? EscolaridadeStatus { get; set; }

    [Column("curso_formacao")]
    [StringLength(150)]
    public string? CursoFormacao { get; set; }

    [Column("experiencia_profissional")]
    public string? ExperienciaProfissional { get; set; }

    [Column("nivel_ingles")]
    [StringLength(20)]
    public string? NivelIngles { get; set; }

    [Column("nivel_espanhol")]
    [StringLength(20)]
    public string? NivelEspanhol { get; set; }

    [Column("link_proeficiencia")]
    [StringLength(255)]
    public string? LinkProeficiencia { get; set; }

    [Column("link_diploma_ledor")]
    [StringLength(255)]
    public string? LinkDiplomaLedor { get; set; }

    [Column("link_nada_consta")]
    [StringLength(255)]
    public string? LinkNadaConsta { get; set; }

    [Column("materias_facilidade")]
    [StringLength(255)]
    public string? MateriasFacilidade { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("DadosAcademico")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}