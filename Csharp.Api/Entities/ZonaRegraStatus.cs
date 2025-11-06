using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Regras de roteamento por status → zona (por pátio), com prioridade.
    /// </summary>
    [Table("ZONA_REGRA_STATUS")]
    [Index(nameof(PateoId), nameof(StatusMoto), nameof(Prioridade), IsUnique = true)]
    [Index(nameof(ZonaId))]
    public class ZonaRegraStatus
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PateoId { get; set; }

        [Required]
        public TipoStatusMoto StatusMoto { get; set; }

        [Required]
        public Guid ZonaId { get; set; }

        /// <summary>Menor número = maior prioridade.</summary>
        [Required]
        public int Prioridade { get; set; }

        [ForeignKey(nameof(ZonaId))]
        public virtual Zona Zona { get; set; } = null!;

        [ForeignKey(nameof(PateoId))]
        public virtual Pateo Pateo { get; set; } = null!;
    }
}
