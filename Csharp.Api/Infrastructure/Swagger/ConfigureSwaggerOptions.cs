using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Csharp.Api.Infrastructure.Swagger
{
    /// <summary>
    /// Gera um documento Swagger (swagger.json) para cada versão de API registrada.
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                var info = new OpenApiInfo
                {
                    Title = "Mottu Fleet API",
                    Version = description.ApiVersion.ToString(),
                    Description = "API para gerenciamento de pátios, beacons e motos da Mottu."
                };

                if (description.IsDeprecated)
                {
                    info.Description += " ⚠️ Esta versão está depreciada.";
                }

                options.SwaggerDoc(description.GroupName, info);
            }
        }
    }
}
