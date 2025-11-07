using Microsoft.ML.Data;

namespace Csharp.Api.ML.Models
{
    public class VistoriaInput
    {
        // Coluna 0: A feature que usamos para prever
        [LoadColumn(0)]
        public string Modelo { get; set; } = string.Empty;

        // Coluna 1: A Label que o schema do modelo espera
        // (Mesmo que não seja usada para *prever*, ela precisa estar aqui
        // para o schema bater com o do 'model.zip')
        [LoadColumn(1)]
        public bool Label { get; set; }
    }
}