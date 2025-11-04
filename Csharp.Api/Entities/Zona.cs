using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// NOTA: Para usar 'Polygon', você precisará do pacote NuGet 'NetTopologySuite.IO.Oracle'
// e configurar o DbContext. Por enquanto, vamos salvar como WKT (texto) para simplificar.
// using NetTopologySuite.Geometries; 

namespace Csharp.Api.Entities
{
    // Cópia da tabela "zona" do Java
    [Table("ZONAS_SYNC")]
    public class Zona
    {
        [Key]
        [Column("ID")]
        public Guid Id { get; set; } // Vem do Java

        [Required]
        [StringLength(100)]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [Column("PATEO_ID")]
        public Guid PateoId { get; set; }

        [ForeignKey("PateoId")]
        public virtual Pateo Pateo { get; set; } = null!;

        [Column("CRIADO_POR_ID")]
        public Guid CriadoPorId { get; set; } // ID do UsuarioAdmin

        [Required]
        [Column("COORDENADAS_WKT")]
        public string CoordenadasWKT { get; set; } = string.Empty;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }
    }
}