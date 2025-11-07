using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace Csharp.Api.Infrastructure.Swagger
{
    /// <summary>
    /// Aplica valores padrão às operações do Swagger.
    /// Compatibiliza informações do ApiExplorer com as operações geradas pelo Swagger.
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            // Obtém metadados de versão associados à ação
            var metadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
            var model = metadata.Map(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

            // Detecta se a versão corrente do documento está marcada como depreciada
            if (model.DeprecatedApiVersions.Any())
            {
                var group = apiDescription.GroupName;

                if (!string.IsNullOrEmpty(group) && group.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    var versionText = group.Substring(1); // "1" ou "2.0"

                    // Compara textual (sem parser)
                    var isDeprecated = model.DeprecatedApiVersions
                        .Any(v => string.Equals(v.ToString(), versionText, StringComparison.OrdinalIgnoreCase));

                    if (isDeprecated)
                        operation.Deprecated = true;
                }
            }

            // Ajusta descrições de parâmetros usando metadados do ApiExplorer
            if (operation.Parameters == null)
                return;

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions
                    .FirstOrDefault(p => p.Name == parameter.Name);

                if (description == null)
                    continue;

                parameter.Description ??= description.ModelMetadata?.Description;
            }
        }
    }
}
