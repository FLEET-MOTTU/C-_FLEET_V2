using Csharp.Api.DTOs;
using Csharp.Api.Entities.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Csharp.Api.SwaggerExamples
{
    public class CreateMotoDtoExample : IExamplesProvider<CreateMotoDto>
    {
        public CreateMotoDto GetExamples()
        {
            return new CreateMotoDto
            {
                Placa = "AAA-1B23",
                Modelo = TipoModeloMoto.ModeloUrbana125,
                StatusMoto = TipoStatusMoto.PendenteColeta,
                CodigoUnicoTagParaNovaTag = "TAG_MOTTU_001A",
                FuncionarioRecolhimentoId = Guid.NewGuid(),
                DataRecolhimento = DateTime.UtcNow
            };
        }
    }
}