using Csharp.Api.DTOs;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    /// <summary>Orquestra a entrega do evento IoT ao processador de posição.</summary>
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
