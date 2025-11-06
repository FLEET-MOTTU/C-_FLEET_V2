using System;
using System.Collections.Generic;

namespace Csharp.Api.DTOs
{
    /// <summary>
    /// DTO de visualização para uma Zona (cópia de sync).
    /// </summary>
    public class ZonaDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CoordenadasWKT { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de visualização para os detalhes de um Pátio.
    /// </summary>
    public class PateoDetailDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? PlantaBaixaUrl { get; set; }
        public int? PlantaLargura { get; set; }
        public int? PlantaAltura { get; set; }
        public List<ZonaDto> Zonas { get; set; } = new();
    }
}