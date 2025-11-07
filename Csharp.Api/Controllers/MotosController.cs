using Asp.Versioning;
using ApiVersion = Asp.Versioning.ApiVersion;
using ApiVersionAttribute = Asp.Versioning.ApiVersionAttribute;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Csharp.Api.DTOs;
using Csharp.Api.Services;
using Csharp.Api.ML.Models;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// Operações de motos (CRUD + utilidades operacionais).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/motos")]
    [Authorize(Policy = "RolesOperacionais")]
    [Produces("application/json")]
    public class MotosController : ControllerBase
    {
    private readonly IMotoService _motoService;
    private readonly ILogger<MotosController> _logger;
    private readonly IPredictionService _predictionService;

        public MotosController(IMotoService motoService, IPredictionService predictionService, ILogger<MotosController> logger)
        {
            _motoService = motoService;
            _logger = logger;
            _predictionService = predictionService;
        }

        /// <summary>Cria uma nova moto com tag associada.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateMoto([FromBody] CreateMotoDto createMotoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateMoto: ModelState inválido.");
                return ValidationProblem(ModelState);
            }

            var motoCriada = await _motoService.CreateMotoAsync(createMotoDto);
            return CreatedAtAction(nameof(GetMotoById), new { id = motoCriada.Id }, motoCriada);
        }

        /// <summary>Lista motos com filtros e paginação.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<MotoViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllMotos(
            [FromQuery] string? status,
            [FromQuery] string? placa,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var motos = await _motoService.GetAllMotosAsync(status, placa, page, pageSize);
            return Ok(motos);
        }

        /// <summary>Obtém moto por ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMotoById(Guid id)
        {
            var moto = await _motoService.GetMotoByIdAsync(id);
            return Ok(moto);
        }

        /// <summary>Obtém moto por placa.</summary>
        [HttpGet("por-placa/{placa}")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMotoByPlaca(string placa)
        {
            var moto = await _motoService.GetMotoByPlacaAsync(placa);
            return Ok(moto);
        }

        /// <summary>Atualiza moto.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateMoto(Guid id, [FromBody] UpdateMotoDto updateMotoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateMoto: ModelState inválido para ID {MotoId}.", id);
                return ValidationProblem(ModelState);
            }

            var motoAtualizadaDto = await _motoService.UpdateMotoAsync(id, updateMotoDto);
            return Ok(motoAtualizadaDto);
        }

        /// <summary>Exclui moto (e a tag associada).</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMoto(Guid id)
        {
            await _motoService.DeleteMotoAsync(id);
            return NoContent();
        }

        // ----------- Utilidades Operacionais -----------

        /// <summary>
        /// Upsert por placa: cria/atualiza e garante vínculo com a Tag e (opcional) Zona.
        /// </summary>
        [HttpPost("upsert-por-placa")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpsertPorPlaca([FromBody] UpsertMotoPorPlacaDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var result = await _motoService.UpsertPorPlacaAsync(dto.Placa, dto.Modelo, dto.StatusMoto, dto.CodigoUnicoTag, dto.ZonaId);
            return Ok(result);
        }

        /// <summary>
        /// Reatribui a Tag de uma moto (substituição de tag).
        /// </summary>
        [HttpPut("{id:guid}/reassign-tag")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReassignTag(Guid id, [FromBody] ReassignTagDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            await _motoService.ReassignTagAsync(id, dto.CodigoUnicoTagNovo);
            return NoContent();
        }

        /// <summary>
        /// (ADMIN) Usa ML.NET para prever o tipo de reparo (Simples/Complexo) de uma moto.
        /// </summary>
        /// <remarks>
        /// Este endpoint atende ao Requisito 4 (ML.NET). Ele usa um modelo de classificação binária
        /// treinado para prever se, com base no modelo da moto, ela provavelmente
        /// necessitará de reparos simples ou complexos.
        /// </remarks>
        /// <param name="id">O ID da moto (GUID) que está 'AguardandoVistoria'.</param>
        /// <response code="200">Retorna a previsão do modelo.</response>
        /// <response code="404">Moto não encontrada.</response>
        [HttpGet("{id:guid}/prever-vistoria")]
        [Authorize(Roles = "ADMINISTRATIVO, OPERACIONAL")]
        [ProducesResponseType(typeof(VistoriaOutput), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PreverVistoria(Guid id)
        {
            var moto = await _motoService.GetMotoByIdAsync(id);
            if (moto == null)
            {
                return NotFound(new { message = "Moto não encontrada." });
            }

            // MotoViewDto.Modelo is a string; convert to TipoModeloMoto enum before predicting.
            if (!Enum.TryParse<TipoModeloMoto>(moto.Modelo, true, out var modeloEnum))
            {
                _logger.LogWarning("PreverVistoria: modelo da moto '{Modelo}' não corresponde a um TipoModeloMoto válido.", moto.Modelo);
                return BadRequest(new { message = "Modelo da moto inválido para predição." });
            }

            var previsao = _predictionService.PreverVistoria(modeloEnum);
            
            return Ok(previsao);
        }      
    
    }
}
