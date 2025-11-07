using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Entities
{
    /// <summary>
    /// Tag BLE física afixada à moto; referência primária de telemetria e associação com a entidade <see cref="Moto"/>.
    /// </summary>
    [Index(nameof(CodigoUnicoTag), IsUnique = true)]
    public class TagBle
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O código único da tag é obrigatório.")]
        [StringLength(50, ErrorMessage = "O código único da tag deve ter no máximo 50 caracteres.")]
        public string CodigoUnicoTag { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Nível da bateria deve ser entre 0 e 100.")]
        public int NivelBateria { get; set; }

    public virtual Moto? Moto { get; set; }
    }
}
