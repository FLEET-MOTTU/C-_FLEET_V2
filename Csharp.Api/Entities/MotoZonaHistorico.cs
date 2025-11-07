using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Registro histórico de entradas/saídas de motos nas zonas do pátio (auditoria de posição).
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
