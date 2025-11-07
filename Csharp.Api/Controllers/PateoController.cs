using Asp.Versioning;
using ApiVersion = Asp.Versioning.ApiVersion;
using ApiVersionAttribute = Asp.Versioning.ApiVersionAttribute;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Csharp.Api.Services;
using System.Security.Claims;
using Csharp.Api.DTOs;
using System;
using System.Threading.Tasks;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// Operações de leitura do Pátio (mapa, zonas) para o funcionário logado.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/pateo")]
    [Authorize(Policy = "RolesOperacionais")]
    [Produces("application/json")]
    public class PateoController : ControllerBase
    {
        private readonly IPateoService _pateoService;

        public PateoController(IPateoService pateoService)
        {
            _pateoService = pateoService;
        }

    /// <summary>
    /// Obtém os detalhes do pátio (planta e zonas) associado ao funcionário logado.
    /// </summary>
    /// <returns>200 OK com <see cref="PateoDetailDto"/> contendo planta e zonas do pátio.</returns>
    /// <response code="404">Funcionário ou pátio associado não encontrado (erro de sincronização).</response>
        [HttpGet("meu-pateo")]
        [ProducesResponseType(typeof(PateoDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMeuPateo()
        {
            var funcionarioTelefone = User.Identity?.Name;
            
            if (string.IsNullOrEmpty(funcionarioTelefone))
            {
                return Unauthorized("Claim de identificação (telefone) não encontrada no token.");
            }

            var pateoDto = await _pateoService.GetMyPateoAsync(funcionarioTelefone);
            return Ok(pateoDto);
        }
    }
}