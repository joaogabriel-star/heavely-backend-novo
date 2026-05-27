using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("dados_administrativo")]
[Index("IdUsuario", Name = "dados_administrativo_id_usuario_key", IsUnique = true)]
public partial class DadosAdministrativo
{
    [Key]
    [Column("id_dados_administrativo")]
    public int IdDadosAdministrativo { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("cargo_instituicao")]
    [StringLength(100)]
    public string CargoInstituicao { get; set; } = null!;

    [Column("email_institucional")]
    [StringLength(255)]
    public string? EmailInstitucional { get; set; }

    [Column("status_aprovacao")]
    [StringLength(20)]
    public string? StatusAprovacao { get; set; }

    [Column("aprovado_por")]
    public int? AprovadoPor { get; set; }

    [ForeignKey("AprovadoPor")]
    [InverseProperty("DadosAdministrativoAprovadoPorNavigations")]
    public virtual Usuario? AprovadoPorNavigation { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("DadosAdministrativoIdUsuarioNavigation")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}