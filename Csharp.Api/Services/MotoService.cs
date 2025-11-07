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
    /// <summary>
    /// Implementação do serviço de motos.
    /// Contém regras de negócio e operações para criação, atualização, consulta e remoção de motos.
    /// </summary>
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

    /// <summary>
    /// Cria uma nova moto com a tag associada.
    /// </summary>
    /// <param name="createMotoDto">Dados necessários para criação.</param>
    /// <returns>Representação da moto criada.</returns>
    public async Task<MotoViewDto> CreateMotoAsync(CreateMotoDto createMotoDto)
        {
            var placaUpper = string.IsNullOrWhiteSpace(createMotoDto.Placa) ? null : createMotoDto.Placa.ToUpperInvariant();
            var tagCodeUpper = createMotoDto.CodigoUnicoTagParaNovaTag.ToUpperInvariant();

            _logger.LogInformation("Criando moto: Placa={Placa}, Tag={Tag}", placaUpper ?? "N/A", tagCodeUpper);

            // tag deve ser única
            var tagExists = await _context.TagsBle.AnyAsync(t => t.CodigoUnicoTag.ToUpper() == tagCodeUpper);
            if (tagExists) throw new TagJaExisteException(createMotoDto.CodigoUnicoTagParaNovaTag);

            // placa (se informada) deve ser única
            if (placaUpper != null)
            {
                var placaExists = await _context.Motos.AnyAsync(m => m.Placa == placaUpper);
                if (placaExists) throw new PlacaJaExisteException(createMotoDto.Placa!);
            }

            try
            {
                var tag = new TagBle
                {
                    Id = Guid.NewGuid(),
                    CodigoUnicoTag = tagCodeUpper,
                    NivelBateria = 100
                };
                _context.TagsBle.Add(tag);

                var moto = new Moto
                {
                    Id = Guid.NewGuid(),
                    Placa = placaUpper,
                    Modelo = createMotoDto.Modelo,
                    StatusMoto = createMotoDto.StatusMoto,
                    DataCriacaoRegistro = DateTime.UtcNow,
                    TagBleId = tag.Id
                };

                _context.Motos.Add(moto);
                await _context.SaveChangesAsync();

                return _mapper.Map<MotoViewDto>(moto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao criar moto.");
                throw new ConcorrenciaException("Conflito ao criar moto. Verifique placa/tag.");
            }
        }

    /// <summary>
    /// Obtém motos paginadas com filtros opcionais.
    /// </summary>
    /// <param name="status">Filtro por status da moto.</param>
    /// <param name="placa">Filtro por placa.</param>
    /// <param name="page">Número da página (1-based).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <returns>Resposta paginada com <see cref="MotoViewDto"/>.</returns>
    public async Task<PaginatedResponseDto<MotoViewDto>> GetAllMotosAsync(string? status, string? placa, int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
                throw new EntradaInvalidaException("Os parâmetros 'page' e 'pageSize' devem ser maiores que zero.");

            var query = _context.Motos.AsNoTracking().Include(m => m.Tag).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TipoStatusMoto>(status, true, out var statusEnum))
                {
                    query = query.Where(m => m.StatusMoto == statusEnum);
                }
                else
                {
                    throw new EntradaInvalidaException(nameof(status), $"Status inválido: '{status}'.");
                }
            }

            if (!string.IsNullOrWhiteSpace(placa))
            {
                var p = placa.ToUpperInvariant();
                query = query.Where(m => m.Placa != null && m.Placa.Contains(p));
            }

            var totalItems = await query.CountAsync();
            var motos = await query
                .OrderByDescending(m => m.DataCriacaoRegistro)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<MotoViewDto>>(motos).ToList();
            foreach (var m in dto)
            {
                m.Links.Add(new LinkDto($"api/motos/{m.Id}", "self", "GET"));
                m.Links.Add(new LinkDto($"api/motos/{m.Id}", "update_moto", "PUT"));
                m.Links.Add(new LinkDto($"api/motos/{m.Id}", "delete_moto", "DELETE"));
            }

            return new PaginatedResponseDto<MotoViewDto>
            {
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                Items = dto
            };
        }

    /// <summary>
    /// Recupera uma moto por seu identificador.
    /// </summary>
    /// <param name="id">ID da moto.</param>
    /// <returns>DTO da moto ou null se não encontrada.</returns>
    public async Task<MotoViewDto?> GetMotoByIdAsync(Guid id)
        {
            var moto = await _context.Motos.AsNoTracking()
                .Include(m => m.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (moto == null) throw new MotoNotFoundException(id);
            return _mapper.Map<MotoViewDto>(moto);
        }

    /// <summary>
    /// Recupera uma moto pela placa.
    /// </summary>
    /// <param name="placa">Placa da moto.</param>
    /// <returns>DTO da moto ou null se não encontrada.</returns>
    public async Task<MotoViewDto?> GetMotoByPlacaAsync(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                throw new EntradaInvalidaException(nameof(placa), "A placa não pode ser vazia.");

            var p = placa.ToUpperInvariant();
            var moto = await _context.Motos.AsNoTracking()
                .Include(m => m.Tag)
                .FirstOrDefaultAsync(m => m.Placa == p);

            if (moto == null) throw new MotoNotFoundException(placa);
            return _mapper.Map<MotoViewDto>(moto);
        }

    /// <summary>
    /// Atualiza os dados de uma moto existente.
    /// </summary>
    /// <param name="id">Identificador da moto.</param>
    /// <param name="updateMotoDto">Dados de atualização.</param>
    /// <returns>DTO da moto atualizada.</returns>
    public async Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto updateMotoDto)
        {
            var moto = await _context.Motos.Include(m => m.Tag).FirstOrDefaultAsync(m => m.Id == id);
            if (moto == null) throw new MotoNotFoundException(id);

            if (updateMotoDto.StatusMoto != TipoStatusMoto.SemPlacaEmColeta && string.IsNullOrWhiteSpace(updateMotoDto.Placa))
                throw new EntradaInvalidaException(nameof(updateMotoDto.Placa), "Placa é obrigatória para este status.");

            var novaPlacaUpper = string.IsNullOrWhiteSpace(updateMotoDto.Placa) ? null : updateMotoDto.Placa.ToUpperInvariant();
            if (novaPlacaUpper != moto.Placa && novaPlacaUpper != null)
            {
                var duplicada = await _context.Motos.AnyAsync(m => m.Id != id && m.Placa == novaPlacaUpper);
                if (duplicada) throw new PlacaJaExisteException(updateMotoDto.Placa!);
            }

            moto.Placa = novaPlacaUpper;
            moto.Modelo = updateMotoDto.Modelo;
            moto.StatusMoto = updateMotoDto.StatusMoto;

            try
            {
                await _context.SaveChangesAsync();
                return _mapper.Map<MotoViewDto>(moto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concorrência ao atualizar moto {Id}", id);
                throw new ConcorrenciaException("Dados modificados por outra transação. Tente novamente.");
            }
        }

    /// <summary>
    /// Remove uma moto do sistema.
    /// </summary>
    /// <param name="id">Identificador da moto.</param>
    /// <returns>True se removida com sucesso, caso contrário false.</returns>
    public async Task<bool> DeleteMotoAsync(Guid id)
        {
            var moto = await _context.Motos.Include(m => m.Tag).FirstOrDefaultAsync(m => m.Id == id);
            if (moto == null) throw new MotoNotFoundException(id);

            _context.Motos.Remove(moto);
            if (moto.Tag != null) _context.TagsBle.Remove(moto.Tag);

            await _context.SaveChangesAsync();
            return true;
        }

    /// <summary>
    /// Insere ou atualiza uma moto com base na placa. Se a placa existir, atualiza; caso contrário, cria.
    /// </summary>
    /// <param name="placa">Placa (pode ser nula para criação por tag apenas).</param>
    /// <param name="modelo">Modelo da moto.</param>
    /// <param name="status">Status inicial da moto.</param>
    /// <param name="codigoTag">Código único da tag BLE.</param>
    /// <param name="zonaId">Zona opcional.</param>
    /// <returns>DTO da moto criada ou atualizada.</returns>
    public async Task<MotoViewDto> UpsertPorPlacaAsync(string? placa, TipoModeloMoto modelo, TipoStatusMoto status, string codigoTag, Guid? zonaId = null)
        {
            var placaUpper   = string.IsNullOrWhiteSpace(placa) ? null : placa.ToUpperInvariant();
            var tagCodeUpper = codigoTag.ToUpperInvariant();

            _logger.LogInformation("UpsertPorPlaca: Placa={Placa}, Tag={Tag}", placaUpper ?? "N/A", tagCodeUpper);

            Moto? moto = null;
            if (placaUpper != null)
            {
                moto = await _context.Motos.Include(m => m.Tag)
                    .FirstOrDefaultAsync(m => m.Placa == placaUpper);
            }

            var tag = await _context.TagsBle.Include(t => t.Moto)
                .FirstOrDefaultAsync(t => t.CodigoUnicoTag.ToUpper() == tagCodeUpper);

            if (tag == null)
            {
                tag = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = tagCodeUpper, NivelBateria = 100 };
                _context.TagsBle.Add(tag);
            }

            // IMPORTANTE: não "desgruda" tag de outra moto aqui para evitar FK inválida.
            // Caso a tag já pertença a outra moto diferente da alvo, peça para usar o endpoint de reassign (transacional).
            if (tag.Moto != null && (moto == null || tag.Moto.Id != moto.Id))
                throw new ConcorrenciaException("A Tag informada já está vinculada a outra moto. Use o endpoint de reassign-tag para uma troca segura.");

            if (moto == null)
            {
                // se placa vier nula, criamos moto sem placa (caso SemPlacaEmColeta)
                moto = new Moto
                {
                    Id = Guid.NewGuid(),
                    Placa = placaUpper,
                    Modelo = modelo,
                    StatusMoto = status,
                    DataCriacaoRegistro = DateTime.UtcNow,
                    TagBleId = tag.Id,
                    ZonaId = zonaId
                };
                _context.Motos.Add(moto);
            }
            else
            {
                // atualizar
                // valida duplicidade de placa se houver mudança (não é o caso aqui porque a busca foi pela placa)
                moto.Modelo = modelo;
                moto.StatusMoto = status;
                moto.TagBleId = tag.Id;
                if (zonaId.HasValue) moto.ZonaId = zonaId;
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<MotoViewDto>(moto);
        }

    /// <summary>
    /// Reatribui uma nova tag a uma moto existente.
    /// </summary>
    /// <param name="motoId">ID da moto.</param>
    /// <param name="codigoTagNovo">Código da nova tag.</param>
    public async Task ReassignTagAsync(Guid motoId, string codigoTagNovo)
        {
            var tagCodeUpper = codigoTagNovo.ToUpperInvariant();
            _logger.LogInformation("ReassignTag: Moto={MotoId}, TagNova={Tag}", motoId, tagCodeUpper);

            await using var tx = await _context.Database.BeginTransactionAsync();

            var moto = await _context.Motos.FirstOrDefaultAsync(m => m.Id == motoId);
            if (moto == null) throw new MotoNotFoundException(motoId);

            var tagNova = await _context.TagsBle.Include(t => t.Moto)
                .FirstOrDefaultAsync(t => t.CodigoUnicoTag.ToUpper() == tagCodeUpper);

            if (tagNova == null)
            {
                tagNova = new TagBle { Id = Guid.NewGuid(), CodigoUnicoTag = tagCodeUpper, NivelBateria = 100 };
                _context.TagsBle.Add(tagNova);
                await _context.SaveChangesAsync();
            }

            if (tagNova.Moto != null && tagNova.Moto.Id != moto.Id)
            {
                // SWAP seguro de tags entre as duas motos
                var outraMoto = await _context.Motos.FirstAsync(m => m.Id == tagNova.Moto.Id);
                var tagAntigaDaAtual = await _context.TagsBle.FirstAsync(t => t.Id == moto.TagBleId);

                outraMoto.TagBleId = tagAntigaDaAtual.Id;
                moto.TagBleId = tagNova.Id;
            }
            else
            {
                moto.TagBleId = tagNova.Id;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
