using Microsoft.ML;
using Microsoft.Extensions.ML;
using Csharp.Api.ML.Models;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Services
{
    public interface IPredictionService
    {
        VistoriaOutput PreverVistoria(TipoModeloMoto modelo);
    }

    /// <summary>
    /// Serviço de predição que encapsula a lógica de inferência usando o modelo ML embarcado.
    /// Fornece métodos para executar previsões a partir do modelo treinado.
    /// </summary>
    public class PredictionService : IPredictionService
    {
        private readonly PredictionEnginePool<VistoriaInput, VistoriaOutput> _pool;
        private readonly ILogger<PredictionService> _logger;

        public PredictionService(PredictionEnginePool<VistoriaInput, VistoriaOutput> pool, ILogger<PredictionService> logger)
        {
            _pool = pool;
            _logger = logger;
        }

        public VistoriaOutput PreverVistoria(TipoModeloMoto modelo)
        {
            var input = new VistoriaInput { Modelo = modelo.ToString() };
            var output = _pool.Predict(input);
            
            _logger.LogInformation("Predição ML.NET: Modelo={Modelo}, PrevistoComplexo={Prev}, Score={Score}", 
                input.Modelo, output.PrecisaReparoComplexo, output.Score);
                
            return output;
        }
    }
}