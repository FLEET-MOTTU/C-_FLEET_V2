using Asp.Versioning;
using ApiVersion = Asp.Versioning.ApiVersion;
using ApiVersionAttribute = Asp.Versioning.ApiVersionAttribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Csharp.Api.DTOs;
using Csharp.Api.Services;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// Recebe eventos simulados de IoT (detecção de tag por beacon).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/iot-events")]
    [Produces("application/json")]
    public class IoTEventsController : ControllerBase
    {
        private readonly IIoTEventService _iotEventService;
        private readonly ILogger<IoTEventsController> _logger;

        public IoTEventsController(IIoTEventService iotEventService, ILogger<IoTEventsController> logger)
        {
            _iotEventService = iotEventService;
            _logger = logger;
        }

        /// <summary>Processa evento de interação de uma tag BLE.</summary>
        /// <response code="202">Evento aceito.</response>
        /// <response code="400">Payload inválido.</response>
        [HttpPost("tag-interaction")]
        [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostTagInteraction([FromBody] TagInteractionEventDto eventoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostTagInteraction: ModelState inválido.");
                return ValidationProblem(ModelState);
            }

            await _iotEventService.ProcessarInteracaoTagAsync(eventoDto);
            return Accepted(new { message = "Evento de interação da tag recebido e processado." });
        }
    }
}
