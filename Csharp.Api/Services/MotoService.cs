using AutoMapper;
using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Csharp.Api.Entities.Enums;
using Csharp.Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    public class MotoService : IMotoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MotoService> _logger;
        private readonly IMapper _mapper;

        public MotoService(AppDbContext context, ILogger<MotoService> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<MotoViewDto> CreateMotoAsync(CreateMotoDto createMotoDto)
        {
            _logger.LogInformation("Tentativa de criar nova moto com placa {Placa} e tag {Tag}",
                createMotoDto.Placa ?? "N/A", createMotoDto.CodigoUnicoTagParaNovaTag);

            var existingTag = await _context.TagsBle.FirstOrDefaultAsync(t => t.CodigoUnicoTag.ToUpper() == createMotoDto.CodigoUnicoTagParaNovaTag.ToUpper());
            if (existingTag != null)
            {
                throw new TagJaExisteException(createMotoDto.CodigoUnicoTagParaNovaTag);
            }

            if (!string.IsNullOrEmpty(createMotoDto.Placa))
            {
                var placaNormalizada = createMotoDto.Placa.ToUpper();
                var motoComPlacaExistente = await _context.Motos.FirstOrDefaultAsync(m => m.Placa == placaNormalizada);
                if (motoComPlacaExistente != null)
                {
                    throw new PlacaJaExisteException(createMotoDto.Placa);
                }
            }

            try
            {
                var newTag = _mapper.Map<TagBle>(createMotoDto);
                newTag.Id = Guid.NewGuid();
                newTag.NivelBateria = 100;
                _context.TagsBle.Add(newTag);

                var moto = _mapper.Map<Moto>(createMotoDto);
                moto.Id = Guid.NewGuid();
                moto.TagBleId = newTag.Id;
                moto.DataCriacaoRegistro = DateTime.UtcNow;

                _context.Motos.Add(moto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Moto com ID {MotoId} criada com sucesso.", moto.Id);

                return _mapper.Map<MotoViewDto>(moto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao criar nova moto.");
                throw new ConcorrenciaException("Erro ao criar moto. Dados de placa ou tag já existem.");
            }
        }
        
        public async Task<PaginatedResponseDto<MotoViewDto>> GetAllMotosAsync(string? status, string? placa, int page, int pageSize)
        {
            _logger.LogInformation("Buscando motos com paginação. Página: {Page}, Tamanho: {PageSize}", page, pageSize);
            
            if (page < 1 || pageSize < 1)
            {
                throw new EntradaInvalidaException("Os parâmetros 'page' e 'pageSize' devem ser maiores que zero.");
            }

            var query = _context.Motos.Include(m => m.Tag).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TipoStatusMoto>(status, true, out var statusEnum))
                {
                    query = query.Where(m => m.StatusMoto == statusEnum);
                }
                else
                {
                    throw new EntradaInvalidaException(nameof(status), $"O valor '{status}' não é um status de moto válido.");
                }
            }

            if (!string.IsNullOrWhiteSpace(placa))
            {
                query = query.Where(m => m.Placa != null && m.Placa.Contains(placa.ToUpper()));
            }

            var totalItems = await query.CountAsync();
            
            var motos = await query
                .OrderBy(m => m.DataCriacaoRegistro)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var motoDtos = _mapper.Map<IEnumerable<MotoViewDto>>(motos).ToList();

            foreach (var motoDto in motoDtos)
            {
                motoDto.Links.Add(new LinkDto($"api/motos/{motoDto.Id}", "self", "GET"));
                motoDto.Links.Add(new LinkDto($"api/motos/{motoDto.Id}", "update_moto", "PUT"));
                motoDto.Links.Add(new LinkDto($"api/motos/{motoDto.Id}", "delete_moto", "DELETE"));
            }

            return new PaginatedResponseDto<MotoViewDto>
            {
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                Items = motoDtos
            };
        }

        public async Task<MotoViewDto?> GetMotoByIdAsync(Guid id)
        {
            _logger.LogInformation("Buscando moto com ID: {MotoId}", id);
            var moto = await _context.Motos.Include(m => m.Tag).FirstOrDefaultAsync(m => m.Id == id);
            
            if (moto == null)
            {
                throw new MotoNotFoundException(id);
            }
            return _mapper.Map<MotoViewDto>(moto);
        }

        public async Task<MotoViewDto?> GetMotoByPlacaAsync(string placa)
        {
            _logger.LogInformation("Buscando moto com Placa: {Placa}", placa);
            if (string.IsNullOrWhiteSpace(placa))
            {
                throw new EntradaInvalidaException(nameof(placa), "A placa não pode ser nula ou vazia para busca.");
            }
            var moto = await _context.Motos.Include(m => m.Tag).FirstOrDefaultAsync(m => m.Placa != null && m.Placa.ToUpper() == placa.ToUpper());
            
            if (moto == null)
            {
                throw new MotoNotFoundException(placa);
            }
            return _mapper.Map<MotoViewDto>(moto);
        }

        public async Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto updateMotoDto)
        {
            _logger.LogInformation("Tentativa de atualizar moto com ID: {MotoId}", id);
            var motoExistente = await _context.Motos.Include(m => m.Tag).FirstOrDefaultAsync(m => m.Id == id);

            if (motoExistente == null)
            {
                throw new MotoNotFoundException(id);
            }

            if (updateMotoDto.StatusMoto != TipoStatusMoto.SemPlacaEmColeta && string.IsNullOrWhiteSpace(updateMotoDto.Placa))
            {
                throw new EntradaInvalidaException(nameof(updateMotoDto.Placa), "A placa é obrigatória para o status da moto selecionado.");
            }

            var novaPlacaUpper = string.IsNullOrWhiteSpace(updateMotoDto.Placa) ? null : updateMotoDto.Placa.ToUpper();
            if (novaPlacaUpper != motoExistente.Placa && !string.IsNullOrWhiteSpace(novaPlacaUpper))
            {
                var outraMotoComMesmaPlaca = await _context.Motos.FirstOrDefaultAsync(m => m.Id != id && m.Placa == novaPlacaUpper);
                if (outraMotoComMesmaPlaca != null)
                {
                    throw new PlacaJaExisteException(updateMotoDto.Placa!);
                }
            }

            _mapper.Map(updateMotoDto, motoExistente);
            
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Moto com ID {MotoId} atualizada com sucesso.", id);
                return _mapper.Map<MotoViewDto>(motoExistente);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao tentar atualizar moto ID {MotoId}.", id);
                throw new ConcorrenciaException("Os dados foram modificados por outra transação. Por favor, tente novamente.");
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
            
            _context.Motos.Remove(motoParaDeletar);
            if (motoParaDeletar.Tag != null)
            {
                _context.TagsBle.Remove(motoParaDeletar.Tag);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Moto com ID {MotoId} e Tag associada deletadas com sucesso.", id);
            return true;
        }
    }
}