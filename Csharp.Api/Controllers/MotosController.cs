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

        // POST: api/motos
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

        // GET: api/motos
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MotoViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMotos([FromQuery] string? status, [FromQuery] string? placa)
        {
            var motos = await _motoService.GetAllMotosAsync(status, placa);
            return Ok(motos);
        }

        // GET: api/motos/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMotoById(Guid id)
        {
            var moto = await _motoService.GetMotoByIdAsync(id);
            return Ok(moto);
        }

        // GET: api/motos/por-placa/{placa}
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

        // PUT: api/motos/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
            await _motoService.UpdateMotoAsync(id, updateMotoDto);
            return NoContent();
        }

        // DELETE: api/motos/{id}
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
