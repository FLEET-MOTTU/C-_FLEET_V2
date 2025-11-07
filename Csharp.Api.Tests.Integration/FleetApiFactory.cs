using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Csharp.Api.Data;
using Csharp.Api.Entities;
using Csharp.Api.Services;
using Microsoft.Extensions.DependencyInjection;


namespace Csharp.Api.Tests.Integration
{
    public class FleetApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Ensure Program.cs finds a connection string so it doesn't throw during startup.
            // We'll replace the real DbContext later with an in-memory one.
            Environment.SetEnvironmentVariable("ConnectionStrings__OracleConnection", "DataSource=Test");
            // Provide a dummy JWT key so AddJwtBearer doesn't throw during test startup.
            Environment.SetEnvironmentVariable("JwtSettings__Key", "ThisIsATestJwtKey0123456789012345");
            Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer");
            Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience");

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:OracleConnection"] = "connection_string_para_teste"
                });
            });

            // Substitui os serviços
            builder.ConfigureServices(services =>
            {
                // Use a stable in-memory DB name for this factory instance so seeding and the
                // app use the same named database.
                var inMemoryDbName = Guid.NewGuid().ToString();
                // Remove o DbContext do Oracle
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Adiciona o DbContext em Memória (use a DB por-fábrica para isolamento)
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(inMemoryDbName);
                });

                // Seed DB with known entities used by tests (patio, funcionario, zonas)
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();

                    // Ensure database created
                    db.Database.EnsureCreated();

                    // Seed Pateo
                    var pateoId = Guid.Parse("CDB08828-8D80-BD46-88F5-9673D2477BC0");
                    if (!db.Pateos.Any(p => p.Id == pateoId))
                    {
                        db.Pateos.Add(new Pateo
                        {
                            Id = pateoId,
                            Nome = "Patio teste C#",
                            PlantaBaixaUrl = "https://stfleetjourneytiago7.blob.core.windows.net/plantas/30a34137-54c3-49f8-8ecc-6f4fcea561df.png",
                            PlantaLargura = 582,
                            PlantaAltura = 538,
                            GerenciadoPorId = Guid.NewGuid(),
                            Status = "ATIVO",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Seed Funcionario
                    var funcId = Guid.Parse("D5A42417-370C-B649-B58F-F32EA74DAE67");
                    if (!db.Funcionarios.Any(f => f.Id == funcId))
                    {
                        db.Funcionarios.Add(new Funcionario
                        {
                            Id = funcId,
                            Nome = "Udyr",
                            Email = "udyr@email.com",
                            Telefone = "11987654331",
                            Cargo = "OPERACIONAL",
                            Status = "ATIVO",
                            PateoId = pateoId,
                            FotoUrl = "https://stfleetjourneytiago7.blob.core.windows.net/fotos/2c730248-f4c9-4968-8429-f13b2b0d132e.png"
                        });
                    }

                    // Seed Zonas
                    var zona1Id = Guid.Parse("75B305D1-72B8-CF40-8A79-7141B2479E02");
                    var zona2Id = Guid.Parse("5E5009EF-289D-2F42-85AD-99A42F415D3F");
                    if (!db.Zonas.Any(z => z.Id == zona1Id))
                    {
                        db.Zonas.Add(new Zona
                        {
                            Id = zona1Id,
                            Nome = "Reparos Simples",
                            PateoId = pateoId,
                            CriadoPorId = Guid.Parse("C5BFDA5360C1544F8A17230D88203E18"),
                            CoordenadasWKT = "POLYGON ((0.1 0.1, 0.4 0.1, 0.4 0.4, 0.1 0.4, 0.1 0.1))",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    if (!db.Zonas.Any(z => z.Id == zona2Id))
                    {
                        db.Zonas.Add(new Zona
                        {
                            Id = zona2Id,
                            Nome = "Motor Defeituoso",
                            PateoId = pateoId,
                            CriadoPorId = Guid.Parse("C5BFDA5360C1544F8A17230D88203E18"),
                            CoordenadasWKT = "POLYGON ((0.6 0.6, 0.9 0.6, 0.9 0.9, 0.6 0.9, 0.6 0.6))",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    db.SaveChanges();
                }

                // Remove the Listeners (IHostedService)
                var hostedServices = services
                    .Where(d => d.ImplementationType?
                    .GetInterfaces().Contains(typeof(IHostedService)) ?? false).ToList();
                foreach (var service in hostedServices)
                {
                    services.Remove(service);
                }

                // Replace the real prediction service with a deterministic test implementation so
                // integration tests don't need the ML model file on disk.
                services.AddSingleton<IPredictionService, TestPredictionService>();

                // Substitui a Autenticação Real (JWT) pela de teste e garante que é o esquema padrão
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(
                        "TestScheme", options => { });
            });

            builder.UseEnvironment("Testing");
        }
    }
}