using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Pátio (réplica de fonte Java). Não altere schema/colunas daqui.
    /// </summary>
    [Table("PATEOS_SYNC")]
    public class Pateo
    {
        [Key]
        [Column("ID")]
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [Column("PLANTA_BAIXA_URL")]
        public string? PlantaBaixaUrl { get; set; }

        [Column("PLANTA_LARGURA")]
        public int? PlantaLargura { get; set; }

        [Column("PLANTA_ALTURA")]
        public int? PlantaAltura { get; set; }

        [Column("GERENCIADO_POR_ID")]
        public Guid GerenciadoPorId { get; set; }

        [Required, StringLength(20)]
        [Column("STATUS")]
        public string Status { get; set; } = string.Empty;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }

        // Navegações (lado C#)
        public virtual ICollection<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();
        public virtual ICollection<Zona> Zonas { get; set; } = new List<Zona>();
    }
}
