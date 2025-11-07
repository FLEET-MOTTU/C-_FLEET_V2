using System;
using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{
    /// <summary>
    /// DTO para criação de um Beacon (gateway).
    /// </summary>
    public class CreateBeaconDto
    {
        [Required, StringLength(100)]
        public string BeaconId { get; set; } = string.Empty;

        public bool Ativo { get; set; }
    }

    /// <summary>
    /// DTO para atualização de um Beacon.
    /// </summary>
    public class UpdateBeaconDto
    {
        public bool Ativo { get; set; }

        [StringLength(100)]
        public string? UltimaZonaDetectada { get; set; }
    }

    /// <summary>
    /// DTO de visualização de Beacon.
    /// </summary>
    public class BeaconDto
    {
        public Guid Id { get; set; }
        public string BeaconId { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string? UltimaZonaDetectada { get; set; }
        public DateTime? UltimaVezVisto { get; set; }
    }
}
