using AutoMapper;
using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Csharp.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Services
{
    /// <summary>
    /// Implementação do serviço de Beacons.
    /// Fornece operações CRUD e regras de negócio relacionadas a dispositivos Beacon.
    /// </summary>
    public class BeaconService : IBeaconService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BeaconService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

    /// <summary>
    /// Retorna uma página de beacons cadastrados.
    /// </summary>
    /// <param name="page">Número da página (1-based).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <returns>Lista paginada de <see cref="BeaconDto"/>.</returns>
    public async Task<PaginatedResponseDto<BeaconDto>> GetAllBeaconsAsync(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
                throw new EntradaInvalidaException("Os parâmetros 'page' e 'pageSize' devem ser maiores que zero.");

            var query = _context.Beacons.AsNoTracking();

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(b => b.BeaconId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var beaconDtos = _mapper.Map<IEnumerable<BeaconDto>>(items);

            return new PaginatedResponseDto<BeaconDto>
            {
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                Items = beaconDtos
            };
        }

    /// <summary>
    /// Obtém um beacon por seu identificador interno (GUID).
    /// </summary>
    /// <param name="id">Identificador do beacon.</param>
    /// <returns>Dados do beacon.</returns>
    public async Task<BeaconDto> GetBeaconByIdAsync(Guid id)
        {
            var beacon = await _context.Beacons.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (beacon == null) throw new BeaconNotFoundException(id);
            return _mapper.Map<BeaconDto>(beacon);
        }

    /// <summary>
    /// Obtém um beacon pelo seu código público (BeaconId).
    /// </summary>
    /// <param name="beaconId">Código público do beacon.</param>
    /// <returns>Dados do beacon.</returns>
    public async Task<BeaconDto> GetBeaconByBeaconIdAsync(string beaconId)
        {
            var key = beaconId.ToUpperInvariant();
            var beacon = await _context.Beacons.AsNoTracking()
                .FirstOrDefaultAsync(b => b.BeaconId.ToUpper() == key);

            if (beacon == null) throw new BeaconNotFoundException(beaconId);
            return _mapper.Map<BeaconDto>(beacon);
        }

    /// <summary>
    /// Cria um novo beacon.
    /// </summary>
    /// <param name="createBeaconDto">DTO com os dados de criação.</param>
    /// <returns>Beacon criado.</returns>
    public async Task<BeaconDto> CreateBeaconAsync(CreateBeaconDto createBeaconDto)
        {
            var key = createBeaconDto.BeaconId.ToUpperInvariant();

            var exists = await _context.Beacons.AnyAsync(b => b.BeaconId.ToUpper() == key);
            if (exists) throw new BeaconJaExisteException(createBeaconDto.BeaconId);

            var beacon = _mapper.Map<Beacon>(createBeaconDto);
            beacon.BeaconId = key;

            _context.Beacons.Add(beacon);
            await _context.SaveChangesAsync();

            return _mapper.Map<BeaconDto>(beacon);
        }

    /// <summary>
    /// Atualiza os dados de um beacon existente.
    /// </summary>
    /// <param name="id">Identificador do beacon.</param>
    /// <param name="updateBeaconDto">DTO com os campos a atualizar.</param>
    /// <returns>Beacon atualizado.</returns>
    public async Task<BeaconDto> UpdateBeaconAsync(Guid id, UpdateBeaconDto updateBeaconDto)
        {
            var beacon = await _context.Beacons.FirstOrDefaultAsync(b => b.Id == id);
            if (beacon == null) throw new BeaconNotFoundException(id);

            _mapper.Map(updateBeaconDto, beacon);
            await _context.SaveChangesAsync();

            return _mapper.Map<BeaconDto>(beacon);
        }

    /// <summary>
    /// Remove um beacon do sistema.
    /// </summary>
    /// <param name="id">Identificador do beacon a remover.</param>
    public async Task DeleteBeaconAsync(Guid id)
        {
            var beacon = await _context.Beacons.FirstOrDefaultAsync(b => b.Id == id);
            if (beacon == null) throw new BeaconNotFoundException(id);

            _context.Beacons.Remove(beacon);
            await _context.SaveChangesAsync();
        }
    }
}
