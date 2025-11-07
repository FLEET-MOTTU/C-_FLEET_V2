using Csharp.Api.DTOs;
using System;
using System.Threading.Tasks;

namespace Csharp.Api.Services
{
    /// <summary>Contrato para operações relacionadas ao Pátio</summary>
    public interface IPateoService
    {
        Task<PateoDetailDto> GetMyPateoAsync(string funcionarioTelefone);
    }
}