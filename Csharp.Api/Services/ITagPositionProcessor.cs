using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    /// <summary>
    /// Aplica regra de posicionamento:
    /// - atualiza último beacon/timestamp na moto;
    /// - movimenta zona e histórico se o beacon estiver vinculado a uma Zona.
    /// </summary>
    public interface ITagPositionProcessor
    {
        Task ProcessAsync(TagInteractionEventDto eventoDto);
    }
}
