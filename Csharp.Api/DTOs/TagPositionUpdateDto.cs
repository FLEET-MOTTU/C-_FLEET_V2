using System;
using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{
    /// <summary>Mensagem consumida da fila de posicionamento (gateways/beacons).</summary>
    public class TagPositionUpdateDto
    {
        [Required, StringLength(50)]
        public string CodigoUnicoTag { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string BeaconIdDetectado { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>Opcional. Se informado, atualiza a % de bateria da Tag.</summary>
        [Range(0, 100)]
        public int? NivelBateria { get; set; }
    }
}
