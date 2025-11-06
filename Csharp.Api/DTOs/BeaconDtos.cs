using System;
using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{
    public class CreateBeaconDto
    {
        [Required, StringLength(100)]
        public string BeaconId { get; set; } = string.Empty;

        public bool Ativo { get; set; }
    }

    public class UpdateBeaconDto
    {
        public bool Ativo { get; set; }

        [StringLength(100)]
        public string? UltimaZonaDetectada { get; set; }
    }

    public class BeaconDto
    {
        public Guid Id { get; set; }
        public string BeaconId { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string? UltimaZonaDetectada { get; set; }
        public DateTime? UltimaVezVisto { get; set; }
    }
}
