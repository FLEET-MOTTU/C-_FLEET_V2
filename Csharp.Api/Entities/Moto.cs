using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Representa uma moto rastreável por Tag BLE, com associação a zona e dados de telemetria.
    /// </summary>
    [Index(nameof(Placa), IsUnique = true)]
    [Index(nameof(TagBleId), IsUnique = true)]
    [Index(nameof(ZonaId))]
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

    [StringLength(100)]
    public string? UltimoBeaconConhecidoId { get; set; }

        public DateTime? UltimaVezVistoEmPatio { get; set; }

    [Required(ErrorMessage = "A associação com uma Tag BLE é obrigatória.")]
    public Guid TagBleId { get; set; }

        [ForeignKey(nameof(TagBleId))]
        public virtual TagBle Tag { get; set; } = null!;

    public Guid? ZonaId { get; set; }

        [ForeignKey(nameof(ZonaId))]
        public virtual Zona? Zona { get; set; }
    }
}
