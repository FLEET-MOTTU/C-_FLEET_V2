using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    /// <summary>
    /// Implementação do serviço de classificação de lotes.
    /// Encapsula regras que sugerem zonas e classificam motos conforme regras do pátio.
    /// </summary>
    public class ClassificationService : IClassificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClassificationService> _logger;

        public ClassificationService(AppDbContext context, ILogger<ClassificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

    /// <summary>
    /// Analisa um lote de motos e sugere zonas para cada item conforme regras configuradas.
    /// </summary>
    /// <param name="request">Dados de entrada contendo motos e contexto do pátio.</param>
    /// <returns>Resposta com sugestões de zonas e classificação.</returns>
    public async Task<LoteClassificacaoRespostaDto> SugerirZonasPorEstadoAsync(LoteClassificacaoRequestDto request)
        {
            var resposta = new LoteClassificacaoRespostaDto { PateoId = request.PateoId };

            // cache local de regras por pátio
            var regras = await _context.ZonaRegrasStatus
                .AsNoTracking()
                .Include(r => r.Zona)
                .Where(r => r.PateoId == request.PateoId)
                .ToListAsync();

            foreach (var item in request.Itens)
            {
                var regra = regras
                    .Where(r => r.StatusMoto == item.StatusMoto)
                    .OrderBy(r => r.Prioridade)
                    .FirstOrDefault();

                var sugestao = new LoteClassificacaoSugestaoDto
                {
                    Placa = item.Placa?.ToUpperInvariant(),
                    TagCodigo = item.TagCodigo.ToUpperInvariant(),
                    StatusMoto = item.StatusMoto.ToString()
                };

                if (regra == null)
                {
                    sugestao.Justificativa = "Sem regra mapeada para este status neste pátio. Configure ZONA_REGRA_STATUS.";
                }
                else
                {
                    sugestao.ZonaIdSugerida = regra.ZonaId;
                    sugestao.ZonaNomeSugerida = regra.Zona.Nome;
                    sugestao.Justificativa = $"Regra: {item.StatusMoto} → '{regra.Zona.Nome}' (prioridade {regra.Prioridade}).";
                    sugestao.Links.Add(new LinkDto($"api/zonas/{regra.ZonaId}", "zona_detalhes", "GET"));
                }

                if (!string.IsNullOrWhiteSpace(sugestao.Placa))
                    sugestao.Links.Add(new LinkDto($"api/motos/por-placa/{sugestao.Placa}", "ver_moto_por_placa", "GET"));

                resposta.Sugestoes.Add(sugestao);
            }

            return resposta;
        }
    }
}
