using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface IBeaconService
    {
        Task<PaginatedResponseDto<BeaconDto>> GetAllBeaconsAsync(int page, int pageSize);
        Task<BeaconDto> GetBeaconByIdAsync(Guid id);
        Task<BeaconDto> GetBeaconByBeaconIdAsync(string beaconId);
        Task<BeaconDto> CreateBeaconAsync(CreateBeaconDto createBeaconDto);
        Task<BeaconDto> UpdateBeaconAsync(Guid id, UpdateBeaconDto updateBeaconDto);
        Task DeleteBeaconAsync(Guid id);
    }
}