using Asp.Versioning;
using ApiVersion = Asp.Versioning.ApiVersion;
using ApiVersionAttribute = Asp.Versioning.ApiVersionAttribute;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Csharp.Api.DTOs;
using Csharp.Api.Services;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// Operações de pátio (classificação de lote em zonas).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/patios")]
    [Authorize(Roles = "OPERACIONAL, ADMINISTRATIVO, TEMPORARIO")]
    [Produces("application/json")]
    public class PatioOperacoesController : ControllerBase
    {
        private readonly IClassificationService _classificador;

        public PatioOperacoesController(IClassificationService classificador)
        {
            _classificador = classificador;
        }

    /// <summary>
    /// Classifica um lote de motos por status e sugere zonas do pátio com base em regras.
    /// </summary>
    /// <param name="pateoId">ID do pátio onde a classificação será aplicada.</param>
    /// <param name="request">Dados do lote a ser classificado.</param>
    /// <returns>200 OK com <see cref="LoteClassificacaoRespostaDto"/> contendo as sugestões de zona.</returns>
        [HttpPost("{pateoId:guid}/classificar-motos")]
        [ProducesResponseType(typeof(LoteClassificacaoRespostaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Classificar(Guid pateoId, [FromBody] LoteClassificacaoRequestDto request)
        {
            if (request == null) return BadRequest("Payload obrigatório.");
            request.PateoId = pateoId;

            var result = await _classificador.SugerirZonasPorEstadoAsync(request);
            return Ok(result);
        }
    }
}
