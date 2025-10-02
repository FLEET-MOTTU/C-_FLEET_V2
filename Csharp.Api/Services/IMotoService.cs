using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface IMotoService
    {
        Task<MotoViewDto> CreateMotoAsync(CreateMotoDto createMotoDto);
        Task<PaginatedResponseDto<MotoViewDto>> GetAllMotosAsync(string? status, string? placa, int page, int pageSize);
        Task<MotoViewDto?> GetMotoByIdAsync(Guid id);
        Task<MotoViewDto?> GetMotoByPlacaAsync(string placa);
        Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto updateMotoDto);
        Task<bool> DeleteMotoAsync(Guid id);
    }
}