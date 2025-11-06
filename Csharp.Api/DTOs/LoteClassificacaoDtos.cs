using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.DTOs
{
    /// <summary>Request para sugerir zonas por estado (lote).</summary>
    public class LoteClassificacaoRequestDto
    {
        [Required]
        public Guid PateoId { get; set; }

        [Required]
        public List<LoteClassificacaoItemDto> Itens { get; set; } = new();
    }

    public class LoteClassificacaoItemDto
    {
        public string? Placa { get; set; }

        [Required]
        public string TagCodigo { get; set; } = string.Empty;

        [Required]
        public TipoStatusMoto StatusMoto { get; set; }
    }

    /// <summary>Resposta com sugestões por item + resumo.</summary>
    public class LoteClassificacaoRespostaDto
    {
        public Guid PateoId { get; set; }
        public List<LoteClassificacaoSugestaoDto> Sugestoes { get; set; } = new();
    }

    public class LoteClassificacaoSugestaoDto
    {
        public string? Placa { get; set; }
        public string TagCodigo { get; set; } = string.Empty;
        public string StatusMoto { get; set; } = string.Empty;

        public Guid? ZonaIdSugerida { get; set; }
        public string? ZonaNomeSugerida { get; set; }

        public string Justificativa { get; set; } = string.Empty;

        public List<LinkDto> Links { get; set; } = new();
    }
}
