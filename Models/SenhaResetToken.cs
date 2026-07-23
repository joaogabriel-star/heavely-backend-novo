using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaHEAVELYBackend.Models;

[Table("senha_reset_tokens")]
[Index("TokenHash", Name = "senha_reset_tokens_token_hash_key", IsUnique = true)]
public partial class SenhaResetToken
{
    [Key]
    [Column("id_token")]
    public int IdToken { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    // Hash SHA256 do token — o token em texto puro só existe no link do email,
    // nunca é persistido (equivale a uma senha temporária de quem tiver o link).
    [Column("token_hash")]
    [StringLength(64)]
    public string TokenHash { get; set; } = null!;

    [Column("expira_em", TypeName = "timestamp without time zone")]
    public DateTime ExpiraEm { get; set; }

    [Column("criado_em", TypeName = "timestamp without time zone")]
    public DateTime CriadoEm { get; set; }

    [Column("usado_em", TypeName = "timestamp without time zone")]
    public DateTime? UsadoEm { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
