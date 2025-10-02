using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Entities
{
    [Index(nameof(BeaconId), IsUnique = true)]
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
    }
}