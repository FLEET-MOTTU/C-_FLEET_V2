using Microsoft.ML.Data;

namespace ModelTrainer.Models
{
    public class VistoriaInput
    {
        [LoadColumn(0)]
        public string Modelo { get; set; } = string.Empty;
        
        [LoadColumn(1)]
        public bool Label { get; set; }
    }
}