using Microsoft.ML.Data;

namespace Csharp.Api.ML.Models
{
    public class VistoriaOutput
    {
        /// <summary>
        /// Resultado da previsão binária gerada pelo modelo ML.
        /// true indica que o veículo provavelmente precisa de reparo/vistoria complexo.
        /// </summary>
        [ColumnName("Prediction")]
        public bool PrecisaReparoComplexo { get; set; }

        /// <summary>
        /// Vetor de probabilidades/score retornado pelo modelo (por exemplo, [p0, p1]).
        /// Mapeado do schema ML como um vetor de float; inicializado para evitar aviso de não anulabilidade.
        /// </summary>
        [ColumnName("Score")]
        public float[] Score { get; set; } = System.Array.Empty<float>();
    }
}