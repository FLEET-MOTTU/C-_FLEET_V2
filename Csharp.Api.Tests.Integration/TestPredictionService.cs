using Csharp.Api.ML.Models;
using Csharp.Api.Entities.Enums;
using Csharp.Api.Services;

namespace Csharp.Api.Tests.Integration
{
    // Simple deterministic prediction service used during integration tests
    public class TestPredictionService : IPredictionService
    {
        public VistoriaOutput PreverVistoria(TipoModeloMoto modelo)
        {
            // Return a simple deterministic result so controllers can function without a real ML model
            return new VistoriaOutput
            {
                PrecisaReparoComplexo = false,
                Score = new float[] { 0.0f }
            };
        }
    }
}
