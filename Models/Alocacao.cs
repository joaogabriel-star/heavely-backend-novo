using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace SistemaHEAVELYBackend.Models;

[Table("alocacoes")]
[Index("IdEvento", "IdUsuario", Name = "alocacoes_id_evento_id_usuario_key", IsUnique = true)]
public partial class Alocaco
{
    [Key]
    [Column("id_alocacao")]
    public int IdAlocacao { get; set; }

    [Column("id_evento")]
    public int IdEvento { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("papel_evento")]
    [StringLength(50)]
    public string PapelEvento { get; set; } = null!;

    [Column("status_participacao")]
    [StringLength(50)]
    public string StatusParticipacao { get; set; } = null!;

    [Column("posicao_reserva")]
    public int? PosicaoReserva { get; set; }

    [Column("data_inscricao", TypeName = "timestamp without time zone")]
    public DateTime? DataInscricao { get; set; }

    [Column("check_in_time", TypeName = "timestamp without time zone")]
    public DateTime? CheckInTime { get; set; }

    [Column("check_out_time", TypeName = "timestamp without time zone")]
    public DateTime? CheckOutTime { get; set; }

    [Column("horas_trabalhadas", TypeName = "numeric(5,2)")] // ou "real", dependendo do seu PostgreSQL
     public double? HorasTrabalhadas {get; set;}

     [Column("sala_designada")]
    public string? SalaDesignada { get; set; }
    [Column ("observacoes")]
    public string? Observacoes { get; set; }

    [ForeignKey("IdEvento")]
    [InverseProperty("Alocacos")]
    public virtual EventosProva IdEventoNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Alocacos")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    public string MotivoCancelamento { get; internal set; }
    public DateTime? DataCancelamento { get; internal set; }
}