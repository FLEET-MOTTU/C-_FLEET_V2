using Microsoft.EntityFrameworkCore;
using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Csharp.Api.Entities.Enums;
using Csharp.Api.Exceptions;

namespace Csharp.Api.Services
{
    public class MotoService : IMotoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MotoService> _logger;

        public MotoService(AppDbContext context, ILogger<MotoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MotoViewDto> CreateMotoAsync(CreateMotoDto createMotoDto)
        {
            _logger.LogInformation("Tentativa de criar nova moto com placa {Placa} e tag {Tag}",
                createMotoDto.Placa ?? "N/A", createMotoDto.CodigoUnicoTagParaNovaTag);
                
            // Regra de Negócio: Verificar se CodigoUnicoTag já existe
            var tagExistente = await _context.TagsBle
                                 .FirstOrDefaultAsync(t => t.CodigoUnicoTag == createMotoDto.CodigoUnicoTagParaNovaTag);
            if (tagExistente != null)
            {
                throw new TagJaExisteException(createMotoDto.CodigoUnicoTagParaNovaTag);
            }

            // Regra de Negócio: Verificar se Placa já existe
            if (!string.IsNullOrWhiteSpace(createMotoDto.Placa))
            {
                var placaNormalizada = createMotoDto.Placa.ToUpper();
                var motoComPlacaExistente = await _context.Motos
                                                        .FirstOrDefaultAsync(m => m.Placa == placaNormalizada);
                if (motoComPlacaExistente != null)
                {
                    throw new PlacaJaExisteException(createMotoDto.Placa);
                }
            }

            var novaTag = new TagBle
            {
                Id = Guid.NewGuid(),
                CodigoUnicoTag = createMotoDto.CodigoUnicoTagParaNovaTag,
                NivelBateria = 100,
            };

            var novaMoto = new Moto
            {
                Id = Guid.NewGuid(),
                Placa = string.IsNullOrWhiteSpace(createMotoDto.Placa) ? null : createMotoDto.Placa.ToUpper(),Modelo = createMotoDto.Modelo,
                StatusMoto = createMotoDto.StatusMoto,
                DataCriacaoRegistro = DateTime.UtcNow,
                DataRecolhimento = createMotoDto.DataRecolhimento ?? DateTime.UtcNow,            
                FuncionarioRecolhimentoId = createMotoDto.FuncionarioRecolhimentoId,
                TagBleId = novaTag.Id,
                Tag = novaTag
            };
            _context.Motos.Add(novaMoto);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Moto com ID {MotoId} criada com sucesso.", novaMoto.Id);
            return MapMotoToViewDto(novaMoto);
        }


        public async Task<IEnumerable<MotoViewDto>> GetAllMotosAsync(string? status, string? placa)
        {
            _logger.LogInformation("Buscando todas as motos. Filtro Status: {Status}, Filtro Placa: {Placa}", status, placa);
            var query = _context.Motos.Include(m => m.Tag).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TipoStatusMoto>(status, true, out var statusEnum))
                {
                    query = query.Where(m => m.StatusMoto == statusEnum);
                }
                else
                {
                    _logger.LogWarning("Status inválido fornecido para filtro: {Status}", status);
                    throw new EntradaInvalidaException(nameof(status), $"O valor '{status}' não é um status de moto válido.");

                }
            }

            if (!string.IsNullOrWhiteSpace(placa))
            {
                query = query.Where(m => m.Placa != null && m.Placa.Contains(placa.ToUpper()));
            }

            var motos = await query.ToListAsync();
            return motos.Select(MapMotoToViewDto);
        }


        public async Task<MotoViewDto?> GetMotoByIdAsync(Guid id)
        {
            _logger.LogInformation("Buscando moto com ID: {MotoId}", id);
            var moto = await _context.Motos
                                     .Include(m => m.Tag)
                                     .FirstOrDefaultAsync(m => m.Id == id);

            if (moto == null)
            {
                throw new MotoNotFoundException(id);
            }
            return MapMotoToViewDto(moto);
        }


        public async Task<MotoViewDto?> GetMotoByPlacaAsync(string placa)
        {
            _logger.LogInformation("Buscando moto com Placa: {Placa}", placa);

            if (string.IsNullOrWhiteSpace(placa))
            {
                throw new EntradaInvalidaException(nameof(placa), "A placa não pode ser nula ou vazia para busca.");
            }

            var moto = await _context.Motos
                                     .Include(m => m.Tag)
                                     .FirstOrDefaultAsync(m => m.Placa == placa.ToUpper());

            if (moto == null)
            {
                throw new MotoNotFoundException(placa);
            }
            return MapMotoToViewDto(moto);
        }


        public async Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto updateMotoDto)
        {
            _logger.LogInformation("Tentativa de atualizar moto com ID: {MotoId}", id);
            var motoExistente = await _context.Motos.Include(m => m.Tag).FirstOrDefaultAsync(m => m.Id == id);

            if (motoExistente == null)
            {
                throw new MotoNotFoundException(id);
            }

            // Regra de Negócio: Placa obrigatória se StatusMoto não for "SemPlacaEmColeta"
            if (updateMotoDto.StatusMoto != TipoStatusMoto.SemPlacaEmColeta && string.IsNullOrWhiteSpace(updateMotoDto.Placa))
            {
                throw new EntradaInvalidaException(nameof(updateMotoDto.Placa), "A placa é obrigatória para o status da moto selecionado.");
            }

            // Regra de Negócio: Se a placa mudou, verificar se a nova placa já existe em outra moto
            var novaPlacaUpper = string.IsNullOrWhiteSpace(updateMotoDto.Placa) ? null : updateMotoDto.Placa.ToUpper();
           if (novaPlacaUpper != motoExistente.Placa &&
                !string.IsNullOrWhiteSpace(novaPlacaUpper))
            {
                var outraMotoComMesmaPlaca = await _context.Motos
                                            .FirstOrDefaultAsync(m => m.Id != id && m.Placa == novaPlacaUpper);
                if (outraMotoComMesmaPlaca != null)
                {
                    _logger.LogWarning("Tentativa de atualizar moto ID {MotoId} para placa '{NovaPlaca}' que já está registrada em outra moto ID {OutraMotoId}.", 
                        id, novaPlacaUpper, outraMotoComMesmaPlaca.Id);
                    throw new PlacaJaExisteException(updateMotoDto.Placa!);
                }
            }

            motoExistente.Placa = novaPlacaUpper;
            motoExistente.Modelo = updateMotoDto.Modelo;
            motoExistente.StatusMoto = updateMotoDto.StatusMoto;
  
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Moto com ID {MotoId} atualizada com sucesso.", id);
                return MapMotoToViewDto(motoExistente);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro ao tentar atualizar moto ID {MotoId}.", id);
                throw; 
            }
        }


        public async Task<bool> DeleteMotoAsync(Guid id)
        {
            _logger.LogInformation("Tentativa de deletar moto com ID: {MotoId}", id);
            var motoParaDeletar = await _context.Motos.Include(m => m.Tag).FirstOrDefaultAsync(m => m.Id == id);

            if (motoParaDeletar == null)
            {
                throw new MotoNotFoundException(id);
            }
 
            if (motoParaDeletar.Tag != null)
            {
                _context.TagsBle.Remove(motoParaDeletar.Tag);
            }
            _context.Motos.Remove(motoParaDeletar);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Moto com ID {MotoId} deletada com sucesso.", id);
            return true;
        }


        private MotoViewDto MapMotoToViewDto(Moto moto)
        {
            return new MotoViewDto
            {
                Id = moto.Id,
                Placa = moto.Placa,
                Modelo = moto.Modelo.ToString(),
                StatusMoto = moto.StatusMoto.ToString(),
                DataCriacaoRegistro = moto.DataCriacaoRegistro,
                DataRecolhimento = moto.DataRecolhimento,
                FuncionarioRecolhimentoId = moto.FuncionarioRecolhimentoId,
                DataEntradaPatio = moto.DataEntradaPatio,
                UltimoBeaconConhecidoId = moto.UltimoBeaconConhecidoId,
                UltimaVezVistoEmPatio = moto.UltimaVezVistoEmPatio,
                Tag = moto.Tag == null ? null : new TagBleViewDto
                {
                    Id = moto.Tag.Id,
                    CodigoUnicoTag = moto.Tag.CodigoUnicoTag,
                    NivelBateria = moto.Tag.NivelBateria
                }
            };
        }

    }
}
