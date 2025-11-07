namespace Csharp.Api.Entities.Enums
{
    /// <summary> Estados de ciclo de vida e operação da moto (coleta, pátio e externos).</summary>
    public enum TipoStatusMoto
    {
        // Coleta
        PendenteColeta,
        SemPlacaEmColeta,
        MinhaMottuEmColeta,
        EmTransitoComFuncionario,

        // Pátio
        AguardandoVistoria,
        EmReparosSimples,
        EmReparosComplexos,
        AgendadaParaManutencaoExterna,
        ManutencaoInternaEmAndamento,
        ManutencaoConcluida,
        ProntaParaAluguel,

        // Externos
        Alugada,
        Baixada
    }
}