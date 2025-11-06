using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Unidade do ativo (moto) rastreável por Tag BLE e endereçada em uma Zona de pátio.
    /// </summary>
    [Index(nameof(Placa), IsUnique = true)]
    [Index(nameof(TagBleId), IsUnique = true)]
    [Index(nameof(ZonaId))]
    public class Moto
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Formato livre validado na camada de serviço (normalizado em UPPER).</summary>
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

        /// <summary>Último beacon que detectou a tag, para telemetria básica.</summary>
        [StringLength(100)]
        public string? UltimoBeaconConhecidoId { get; set; }

        public DateTime? UltimaVezVistoEmPatio { get; set; }

        /// <summary>Chave 1-1 obrigatória da tag BLE associada.</summary>
        [Required(ErrorMessage = "A associação com uma Tag BLE é obrigatória.")]
        public Guid TagBleId { get; set; }

        [ForeignKey(nameof(TagBleId))]
        public virtual TagBle Tag { get; set; } = null!;

        /// <summary>Zona atual no pátio (null = fora do pátio).</summary>
        public Guid? ZonaId { get; set; }

        [ForeignKey(nameof(ZonaId))]
        public virtual Zona? Zona { get; set; }
    }
}
