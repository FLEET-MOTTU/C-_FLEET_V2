using Csharp.Api.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;


namespace Csharp.Api.Tests.Integration
{
    public class FleetApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
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
                // Remove o DbContext do Oracle
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Adiciona o DbContext em Memória
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDb");
                });

                // Remove os Listeners de Fila (IHostedService)
                var hostedServices = services
                    .Where(d => d.ImplementationType?
                    .GetInterfaces().Contains(typeof(IHostedService)) ?? false).ToList();
                foreach (var service in hostedServices)
                {
                    services.Remove(service);
                }

                // Substitui a Autenticação Real (JWT) pela de teste
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(
                        "TestScheme", options => { });
            });

            builder.UseEnvironment("Testing");
        }
    }
}