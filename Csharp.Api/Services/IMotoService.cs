using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface IMotoService
    {
        Task<MotoViewDto?> GetMotoByIdAsync(Guid id);
        Task<IEnumerable<MotoViewDto>> GetAllMotosAsync(string? status, string? placa);
        Task<MotoViewDto> CreateMotoAsync(CreateMotoDto createMotoDto);
        Task<bool> UpdateMotoAsync(Guid id, UpdateMotoDto updateMotoDto);
        Task<bool> DeleteMotoAsync(Guid id);
        Task<MotoViewDto?> GetMotoByPlacaAsync(string placa);

        // Poderíamos adicionar um método para o endpoint de evento IoT aqui também no futuro,
        // ou criar um serviço separado para eventos IoT.
        // Task ProcessarEventoIoTAsync(SimulatedTagEventDto eventoDto);
    }
}