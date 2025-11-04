using System.Text.Json;

using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;

using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.DTOs.ValidationAttributes;
using Csharp.Api.Entities;


namespace Csharp.Api.Services
{
    /**
     * Serviço de background (IHostedService) que ouve a fila de sincronização
     * inter-serviços (populada pelo Java) e atualiza o banco de dados local do C#.
     */
    public class InterServiceSyncService : IHostedService
    {
        private readonly ILogger<InterServiceSyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusProcessor _processor;
        private readonly JsonSerializerOptions _jsonOptions;

        // Construtor com o nome da classe atualizado
        public InterServiceSyncService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<InterServiceSyncService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            var connectionString = configuration["AzureServiceBus:ConnectionString"];
            var queueName = configuration["AzureServiceBus:QueueName"];

            var client = new ServiceBusClient(connectionString);
            _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando InterServiceSyncService (Listener da Fila)..."); // Nome do log mudou
            _processor.ProcessMessageAsync += HandleMessageAsync;
            _processor.ProcessErrorAsync += HandleErrorAsync;
            await _processor.StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Encerrando InterServiceSyncService..."); // Nome do log mudou
            await _processor.StopProcessingAsync(cancellationToken);
        }

        private Task HandleErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Erro ao processar mensagem da fila: {ErrorSource} {EntityPath}", args.ErrorSource, args.EntityPath);
            return Task.CompletedTask;
        }

        // --- MÉTODO HandleMessageAsync ATUALIZADO ---
        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            _logger.LogInformation("Mensagem de sincronização recebida: {Body}", body);

            try
            {
                // 1. Desserializa para o "envelope" genérico InterServiceMessage
                var message = JsonSerializer.Deserialize<InterServiceMessage>(body, _jsonOptions);
                if (message == null || string.IsNullOrEmpty(message.EventType))
                {
                    _logger.LogWarning("Não foi possível desserializar o envelope da mensagem.");
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                // Cria um escopo para o DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // 2. ROTEIA a mensagem com base no eventType
                    await ProcessEvent(context, message.EventType, message.Data);
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao processar evento. Enviando para Dead Letter Queue.");
                await args.DeadLetterMessageAsync(args.Message, ex.Message);
            }
        }

        // --- MÉTODO ProcessEvent ATUALIZADO (O Roteador) ---
        // Recebe o JsonElement genérico e decide o que fazer
        private async Task ProcessEvent(AppDbContext context, string eventType, JsonElement data)
        {
            // Roteia para o processador correto
            switch (eventType)
            {
                // Eventos de Funcionário
                case "FUNCIONARIO_CRIADO":
                case "FUNCIONARIO_ATUALIZADO":
                case "FUNCIONARIO_ATUALIZADO_FOTO":
                case "FUNCIONARIO_REATIVADO":
                case "FUNCIONARIO_DESATIVADO":
                    var funcPayload = data.Deserialize<FuncionarioSyncPayload>(_jsonOptions);
                    if (funcPayload != null) await ProcessFuncionarioEvent(context, funcPayload);
                    break;

                // Eventos de Pátio
                case "PATEO_CRIADO":
                case "PATEO_ATUALIZADO":
                    var pateoPayload = data.Deserialize<PateoSyncPayload>(_jsonOptions);
                    if (pateoPayload != null) await ProcessPateoEvent(context, pateoPayload);
                    break;

                // Eventos de Zona
                case "ZONA_CRIADA":
                case "ZONA_ATUALIZADA":
                    var zonaPayload = data.Deserialize<ZonaSyncPayload>(_jsonOptions);
                    if (zonaPayload != null) await ProcessZonaEvent(context, zonaPayload);
                    break;

                case "ZONA_DELETADA":
                    var zonaDelPayload = data.Deserialize<ZonaSyncPayload>(_jsonOptions);
                    if (zonaDelPayload != null) await ProcessZonaDeleteEvent(context, zonaDelPayload);
                    break;

                default:
                    _logger.LogWarning("Tipo de evento desconhecido: {EventType}", eventType);
                    break;
            }

            // Salva as mudanças (se houver alguma) no final do roteamento
            await context.SaveChangesAsync();
        }

        // --- MÉTODOS DE PROCESSAMENTO (O que já tínhamos + os novos) ---

        // Lógica de Funcionário (o que já tínhamos)
        private async Task ProcessFuncionarioEvent(AppDbContext context, FuncionarioSyncPayload data)
        {
            var funcionario = await context.Funcionarios.FindAsync(data.Id);
            if (funcionario == null)
            {
                _logger.LogInformation("Criando novo funcionário no banco C#: {FuncionarioId}", data.Id);
                funcionario = new Funcionario { Id = data.Id }; // O Id vem do Java
                context.Funcionarios.Add(funcionario);
            }
            else
            {
                _logger.LogInformation("Atualizando funcionário no banco C#: {FuncionarioId}", data.Id);
            }

            // Mapeamento (Atualiza/Insere)
            funcionario.Nome = data.Nome;
            funcionario.Email = data.Email;
            funcionario.Telefone = data.Telefone;
            funcionario.Cargo = data.Cargo;
            funcionario.Status = data.Status;
            funcionario.PateoId = data.PateoId;
            funcionario.FotoUrl = data.FotoUrl;
        }

        // --- NOVA LÓGICA DE PÁTIO (Criar/Atualizar) ---
        private async Task ProcessPateoEvent(AppDbContext context, PateoSyncPayload data)
        {
            var pateo = await context.Pateos.FindAsync(data.Id);
            if (pateo == null)
            {
                _logger.LogInformation("Criando novo pátio no banco C#: {PateoId}", data.Id);
                pateo = new Pateo { Id = data.Id };
                context.Pateos.Add(pateo);
            }
            else
            {
                _logger.LogInformation("Atualizando pátio no banco C#: {PateoId}", data.Id);
            }

            pateo.Nome = data.Nome;
            pateo.Status = data.Status;
            pateo.PlantaBaixaUrl = data.PlantaBaixaUrl;
            pateo.PlantaLargura = data.PlantaLargura;
            pateo.PlantaAltura = data.PlantaAltura;
            pateo.GerenciadoPorId = data.GerenciadoPorId;
            pateo.CreatedAt = DateTime.UtcNow; // Aproximação do tempo de criação
        }

        // --- NOVA LÓGICA DE ZONA (Criar/Atualizar) ---
        private async Task ProcessZonaEvent(AppDbContext context, ZonaSyncPayload data)
        {
            var zona = await context.Zonas.FindAsync(data.Id);
            if (zona == null)
            {
                _logger.LogInformation("Criando nova zona no banco C#: {ZonaId}", data.Id);
                zona = new Zona { Id = data.Id };
                context.Zonas.Add(zona);
            }
            else
            {
                _logger.LogInformation("Atualizando zona no banco C#: {ZonaId}", data.Id);
            }

            zona.Nome = data.Nome;
            zona.PateoId = data.PateoId;
            zona.CriadoPorId = data.CriadoPorId;
            zona.CoordenadasWKT = data.CoordenadasWKT; // Salvamos o WKT (texto)
            zona.CreatedAt = DateTime.UtcNow; // Aproximação
        }

        // --- NOVA LÓGICA DE ZONA (Deletar) ---
        private async Task ProcessZonaDeleteEvent(AppDbContext context, ZonaSyncPayload data)
        {
            var zona = await context.Zonas.FindAsync(data.Id);
            if (zona != null)
            {
                _logger.LogInformation("Deletando zona do banco C#: {ZonaId}", data.Id);
                context.Zonas.Remove(zona);
            }
            else
            {
                _logger.LogWarning("Recebido evento ZONA_DELETADA, mas zona {ZonaId} não foi encontrada.", data.Id);
            }
        }
    }
}