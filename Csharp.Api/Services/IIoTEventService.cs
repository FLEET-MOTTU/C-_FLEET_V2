using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface IIoTEventService
    {
        Task ProcessarInteracaoTagAsync(TagInteractionEventDto eventoDto);
    }
}