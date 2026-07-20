using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("ocorrencias")]
public partial class Ocorrencia
{
    [Key]
    [Column("id_ocorrencia")]
    public int IdOcorrencia { get; set; }

    [Column("id_evento")]
    public int IdEvento { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    public string Tipo { get; set; } = null!;

    [Column("descricao")]
    public string Descricao { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("IdEvento")]
    public virtual EventosProva IdEventoNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
