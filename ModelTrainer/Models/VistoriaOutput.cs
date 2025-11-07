using Microsoft.ML.Data;

namespace ModelTrainer.Models
{
    public class VistoriaOutput
    {
        [ColumnName("Prediction")]
        public bool PrecisaReparoComplexo { get; set; }

        public float Score { get; set; }
    }
}