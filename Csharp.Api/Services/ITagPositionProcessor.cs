using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    /// <summary>Aplica regra de posicionamentoa moto</summary>
    public interface ITagPositionProcessor
    {
        Task ProcessAsync(TagInteractionEventDto eventoDto);
    }
}
