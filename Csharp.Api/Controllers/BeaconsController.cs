using Csharp.Api.DTOs;
using Csharp.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// Gerencia as operações CRUD para os Beacons.
    /// </summary>
    [ApiController]
    [Route("api/beacons")]
    public class BeaconsController : ControllerBase
    {
        private readonly IBeaconService _beaconService;

        public BeaconsController(IBeaconService beaconService)
        {
            _beaconService = beaconService;
        }

        /// <summary>
        /// Cria um novo beacon no sistema.
        /// </summary>
        /// <param name="createBeaconDto">Objeto contendo os dados do beacon a ser criado.</param>
        /// <response code="201">Retorna o beacon recém-criado e a URL para acessá-lo.</response>
        /// <response code="400">Se os dados fornecidos forem inválidos.</response>
        /// <response code="409">Se o ID do beacon já existir no sistema.</response>
        [HttpPost]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateBeacon([FromBody] CreateBeaconDto createBeaconDto)
        {
            var beacon = await _beaconService.CreateBeaconAsync(createBeaconDto);
            return CreatedAtAction(nameof(GetBeaconById), new { id = beacon.Id }, beacon);
        }

        /// <summary>
        /// Lista todos os beacons cadastrados, com suporte a paginação.
        /// </summary>
        /// <param name="page">Número da página a ser retornada. Valor padrão é 1.</param>
        /// <param name="pageSize">Número de itens por página. Valor padrão é 10.</param>
        /// <returns>Uma lista paginada de beacons.</returns>
        /// <response code="200">Retorna a lista paginada de beacons (pode ser vazia).</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<BeaconDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllBeacons([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var beacons = await _beaconService.GetAllBeaconsAsync(page, pageSize);
            return Ok(beacons);
        }

        /// <summary>
        /// Obtém os detalhes de um beacon específico pelo seu ID.
        /// </summary>
        /// <param name="id">O ID (GUID) do beacon a ser recuperado.</param>
        /// <returns>Os detalhes do beacon.</returns>
        /// <response code="200">Retorna os detalhes do beacon.</response>
        /// <response code="404">Se o beacon com o ID especificado não for encontrado.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBeaconById(Guid id)
        {
            var beacon = await _beaconService.GetBeaconByIdAsync(id);
            return Ok(beacon);
        }
        
        /// <summary>
        /// Obtém os detalhes de um beacon específico pelo seu BeaconId.
        /// </summary>
        /// <param name="beaconId">O ID único do beacon a ser recuperado.</param>
        /// <returns>Os detalhes do beacon.</returns>
        /// <response code="200">Retorna os detalhes do beacon.</response>
        /// <response code="404">Se o beacon com o BeaconId especificado não for encontrado.</response>
        [HttpGet("by-beaconid/{beaconId}")]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBeaconByBeaconId(string beaconId)
        {
            var beacon = await _beaconService.GetBeaconByBeaconIdAsync(beaconId);
            return Ok(beacon);
        }

        /// <summary>
        /// Atualiza os dados de um beacon existente.
        /// </summary>
        /// <param name="id">O ID (GUID) do beacon a ser atualizado.</param>
        /// <param name="updateBeaconDto">Objeto com os dados para atualização.</param>
        /// <returns>Retorna o objeto do beacon atualizado.</returns>
        /// <response code="200">Retorna o beacon atualizado.</response>
        /// <response code="400">Se os dados fornecidos forem inválidos.</response>
        /// <response code="404">Se o beacon com o ID especificado não for encontrado.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBeacon(Guid id, [FromBody] UpdateBeaconDto updateBeaconDto)
        {
            var beacon = await _beaconService.UpdateBeaconAsync(id, updateBeaconDto);
            return Ok(beacon);
        }

        /// <summary>
        /// Remove um beacon do sistema.
        /// </summary>
        /// <param name="id">O ID (GUID) do beacon a ser removido.</param>
        /// <returns>Nenhum conteúdo se a remoção for bem-sucedida.</returns>
        /// <response code="204">Beacon removido com sucesso.</response>
        /// <response code="404">Se o beacon com o ID especificado não for encontrado.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBeacon(Guid id)
        {
            await _beaconService.DeleteBeaconAsync(id);
            return NoContent();
        }
    }
}