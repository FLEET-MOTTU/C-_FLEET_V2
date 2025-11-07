using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    /// <summary>
    /// Aplica a movimentação de zona/histórico com base no beacon detectado.
    /// </summary>
    /// <summary>
    /// Processador responsável por regras que tratam atualizações de posição de tags (posição e zona).
    /// Utilizado internamente para aplicar lógica de negócio e atualização do estado das motos.
    /// </summary>
    public class TagPositionProcessor : ITagPositionProcessor
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TagPositionProcessor> _logger;

        public TagPositionProcessor(AppDbContext context, ILogger<TagPositionProcessor> logger)
        {
            _context = context;
            _logger = logger;
        }

    /// <summary>
    /// Processa a atualização de posição de uma tag (determina zona e atualiza estado da moto).
    /// </summary>
    /// <param name="eventoDto">Evento de interação da tag com informações de beacon/zona.</param>
    public async Task ProcessAsync(TagInteractionEventDto eventoDto)
        {
            var tagCodigo = eventoDto.CodigoUnicoTag?.ToUpperInvariant();
            var beaconId  = eventoDto.BeaconIdDetectado?.ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(tagCodigo) || string.IsNullOrWhiteSpace(beaconId))
            {
                _logger.LogWarning("Processor: tag/beacon vazio.");
                return;
            }

            var tag = await _context.TagsBle
                .Include(t => t.Moto)
                .SingleOrDefaultAsync(t => t.CodigoUnicoTag.ToUpper() == tagCodigo);

            if (tag == null)
            {
                _logger.LogWarning("Processor: Tag desconhecida {Tag}.", tagCodigo);
                return;
            }

            if (eventoDto.NivelBateria.HasValue && tag.NivelBateria != eventoDto.NivelBateria.Value)
                tag.NivelBateria = eventoDto.NivelBateria.Value;

            if (tag.Moto == null)
            {
                _logger.LogWarning("Processor: Tag {Tag} sem moto. Persisti bateria (se houver) e fim.", tagCodigo);
                await _context.SaveChangesAsync();
                return;
            }

            var moto = tag.Moto;
            moto.UltimoBeaconConhecidoId = beaconId;
            moto.UltimaVezVistoEmPatio = eventoDto.Timestamp;

            var beacon = await _context.Beacons.AsNoTracking()
                .SingleOrDefaultAsync(b => b.BeaconId.ToUpper() == beaconId);

            if (beacon?.ZonaId is Guid novaZonaId && moto.ZonaId != novaZonaId)
            {
                var aberto = await _context.MotoZonasHistorico
                    .Where(h => h.MotoId == moto.Id && h.SaidaEm == null)
                    .OrderByDescending(h => h.EntradaEm)
                    .FirstOrDefaultAsync();

                if (aberto != null) aberto.SaidaEm = eventoDto.Timestamp;

                _context.MotoZonasHistorico.Add(new MotoZonaHistorico
                {
                    Id = Guid.NewGuid(),
                    MotoId = moto.Id,
                    ZonaId = novaZonaId,
                    FuncionarioId = null,
                    EntradaEm = eventoDto.Timestamp
                });

                moto.ZonaId = novaZonaId;
                _logger.LogInformation("Processor: Moto {MotoId} movida p/ Zona {ZonaId}.", moto.Id, novaZonaId);
            }

            await _context.SaveChangesAsync();
        }
    }
}
