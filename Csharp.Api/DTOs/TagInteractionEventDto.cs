using System;
using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{

    public class TagInteractionEventDto
    {

        [Required(ErrorMessage = "O código único da tag é obrigatório.")]
        [StringLength(50, ErrorMessage = "O código único da tag deve ter no máximo 50 caracteres.")]
        public string CodigoUnicoTag { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ID do beacon detectado é obrigatório.")]
        [StringLength(100, ErrorMessage = "O ID do beacon deve ter no máximo 100 caracteres.")]
        public string BeaconIdDetectado { get; set; } = string.Empty;

        [Required(ErrorMessage = "O timestamp do evento é obrigatório.")]
        public DateTime Timestamp { get; set; } 

        [Range(0, 100, ErrorMessage = "Nível da bateria deve ser entre 0 e 100, se informado.")]
        public int? NivelBateria { get; set; }
        
        [StringLength(50, ErrorMessage = "O tipo do evento deve ter no máximo 50 caracteres.")]
        public string? TipoEvento { get; set; }
    }
}
