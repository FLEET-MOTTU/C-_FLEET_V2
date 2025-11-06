using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    /// <summary>Ingestão de eventos IoT vindos via HTTP.</summary>
    public interface IIoTEventService
    {
        Task ProcessarInteracaoTagAsync(TagInteractionEventDto eventoDto);
    }
}
