using Microsoft.ML.Data;

namespace Csharp.Api.ML.Models
{
    public class VistoriaOutput
    {
        // Esta é a previsão final (true ou false)
        // O seu 'ModelTrainer' antigo (que gerou o model.zip) tinha a linha:
        // .Append(mlContext.Transforms.Conversion.MapKeyToValue("Prediction"));
        // ... então esta propriedade deve estar correta.
        [ColumnName("Prediction")]
        public bool PrecisaReparoComplexo { get; set; }

        // --- A CORREÇÃO ESTÁ AQUI ---
        // O log de erro disse que 'Score' é um 'Vector<Single, 2>'.
        // Nós mapeamos isso para um array de floats (float[]).
        [ColumnName("Score")]
        public float[] Score { get; set; }
    }
}