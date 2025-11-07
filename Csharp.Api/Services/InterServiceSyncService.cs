using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Csharp.Api.Data;
using Csharp.Api.DTOs.Sync;
using Csharp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Csharp.Api.Services
{
    /// <summary>
    /// Consome fila de sync (Java → C#) e replica Pátio/Zona/Funcionário.
    /// </summary>
    /// <summary>
    /// Serviço hospedado que realiza sincronização periódica entre sistemas (integração com outros serviços).
    /// Utilizado para manter entidades sincronizadas entre sistemas externos e este contexto.
    /// </summary>
    public class InterServiceSyncService : IHostedService
    {
        private readonly ILogger<InterServiceSyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusProcessor _processor;
        private readonly JsonSerializerOptions _jsonOptions;

        public InterServiceSyncService(IConfiguration configuration,
                                       IServiceProvider serviceProvider,
                                       ILogger<InterServiceSyncService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            var connectionString = configuration["AzureServiceBus:ConnectionString"];
            var queueName        = configuration["AzureServiceBus:QueueName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(queueName))
                throw new InvalidOperationException("Config AzureServiceBus (ConnectionString/QueueName) ausente.");

            var client = new ServiceBusClient(connectionString);
            _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 4,
                AutoCompleteMessages = false
            });

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

    /// <summary>
    /// Inicializa a rotina de sincronização entre serviços.
    /// Executado quando o host sobe; agenda tarefas periódicas conforme configuração.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando InterServiceSyncService...");
            _processor.ProcessMessageAsync += HandleMessageAsync;
            _processor.ProcessErrorAsync   += HandleErrorAsync;
            await _processor.StartProcessingAsync(cancellationToken);
        }

    /// <summary>
    /// Encerra a rotina de sincronização e limpa recursos alocados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Encerrando InterServiceSyncService...");
            await _processor.StopProcessingAsync(cancellationToken);
        }

        private Task HandleErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Erro fila sync: {Source} {Path}", args.ErrorSource, args.EntityPath);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            _logger.LogDebug("Sync mensagem: {Body}", body);

            try
            {
                var envelope = JsonSerializer.Deserialize<InterServiceMessage>(body, _jsonOptions);
                if (envelope == null || string.IsNullOrWhiteSpace(envelope.EventType))
                {
                    _logger.LogWarning("Envelope inválido. DLQ.");
                    await args.DeadLetterMessageAsync(args.Message, "Invalid envelope");
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await ProcessEventAsync(db, envelope.EventType, envelope.Data);

                await db.SaveChangesAsync();
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar mensagem de sync. DLQ.");
                await args.DeadLetterMessageAsync(args.Message, ex.Message);
            }
        }

        private async Task ProcessEventAsync(AppDbContext db, string eventType, JsonElement data)
        {
            switch (eventType)
            {
                // Funcionário
                case "FUNCIONARIO_CRIADO":
                case "FUNCIONARIO_ATUALIZADO":
                case "FUNCIONARIO_ATUALIZADO_FOTO":
                case "FUNCIONARIO_REATIVADO":
                case "FUNCIONARIO_DESATIVADO":
                {
                    var payload = data.Deserialize<FuncionarioSyncPayload>(_jsonOptions);
                    if (payload != null) await UpsertFuncionarioAsync(db, payload);
                    break;
                }

                // Pátio
                case "PATEO_CRIADO":
                case "PATEO_ATUALIZADO":
                {
                    var payload = data.Deserialize<PateoSyncPayload>(_jsonOptions);
                    if (payload != null) await UpsertPateoAsync(db, payload);
                    break;
                }

                // Zona
                case "ZONA_CRIADA":
                case "ZONA_ATUALIZADA":
                {
                    var payload = data.Deserialize<ZonaSyncPayload>(_jsonOptions);
                    if (payload != null) await UpsertZonaAsync(db, payload);
                    break;
                }

                case "ZONA_DELETADA":
                {
                    var payload = data.Deserialize<ZonaSyncPayload>(_jsonOptions);
                    if (payload != null) await DeleteZonaAsync(db, payload);
                    break;
                }

                default:
                    _logger.LogWarning("EventType desconhecido: {EventType}", eventType);
                    break;
            }
        }

        private static async Task UpsertFuncionarioAsync(AppDbContext db, FuncionarioSyncPayload d)
        {
            var f = await db.Funcionarios.FirstOrDefaultAsync(x => x.Id == d.Id);
            if (f == null)
            {
                f = new Funcionario { Id = d.Id };
                db.Funcionarios.Add(f);
            }
            f.Nome = d.Nome;
            f.Email = d.Email;
            f.Telefone = d.Telefone;
            f.Cargo = d.Cargo;
            f.Status = d.Status;
            f.PateoId = d.PateoId;
            f.FotoUrl = d.FotoUrl;
        }

        private static async Task UpsertPateoAsync(AppDbContext db, PateoSyncPayload d)
        {
            var p = await db.Pateos.FirstOrDefaultAsync(x => x.Id == d.Id);
            if (p == null)
            {
                p = new Pateo { Id = d.Id, CreatedAt = DateTime.UtcNow };
                db.Pateos.Add(p);
            }
            p.Nome = d.Nome;
            p.Status = d.Status;
            p.PlantaBaixaUrl = d.PlantaBaixaUrl;
            p.PlantaLargura = d.PlantaLargura;
            p.PlantaAltura = d.PlantaAltura;
            p.GerenciadoPorId = d.GerenciadoPorId;
            // não tocar CreatedAt em updates
        }

        private static async Task UpsertZonaAsync(AppDbContext db, ZonaSyncPayload d)
        {
            var z = await db.Zonas.FirstOrDefaultAsync(x => x.Id == d.Id);
            if (z == null)
            {
                z = new Zona { Id = d.Id, CreatedAt = DateTime.UtcNow };
                db.Zonas.Add(z);
            }
            z.Nome = d.Nome;
            z.PateoId = d.PateoId;
            z.CriadoPorId = d.CriadoPorId;
            z.CoordenadasWKT = d.CoordenadasWKT;
            // não tocar CreatedAt em updates
        }

        private static async Task DeleteZonaAsync(AppDbContext db, ZonaSyncPayload d)
        {
            var z = await db.Zonas.FirstOrDefaultAsync(x => x.Id == d.Id);
            if (z != null) db.Zonas.Remove(z);
        }
    }
}
