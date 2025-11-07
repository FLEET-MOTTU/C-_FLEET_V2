using Microsoft.ML;
using ModelTrainer.Models; // local model namespace for self-contained run

// 1. Nossos dados "falsos" de treinamento
//    (Agora usamos a classe VistoriaInput local)
var dummyData = new[] {
    new VistoriaInput { Modelo = "ModeloSport100", Label = true },
    new VistoriaInput { Modelo = "ModeloSport100", Label = true },
    new VistoriaInput { Modelo = "ModeloTrilha150", Label = true },
    new VistoriaInput { Modelo = "ModeloUrbana125", Label = false },
    new VistoriaInput { Modelo = "ModeloUrbana125", Label = false }
};

var mlContext = new MLContext();
var dataView = mlContext.Data.LoadFromEnumerable(dummyData);

// 2. O "Pipeline" de ML (Transformar texto em números e treinar)
var pipeline = mlContext.Transforms.Conversion.MapValueToKey(nameof(VistoriaInput.Label))
    .Append(mlContext.Transforms.Text.FeaturizeText("Features", nameof(VistoriaInput.Modelo)))
    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
    // Map the trainer's PredictedLabel key back to the original label value into column "Prediction"
    .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "Prediction", inputColumnName: "PredictedLabel"));

// 3. Treinar
var model = pipeline.Fit(dataView);

// 4. SALVAR O MODELO
mlContext.Model.Save(model, dataView.Schema, "model.zip");
Console.WriteLine("Arquivo 'model.zip' criado com sucesso!");