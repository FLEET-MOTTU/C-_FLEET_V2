using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Entities
{
    [Table("FUNCIONARIOS_SYNC")]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Telefone), IsUnique = true)]
    public class Funcionario
    {
        [Key]
        [Column("ID")]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("EMAIL")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("TELEFONE")]
        public string Telefone { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("CARGO")]
        public string Cargo { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("STATUS")]
        public string Status { get; set; } = string.Empty;

        [Column("PATEO_ID")]
        public Guid PateoId { get; set; }

        [Column("FOTO_URL")]
        public string? FotoUrl { get; set; }

        [ForeignKey("PateoId")]
        public virtual Pateo Pateo { get; set; } = null!;
    }
}
