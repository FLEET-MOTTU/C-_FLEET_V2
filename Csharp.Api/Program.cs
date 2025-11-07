using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Oracle.EntityFrameworkCore.Storage.Internal;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Middleware;
using Csharp.Api.Profiles;
using Csharp.Api.Services;
using Csharp.Api.Enums;
using Csharp.Api.ML.Models;
using Microsoft.Extensions.ML;
using Csharp.Api.Infrastructure.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// ==================================================
// 1️⃣ Configuração (JSON + variáveis de ambiente)
// ==================================================
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables(); // <-- lê .env (via Docker)


// ==================================================
// 2️⃣ Banco de Dados (Oracle)
// ==================================================
var oracleConnectionString =
    builder.Configuration.GetConnectionString("OracleConnection")
    ?? builder.Configuration["ConnectionStrings:OracleConnection"];

if (!builder.Environment.IsEnvironment("Testing"))
{
    if (string.IsNullOrEmpty(oracleConnectionString))
    {
        throw new InvalidOperationException("String de conexão 'OracleConnection' não foi encontrada. Verifique o .env ou o docker-compose.");
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(oracleConnectionString, oraOpt =>
        {
            oraOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
    );
}

// ==================================================
// 3️⃣ Injeção de Dependências
// ==================================================
builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<IIoTEventService, IoTEventService>();
builder.Services.AddScoped<IBeaconService, BeaconService>();
builder.Services.AddScoped<IPateoService, PateoService>();
builder.Services.AddScoped<ITagPositionProcessor, TagPositionProcessor>();
builder.Services.AddScoped<IClassificationService, ClassificationService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();

builder.Services.AddPredictionEnginePool<VistoriaInput, VistoriaOutput>()
    .FromFile(filePath: "MLModel/model.zip", watchForChanges: true);

builder.Services.AddHostedService<TagPositionConsumerService>();
builder.Services.AddHostedService<InterServiceSyncService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

// ==================================================
// 4️⃣ Controllers & API Configuration
// ==================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure API versioning and API Explorer
builder.Services.AddEndpointsApiExplorer();

// ==================================================
// ✅ API Versioning + Explorer (config oficial Asp.Versioning 8.x)
// ==================================================
builder.Services
    .AddApiVersioning(options =>
    {
        // define a default version, mas não força os controllers
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    // habilita suporte aos atributos [ApiVersion] nos controllers
    .AddMvc()
    // adiciona o ApiExplorer, que é o que gera os docs no Swagger
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; // gera v1, v2, etc
        options.SubstituteApiVersionInUrl = true;
    });



// ==================================================
// 5️⃣ Swagger Configuration
// ==================================================
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<SwaggerDefaultValues>();

    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.ExampleFilters();

    // Auth (Bearer)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Autenticação JWT (Bearer). Insira 'Bearer [espaço] e o token JWT.'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateMotoDtoExample>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHealthChecks()
        .AddOracle(
            connectionString: oracleConnectionString ?? throw new InvalidOperationException("Oracle connection string is not configured."),
            name: "Oracle DB (FIAP)",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "database", "ready" },
            timeout: TimeSpan.FromSeconds(10)
        );
}
else
{
    // For testing we don't have Oracle; register health checks without Oracle probe.
    builder.Services.AddHealthChecks();
}

// ==================================================
// 6️⃣ Auth (JWT)
// ==================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RolesOperacionais", policy =>
        policy.RequireRole(
            nameof(UserRole.OPERACIONAL),
            nameof(UserRole.ADMINISTRATIVO),
            nameof(UserRole.TEMPORARIO)
        ));
});

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
        throw new InvalidOperationException("Chave JWT ('JwtSettings:Key') não encontrada. Verifique o .env.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    };
});

// ==================================================
// 7️⃣ Pipeline / Execução
// ==================================================
var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Migrations automáticas
try
{
    Console.WriteLine("INFO: Verificando e aplicando migrations pendentes do EF Core...");
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // During integration tests we use the InMemory provider which doesn't support
    // relational migrations. Skip calling Migrate when running under the Testing
    // environment (the test factory will ensure the test DB is created/seeded).
    if (!app.Environment.IsEnvironment("Testing"))
    {
        dbContext.Database.Migrate();
    }
    Console.WriteLine("INFO: Migrations aplicadas com sucesso (ou nenhuma pendente).");
}
catch (Exception ex)
{
    Console.WriteLine($"ERRO CRÍTICO ao aplicar migrations: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.DisplayRequestDuration();
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);

    // 🔹 Adiciona todas as versões
    foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(d => d.ApiVersion))
    {
        Console.WriteLine($"Swagger doc added: /swagger/{description.GroupName}/swagger.json");

        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"Mottu Fleet API {description.ApiVersion}"
        );
    }

    // 🔹 FORÇA O DROPDOWN DE VERSÕES
    options.EnableTryItOutByDefault();       // exibe botão "Try it out"
    options.EnableValidator();               // exibe validação
    options.EnableDeepLinking();             // mantém URL estável
    options.EnablePersistAuthorization();    // guarda token entre versões

    // 🔹 ESSENCIAL — ativa o seletor de versão manualmente
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottu Fleet API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Mottu Fleet API v2");
});



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");


// 🚨 DEBUG TEMPORÁRIO — imprime as versões detectadas pelo ApiExplorer
using (var scope = app.Services.CreateScope())
{
    var providerDebug = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

    Console.WriteLine("========== API VERSIONS DETECTED ==========");
    foreach (var desc in providerDebug.ApiVersionDescriptions)
    {
        Console.WriteLine($"✅ Version {desc.GroupName} (deprecated={desc.IsDeprecated})");
    }
    Console.WriteLine("===========================================");
}


app.Run();

public partial class Program { }