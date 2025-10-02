using System.ComponentModel.DataAnnotations;
using Csharp.Api.Entities.Enums;
using Csharp.Api.DTOs.ValidationAttributes;

namespace Csharp.Api.DTOs
{

    [PlacaCondicionalObrigatoria(ErrorMessage = "A placa é obrigatória para o status da moto selecionado.")]
    public class CreateMotoDto
    {
        [StringLength(8, ErrorMessage = "A placa deve ter no máximo 8 caracteres.")]
        public string? Placa { get; set; }
        [Required(ErrorMessage = "O modelo da moto é obrigatório.")]
        public TipoModeloMoto Modelo { get; set; }
        [Required(ErrorMessage = "O status inicial da moto é obrigatório.")]
        public TipoStatusMoto StatusMoto { get; set; }
        [Required(ErrorMessage = "O código único da tag BLE é obrigatório para associar à moto.")]
        [StringLength(50, ErrorMessage = "O código único da tag deve ter no máximo 50 caracteres.")]
        public string CodigoUnicoTagParaNovaTag { get; set; } = string.Empty;
        public Guid? FuncionarioRecolhimentoId { get; set; }
        public DateTime? DataRecolhimento { get; set; }
    }

    public class UpdateMotoDto
    {
        [StringLength(8, ErrorMessage = "A placa deve ter no máximo 8 caracteres.")]
        public string? Placa { get; set; }
        [Required(ErrorMessage = "O modelo da moto é obrigatório.")]
        public TipoModeloMoto Modelo { get; set; }
        [Required(ErrorMessage = "O status da moto é obrigatório.")]
        public TipoStatusMoto StatusMoto { get; set; }
    }
    
    public class TagBleViewDto
    {
        public Guid Id { get; set; }
        public string CodigoUnicoTag { get; set; } = string.Empty;
        public int NivelBateria { get; set; }
    }

    public class MotoViewDto
    {
        public Guid Id { get; set; }
        public string? Placa { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public string StatusMoto { get; set; } = string.Empty;
        public DateTime DataCriacaoRegistro { get; set; }
        public DateTime? DataRecolhimento { get; set; }
        public Guid? FuncionarioRecolhimentoId { get; set; }
        public DateTime? DataEntradaPatio { get; set; }
        public string? UltimoBeaconConhecidoId { get; set; }
        public DateTime? UltimaVezVistoEmPatio { get; set; }
        public TagBleViewDto? Tag { get; set; }
        public ICollection<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}