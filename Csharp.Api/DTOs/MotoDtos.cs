using System.ComponentModel.DataAnnotations;
using Csharp.Api.Entities.Enums;
using Csharp.Api.DTOs.ValidationAttributes;

namespace Csharp.Api.DTOs
{

    /// <summary>
    /// DTO para criação de uma nova moto. Contém os dados iniciais da moto e da tag a ser associada.
    /// </summary>
    [PlacaCondicionalObrigatoria(ErrorMessage = "A placa é obrigatória para o status da moto selecionado.")]

    public class CreateMotoDto
    {
        /// <summary>
        /// Placa da moto (formato Mercosul LLLNLNN ou antiga LLL-NNNN).
        /// Opcional apenas se statusMoto for 'SemPlacaEmColeta'.
        /// </summary>
        [StringLength(8, ErrorMessage = "A placa deve ter no máximo 8 caracteres.")]
        public string? Placa { get; set; }

        /// <summary>
        /// Modelo da moto.
        /// Valores permitidos (placeholders): ModeloSport100, ModeloUrbana125, ModeloTrilha150.
        /// </summary>
        [Required(ErrorMessage = "O modelo da moto é obrigatório.")]
        public TipoModeloMoto Modelo { get; set; }
        
        /// <summary>
        /// Status inicial da moto no momento da coleta. Deve corresponder a um dos valores definidos no enum TipoStatusMoto.
        /// </summary>
        [Required(ErrorMessage = "O status inicial da moto é obrigatório.")]
        public TipoStatusMoto StatusMoto { get; set; }

        /// <summary>
        /// Código único da nova Tag BLE que será criada e associada a esta moto.
        /// </summary>
        [Required(ErrorMessage = "O código único da tag BLE é obrigatório para associar à moto.")]
        [StringLength(50, ErrorMessage = "O código único da tag deve ter no máximo 50 caracteres.")]
        public string CodigoUnicoTagParaNovaTag { get; set; } = string.Empty;
        
        /// <summary>
        /// ID (GUID) do funcionário que realizou o recolhimento.
        /// Opcional, mas recomendado para rastreabilidade.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid? FuncionarioRecolhimentoId { get; set; }

        /// <summary>
        /// Data e hora em que o recolhimento da moto foi registrado.
        /// Deve ser enviado pelo dispositivo do funcionário no formato UTC.
        /// </summary>
        /// <example>2025-05-20T10:00:00Z</example>
        public DateTime? DataRecolhimento { get; set; }
    }


    /// <summary>
    /// DTO para atualização dos dados de uma moto existente.
    /// Permite alterar a placa, modelo e status da moto.
    /// </summary>
    public class UpdateMotoDto
    {
        /// <summary>
        /// Nova placa para a moto (opcional). Se fornecido, substitui a placa existente.
        /// Formatos aceitos: Mercosul (LLLNLNN) ou antiga (LLL-NNNN / LLLNNNN).
        /// </summary>
        [StringLength(8, ErrorMessage = "A placa deve ter no máximo 8 caracteres.")]
        public string? Placa { get; set; }

        /// <summary>
        /// Novo modelo para a moto.
        /// Valores permitidos (placeholders): ModeloSport100, ModeloUrbana125, ModeloTrilha150.
        /// </summary>
        [Required(ErrorMessage = "O modelo da moto é obrigatório.")]
        public TipoModeloMoto Modelo { get; set; }

        /// <summary>
        /// Novo status para a moto. Deve corresponder a um dos valores definidos no enum TipoStatusMoto.
        /// </summary>
        [Required(ErrorMessage = "O status da moto é obrigatório.")]
        public TipoStatusMoto StatusMoto { get; set; }
    }

    /// <summary>
    /// DTO para visualização dos dados resumidos de uma Tag BLE.
    /// </summary>
    public class TagBleViewDto
    {

        /// <summary>
        /// O ID único (GUID) da entidade TagBle no sistema.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// O código único da tag física, usado para identificação pelos beacons.
        /// </summary>
        /// <example>TAG_MOTO_001A</example>
        public string CodigoUnicoTag { get; set; } = string.Empty;

        /// <summary>
        /// Nível atual da bateria da tag, em porcentagem (0-100).
        /// </summary>
        public int NivelBateria { get; set; }
    }

    /// <summary>
    /// DTO para visualização dos dados completos de uma moto, incluindo sua tag associada.
    /// </summary>
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
    }
}
