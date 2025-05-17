using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Entities
{
    public class Moto
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(8, ErrorMessage = "A placa deve ter no máximo 8 caracteres (ex: AAA-1234 ou ABC1D23).")]
        public string? Placa { get; set; }

        [Required(ErrorMessage = "O modelo da moto é obrigatório.")]
        public TipoModeloMoto Modelo { get; set; }
        
        [Required(ErrorMessage = "O status da moto é obrigatório.")]
        public TipoStatusMoto StatusMoto { get; set; }

        public DateTime DataCriacaoRegistro { get; set; }

        public DateTime? DataRecolhimento { get; set; }

        public Guid? FuncionarioRecolhimentoId { get; set; }

        public DateTime? DataEntradaPatio { get; set; }

        // Rastreamento IoT simulado (dentro do pátio)
        [StringLength(100)]
        public string? UltimoBeaconConhecidoId { get; set; }
        public DateTime? UltimaVezVistoEmPatio { get; set; }

        // Relacionamento com TagBle (toda moto no sistema deve ter uma tag)
        [Required(ErrorMessage = "A associação com uma Tag BLE é obrigatória.")]
        public Guid TagBleId { get; set; }

        [ForeignKey("TagBleId")]
        public virtual TagBle Tag { get; set; } = null!;
    }
}
