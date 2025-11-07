using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Csharp.Api.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    /// <summary>Consumer da fila de posicionamento; delega ao ITagPositionProcessor.</summary>
    /// <summary>
    /// Serviço hospedado responsável por consumir eventos de posição de tags (background worker).
    /// Executa leitura de filas e invoca o processador de posição conforme necessário.
    /// </summary>
    public class TagPositionConsumerService : IHostedService
    {
        private readonly ILogger<TagPositionConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusProcessor _processor;
        private readonly JsonSerializerOptions _jsonOptions;

        public TagPositionConsumerService(IConfiguration config,
                                          IServiceProvider serviceProvider,
                                          ILogger<TagPositionConsumerService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            var connectionString = config["AzureServiceBus:ConnectionString"];
            var queueName        = config["AzureServiceBus:PositionQueueName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(queueName))
                throw new InvalidOperationException("Config AzureServiceBus PositionQueueName ausente.");

            var client = new ServiceBusClient(connectionString);
            _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 4,
                AutoCompleteMessages = false
            });

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

    /// <summary>
    /// Inicializa o serviço hospedado de consumo de eventos de posição.
    /// Normalmente inicia listeners/consumidores de fila.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando TagPositionConsumerService...");
            _processor.ProcessMessageAsync += HandleMessageAsync;
            _processor.ProcessErrorAsync   += HandleErrorAsync;
            await _processor.StartProcessingAsync(cancellationToken);
        }

    /// <summary>
    /// Encerra o serviço hospedado de consumo, liberando recursos e finalizando listeners.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Encerrando TagPositionConsumerService...");
            await _processor.StopProcessingAsync(cancellationToken);
        }

        private Task HandleErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Erro consumo fila posicionamento: {Source} {Path}", args.ErrorSource, args.EntityPath);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            _logger.LogDebug("Msg posicionamento: {Body}", body);

            try
            {
                // usar o mesmo DTO do processor
                var dto = JsonSerializer.Deserialize<TagInteractionEventDto>(body, _jsonOptions);
                if (dto == null)
                {
                    _logger.LogWarning("Payload inválido. DLQ.");
                    await args.DeadLetterMessageAsync(args.Message, "Invalid payload");
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ITagPositionProcessor>();

                await processor.ProcessAsync(dto);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar mensagem de posicionamento. DLQ.");
                await args.DeadLetterMessageAsync(args.Message, ex.Message);
            }
        }
    }
}
