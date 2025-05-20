using System;
using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{

    /// <summary>
    /// Representa os dados de um evento de interação de uma tag BLE, 
    /// tipicamente quando uma tag é detectada por um beacon ou reporta seu status.
    /// Este DTO é o payload esperado do simulador Python e futuramente de um gateway IoT.
    /// </summary>
    public class TagInteractionEventDto
    {

        /// <summary>
        /// O código único e identificador da tag BLE que disparou o evento.
        /// Este código deve corresponder ao 'CodigoUnicoTag' de uma entidade TagBle registrada no sistema.
        /// </summary>
        /// <example>BEACON_PATIO_ZONA_A1</example>
        [Required(ErrorMessage = "O código único da tag é obrigatório.")]
        [StringLength(50, ErrorMessage = "O código único da tag deve ter no máximo 50 caracteres.")]
        public string CodigoUnicoTag { get; set; } = string.Empty;

        /// <summary>
        /// O identificador do beacon que detectou a tag ou ao qual este evento está associado.
        /// </summary>
        /// <example>BEACON_PATIO_ZONA_A1</example>
        [Required(ErrorMessage = "O ID do beacon detectado é obrigatório.")]
        [StringLength(100, ErrorMessage = "O ID do beacon deve ter no máximo 100 caracteres.")]
        public string BeaconIdDetectado { get; set; } = string.Empty;

        /// <summary>
        /// A data e hora exatas (UTC) em que a interação ou leitura ocorreu.
        /// </summary>
        /// <example>2025-05-20T14:30:00Z</example>
        [Required(ErrorMessage = "O timestamp do evento é obrigatório.")]
        public DateTime Timestamp { get; set; } 
        
        /// <summary>
        /// (Temporariamente Opcional) O nível atual da bateria da tag, em porcentagem (0-100).
        /// Se não informado, o nível da bateria da tag não será atualizado por este evento.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Nível da bateria deve ser entre 0 e 100, se informado.")]
        public int? NivelBateria { get; set; }
        
        /// <summary>
        /// (Temporariamente Opcional) O tipo de evento enviado pelo beacon ou pela tag.
        /// </summary>
        /// <example>entrada_zona_manutencao</example>
        /// <example>leitura_periodica_bateria</example>
        [StringLength(50, ErrorMessage = "O tipo do evento deve ter no máximo 50 caracteres.")]
        public string? TipoEvento { get; set; }
    }
}
