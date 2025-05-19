using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Csharp.Api.DTOs;
using Csharp.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Csharp.Api.Controllers
{
    [ApiController]
    [Route("api/iot-events")]
    public class IoTEventsController : ControllerBase
    {
        private readonly IIoTEventService _iotEventService;
        private readonly ILogger<IoTEventsController> _logger;

        public IoTEventsController(IIoTEventService iotEventService, ILogger<IoTEventsController> logger)
        {
            _iotEventService = iotEventService;
            _logger = logger;
        }

        // POST: api/iot-events/tag-interaction
        [HttpPost("tag-interaction")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostTagInteraction([FromBody] TagInteractionEventDto eventoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostTagInteraction: ModelState inválido.");
                return BadRequest(ModelState);
            }

            await _iotEventService.ProcessarInteracaoTagAsync(eventoDto);

            return Accepted(new { message = "Evento de interação da tag recebido e processado." }); 
        }
    }
}
