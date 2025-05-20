using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Csharp.Api.DTOs;
using Csharp.Api.Services;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// Gerencia as operações CRUD para as motos.
    /// </summary>
    [ApiController]
    [Route("api/motos")]
    public class MotosController : ControllerBase
    {
        private readonly IMotoService _motoService;
        private readonly ILogger<MotosController> _logger;

        public MotosController(IMotoService motoService, ILogger<MotosController> logger)
        {
            _motoService = motoService;
            _logger = logger;
        }

        /// <summary>
        /// Cria uma nova moto no sistema.
        /// </summary>
        /// <remarks>
        /// A placa da moto é opcional apenas se o 'statusMoto' for 'SemPlacaEmColeta'.
        /// Uma nova Tag BLE será criada e associada automaticamente à moto com base no 'codigoUnicoTagParaNovaTag'.
        /// </remarks>
        /// <response code="201">Retorna a moto recém-criada e a URL para acessá-la.</response>
        /// <response code="400">Se os dados fornecidos forem inválidos ou incompletos.</response>
        /// <response code="409">Se a placa ou o código da tag já existirem no sistema.</response>
        [HttpPost]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMoto([FromBody] CreateMotoDto createMotoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateMoto: ModelState inválido ao tentar criar moto.");
                return BadRequest(ModelState);
            }
            var motoCriada = await _motoService.CreateMotoAsync(createMotoDto);
            return CreatedAtAction(nameof(GetMotoById), new { id = motoCriada.Id }, motoCriada);
        }

        /// <summary>
        /// Lista todas as motos cadastradas, com opção de filtros.
        /// </summary>
        /// <param name="status">Filtra motos por um status específico (ex: "ProntaParaAluguel"). Case-insensitive.</param>
        /// <param name="placa">Filtra motos por parte da placa (busca parcial, case-insensitive).</param>
        /// <returns>Uma lista de motos.</returns>
        /// <response code="200">Retorna a lista de motos (pode ser vazia).</response>
        /// <response code="400">Se o parâmetro de filtro 'status' for inválido.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MotoViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMotos([FromQuery] string? status, [FromQuery] string? placa)
        {
            var motos = await _motoService.GetAllMotosAsync(status, placa);
            return Ok(motos);
        }

        /// <summary>
        /// Obtém os detalhes de uma moto específica pelo seu ID.
        /// </summary>
        /// <param name="id">O ID (GUID) da moto a ser recuperada.</param>
        /// <returns>Os detalhes da moto.</returns>
        /// <response code="200">Retorna os detalhes da moto.</response>
        /// <response code="404">Se a moto com o ID especificado não for encontrada.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMotoById(Guid id)
        {
            var moto = await _motoService.GetMotoByIdAsync(id);
            return Ok(moto);
        }

        /// <summary>
        /// Obtém os detalhes de uma moto específica pela sua placa.
        /// </summary>
        /// <param name="placa">A placa da moto a ser recuperada.</param>
        /// <returns>Os detalhes da moto.</returns>
        /// <response code="200">Retorna os detalhes da moto.</response>
        /// <response code="400">Se a placa fornecida for inválida (ex: vazia).</response>
        /// <response code="404">Se a moto com a placa especificada não for encontrada.</response>
        [HttpGet("por-placa/{placa}")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMotoByPlaca(string placa)
        {
            var moto = await _motoService.GetMotoByPlacaAsync(placa);
            return Ok(moto);
        }

        /// <summary>
        /// Atualiza os dados de uma moto existente.
        /// </summary>
        /// <param name="id">O ID (GUID) da moto a ser atualizada.</param>
        /// <param name="updateMotoDto">Objeto contendo os dados da moto para atualização (Placa, Modelo, StatusMoto).</param>
        /// <returns>Retorna o objeto da moto atualizada.</returns>
        /// <response code="200">Retorna a moto atualizada.</response>
        /// <response code="400">Se os dados fornecidos forem inválidos (ex: campos obrigatórios faltando, placa condicionalmente obrigatória não fornecida).</response>
        /// <response code="404">Se a moto com o ID especificado não for encontrada.</response>
        /// <response code="409">Se a nova placa já existir em outra moto, ou se houver um conflito de concorrência.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMoto(Guid id, [FromBody] UpdateMotoDto updateMotoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateMoto: ModelState inválido para ID {MotoId}.", id);
                return BadRequest(ModelState);
            }
            var motoAtualizadaDto = await _motoService.UpdateMotoAsync(id, updateMotoDto);
            return Ok(motoAtualizadaDto);
        }

        /// <summary>
        /// Remove uma moto do sistema.
        /// </summary>
        /// <remarks>
        /// Ao remover uma moto, a Tag BLE associada a ela também será removida.
        /// </remarks>
        /// <param name="id">O ID (GUID) da moto a ser removida.</param>
        /// <returns>Nenhum conteúdo se a remoção for bem-sucedida.</returns>
        /// <response code="204">Moto removida com sucesso.</response>
        /// <response code="404">Se a moto com o ID especificado não for encontrada.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMoto(Guid id)
        {
            await _motoService.DeleteMotoAsync(id);
            return NoContent();
        }
    }
}
