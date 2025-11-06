using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Csharp.Api.DTOs.Validation;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.DTOs
{
    /// <summary>DTO de criação de moto (form vindo do front após scan).</summary>
    [PlacaCondicionalObrigatoria(ErrorMessage = "A placa é obrigatória para o status da moto selecionado.")]
    public class CreateMotoDto
    {
        [StringLength(8)]
        public string? Placa { get; set; }

        [Required]
        public TipoModeloMoto Modelo { get; set; }

        [Required]
        public TipoStatusMoto StatusMoto { get; set; }

        [Required, StringLength(50)]
        public string CodigoUnicoTagParaNovaTag { get; set; } = string.Empty;

        public Guid? FuncionarioRecolhimentoId { get; set; }
        public DateTime? DataRecolhimento { get; set; }
    }

    /// <summary>DTO de atualização de moto (dados principais).</summary>
    public class UpdateMotoDto
    {
        [StringLength(8)]
        public string? Placa { get; set; }

        [Required]
        public TipoModeloMoto Modelo { get; set; }

        [Required]
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

    /// <summary>Upsert por placa (cria/atualiza e associa Tag).</summary>
    public class UpsertMotoPorPlacaDto
    {
        [StringLength(8)]
        public string? Placa { get; set; }

        [Required]
        public TipoModeloMoto Modelo { get; set; }

        [Required]
        public TipoStatusMoto StatusMoto { get; set; }

        [Required, StringLength(50)]
        public string CodigoUnicoTag { get; set; } = string.Empty;

        public Guid? ZonaId { get; set; }
    }

    /// <summary>Reatribuição (substituição) de Tag de uma moto.</summary>
    public class ReassignTagDto
    {
        [Required, StringLength(50)]
        public string CodigoUnicoTagNovo { get; set; } = string.Empty;
    }
}
