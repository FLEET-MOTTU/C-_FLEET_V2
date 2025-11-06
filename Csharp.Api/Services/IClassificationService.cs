using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    /// <summary>Classificação de lote por status para sugerir zonas do pátio.</summary>
    public interface IClassificationService
    {
        Task<LoteClassificacaoRespostaDto> SugerirZonasPorEstadoAsync(LoteClassificacaoRequestDto request);
    }
}
