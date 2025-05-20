using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Csharp.Api.DTOs;
using Csharp.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Csharp.Api.Controllers
{
    /// <summary>
    /// Lida com o recebimento e processamento de eventos de dispositivos IoT (simulados).
    /// Estes eventos são tipicamente enviados por beacons que detectam tags BLE associadas às motos.
    /// </summary>
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

        /// <summary>
        /// Recebe e processa um evento de interação de uma tag BLE.
        /// </summary>
        /// <remarks>
        /// Este endpoint é o principal ponto de entrada para dados simulados de IoT, representando uma tag
        /// sendo detectada por um beacon. A API utiliza esses dados para atualizar o último local conhecido
        /// da moto associada à tag, o timestamp dessa detecção e, opcionalmente, o nível da bateria da tag.
        /// 
        /// **Exemplo de payload:**
        /// 
        ///     {
        ///       "codigoUnicoTag": "TAG_MOTO_001",
        ///       "beaconIdDetectado": "BEACON_ZONA_MANUTENCAO_SUL",
        ///       "timestamp": "2025-05-20T10:30:00Z",
        ///       "nivelBateria": 85,
        ///       "tipoEvento": "leitura_de_beacon"
        ///     }
        /// 
        /// O campo `nivelBateria` e `tipoEvento` são opcionais temporariamente.
        /// Se a tag (`codigoUnicoTag`) não for encontrada no sistema, o evento será logado como aviso,
        /// mas a requisição ainda retornará sucesso (202 Accepted)
        /// </remarks>
        /// <param name="eventoDto">Objeto contendo os dados do evento de interação da tag.</param>
        /// <response code="202">Evento recebido e aceito para processamento (não garante que a tag foi encontrada ou que alguma alteração ocorreu, apenas que o evento foi validado e entregue ao serviço).</response>
        /// <response code="400">Se os dados fornecidos no `eventoDto` forem inválidos (ex: campos obrigatórios faltando, formatos incorretos).</response>
        [HttpPost("tag-interaction")]
        [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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
