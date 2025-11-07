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

    /// <summary>
    /// Processa um evento de interação de uma tag BLE detectada por um beacon.
    /// </summary>
    /// <param name="eventoDto">Dados do evento de interação (tag, beacon, timestamp, RSSI).</param>
    /// <returns>202 Accepted quando o evento foi aceito para processamento assíncrono.</returns>
    /// <response code="400">Retorna 400 quando o payload é inválido.</response>
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
