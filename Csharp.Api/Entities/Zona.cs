using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Zona física do pátio. Coordenadas armazenadas em WKT (geometria como texto).
    /// </summary>
    [Table("ZONAS_SYNC")]
    public class Zona
    {
        [Key]
        [Column("ID")]
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [Column("PATEO_ID")]
        public Guid PateoId { get; set; }

        [ForeignKey(nameof(PateoId))]
        public virtual Pateo Pateo { get; set; } = null!;

        [Column("CRIADO_POR_ID")]
        public Guid CriadoPorId { get; set; }

        [Required]
        [Column("COORDENADAS_WKT")]
        public string CoordenadasWKT { get; set; } = string.Empty;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }
    }
}
