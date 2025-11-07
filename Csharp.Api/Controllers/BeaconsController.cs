using Asp.Versioning;
using ApiVersion = Asp.Versioning.ApiVersion;
using ApiVersionAttribute = Asp.Versioning.ApiVersionAttribute;
using Csharp.Api.DTOs;
using Csharp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// CRUD de Beacons (gateways do pátio).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/beacons")]
    [Produces("application/json")]
    public class BeaconsController : ControllerBase
    {
        private readonly IBeaconService _beaconService;

        public BeaconsController(IBeaconService beaconService)
        {
            _beaconService = beaconService;
        }

        /// <summary>Cria um novo beacon.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateBeacon([FromBody] CreateBeaconDto createBeaconDto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var beacon = await _beaconService.CreateBeaconAsync(createBeaconDto);
            return CreatedAtAction(nameof(GetBeaconById), new { id = beacon.Id }, beacon);
        }

        /// <summary>Lista beacons com paginação.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<BeaconDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllBeacons([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var beacons = await _beaconService.GetAllBeaconsAsync(page, pageSize);
            return Ok(beacons);
        }

        /// <summary>Obtém beacon por ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBeaconById(Guid id)
        {
            var beacon = await _beaconService.GetBeaconByIdAsync(id);
            return Ok(beacon);
        }

        /// <summary>Obtém beacon por BeaconId (identificador lógico).</summary>
        [HttpGet("by-beaconid/{beaconId}")]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBeaconByBeaconId(string beaconId)
        {
            if (string.IsNullOrWhiteSpace(beaconId)) return BadRequest("beaconId é obrigatório.");
            var beacon = await _beaconService.GetBeaconByBeaconIdAsync(beaconId);
            return Ok(beacon);
        }

        /// <summary>Atualiza um beacon.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BeaconDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBeacon(Guid id, [FromBody] UpdateBeaconDto updateBeaconDto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var beacon = await _beaconService.UpdateBeaconAsync(id, updateBeaconDto);
            return Ok(beacon);
        }

        /// <summary>Remove um beacon.</summary>
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
