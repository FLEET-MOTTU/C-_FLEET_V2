using Csharp.Api.DTOs;
using System;
using System.Threading.Tasks;

namespace Csharp.Api.Services
{
    /// <summary>
    /// Contrato para operações relacionadas ao Pátio
    /// (consumidas pelo app do funcionário).
    /// </summary>
    public interface IPateoService
    {
        /// <summary>
        /// Busca o pátio (e suas zonas) ao qual um funcionário pertence.
        /// </summary>
        /// <param name="funcionarioTelefone">O telefone (username do JWT) do funcionário logado.</param>
        /// <returns>Os detalhes do pátio para desenhar o mapa.</returns>
        Task<PateoDetailDto> GetMyPateoAsync(string funcionarioTelefone);
    }
}