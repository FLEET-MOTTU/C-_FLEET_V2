using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Histórico de passagem/posicionamento da moto pelas Zonas do pátio.
    /// </summary>
    [Table("MOTO_ZONA_HIST")]
    [Index(nameof(MotoId), nameof(EntradaEm))]
    [Index(nameof(ZonaId))]
    public class MotoZonaHistorico
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MotoId { get; set; }

        [Required]
        public Guid ZonaId { get; set; }

        /// <summary>Funcionário responsável no evento (opcional, p/ auditoria).</summary>
        public Guid? FuncionarioId { get; set; }

        [Required]
        public DateTime EntradaEm { get; set; }

        public DateTime? SaidaEm { get; set; }

        [ForeignKey(nameof(MotoId))]
        public virtual Moto Moto { get; set; } = null!;

        [ForeignKey(nameof(ZonaId))]
        public virtual Zona Zona { get; set; } = null!;
    }
}
