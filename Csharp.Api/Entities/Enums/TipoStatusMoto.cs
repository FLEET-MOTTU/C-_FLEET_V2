namespace Csharp.Api.Entities.Enums
{
    public enum TipoStatusMoto
    {
        // Status relacionados ao processo de coleta
        PendenteColeta,
        SemPlacaEmColeta,
        MinhaMottuEmColeta,
        EmTransitoComFuncionario,

        // Status dentro do pátio
        AguardandoVistoria,
        EmReparosSimples,
        EmReparosComplexos,
        AgendadaParaManutencaoExterna,
        ManutencaoInternaEmAndamento,
        ManutencaoConcluida,
        ProntaParaAluguel,

        // Status mais amplos
        Alugada,
        Baixada
    }
}
