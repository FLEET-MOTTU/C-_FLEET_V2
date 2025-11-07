using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Dispositivo físico (gateway) que detecta tags BLE e pode estar associado a uma Zona.
    /// </summary>
    [Index(nameof(BeaconId), IsUnique = true)]
    [Index(nameof(ZonaId))]
    public class Beacon
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O ID único do beacon é obrigatório.")]
        [StringLength(100, ErrorMessage = "O ID do beacon deve ter no máximo 100 caracteres.")]
        public string BeaconId { get; set; } = string.Empty;

        public bool Ativo { get; set; }

    [StringLength(100)]
    public string? UltimaZonaDetectada { get; set; }

        public DateTime? UltimaVezVisto { get; set; }

        /// <summary>Zona física (opcional) em que o beacon está instalado.</summary>
        public Guid? ZonaId { get; set; }

        [ForeignKey(nameof(ZonaId))]
        public virtual Zona? Zona { get; set; }
    }
}
