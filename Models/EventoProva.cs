using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("eventos_prova")]
public partial class EventosProva
{
    [Key]
    [Column("id_evento")]
    public int IdEvento { get; set; }

    [Column("id_criador_admin")]
    public int IdCriadorAdmin { get; set; }

    [Column("titulo_prova")]
    [StringLength(150)]
    public string TituloProva { get; set; } = null!;

    [Column("local_prova")]
    [StringLength(150)]
    public string? LocalProva { get; set; }

    [Column("data_prova", TypeName = "timestamp without time zone")]
    public DateTime DataProva { get; set; }

    [Column("horario_fim", TypeName = "timestamp without time zone")]
    public DateTime HorarioFim { get; set; }

    [Column("vagas_ledor")]
    public int VagasLedor { get; set; }

    [Column("vagas_fiscal")]
    public int VagasFiscal { get; set; }

    [Column("status_evento")]
    [StringLength(50)]
    public string? StatusEvento { get; set; }

    [Column("observacoes")]
    public string? Observacoes { get; set; }

    [InverseProperty("IdEventoNavigation")]
    public virtual ICollection<Alocaco> Alocacos { get; set; } = new List<Alocaco>();

    [ForeignKey("IdCriadorAdmin")]
    [InverseProperty("EventosProvas")]
    public virtual Usuario IdCriadorAdminNavigation { get; set; } = null!;
}