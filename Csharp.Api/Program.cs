using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Oracle.EntityFrameworkCore.Storage.Internal;
using Swashbuckle.AspNetCore.Filters;

using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Middleware;
using Csharp.Api.Profiles;
using Csharp.Api.Services;
using Csharp.Api.Enums;

var builder = WebApplication.CreateBuilder(args);

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

if (string.IsNullOrEmpty(oracleConnectionString))
{
    throw new InvalidOperationException("String de conexão 'OracleConnection' não foi encontrada. Verifique o .env ou o docker-compose.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnectionString, oraOpt =>
    {
        oraOpt.ExecutionStrategy(c => new OracleExecutionStrategy(c));
    })
);

// ==================================================
// 3️⃣ Injeção de Dependências
// ==================================================
builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<IIoTEventService, IoTEventService>();
builder.Services.AddScoped<IBeaconService, BeaconService>();
builder.Services.AddScoped<IPateoService, PateoService>();
builder.Services.AddScoped<ITagPositionProcessor, TagPositionProcessor>();
builder.Services.AddScoped<IClassificationService, ClassificationService>();

builder.Services.AddHostedService<TagPositionConsumerService>();
builder.Services.AddHostedService<InterServiceSyncService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);


builder.Services.AddHealthChecks()
    .AddOracle(
        connectionString: oracleConnectionString,
        name: "Oracle DB (FIAP)",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "database", "ready" },
        timeout: TimeSpan.FromSeconds(10)
    );


// ==================================================
// 4️⃣ Controllers / JSON
// ==================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddEndpointsApiExplorer();

// ==================================================
// 5️⃣ Swagger
// ==================================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Mottu Fleet API - C# (Pátio)",
        Description = "API para gerenciamento de pátios e motos da Mottu."
    });

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
    dbContext.Database.Migrate();
    Console.WriteLine("INFO: Migrations aplicadas com sucesso (ou nenhuma pendente).");
}
catch (Exception ex)
{
    Console.WriteLine($"ERRO CRÍTICO ao aplicar migrations: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
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

app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
