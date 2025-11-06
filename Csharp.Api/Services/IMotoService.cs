using Csharp.Api.DTOs;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Services
{
    /// <summary>Operações de domínio de motos.</summary>
    public interface IMotoService
    {
        Task<MotoViewDto> CreateMotoAsync(CreateMotoDto createMotoDto);
        Task<PaginatedResponseDto<MotoViewDto>> GetAllMotosAsync(string? status, string? placa, int page, int pageSize);
        Task<MotoViewDto?> GetMotoByIdAsync(Guid id);
        Task<MotoViewDto?> GetMotoByPlacaAsync(string placa);
        Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto updateMotoDto);
        Task<bool> DeleteMotoAsync(Guid id);

        // utilidades
        Task<MotoViewDto> UpsertPorPlacaAsync(string? placa, TipoModeloMoto modelo, TipoStatusMoto status, string codigoTag, Guid? zonaId = null);
        Task ReassignTagAsync(Guid motoId, string codigoTagNovo);
    }
}
