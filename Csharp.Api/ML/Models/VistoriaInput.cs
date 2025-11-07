using Microsoft.ML.Data;

namespace Csharp.Api.ML.Models
{
    public class VistoriaInput
    {
        /// <summary>
        /// Nome/modelo do veículo usado como feature de entrada.
        /// Mapeado para a coluna 0 do arquivo de dados/feature set.
        /// </summary>
        [LoadColumn(0)]
        public string Modelo { get; set; } = string.Empty;

        /// <summary>
        /// Label esperada pelo schema do modelo (coluna 1).
        /// Não é usada para inferência, mas necessária para manter o schema compatível com o arquivo do modelo.
        /// </summary>
        [LoadColumn(1)]
        public bool Label { get; set; }
    }
}