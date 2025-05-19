using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Csharp.Api.Exceptions;

namespace Csharp.Api.Services
{
    public class IoTEventService : IIoTEventService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IoTEventService> _logger;

        public IoTEventService(AppDbContext context, ILogger<IoTEventService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessarInteracaoTagAsync(TagInteractionEventDto eventoDto)
        {
            _logger.LogInformation(
                "Processando evento IoT: Tag {TagCodigo} vista no Beacon {BeaconId} em {Timestamp}. Nível Bateria: {NivelBateria}, Tipo Evento: {TipoEvento}", 
                eventoDto.CodigoUnicoTag, 
                eventoDto.BeaconIdDetectado, 
                eventoDto.Timestamp,
                eventoDto.NivelBateria?.ToString() ?? "N/A",
                eventoDto.TipoEvento ?? "N/A"
            );

            var tag = await _context.TagsBle
                                    .Include(t => t.Moto)
                                    .SingleOrDefaultAsync(t => t.CodigoUnicoTag == eventoDto.CodigoUnicoTag);

            if (tag == null)
            {
                _logger.LogWarning("Evento IoT recebido para tag desconhecida: {TagCodigo}", eventoDto.CodigoUnicoTag);
                return;
            }

            if (eventoDto.NivelBateria.HasValue)
            {
                if (tag.NivelBateria != eventoDto.NivelBateria.Value)
                {
                    tag.NivelBateria = eventoDto.NivelBateria.Value;
                    _logger.LogInformation("Nível da bateria da Tag {TagCodigo} atualizado para {NivelBateria}%.", tag.CodigoUnicoTag, tag.NivelBateria);
                }
            }

            if (tag.Moto != null)
            {
                tag.Moto.UltimoBeaconConhecidoId = eventoDto.BeaconIdDetectado;
                tag.Moto.UltimaVezVistoEmPatio = eventoDto.Timestamp;
                
                _logger.LogInformation(
                    "Moto ID {MotoId} (Tag {TagCodigo}) atualizada: Último Beacon Visto='{BeaconId}', Última Vez Vista='{Timestamp}'", 
                    tag.Moto.Id, 
                    tag.CodigoUnicoTag, 
                    tag.Moto.UltimoBeaconConhecidoId, 
                    tag.Moto.UltimaVezVistoEmPatio
                );
            }
            else
            {
                _logger.LogError("Tag {TagCodigo} encontrada, mas não está associada a nenhuma moto no banco de dados!", tag.CodigoUnicoTag);
            }

            // TODO: Lógica para TipoEvento
            // if (!string.IsNullOrEmpty(eventoDto.TipoEvento))
            // {
            //     if (eventoDto.TipoEvento.Equals("entrada", StringComparison.OrdinalIgnoreCase))
            //     {
            //         // Lógica para registrar entrada em zona/beacon
            //     }
            //     // etc.
            // }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao salvar alterações para o evento da tag {TagCodigo}.", eventoDto.CodigoUnicoTag);
                throw; 
            }
        }
    }
}
