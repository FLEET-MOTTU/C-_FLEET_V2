using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Oracle.EntityFrameworkCore.Storage.Internal;
using Swashbuckle.AspNetCore.Filters;

using Csharp.Api.Data;
using Csharp.Api.Services;
using Csharp.Api.Middleware;
using Csharp.Api.Profiles;
using Csharp.Api.SwaggerExamples;

using System.Text.Json.Serialization;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var oracleConnectionString = builder.Configuration.GetConnectionString("OracleConnection");
if (string.IsNullOrEmpty(oracleConnectionString))
{
    throw new InvalidOperationException("String de conexão 'OracleConnection' não foi encontrada ou está vazia. Verifique a configuração.");
}


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnectionString, oraOpt => 
    {
        oraOpt.ExecutionStrategy(c => new OracleExecutionStrategy(c));
    })
);

// injeção de dependência
builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<IIoTEventService, IoTEventService>();
builder.Services.AddScoped<IBeaconService, BeaconService>();
builder.Services.AddHostedService<InterServiceSyncService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

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
        Title = "Mottu Fleet API - C# (Pátio)",
        Description = "API para gerenciamento de pátios e motos da Mottu."
    });

    // XML doc
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Configura a inclusão de exemplos de payloads no Swagger
    options.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateMotoDtoExample>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["JwtSettings:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("Chave JWT ('JwtSettings:Key') não encontrada ou está vazia.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var app = builder.Build();

// Middleware de tratamento global de exceções
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

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Mottu Challenge V1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();