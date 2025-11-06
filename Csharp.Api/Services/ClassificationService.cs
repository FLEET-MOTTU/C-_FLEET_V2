using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClassificationService> _logger;

        public ClassificationService(AppDbContext context, ILogger<ClassificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

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
