using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("materias")]
public partial class Materia
{
    [Key]
    [Column("id_materia")]
    public int IdMateria { get; set; }

    [Column("nome_materia")]
    [StringLength(100)]
    public string NomeMateria { get; set; } = null!;

    [ForeignKey("IdMateria")]
    [InverseProperty("IdMateria")]
    public virtual ICollection<Usuario> IdUsuarios { get; set; } = new List<Usuario>();
}