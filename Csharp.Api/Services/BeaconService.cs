using AutoMapper;
using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Csharp.Api.Exceptions;

namespace Csharp.Api.Services
{
    public class BeaconService : IBeaconService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BeaconService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResponseDto<BeaconDto>> GetAllBeaconsAsync(int page, int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new EntradaInvalidaException("Os parâmetros 'page' e 'pageSize' devem ser maiores que zero.");
            }

            var query = _context.Beacons.AsQueryable();
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

        public async Task<BeaconDto> GetBeaconByIdAsync(Guid id)
        {
            var beacon = await _context.Beacons.FindAsync(id);
            if (beacon == null)
            {
                throw new BeaconNotFoundException(id);
            }
            return _mapper.Map<BeaconDto>(beacon);
        }

        public async Task<BeaconDto> GetBeaconByBeaconIdAsync(string beaconId)
        {
            var beacon = await _context.Beacons.FirstOrDefaultAsync(b => b.BeaconId.ToUpper() == beaconId.ToUpper());
            if (beacon == null)
            {
                throw new BeaconNotFoundException(beaconId);
            }
            return _mapper.Map<BeaconDto>(beacon);
        }

        public async Task<BeaconDto> CreateBeaconAsync(CreateBeaconDto createBeaconDto)
        {
            var existingBeacon = await _context.Beacons.FirstOrDefaultAsync(b => b.BeaconId.ToUpper() == createBeaconDto.BeaconId.ToUpper());
            if (existingBeacon != null)
            {
                throw new BeaconJaExisteException(createBeaconDto.BeaconId);
            }
            
            var beacon = _mapper.Map<Beacon>(createBeaconDto);
            beacon.UltimaVezVisto = DateTime.UtcNow; // Valor inicial
            _context.Beacons.Add(beacon);
            await _context.SaveChangesAsync();
            return _mapper.Map<BeaconDto>(beacon);
        }

        public async Task<BeaconDto> UpdateBeaconAsync(Guid id, UpdateBeaconDto updateBeaconDto)
        {
            var beacon = await _context.Beacons.FindAsync(id);
            if (beacon == null)
            {
                throw new BeaconNotFoundException(id);
            }
            
            _mapper.Map(updateBeaconDto, beacon);
            beacon.UltimaVezVisto = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return _mapper.Map<BeaconDto>(beacon);
        }

        public async Task DeleteBeaconAsync(Guid id)
        {
            var beacon = await _context.Beacons.FindAsync(id);
            if (beacon == null)
            {
                throw new BeaconNotFoundException(id);
            }
            _context.Beacons.Remove(beacon);
            await _context.SaveChangesAsync();
        }
    }
}