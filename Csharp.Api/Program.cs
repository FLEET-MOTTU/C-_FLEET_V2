using Microsoft.EntityFrameworkCore;
using Csharp.Api.Data;
using Csharp.Api.Services;
using Csharp.Api.Middleware;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var oracleConnectionString = builder.Configuration.GetConnectionString("OracleConnection");
if (string.IsNullOrEmpty(oracleConnectionString))
{   
    throw new InvalidOperationException("String de conexão 'OracleConnection' não foi encontrada ou está vazia. Verifique a configuração.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnectionString, oraOpt => {})
);

builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<IIoTEventService, IoTEventService>();

builder.Services.AddControllers()
  .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Mottu Fleet API - C#",
        Description = "API para gerenciamento de pátios e motos da Mottu."
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

{
    Console.WriteLine("INFO: Verificando e aplicando migrations pendentes do EF Core...");
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
            Console.WriteLine("INFO: Migrations aplicadas com sucesso (ou nenhuma estava pendente).");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO CRÍTICO: Ocorreu um erro ao aplicar as migrations do EF Core: {ex.Message}");
        Console.WriteLine($"Detalhes: {ex.StackTrace}");
        throw; 
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottu Fleet API C# V1");
    }); 
}
else
{
    app.UseHsts();
}

// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();
app.Run();
