using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface IMotoService
    {
        Task<MotoViewDto?> GetMotoByIdAsync(Guid id);
        Task<IEnumerable<MotoViewDto>> GetAllMotosAsync(string? status, string? placa);
        Task<MotoViewDto> CreateMotoAsync(CreateMotoDto createMotoDto);
        Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto updateMotoDto);
        Task<bool> DeleteMotoAsync(Guid id);
        Task<MotoViewDto?> GetMotoByPlacaAsync(string placa);
    }
}
