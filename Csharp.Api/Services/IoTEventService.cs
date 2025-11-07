using Csharp.Api.DTOs;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    /// <summary>Orquestra a entrega do evento IoT ao processador de posição.</summary>
    /// <summary>
    /// Implementação do serviço que processa eventos de IoT (detecção de tags pelos beacons).
    /// Aplica lógica de processamento e enfileiramento/consumo conforme necessidade.
    /// </summary>
    public class IoTEventService : IIoTEventService
    {
        private readonly ILogger<IoTEventService> _logger;
        private readonly ITagPositionProcessor _tagPositionProcessor;

        public IoTEventService(ILogger<IoTEventService> logger,
                               ITagPositionProcessor tagPositionProcessor)
        {
            _logger = logger;
            _tagPositionProcessor = tagPositionProcessor;
        }

    /// <summary>
    /// Processa um evento de interação de tag (recebido por API ou fila).
    /// Aplica lógica de negócio e encaminha para processamento/armazenamento.
    /// </summary>
    /// <param name="eventoDto">Dados do evento de interação da tag.</param>
    public async Task ProcessarInteracaoTagAsync(TagInteractionEventDto eventoDto)
        {
            _logger.LogInformation(
                "IoT: Tag {Tag} vista no Beacon {Beacon} às {Ts}. Bateria={Bat}, Tipo={Tipo}",
                eventoDto.CodigoUnicoTag, eventoDto.BeaconIdDetectado, eventoDto.Timestamp,
                eventoDto.NivelBateria?.ToString() ?? "N/A", eventoDto.TipoEvento ?? "N/A");

            await _tagPositionProcessor.ProcessAsync(eventoDto);
        }
    }
}
