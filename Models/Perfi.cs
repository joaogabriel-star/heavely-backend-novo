using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("perfis")]
public partial class Perfi
{
    [Key]
    [Column("id_perfil")]
    public int IdPerfil { get; set; }

    [Column("nome_perfil")]
    [StringLength(50)]
    public string NomePerfil { get; set; } = null!;

    [InverseProperty("IdPerfilNavigation")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}